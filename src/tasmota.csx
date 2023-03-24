#nullable enable


using System.Net;

public class Tasmota
{
    public static string? BackupFolderPath { get; set; }

    private string hostname = string.Empty;
    public string Hostname
    {
        get { return string.IsNullOrEmpty(hostname) ? Address : hostname; }
        set { hostname = value; }
    }


    public string Address { get; set; } = string.Empty;

    public string MacAddress { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Hardware { get; set; } = string.Empty;

    public int Rssi { get; set; }

    public string BackupFile { get; set; } = "-";

    public string BackupDate => extractDate(BackupFile)?.ToShortDateString() ?? "-";

    public Tasmota()
    {
    }

    public Tasmota(IPAddress address, string name)
    {
        Hostname = name;
        Address = address.ToString();
    }

    public string GetFilePrefix()
    {
        if (string.IsNullOrEmpty(MacAddress))
        {
            return Address.ToString().Split('.').Last();
        }
        else
        {
            var parts = MacAddress.Split(':');
            var l = parts.Length;
            return parts[l - 2] + parts[l - 1];
        }
    }
    public void UpdateLastBackup()
    {
        try
        {
            if (string.IsNullOrEmpty(BackupFolderPath))
            {
                BackupFile = "No Folder";
                return;
            }
            var prefix = GetFilePrefix();

            var dir = new DirectoryInfo(BackupFolderPath);
            FileInfo? file = null;
            DateOnly? created = null;
            foreach (var f in dir.GetFiles(prefix + "_*.bin"))
            {
                var d = extractDate(f.Name);
                if (created == null || d > created)
                {
                    file = f;
                    created = d;
                }
            }
            BackupFile = file?.Name ?? "-";

        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error searching backup: " + e.Message);
        }
    }

    private DateOnly? extractDate(string? filename)
    {
        if (!string.IsNullOrEmpty(filename)
                && filename.Contains('.')
                && filename.Contains('_'))
        {
            filename = filename.Substring(0, filename.IndexOf('.'));
            var parts = filename.Split('_');
            if (parts.Length >= 1 && DateOnly.TryParseExact(parts[1], "yyMMdd", out var result))
            {
                return result;
            }
        }

        return null;
    }

}