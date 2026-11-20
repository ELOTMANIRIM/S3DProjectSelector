namespace S3DProjectSelector.Web.Models
{
    /// <summary>
    /// Bound to the "ProjectSettings" section of appsettings.json.
    /// The root path is set server-side only (not editable from the web page)
    /// so a visitor cannot make the server read an arbitrary path.
    /// </summary>
    public class ProjectSettings
    {
        // Root folder that contains all projects. Set it in appsettings.json.
        public string RootPath { get; set; } = "";

        // Sub-folder (relative to RootPath) that contains the S3D project folders,
        // each holding the .vbs file to launch. Set it in appsettings.json.
        public string S3DToolkitRelativePath { get; set; } = "";
    }
}
