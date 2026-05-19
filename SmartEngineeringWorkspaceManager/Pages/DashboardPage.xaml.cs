using System;
using System.Windows;
using System.Windows.Controls;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager.Pages
{
    public partial class DashboardPage : UserControl
    {
        private readonly DashboardService _dashboardService;

        public DashboardPage()
        {
            InitializeComponent();
            _dashboardService = new DashboardService();

            // This runs the moment the Dashboard UI page is opened
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // 1. Fetch & Bind Statistics Cards
                var stats = _dashboardService.GetDashboardStats();

                txtTotalProjects.Text = stats.TotalProjects.ToString();
                txtTotalDocuments.Text = stats.TotalDocuments.ToString();
                txtActiveProjects.Text = stats.ActiveProjects.ToString();
                txtPendingTasks.Text = stats.PendingTasks.ToString();
                txtCompletedTasks.Text = stats.CompletedTasks.ToString();

                // 2. Fetch & Bind Recent Activity List
                var recentActivity = _dashboardService.GetRecentActivity();

                if (recentActivity.Count > 0)
                {
                    icRecentActivity.ItemsSource = recentActivity;
                    icRecentActivity.Visibility = Visibility.Visible;
                    txtNoActivity.Visibility = Visibility.Collapsed;
                }
                else
                {
                    icRecentActivity.Visibility = Visibility.Collapsed;
                    txtNoActivity.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load dashboard data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Allow the user to manually trigger a data pull
            LoadDashboardData();
        }
    }
}