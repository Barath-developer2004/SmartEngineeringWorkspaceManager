using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager.Pages
{
    // Simple Converters for UI formatting (Unread highlighting)
    public class UnreadColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isRead && !isRead)
                return new SolidColorBrush(Color.FromRgb(240, 248, 255)); // AliceBlue for Unread
            return Brushes.White;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => null;
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isRead && !isRead) return Visibility.Visible;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => null;
    }

    public partial class ActivityFeedPage : Page
    {
        private readonly ActivityLogService _activityService;
        private readonly NotificationService _notificationService;

        public ActivityFeedPage()
        {
            InitializeComponent();
            _activityService = new ActivityLogService();
            _notificationService = new NotificationService();

            // Register Converters in page resources manually just for simplicity in this file
            this.Resources.Add("UnreadColorConverter", new UnreadColorConverter());
            this.Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());

            LoadData();
        }

        private void LoadData()
        {
            if (AuthenticationService.CurrentUser == null) return;

            icNotifications.ItemsSource = _notificationService.GetUserNotifications(AuthenticationService.CurrentUser.UserId);
            dgActivityLogs.ItemsSource = _activityService.GetRecentActivities(50);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnMarkRead_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int noteId)
            {
                _notificationService.MarkAsRead(noteId);
                LoadData(); // refresh list
            }
        }
    }
}