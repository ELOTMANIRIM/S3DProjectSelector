using System;
using System.Collections.Generic;
using System.Linq;

namespace S3DProjectSelector.Web.Models
{
    public class HomeViewModel
    {
        public string RootPath { get; set; } = "";

        // ---- Panel 1: Launch S3D ----
        public string SelectedS3DClient { get; set; } = "ClientA";
        public List<string> S3DProjectNames { get; set; } = new();
        public string? SelectedS3DProject { get; set; }
        public List<BatchFileItem> S3DFiles { get; set; } = new();
        public string? SelectedS3DFilePath { get; set; }
        public BatchFileItem? SelectedS3DFile =>
            S3DFiles.FirstOrDefault(f => f.FullPath == SelectedS3DFilePath);

        // ---- Panel 2: Launch review library ----
        public string SelectedClient { get; set; } = "ClientA";
        public List<string> ReviewProjects { get; set; } = new();
        public string? SelectedProject { get; set; }
        public List<BatchFileItem> BatchFiles { get; set; } = new();
        public string? SelectedBatchPath { get; set; }
        public BatchFileItem? SelectedBatch =>
            BatchFiles.FirstOrDefault(b => b.FullPath == SelectedBatchPath);

        public string? StatusMessage { get; set; }
        public string? InfoMessage { get; set; }
    }
}
