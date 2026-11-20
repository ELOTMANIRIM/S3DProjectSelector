# S3D Project Selector (Web)

Small ASP.NET Core (.NET 9) MVC app that lists project files on a shared folder
and launches the selected `.vbs` (S3D) or `.bat` / `.nwd` (review library) file
on the machine running the app.

## Setup

1. Open `appsettings.json` and fill in:
   - `RootPath`: root folder containing your projects (local or network path)
   - `S3DToolkitRelativePath`: sub-folder of `RootPath` holding the S3D project folders
2. Open `Services/ClientCatalog.cs` and replace the placeholder client and
   project names with your own.
3. Run:

   ```
   dotnet run
   ```

## Security notes

- The app starts files with `Process.Start`. Run it locally (localhost) only and
  do not expose it to an untrusted network.
- Only files inside `RootPath` with allowed extensions can be launched.
