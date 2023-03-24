public class Command
{
    public int ID { get; set; }

    public string Briefing { get; set; }

    public Action Execute { get; set; }

    public Command(int id, string briefing, Action execute)
    {
        ID = id;
        Briefing = briefing;
        Execute = execute;
    }
}