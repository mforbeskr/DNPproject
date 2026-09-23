namespace FileRepositories;

internal static class DataDirectory
{
    public static readonly string Path = FindDataDirectory();
    private static string FindDataDirectory()
    {
        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(System.IO.Path.Combine(dir.FullName, "DNPproject.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException("Could not find solution root (DNPproject.sln).");
        }

        string dataDir = System.IO.Path.Combine(dir.FullName, "Data");
        Directory.CreateDirectory(dataDir);
        return dataDir;
    }
}
