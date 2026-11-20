using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3DProjectSelector.Web.Models;
using S3DProjectSelector.Web.Services;

namespace S3DProjectSelector.Web.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string[] S3DAllowedExtensions = { ".vbs" };
        private static readonly string[] ReviewLibAllowedExtensions = { ".bat", ".nwd" };

        private readonly ProjectSettings _settings;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IOptions<ProjectSettings> settings, ILogger<HomeController> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // GET /  or  /Home/Index?s3dClient=...&s3dProject=...&s3dFilePath=...&client=...&project=...&batchPath=...
        [HttpGet]
        public IActionResult Index(string? s3dClient, string? s3dProject, string? s3dFilePath,
                                    string? client, string? project, string? batchPath)
        {
            var vm = BuildViewModel(s3dClient, s3dProject, s3dFilePath, client, project, batchPath);

            if (TempData["Info"] is string info)
                vm.InfoMessage = info;

            return View(vm);
        }

        // GET /Home/LaunchS3D?path=...
        // Runs the selected .vbs file (from the S3D toolkit folder for the chosen
        // client/project) directly on the machine hosting this app.
        [HttpGet]
        public IActionResult LaunchS3D(string path, string? s3dClient, string? s3dProject,
                                        string? client, string? project, string? batchPath)
        {
            var redirectValues = new
            {
                s3dClient,
                s3dProject,
                s3dFilePath = path,
                client,
                project,
                batchPath
            };
            return LaunchFile(path, S3DAllowedExtensions, redirectValues);
        }

        // GET /Home/LaunchReviewLib?path=...
        // Runs the selected .bat / .nwd file directly on the machine hosting this app.
        // This app is meant to run locally on each user's own PC (Kestrel on localhost),
        // so Process.Start here launches the file on that same PC — no download step,
        // no "save to a folder on my computer" involved.
        [HttpGet]
        public IActionResult LaunchReviewLib(string path, string? client, string? project,
                                              string? s3dClient, string? s3dProject, string? s3dFilePath)
        {
            var redirectValues = new
            {
                client,
                project,
                batchPath = path,
                s3dClient,
                s3dProject,
                s3dFilePath
            };
            return LaunchFile(path, ReviewLibAllowedExtensions, redirectValues);
        }

        private IActionResult LaunchFile(string requestedPath, string[] allowedExtensions, object redirectRouteValues)
        {
            if (!IsPathAllowed(requestedPath))
            {
                _logger.LogWarning("Rejected launch attempt outside root: {Path}", requestedPath);
                TempData["Info"] = "Invalid file path.";
                return RedirectToAction(nameof(Index), redirectRouteValues);
            }

            if (string.IsNullOrWhiteSpace(requestedPath) || !System.IO.File.Exists(requestedPath))
            {
                TempData["Info"] = "That file no longer exists — click Refresh and try again.";
                return RedirectToAction(nameof(Index), redirectRouteValues);
            }

            string ext = Path.GetExtension(requestedPath).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Info"] = $"Only {string.Join(" or ", allowedExtensions)} files can be launched here.";
                return RedirectToAction(nameof(Index), redirectRouteValues);
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = requestedPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(requestedPath) ?? string.Empty
                };
                Process.Start(psi);
                TempData["Info"] = $"Launching {Path.GetFileName(requestedPath)}…";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to launch file: {Path}", requestedPath);
                TempData["Info"] = $"Failed to launch {Path.GetFileName(requestedPath)}: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), redirectRouteValues);
        }

        // Prevents a visitor from requesting an arbitrary file on the server —
        // only files inside the configured RootPath can ever be launched.
        private bool IsPathAllowed(string requestedPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(requestedPath) || string.IsNullOrWhiteSpace(_settings.RootPath))
                    return false;

                string fullRequested = Path.GetFullPath(requestedPath);
                string fullRoot = Path.GetFullPath(_settings.RootPath);

                return fullRequested.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // ---- Bottom quick-action row (placeholders, same as the desktop app) ----

        [HttpGet]
        public IActionResult Sbb(string? s3dClient, string? s3dProject, string? s3dFilePath,
                                  string? client, string? project, string? batchPath)
        {
            TempData["Info"] = "Wire this up to your SBB workflow.";
            return RedirectToAction(nameof(Index), new { s3dClient, s3dProject, s3dFilePath, client, project, batchPath });
        }

        [HttpGet]
        public IActionResult Reports(string? s3dClient, string? s3dProject, string? s3dFilePath,
                                      string? client, string? project, string? batchPath)
        {
            TempData["Info"] = "Wire this up to your reports view.";
            return RedirectToAction(nameof(Index), new { s3dClient, s3dProject, s3dFilePath, client, project, batchPath });
        }

        [HttpGet]
        public IActionResult Settings(string? s3dClient, string? s3dProject, string? s3dFilePath,
                                       string? client, string? project, string? batchPath)
        {
            TempData["Info"] = "Wire this up to a settings page (extensions, catalog, etc.).";
            return RedirectToAction(nameof(Index), new { s3dClient, s3dProject, s3dFilePath, client, project, batchPath });
        }

        [HttpGet]
        public IActionResult Error() => View();

        // ---- Shared build logic ----

        private HomeViewModel BuildViewModel(string? s3dClient, string? s3dProject, string? s3dFilePath,
                                              string? client, string? project, string? batchPath)
        {
            string rootPath = _settings.RootPath;

            var vm = new HomeViewModel
            {
                RootPath = rootPath,
                SelectedS3DClient = string.IsNullOrWhiteSpace(s3dClient) ? "ClientA" : s3dClient!,
                SelectedS3DProject = s3dProject,
                SelectedS3DFilePath = s3dFilePath,
                SelectedClient = string.IsNullOrWhiteSpace(client) ? "ClientA" : client!,
                SelectedProject = project,
                SelectedBatchPath = batchPath
            };

            // ---- Panel 1: Launch S3D ----
            vm.S3DProjectNames = ClientCatalog.S3DProjectsByClient.TryGetValue(vm.SelectedS3DClient, out var s3dList)
                ? s3dList.ToList()
                : new List<string>();

            // If the selected project doesn't belong to the current client's list
            // (client just changed, or the list is empty), reset both the project
            // AND the previously chosen file — otherwise a stale project name from
            // a different client keeps being scanned and its .vbs file keeps showing up.
            if (string.IsNullOrEmpty(vm.SelectedS3DProject) || !vm.S3DProjectNames.Contains(vm.SelectedS3DProject))
            {
                vm.SelectedS3DProject = vm.S3DProjectNames.Count > 0 ? vm.S3DProjectNames[0] : null;
                vm.SelectedS3DFilePath = null;
            }

            string s3dStatus;
            if (!string.IsNullOrEmpty(vm.SelectedS3DProject))
            {
                vm.S3DFiles = ProjectScanner.ScanS3DFiles(rootPath, _settings.S3DToolkitRelativePath, vm.SelectedS3DProject);

                if (string.IsNullOrEmpty(vm.SelectedS3DFilePath) && vm.S3DFiles.Count > 0)
                    vm.SelectedS3DFilePath = vm.S3DFiles[0].FullPath;

                string fullProjectPath = Path.Combine(rootPath, _settings.S3DToolkitRelativePath, vm.SelectedS3DProject);
                s3dStatus = vm.S3DFiles.Count == 0
                    ? $"No .vbs file found under {fullProjectPath}. Check the path and server network access."
                    : $"Found {vm.S3DFiles.Count} .vbs file(s) for {vm.SelectedS3DProject}.";
            }
            else
            {
                s3dStatus = vm.S3DProjectNames.Count == 0
                    ? $"No S3D project configured yet for client \"{vm.SelectedS3DClient}\"."
                    : "Select a project to see its .vbs launch file.";
            }

            vm.StatusMessage = s3dStatus;

            // ---- Panel 2: Launch review library ----
            vm.ReviewProjects = ClientCatalog.ProjectsByClient.TryGetValue(vm.SelectedClient, out var list)
                ? list.ToList()
                : new List<string>();

            if (string.IsNullOrEmpty(vm.SelectedProject) && vm.ReviewProjects.Count > 0)
                vm.SelectedProject = vm.ReviewProjects[0];

            if (!string.IsNullOrEmpty(vm.SelectedProject))
            {
                vm.BatchFiles = ProjectScanner.ScanBatchFiles(rootPath, vm.SelectedProject);

                if (string.IsNullOrEmpty(vm.SelectedBatchPath) && vm.BatchFiles.Count > 0)
                    vm.SelectedBatchPath = vm.BatchFiles[0].FullPath;
            }

            return vm;
        }
    }
}
