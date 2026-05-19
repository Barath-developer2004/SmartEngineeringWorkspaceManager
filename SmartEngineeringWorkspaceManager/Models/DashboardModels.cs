using System;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class DashboardStats
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalDocuments { get; set; }
        public int PendingTasks { get; set; }
        public int CompletedTasks { get; set; }
    }

    public class ActivityItem
    {
        public string ActivityDescription { get; set; }
        public DateTime Timestamp { get; set; }

        // Helper to show "2 hours ago" or similar friendly time formats
        public string FriendlyTime 
        {
            get
            {
                var timeSpan = DateTime.Now - Timestamp;
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                return $"{(int)timeSpan.TotalDays} days ago";
            }
        }
    }
}