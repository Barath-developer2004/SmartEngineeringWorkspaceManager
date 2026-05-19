using System;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}