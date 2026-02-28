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

    function toDataset(dataset) {
        return {
            label: dataset.label,
            data: dataset.values,
            borderColor: dataset.colorHex,
            backgroundColor: dataset.colorHex,
            borderWidth: 2,
            pointRadius: 3,
            pointHoverRadius: 5,
            tension: 0.25,
            spanGaps: true,
            yAxisID: dataset.yAxisId || "y"
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

        let datasets = (payload.datasets || []).map(toDataset);
        const useDualAxis = !!payload.useDualAxis;
        const xTickAutoSkip = payload.xTickAutoSkip === true;
        const xTickMaxTicks = Number.isFinite(payload.xTickMaxTicks) ? payload.xTickMaxTicks : undefined;
        const yTickMaxTicks = Number.isFinite(payload.yTickMaxTicks) ? payload.yTickMaxTicks : undefined;

        const scales = {
            x: {
                ticks: {
                    color: "#adb5bd",
                    maxRotation: 0,
                    autoSkip: xTickAutoSkip,
                    maxTicksLimit: xTickMaxTicks
                },
                grid: {
                    color: "rgba(173, 181, 189, 0.15)"
                }
            }
        };

        if (useDualAxis) {
            scales.yDelta = {
                type: "linear",
                position: "right",
                ticks: {
                    color: "#adb5bd",
                    maxTicksLimit: yTickMaxTicks,
                    callback: (value) => formatEuro(value)
                },
                grid: {
                    drawOnChartArea: false,
                    color: "rgba(173, 181, 189, 0.15)"
                }
            };

            scales.yBalance = {
                type: "linear",
                position: "left",
                ticks: {
                    color: "#adb5bd",
                    maxTicksLimit: yTickMaxTicks,
                    callback: (value) => formatEuro(value)
                },
                grid: {
                    color: "rgba(173, 181, 189, 0.15)"
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
                    color: "#adb5bd",
                    maxTicksLimit: yTickMaxTicks,
                    callback: (value) => formatEuro(value)
                },
                grid: {
                    color: "rgba(173, 181, 189, 0.15)"
                }
            };
        }

        const chart = new window.Chart(canvas, {
            type: "line",
            data: {
                labels: payload.labels || [],
                datasets
            },
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

        const chart = new window.Chart(canvas, {
            type: "pie",
            data: {
                labels: payload.labels || [],
                datasets: [
                    {
                        data: payload.values || [],
                        backgroundColor: payload.colors || [],
                        borderColor: "#1f252d",
                        borderWidth: 1.5
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
        disposeAnnualLineChart,
        renderAnnualCompositionChart,
        disposeAnnualCompositionChart,
        downloadCsv,
        downloadChartImage,
        downloadStreamFile
    };
})();
