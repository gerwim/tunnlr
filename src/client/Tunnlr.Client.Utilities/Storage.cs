namespace Tunnlr.Client.Utilities;

public static class Storage
{
    public static string GetTunnlrStorageDirectory()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tunnlr");
        Directory.CreateDirectory(directory);

        return directory;
    }
}