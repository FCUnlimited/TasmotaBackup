#load "tasmota.csx"

#nullable enable


using System.Text.Json;

public class State
{
    public List<Tasmota>? Tasmotas { get; set; }

    public bool SaveState(string dir)
    {
        try
        {
            string path = Path.Combine(dir, "state.json");
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(path, jsonString);
            return true;
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
        return false;
    }

    public static State RestoreState(string dir)
    {
        string path = Path.Combine(dir, "state.json");
        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<State>(jsonString)!;
        }
        else
            return new State();
    }
}