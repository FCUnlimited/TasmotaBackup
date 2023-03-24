#r "nuget: Spectre.Console, 0.46.0"
#load "tasmota.csx"
#load "tasmohttp.csx"
#load "command.csx"
#load "state.csx"

#nullable enable

using Spectre.Console;

public static string BackupFolderPath => "./data";

public State SaveState { get; set; }

public List<Tasmota>? Tasmotas
{
    get { return SaveState.Tasmotas; }
    set { SaveState.Tasmotas = value; }
}


try
{
    var run = true;
    var c = 1;
    var actions = new[]
    {
        new Command(c++, "Scan for Tasmotas", scanTasmotas),
        new Command(c++, "Print Tasmotas", printTasmotas),
        new Command(c++, "Backup Tasmotas", backupTasmotas),
        new Command(c++, "Read backups", readBackups),
        // new Command(c++, "Change MQTT Server", printTasmotas),
        // new Command(c++, "Change Wifi config", printTasmotas),
        new Command(c++, "Save & Exit", () => run = false)
    };


    Directory.CreateDirectory(BackupFolderPath);
    Tasmota.BackupFolderPath = BackupFolderPath;

    SaveState = State.RestoreState(BackupFolderPath);

    if (Tasmotas != null)
    {
        readBackups();
        printTasmotas();
    }


    var selection = new SelectionPrompt<Command>()
                .Title("What can I do for you?")
                .AddChoices(actions)
                .UseConverter(x => x.Briefing);



    while (run)
    {
        var cmnd = AnsiConsole.Prompt(selection);
        cmnd.Execute();
    }

    SaveState.SaveState(BackupFolderPath);
    Console.WriteLine("bYe bYe");
}
catch (System.Exception e)
{
    Console.WriteLine("Error: " + e.Message);
    Console.ReadKey();
}




public void scanTasmotas()
{

    var task = getTasmotas();
    task.Wait();
    Tasmotas = task.Result;
    foreach (var t in Tasmotas)
    {
        t.UpdateLastBackup();
    }
    printTasmotas();
}

public void readBackups()
{
    if (Tasmotas != null)
    {
        foreach (var t in Tasmotas)
        {
            t.UpdateLastBackup();
        }
    }
}

public void printTasmotas()
{

    if (Tasmotas == null)
    {
        AnsiConsole.WriteLine("Please scan before use");
        return;
    }
    var table = new Table();

    table.AddColumn(nameof(Tasmota.Hostname));
    table.AddColumn(nameof(Tasmota.Address));
    table.AddColumn(nameof(Tasmota.Version));
    table.AddColumn(nameof(Tasmota.Rssi));
    table.AddColumn(nameof(Tasmota.MacAddress));
    table.AddColumn(nameof(Tasmota.BackupDate));

    foreach (var t in Tasmotas)
    {
        var add = $"[link=http://{t.Address}]{t.Address}[/]";
        table.AddRow(t.Hostname, $"{add}", t.Version, t.Rssi.ToString(), t.MacAddress, t.BackupDate);
    }

    AnsiConsole.Write(table);
}


public void backupTasmotas()
{
    if (Tasmotas == null)
    {
        AnsiConsole.WriteLine("Please scan before use");
        return;
    }

    var tasmos = AnsiConsole.Prompt(
    new MultiSelectionPrompt<Tasmota>()
        .Title("Please select Tasmotas for backup?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down for more)[/]")
        .InstructionsText(
            "[grey](Press [blue]<space>[/] to toggle a Tasmota, " +
            "[green]<enter>[/] to accept)[/]")
        .AddChoices(Tasmotas)
        .UseConverter(x => x.BackupDate + ": " + x.Hostname)
        );


    var tasks = tasmos.Select(tas =>
    {
        var task = downloadConfig(BackupFolderPath, tas);
        return new { task, tasmota = tas };
    }).ToList();

    try
    {
        Task.WhenAll(tasks.Select(x => x.task)).Wait();
        // TODO Tell which tasmos are done
        foreach (var t in tasmos)
        {
            t.UpdateLastBackup();
        }
    }
    catch (System.Exception e)
    {
        Console.WriteLine(e.Message);
    }

    printTasmotas();
}



