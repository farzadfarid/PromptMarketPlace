document.addEventListener('DOMContentLoaded', function () {
    var canvas = document.getElementById('execChart');
    if (!canvas) return;

    new Chart(canvas.getContext('2d'), {
        type: 'line',
        data: {
            labels: creatorDashboardData.chartLabels,
            datasets: [{
                label: 'اجراها',
                data: creatorDashboardData.chartData,
                borderColor: '#f97316',
                backgroundColor: 'rgba(249,115,22,.08)',
                fill: true,
                tension: 0.4,
                pointRadius: 3
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
        }
    });
});
