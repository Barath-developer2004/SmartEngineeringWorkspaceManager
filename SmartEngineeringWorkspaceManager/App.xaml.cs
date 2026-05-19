using System;
using System.Windows;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up Global Exception Handling to aggressively prevent application crashes!
            // When an unhandled error happens anywhere in the app, catch it, log it, and show a safe popup instead of force closing.
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            this.DispatcherUnhandledException += GlobalDispatcherExceptionHandler;

            // Ensure our saved Theme persists
            ThemeService.ApplySavedTheme();

            // Initialize our Database as soon as the application starts
            DatabaseService dbService = new DatabaseService();

            try 
            {
                dbService.InitializeDatabase();

                // Seed test users to allow login right away, typically done via a Setup screen or migration.
                AuthenticationService auth = new AuthenticationService();
                auth.RegisterUser("admin", "admin", "Admin");
                auth.RegisterUser("engineer", "engineer", "Engineer");

                // Test the connection just to be sure
                if (dbService.TestConnection())
                {
                    // Success log (Optional, you could show a MessageBox here if you wanted)
                    Console.WriteLine("Database initialized and connected successfully!");
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Critical System Failure: Could not attach to the core Database. The application cannot proceed.", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            HandleExceptionGracefully(ex, "A critical background error occurred.");
        }

        private void GlobalDispatcherExceptionHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            HandleExceptionGracefully(e.Exception, "An unexpected UI error occurred.");
            e.Handled = true; // Prevents the application from crashing
        }

        private void HandleExceptionGracefully(Exception ex, string context)
        {
            string errorDetails = ex != null ? ex.Message : "Unknown Error";
            MessageBox.Show($"{context}\n\nDetails: {errorDetails}\n\nPlease try again or contact system administration.", 
                "System Error Detected", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
