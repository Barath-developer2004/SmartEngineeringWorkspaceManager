using System;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class Revision
    {
        public int RevisionId { get; set; }
        public int DocumentId { get; set; }

        // Example: "v1.0", "v1.1", "v2.0"
        public string VersionNumber { get; set; }

        // This stores the physical location of the file for this specific version
        public string FilePath { get; set; }

        // Foreign key referencing the Users table
        public int ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
        public string RevisionComment { get; set; }

        // Helper property for UI binding
        public string ModifierName { get; set; }
    }
}