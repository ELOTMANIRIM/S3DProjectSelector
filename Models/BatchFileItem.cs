using System;

namespace S3DProjectSelector.Web.Models
{
    public class BatchFileItem
    {
        public string FileName { get; set; } = "";
        public string FullPath { get; set; } = "";
        public DateTime LastModified { get; set; }
    }
}
