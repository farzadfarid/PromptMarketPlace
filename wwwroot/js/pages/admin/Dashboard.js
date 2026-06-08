document.addEventListener('DOMContentLoaded', function () {
    var d = adminDashboardData;

    new Chart(document.getElementById('userChart'), {
        type: 'line',
        data: {
            labels: d.userGrowthLabels,
            datasets: [{
                label: 'کاربر جدید',
                data: d.userGrowthData,
                borderColor: '#f97316',
                backgroundColor: 'rgba(249,115,22,0.1)',
                tension: 0.3,
                fill: true,
                pointRadius: 2
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
        }
    });

    new Chart(document.getElementById('revenueChart'), {
        type: 'bar',
        data: {
            labels: d.revenueLabels,
            datasets: [{
                label: 'درآمد',
                data: d.revenueData,
                backgroundColor: 'rgba(249,115,22,0.7)',
                borderRadius: 4
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true } }
        }
    });
});
