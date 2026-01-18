namespace DCWC_TeamAssist.Models;

public class Character
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CharacterRarity Rarity { get; set; }
    public CharacterRole Role { get; set; }
    public int Power { get; set; }
    public int Health { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Abilities { get; set; } = new();
}
