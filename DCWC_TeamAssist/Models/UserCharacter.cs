namespace DCWC_TeamAssist.Models;

public class UserCharacter
{
    public string CharacterId { get; set; } = string.Empty;
    public int Rank { get; set; } = 1; // 1-15
    public int BadgeLevel { get; set; } = 1; // 1-30 for Epic, 1-40 for Legendary
    public bool IsOwned { get; set; } = true;
    
    // Calculated power based on rank and badge
    public int CalculatedPower => Rank * 100 + BadgeLevel * 50;
}
