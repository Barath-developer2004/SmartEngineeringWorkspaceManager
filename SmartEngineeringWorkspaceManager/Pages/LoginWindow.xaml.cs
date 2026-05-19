using System.Windows;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager.Pages
{
    public partial class LoginWindow : Window
    {
        private AuthenticationService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthenticationService();

            // Seed an Admin user if needed for testing (Optional setup logic)
            // You could call this once.
            // _authService.RegisterUser("admin", "admin123", "Admin");
            // _authService.RegisterUser("engineer1", "pass123", "Engineer");
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                txtError.Text = "Please enter both username and password.";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (_authService.Login(username, password))
            {
                // Create a log entry indicating that the user logged in
                ActivityLogService logService = new ActivityLogService();
                logService.LogActivity(AuthenticationService.CurrentUser.UserId, "Login", $"User {username} logged into the system.");

                // Login successful
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close(); // Close the login window
            }
            else
            {
                // Login failed
                txtError.Text = "Invalid Username or Password.";
                txtError.Visibility = Visibility.Visible;
            }
        }
    }
}
