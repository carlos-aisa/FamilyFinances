window.familyFinancesCharts = window.familyFinancesCharts || (function () {
    const chartByCanvasId = new Map();

    const euroFormatter = new Intl.NumberFormat("es-ES", {
        style: "currency",
        currency: "EUR",
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

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

        const datasets = (payload.datasets || []).map(toDataset);
        const useDualAxis = !!payload.useDualAxis;

        const scales = {
            x: {
                ticks: {
                    color: "#adb5bd",
                    maxRotation: 0,
                    autoSkip: false
                },
                grid: {
                    color: "rgba(173, 181, 189, 0.15)"
                }
            },
            yDelta: {
                type: "linear",
                position: useDualAxis ? "right" : "left",
                ticks: {
                    color: "#adb5bd",
                    callback: (value) => euroFormatter.format(value)
                },
                grid: {
                    drawOnChartArea: !useDualAxis,
                    color: "rgba(173, 181, 189, 0.15)"
                }
            }
        };

        if (useDualAxis) {
            scales.yBalance = {
                type: "linear",
                position: "left",
                ticks: {
                    color: "#adb5bd",
                    callback: (value) => euroFormatter.format(value)
                },
                grid: {
                    color: "rgba(173, 181, 189, 0.15)"
                }
            };
        } else {
            scales.yDelta.id = "y";
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
                interaction: {
                    mode: "index",
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: "bottom",
                        labels: {
                            color: "#dee2e6",
                            usePointStyle: true,
                            boxWidth: 10
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: (context) => `${context.dataset.label}: ${euroFormatter.format(context.parsed.y)}`
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
        disposeAnnualCompositionChart
    };
})();
