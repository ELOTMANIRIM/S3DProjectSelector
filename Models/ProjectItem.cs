using System;

namespace S3DProjectSelector.Web.Models
{
    public class ProjectItem
    {
        public string Client { get; set; } = "";
        public string Project { get; set; } = "";
        public string Discipline { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FullPath { get; set; } = "";
        public DateTime LastModified { get; set; }
        public string Category { get; set; } = "S3D"; // "S3D" or "Review Library"

        public string DisplayName => $"{Client} / {Project} / {Discipline}  —  {FileName}";
        public string SizeOrDateInfo => $"Last modified {LastModified:dd MMM yyyy HH:mm}";
    }
}
