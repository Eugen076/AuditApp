
export function renderExchangeRateChart(labels, values, currencySymbol, range) {
    const ctx = document.getElementById("rateChart");

    if (ctx && labels.length > 0 && values.length > 0) {
        const isLongRange = parseInt(range) > 30;

        const chartData = labels.map((label, i) => ({
            x: new Date(label),
            y: values[i]
        }));

        new Chart(ctx, {
            type: "line",
            data: {
                datasets: [{
                    label: `1 ${currencySymbol} în RON`,
                    data: chartData,
                    borderColor: "#3b82f6",
                    backgroundColor: "rgba(59, 130, 246, 0.1)",
                    borderWidth: 2,
                    tension: 0,
                    pointRadius: 0,
                    pointHoverRadius: 0,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                interaction: {
                    mode: 'index',
                    intersect: false,
                    axis: 'x'
                },
                plugins: {
                    crosshair: {
                        line: {
                            color: '#666',   // gri închis
                            width: 1,
                            dashPattern: [4, 4] // punctată
                        },
                        sync: {
                            enabled: false
                        },
                        zoom: {
                            enabled: false
                        }
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        callbacks: {
                            title: (tooltipItems) => {
                                const d = tooltipItems[0].parsed.x;
                                const date = new Date(d);
                                return isLongRange
                                    ? date.toLocaleDateString('ro-RO', { year: '2-digit', month: '2-digit', day: '2-digit' }).replace(/\//g, '.')
                                    : date.toLocaleDateString('en-GB', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: 'short' });
                            }
                        }
                    },
                    legend: { position: "top" }
                },
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: isLongRange ? 'month' : 'day',
                            tooltipFormat: isLongRange ? 'yy.MM.dd' : 'MMM dd',
                            displayFormats: {
                                day: isLongRange ? 'yy.MM.dd' : 'MMM dd',
                                month: isLongRange ? 'yy.MM.dd' : 'MMM dd',
                                hour: 'HH:mm'
                            }
                        },
                        title: { display: true, text: "Data" },
                        ticks: {
                            autoSkip: true,
                            maxTicksLimit: isLongRange ? 10 : 15
                        }
                    },
                    y: {
                        beginAtZero: false,
                        title: { display: true, text: "Curs (RON)" }
                    }
                }
            }
        });
    }
}
