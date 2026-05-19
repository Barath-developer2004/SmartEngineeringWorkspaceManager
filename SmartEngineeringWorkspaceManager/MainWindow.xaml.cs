using System;
using System.Windows;
using System.Windows.Controls;
using SmartEngineeringWorkspaceManager.Pages;

namespace SmartEngineeringWorkspaceManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Load dashboard by default when app starts
            LoadPage(new DashboardPage(), "Dashboard Overview", BtnDashboard);
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null) return;

            // Reset all buttons to the default style
            BtnDashboard.Style = (Style)FindResource("NavButtonStyle");
            BtnActivity.Style = (Style)FindResource("NavButtonStyle");
            BtnProjects.Style = (Style)FindResource("NavButtonStyle");
            BtnDocuments.Style = (Style)FindResource("NavButtonStyle");
            BtnTasks.Style = (Style)FindResource("NavButtonStyle");
            BtnReports.Style = (Style)FindResource("NavButtonStyle");
            BtnSettings.Style = (Style)FindResource("NavButtonStyle");

            // Set the clicked button to the active style
            clickedButton.Style = (Style)FindResource("ActiveNavButtonStyle");

            // Navigate to the respective page
            if (clickedButton == BtnDashboard)
                LoadPage(new DashboardPage(), "Dashboard Overview", clickedButton);
            else if (clickedButton == BtnActivity)
            {
                MainContent.Content = null;
                MainFrame.Navigate(new ActivityFeedPage()); 
                HeaderTitle.Text = "Activity & Notifications";

                if (SmartEngineeringWorkspaceManager.Services.AuthenticationService.CurrentUser != null)
                {
                    txtWelcomeUser.Text = $"Welcome, {SmartEngineeringWorkspaceManager.Services.AuthenticationService.CurrentUser.Username}";
                    txtUserInitial.Text = SmartEngineeringWorkspaceManager.Services.AuthenticationService.CurrentUser.Username.Substring(0, 1).ToUpper();
                }
            }
            else if (clickedButton == BtnProjects)
                LoadPage(new ProjectsPage(), "Projects Management", clickedButton);
            else if (clickedButton == BtnDocuments)
                LoadPage(new DocumentsPage(), "Document Center", clickedButton);
            else if (clickedButton == BtnTasks)
                LoadPage(new TasksPage(), "Task Board", clickedButton);
            else if (clickedButton == BtnReports)
                LoadPage(new ReportsPage(), "Analytics & Reports", clickedButton);
            else if (clickedButton == BtnSettings)
                LoadPage(new SettingsPage(), "System Settings", clickedButton);
        }

        private void LoadPage(UserControl page, string title, Button activeBtn)
        {
            // Clear page if switching back
            if(MainFrame.Content != null) MainFrame.Content = null;
            MainContent.Content = page;
            HeaderTitle.Text = title;

            // Set User info
            if (SmartEngineeringWorkspaceManager.Services.AuthenticationService.CurrentUser != null)
            {
                txtWelcomeUser.Text = $"Welcome, {SmartEngineeringWorkspaceManager.Services.AuthenticationService.CurrentUser.Username}";
                txtUserInitial.Text = SmartEngineeringWorkspaceManager.Services.AuthenticationService.CurrentUser.Username.Substring(0, 1).ToUpper();
            }
        }

        private void BtnSignOut_Click(object sender, RoutedEventArgs e)
        {
            SmartEngineeringWorkspaceManager.Services.AuthenticationService.Logout();

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            this.Close();
        }
    }
}
