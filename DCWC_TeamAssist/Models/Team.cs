namespace DCWC_TeamAssist.Models;

public class Team
{
    public string Name { get; set; } = string.Empty;
    public GameMode GameMode { get; set; }
    public List<string> CharacterIds { get; set; } = new();
    public int TotalPower { get; set; }
}
