using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using S3DProjectSelector.Web.Models;

namespace S3DProjectSelector.Web.Services
{
    public static class ProjectScanner
    {
        // Scans rootPath recursively for files matching the given extensions
        // (default: Navisworks review files) and turns each into a ProjectItem
        // by parsing Client / Project / Discipline out of the folder structure,
        // e.g. Client\Project\Discipline\File.nwd
        public static List<ProjectItem> Scan(string rootPath, string[]? extensions = null)
        {
            extensions ??= new[] { ".nwd", ".nwf", ".nwc" };
            var results = new List<ProjectItem>();

            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
                return results;

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                                  .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            }
            catch (UnauthorizedAccessException)
            {
                return results; // no permission on some sub-folder, skip quietly
            }
            catch (IOException)
            {
                return results; // network path temporarily unavailable
            }

            foreach (var file in files)
            {
                try
                {
                    string relative = Path.GetRelativePath(rootPath, file);
                    string[] parts = relative.Split(Path.DirectorySeparatorChar);

                    string client = parts.Length > 0 ? parts[0] : "";
                    string project = parts.Length > 1 ? parts[1] : "";
                    string discipline = parts.Length > 2 ? parts[2] : "";

                    var info = new FileInfo(file);

                    results.Add(new ProjectItem
                    {
                        Client = client,
                        Project = project,
                        Discipline = discipline,
                        FileName = Path.GetFileName(file),
                        FullPath = file,
                        LastModified = info.LastWriteTime,
                        Category = relative.Contains("Review", StringComparison.OrdinalIgnoreCase)
                                     ? "Review Library"
                                     : "S3D"
                    });
                }
                catch
                {
                    // skip any single file that fails to read (locked, permissions, etc.)
                }
            }

            return results.OrderBy(p => p.Client).ThenBy(p => p.Project).ThenBy(p => p.Discipline).ToList();
        }

        // Review library files live under:
        //   <rootPath>\<ProjectName>\<...some folder whose name contains "batch", spelling/case
        //   not consistent: "Batch", "BATCH_FILES", "Bach_files"...>\User\<the .nwd / .bat files>
        public static List<BatchFileItem> ScanBatchFiles(string rootPath, string projectName, string[]? extensions = null)
        {
            extensions ??= new[] { ".nwd", ".bat" };
            var results = new List<BatchFileItem>();

            if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(projectName))
                return results;

            string projectPath = Path.Combine(rootPath, projectName);
            if (!Directory.Exists(projectPath)) return results;

            string? batchDir = FindDirectoryContaining(projectPath, "batch", maxDepth: 4);
            if (batchDir == null) return results;

            string? userDir = Directory.EnumerateDirectories(batchDir)
                .FirstOrDefault(d => Path.GetFileName(d).Equals("User", StringComparison.OrdinalIgnoreCase));
            if (userDir == null) return results;

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(userDir, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            }
            catch
            {
                return results;
            }

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    results.Add(new BatchFileItem
                    {
                        FileName = info.Name,
                        FullPath = file,
                        LastModified = info.LastWriteTime
                    });
                }
                catch
                {
                    // skip any single file that fails to read
                }
            }

            return results.OrderByDescending(f => f.LastModified).ToList();
        }

        // S3D launch files live under:
        //   <rootPath>\<toolkitRelativePath>\<projectName>\...\*.vbs
        // e.g. ENTER_ROOT_PATH_HERE\ENTER_S3D_TOOLKIT_RELATIVE_PATH_HERE\ProjectName\...
        public static List<BatchFileItem> ScanS3DFiles(string rootPath, string toolkitRelativePath,
                                                         string projectName, string[]? extensions = null)
        {
            extensions ??= new[] { ".vbs" };
            var results = new List<BatchFileItem>();

            if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(projectName))
                return results;

            string projectPath = Path.Combine(rootPath, toolkitRelativePath ?? "", projectName);
            if (!Directory.Exists(projectPath)) return results;

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(projectPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            }
            catch
            {
                return results;
            }

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    results.Add(new BatchFileItem
                    {
                        FileName = info.Name,
                        FullPath = file,
                        LastModified = info.LastWriteTime
                    });
                }
                catch
                {
                    // skip any single file that fails to read
                }
            }

            return results.OrderByDescending(f => f.LastModified).ToList();
        }

        // Breadth-first search, case-insensitive, for the first sub-folder whose name
        // contains `keyword` anywhere in it, up to maxDepth levels deep.
        private static string? FindDirectoryContaining(string root, string keyword, int maxDepth)
        {
            var queue = new Queue<(string path, int depth)>();
            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();

                IEnumerable<string> subDirs;
                try { subDirs = Directory.EnumerateDirectories(current); }
                catch { continue; }

                foreach (var sub in subDirs)
                {
                    string name = Path.GetFileName(sub);
                    if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        return sub;

                    if (depth + 1 < maxDepth)
                        queue.Enqueue((sub, depth + 1));
                }
            }

            return null;
        }
    }
}
