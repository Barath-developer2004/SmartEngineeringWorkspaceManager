using System;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }

        // This is referenced as 'Title' in the SQLite Database, but we map it cleanly here
        public string TaskName { get; set; }
        public string Description { get; set; }

        public int AssignedTo { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        // Extra UI Helper Properties (Populated via SQL JOINS)
        public string ProjectName { get; set; }
        public string AssignedToName { get; set; }

        // We calculate this on the fly: If string == "Completed", then true
        public bool IsCompleted => Status == "Completed"; 
    }
}