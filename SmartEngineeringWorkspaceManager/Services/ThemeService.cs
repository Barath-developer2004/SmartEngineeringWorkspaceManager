using System;
using System.Windows;
using SmartEngineeringWorkspaceManager.Properties;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class ThemeService
    {
        public static void SetTheme(string themeName)
        {
            // Simple approach: Replace the first merged dictionary with the chosen theme.
            // In App.xaml, index 0 is our Theme (Light.xaml), index 1 is CommonStyles.xaml.

            var dict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Themes/{themeName}.xaml", UriKind.Absolute)
            };

            Application.Current.Resources.MergedDictionaries[0] = dict;

            // Save preference so we remember next time the app opens
            Settings.Default.Theme = themeName;
            Settings.Default.Save();
        }

        public static void ApplySavedTheme()
        {
            string savedTheme = Settings.Default.Theme;
            if (string.IsNullOrEmpty(savedTheme)) savedTheme = "Light";
            SetTheme(savedTheme);
        }
    }
}