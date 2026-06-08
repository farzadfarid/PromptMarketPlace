document.addEventListener('DOMContentLoaded', function () {
    new Chart(document.getElementById('revChart'), {
        type: 'bar',
        data: {
            labels: revenueReportData.chartLabels,
            datasets: [{
                label: 'درآمد',
                data: revenueReportData.chartData,
                backgroundColor: 'rgba(25,135,84,0.7)',
                borderRadius: 4
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true } }
        }
    });
});
