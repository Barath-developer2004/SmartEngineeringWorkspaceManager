using System.Windows;
using System.Windows.Controls;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager.Pages
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.SetTheme("Light");
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.SetTheme("Dark");
        }
    }
}