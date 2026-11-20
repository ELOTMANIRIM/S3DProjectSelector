using System;
using System.Collections.Generic;

namespace S3DProjectSelector.Web.Services
{
    // Static catalog: which project folders (under the configured root) belong
    // to which client. Replace the placeholder names below with your own.
    public static class ClientCatalog
    {
        // Projects shown in the "Launch review library" panel.
        public static readonly Dictionary<string, string[]> ProjectsByClient = new()
        {
            ["ClientA"] = new[]
            {
                "ENTER_PROJECT_FOLDER_NAME_1",
                "ENTER_PROJECT_FOLDER_NAME_2"
            },
            ["ClientB"] = new[]
            {
                "ENTER_PROJECT_FOLDER_NAME_3"
            }
        };

        // Projects shown in the "Launch S3D" panel (folders under
        // RootPath\S3DToolkitRelativePath, each containing a .vbs file).
        public static readonly Dictionary<string, string[]> S3DProjectsByClient = new()
        {
            ["ClientA"] = new[]
            {
                "ENTER_S3D_PROJECT_FOLDER_NAME_1",
                "ENTER_S3D_PROJECT_FOLDER_NAME_2"
            },
            ["ClientB"] = Array.Empty<string>()
        };
    }
}
