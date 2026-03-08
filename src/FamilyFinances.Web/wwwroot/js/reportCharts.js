window.familyFinancesCharts = window.familyFinancesCharts || (function () {
    const chartByCanvasId = new Map();
    const formatterCache = new Map();

    function resolveCulture() {
        const docCulture = document?.documentElement?.lang;
        const fallback = "es-ES";

        try {
            if (window.cultureHelper && typeof window.cultureHelper.getCulture === "function") {
                const selected = window.cultureHelper.getCulture();
                if (selected && typeof selected === "string") {
                    return selected;
                }
            }
        } catch {
            // Ignore helper errors and fallback to metadata/default culture.
        }

        return docCulture && typeof docCulture === "string" ? docCulture : fallback;
    }

    function formatEuro(value) {
        const culture = resolveCulture();
        if (!formatterCache.has(culture)) {
            formatterCache.set(culture, new Intl.NumberFormat(culture, {
                style: "currency",
                currency: "EUR",
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }));
        }

        return formatterCache.get(culture).format(value);
    }

    function resolveChartTheme() {
        const style = window.getComputedStyle
            ? window.getComputedStyle(document.documentElement)
            : null;

        const pick = (tokenName, fallback) => {
            if (!style) {
                return fallback;
            }

            const tokenValue = style.getPropertyValue(tokenName);
            return tokenValue && tokenValue.trim().length > 0 ? tokenValue.trim() : fallback;
        };

        return {
            tickColor: pick("--ff-chart-tick-color", "#adb5bd"),
            gridColor: pick("--ff-chart-grid-color", "rgba(173, 181, 189, 0.15)"),
            tooltipBackground: pick("--ff-chart-tooltip-bg", "#223149"),
            tooltipText: pick("--ff-chart-tooltip-text", "#e8efff"),
            surfaceBorder: pick("--ff-border-soft", "#1f252d"),
            cutoffLine: pick("--ff-chart-cutoff-line", "rgba(86, 199, 255, 0.85)"),
            cutoffShade: pick("--ff-chart-cutoff-shade", "rgba(86, 199, 255, 0.08)")
        };
    }

    function toRgba(color, alpha) {
        if (typeof color !== "string") {
            return color;
        }

        const hex = color.trim();
        if (!hex.startsWith("#")) {
            return color;
        }

        const normalized = hex.length === 4
            ? `#${hex[1]}${hex[1]}${hex[2]}${hex[2]}${hex[3]}${hex[3]}`
            : hex;

        if (normalized.length !== 7) {
            return color;
        }

        const r = parseInt(normalized.slice(1, 3), 16);
        const g = parseInt(normalized.slice(3, 5), 16);
        const b = parseInt(normalized.slice(5, 7), 16);
        if (!Number.isFinite(r) || !Number.isFinite(g) || !Number.isFinite(b)) {
            return color;
        }

        return `rgba(${r}, ${g}, ${b}, ${alpha})`;
    }

    function toDataset(dataset, cutoffIndex) {
        const hasCutoff = Number.isFinite(cutoffIndex) && cutoffIndex >= 0;
        return {
            label: dataset.label,
            data: dataset.values,
            borderColor: dataset.colorHex,
            backgroundColor: dataset.colorHex,
            borderWidth: 2.6,
            pointRadius: 0,
            pointHoverRadius: 4,
            pointHitRadius: 12,
            tension: 0.33,
            cubicInterpolationMode: "monotone",
            spanGaps: true,
            fill: false,
            yAxisID: dataset.yAxisId || "y",
            segment: hasCutoff
                ? {
                    borderDash: (context) => context.p0DataIndex >= cutoffIndex ? [7, 5] : undefined,
                    borderColor: (context) => context.p0DataIndex >= cutoffIndex
                        ? toRgba(dataset.colorHex, 0.78)
                        : dataset.colorHex
                }
                : undefined
        };
    }

    function resolveCutoffMetadata(payload) {
        const labelsCount = (payload.labels || []).length;

        const markerDayRaw = Number(payload.markerDay);
        const markerDay = Number.isFinite(markerDayRaw) ? Math.trunc(markerDayRaw) : null;
        const totalDaysRaw = Number(payload.totalDays);
        const totalDays = Number.isFinite(totalDaysRaw)
            ? Math.trunc(totalDaysRaw)
            : labelsCount;

        if (markerDay && markerDay >= 1) {
            return {
                markerIndex: Math.max(0, markerDay - 1),
                totalUnits: Math.max(1, totalDays),
                hasMarker: markerDay < totalDays
            };
        }

        const markerMonthRaw = Number(payload.markerMonth);
        const markerMonth = Number.isFinite(markerMonthRaw) ? Math.trunc(markerMonthRaw) : null;
        const totalMonthsRaw = Number(payload.totalMonths);
        const totalMonths = Number.isFinite(totalMonthsRaw)
            ? Math.trunc(totalMonthsRaw)
            : labelsCount;

        if (markerMonth && markerMonth >= 1) {
            return {
                markerIndex: Math.max(0, markerMonth - 1),
                totalUnits: Math.max(1, totalMonths),
                hasMarker: markerMonth < totalMonths
            };
        }

        return {
            markerIndex: null,
            totalUnits: Math.max(1, labelsCount),
            hasMarker: false
        };
    }

    function buildCutoffMarkerPlugin(markerIndex, totalUnits, theme) {
        if (!Number.isFinite(markerIndex) || markerIndex < 0) {
            return null;
        }

        if (!Number.isFinite(totalUnits) || totalUnits <= 1 || markerIndex >= totalUnits - 1) {
            return null;
        }

        return {
            id: "ffCutoffMarker",
            _getXForDataIndex(chart, dataIndex) {
                const labels = chart.data?.labels || [];
                const labelsCount = labels.length;
                const chartArea = chart.chartArea;
                const xScale = chart.scales?.x;
                if (!xScale || !chartArea || labelsCount === 0) {
                    return null;
                }

                const boundedIndex = Math.min(labelsCount - 1, Math.max(0, dataIndex));
                const labelValue = labels[boundedIndex];

                if (typeof xScale.getPixelForValue === "function") {
                    const pixelForLabel = xScale.getPixelForValue(labelValue);
                    if (Number.isFinite(pixelForLabel)) {
                        return pixelForLabel;
                    }
                }

                if (labelsCount === 1) {
                    return chartArea.left;
                }

                const ratio = boundedIndex / (labelsCount - 1);
                return chartArea.left + (chartArea.right - chartArea.left) * ratio;
            },
            beforeDatasetsDraw(chart) {
                const labelsCount = (chart.data?.labels || []).length;
                if (labelsCount <= 1) {
                    return;
                }

                const chartArea = chart.chartArea;
                if (!chartArea) {
                    return;
                }

                const boundedIndex = Math.min(labelsCount - 1, Math.max(0, markerIndex));
                const markerX = this._getXForDataIndex(chart, boundedIndex);
                const nextTickX = boundedIndex + 1 < labelsCount
                    ? this._getXForDataIndex(chart, boundedIndex + 1)
                    : chartArea.right;
                if (!Number.isFinite(markerX) || !Number.isFinite(nextTickX)) {
                    return;
                }

                const shadeStartX = boundedIndex + 1 < labelsCount
                    ? (markerX + nextTickX) / 2
                    : chartArea.right;

                if (shadeStartX >= chartArea.right) {
                    return;
                }

                const ctx = chart.ctx;
                ctx.save();
                ctx.fillStyle = theme.cutoffShade || "rgba(86, 199, 255, 0.18)";
                ctx.fillRect(
                    shadeStartX,
                    chartArea.top,
                    chartArea.right - shadeStartX,
                    chartArea.bottom - chartArea.top);
                ctx.restore();
            },
            afterDatasetsDraw(chart) {
                const labelsCount = (chart.data?.labels || []).length;
                if (labelsCount === 0) {
                    return;
                }

                const chartArea = chart.chartArea;
                if (!chartArea) {
                    return;
                }

                const boundedIndex = Math.min(labelsCount - 1, Math.max(0, markerIndex));
                const markerX = this._getXForDataIndex(chart, boundedIndex);
                if (!Number.isFinite(markerX)) {
                    return;
                }

                const ctx = chart.ctx;
                ctx.save();
                ctx.strokeStyle = theme.cutoffLine || "rgba(86, 199, 255, 1)";
                ctx.lineWidth = 2.1;
                ctx.setLineDash([6, 4]);
                ctx.beginPath();
                ctx.moveTo(markerX, chartArea.top);
                ctx.lineTo(markerX, chartArea.bottom);
                ctx.stroke();
                ctx.setLineDash([]);

                ctx.fillStyle = theme.cutoffLine || "rgba(86, 199, 255, 1)";
                ctx.beginPath();
                ctx.arc(markerX, chartArea.top + 6, 3, 0, Math.PI * 2);
                ctx.fill();
                ctx.restore();
            }
        };
    }

    function triggerBrowserDownload(blob, fileName) {
        if (!blob || !fileName) {
            return;
        }

        const anchor = document.createElement("a");
        const objectUrl = URL.createObjectURL(blob);
        anchor.href = objectUrl;
        anchor.download = fileName;
        anchor.style.display = "none";
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        URL.revokeObjectURL(objectUrl);
    }

    function downloadCsv(fileName, csvContent) {
        if (!fileName || typeof csvContent !== "string") {
            return;
        }

        const bom = "\uFEFF";
        const blob = new Blob([bom, csvContent], { type: "text/csv;charset=utf-8;" });
        triggerBrowserDownload(blob, fileName);
    }

    function downloadChartImage(canvasId, fileName) {
        if (!canvasId || !fileName) {
            return;
        }

        const canvas = document.getElementById(canvasId);
        if (!canvas || typeof canvas.toBlob !== "function") {
            return;
        }

        canvas.toBlob((blob) => {
            if (!blob) {
                return;
            }

            triggerBrowserDownload(blob, fileName);
        }, "image/png");
    }

    async function downloadStreamFile(fileName, contentType, streamReference) {
        if (!fileName || !streamReference) {
            return;
        }

        const arrayBuffer = await streamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer], { type: contentType || "application/octet-stream" });
        triggerBrowserDownload(blob, fileName);
    }

    function renderAnnualLineChart(canvasId, payload) {
        if (!canvasId || !payload || !window.Chart) {
            return;
        }

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            return;
        }

        const previous = chartByCanvasId.get(canvasId);
        if (previous) {
            previous.destroy();
            chartByCanvasId.delete(canvasId);
        }

        const cutoff = resolveCutoffMetadata(payload);
        let datasets = (payload.datasets || []).map((dataset) => toDataset(dataset, cutoff.markerIndex));
        const useDualAxis = !!payload.useDualAxis;
        const xTickAutoSkip = payload.xTickAutoSkip === true;
        const xTickMaxTicks = Number.isFinite(payload.xTickMaxTicks) ? payload.xTickMaxTicks : undefined;
        const yTickMaxTicks = Number.isFinite(payload.yTickMaxTicks) ? payload.yTickMaxTicks : undefined;

        const theme = resolveChartTheme();
        const cutoffMarkerPlugin = cutoff.hasMarker
            ? buildCutoffMarkerPlugin(cutoff.markerIndex, cutoff.totalUnits, theme)
            : null;

        const scales = {
            x: {
                ticks: {
                    color: theme.tickColor,
                    maxRotation: 0,
                    autoSkip: xTickAutoSkip,
                    maxTicksLimit: xTickMaxTicks
                },
                grid: {
                    color: theme.gridColor,
                    borderDash: [4, 4]
                }
            }
        };

        if (useDualAxis) {
            scales.yDelta = {
                type: "linear",
                position: "right",
                ticks: {
                    color: theme.tickColor,
                    maxTicksLimit: yTickMaxTicks,
                    callback: (value) => formatEuro(value)
                },
                grid: {
                    drawOnChartArea: false,
                    color: theme.gridColor,
                    borderDash: [4, 4]
                }
            };

            scales.yBalance = {
                type: "linear",
                position: "left",
                ticks: {
                    color: theme.tickColor,
                    maxTicksLimit: yTickMaxTicks,
                    callback: (value) => formatEuro(value)
                },
                grid: {
                    color: theme.gridColor,
                    borderDash: [4, 4]
                }
            };
        } else {
            datasets = datasets.map((dataset) => ({
                ...dataset,
                yAxisID: "y"
            }));

            scales.y = {
                type: "linear",
                position: "left",
                ticks: {
                    color: theme.tickColor,
                    maxTicksLimit: yTickMaxTicks,
                    callback: (value) => formatEuro(value)
                },
                grid: {
                    color: theme.gridColor,
                    borderDash: [4, 4]
                }
            };
        }

        const chart = new window.Chart(canvas, {
            type: "line",
            data: {
                labels: payload.labels || [],
                datasets
            },
            plugins: cutoffMarkerPlugin ? [cutoffMarkerPlugin] : [],
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false,
                interaction: {
                    mode: "index",
                    intersect: false
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: theme.tooltipBackground,
                        titleColor: theme.tooltipText,
                        bodyColor: theme.tooltipText,
                        borderColor: theme.gridColor,
                        borderWidth: 1,
                        cornerRadius: 10,
                        padding: 10,
                        callbacks: {
                            label: (context) => `${context.dataset.label}: ${formatEuro(context.parsed.y)}`
                        }
                    }
                },
                scales
            }
        });

        chartByCanvasId.set(canvasId, chart);
    }

    function renderAnnualBarChart(canvasId, payload) {
        if (!canvasId || !payload || !window.Chart) {
            return;
        }

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            return;
        }

        const previous = chartByCanvasId.get(canvasId);
        if (previous) {
            previous.destroy();
            chartByCanvasId.delete(canvasId);
        }

        const theme = resolveChartTheme();
        const cutoff = resolveCutoffMetadata(payload);
        const cutoffMarkerPlugin = cutoff.hasMarker
            ? buildCutoffMarkerPlugin(cutoff.markerIndex, cutoff.totalUnits, theme)
            : null;
        const yTickMaxTicks = Number.isFinite(payload.yTickMaxTicks) ? payload.yTickMaxTicks : undefined;
        const datasets = (payload.datasets || []).map((dataset) => ({
            label: dataset.label,
            data: dataset.values,
            borderColor: dataset.colorHex,
            backgroundColor: `${dataset.colorHex}B3`,
            borderWidth: 1.4,
            borderRadius: 4,
            maxBarThickness: 28
        }));

        const chart = new window.Chart(canvas, {
            type: "bar",
            data: {
                labels: payload.labels || [],
                datasets
            },
            plugins: cutoffMarkerPlugin ? [cutoffMarkerPlugin] : [],
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false,
                interaction: {
                    mode: "index",
                    intersect: false
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: theme.tooltipBackground,
                        titleColor: theme.tooltipText,
                        bodyColor: theme.tooltipText,
                        borderColor: theme.gridColor,
                        borderWidth: 1,
                        cornerRadius: 10,
                        padding: 10,
                        callbacks: {
                            label: (context) => `${context.dataset.label}: ${formatEuro(context.parsed.y)}`
                        }
                    }
                },
                scales: {
                    x: {
                        ticks: {
                            color: theme.tickColor,
                            maxRotation: 0
                        },
                        grid: {
                            color: theme.gridColor,
                            borderDash: [4, 4]
                        }
                    },
                    y: {
                        type: "linear",
                        position: "left",
                        ticks: {
                            color: theme.tickColor,
                            maxTicksLimit: yTickMaxTicks,
                            callback: (value) => formatEuro(value)
                        },
                        grid: {
                            color: theme.gridColor,
                            borderDash: [4, 4]
                        }
                    }
                }
            }
        });

        chartByCanvasId.set(canvasId, chart);
    }

    function disposeAnnualLineChart(canvasId) {
        const existing = chartByCanvasId.get(canvasId);
        if (!existing) {
            return;
        }

        existing.destroy();
        chartByCanvasId.delete(canvasId);
    }

    function renderAnnualCompositionChart(canvasId, payload) {
        if (!canvasId || !payload || !window.Chart) {
            return;
        }

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            return;
        }

        const previous = chartByCanvasId.get(canvasId);
        if (previous) {
            previous.destroy();
            chartByCanvasId.delete(canvasId);
        }

        const theme = resolveChartTheme();
        const chart = new window.Chart(canvas, {
            type: "pie",
            data: {
                labels: payload.labels || [],
                datasets: [
                    {
                        data: payload.values || [],
                        backgroundColor: payload.colors || [],
                        borderColor: theme.surfaceBorder,
                        borderWidth: 2,
                        hoverOffset: 8,
                        spacing: 2
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: theme.tooltipBackground,
                        titleColor: theme.tooltipText,
                        bodyColor: theme.tooltipText,
                        borderColor: theme.gridColor,
                        borderWidth: 1,
                        cornerRadius: 10,
                        padding: 10,
                        callbacks: {
                            label: (context) => `${context.label}: ${Number(context.parsed || 0).toFixed(2)}%`
                        }
                    }
                }
            }
        });

        chartByCanvasId.set(canvasId, chart);
    }

    function disposeAnnualCompositionChart(canvasId) {
        const existing = chartByCanvasId.get(canvasId);
        if (!existing) {
            return;
        }

        existing.destroy();
        chartByCanvasId.delete(canvasId);
    }

    return {
        renderAnnualLineChart,
        renderAnnualBarChart,
        disposeAnnualLineChart,
        renderAnnualCompositionChart,
        disposeAnnualCompositionChart,
        downloadCsv,
        downloadChartImage,
        downloadStreamFile
    };
})();
