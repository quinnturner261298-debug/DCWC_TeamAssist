using DCWC_TeamAssist.Models;

namespace DCWC_TeamAssist.Services;

public class CharacterDataService
{
    // This will store all available characters in the game
    private List<Character> _allCharacters = new();

    public CharacterDataService()
    {
        InitializeCharacters();
    }

    private void InitializeCharacters()
    {
        // TODO: Add actual DC World's Collide characters
        // For now, adding sample characters
        _allCharacters = new List<Character>
        {
            new Character
            {
                Id = "batman",
                Name = "Batman",
                Rarity = CharacterRarity.Legendary,
                Role = CharacterRole.Tank,
                Power = 5000,
                Health = 8000,
                Attack = 1200,
                Defense = 900,
                ImageUrl = "/images/batman.png",
                Abilities = new List<string> { "Dark Knight", "Batarang", "Grapple Hook" }
            },
            new Character
            {
                Id = "superman",
                Name = "Superman",
                Rarity = CharacterRarity.Legendary,
                Role = CharacterRole.DPS,
                Power = 5500,
                Health = 7500,
                Attack = 1500,
                Defense = 800,
                ImageUrl = "/images/superman.png",
                Abilities = new List<string> { "Heat Vision", "Super Strength", "Flight" }
            },
            new Character
            {
                Id = "wonderwoman",
                Name = "Wonder Woman",
                Rarity = CharacterRarity.Legendary,
                Role = CharacterRole.Tank,
                Power = 5200,
                Health = 7800,
                Attack = 1300,
                Defense = 950,
                ImageUrl = "/images/wonderwoman.png",
                Abilities = new List<string> { "Lasso of Truth", "Shield Block", "Godly Strike" }
            },
            new Character
            {
                Id = "flash",
                Name = "The Flash",
                Rarity = CharacterRarity.Legendary,
                Role = CharacterRole.DPS,
                Power = 5100,
                Health = 6500,
                Attack = 1600,
                Defense = 600,
                ImageUrl = "/images/flash.png",
                Abilities = new List<string> { "Speed Force", "Lightning Punch", "Time Remnant" }
            },
            new Character
            {
                Id = "greenlantern",
                Name = "Green Lantern",
                Rarity = CharacterRarity.Epic,
                Role = CharacterRole.Support,
                Power = 4200,
                Health = 6800,
                Attack = 1100,
                Defense = 750,
                ImageUrl = "/images/greenlantern.png",
                Abilities = new List<string> { "Power Ring", "Construct Shield", "Energy Blast" }
            },
            new Character
            {
                Id = "aquaman",
                Name = "Aquaman",
                Rarity = CharacterRarity.Epic,
                Role = CharacterRole.Tank,
                Power = 4500,
                Health = 7200,
                Attack = 1000,
                Defense = 850,
                ImageUrl = "/images/aquaman.png",
                Abilities = new List<string> { "Trident Strike", "Tidal Wave", "Telepathy" }
            },
            new Character
            {
                Id = "cyborg",
                Name = "Cyborg",
                Rarity = CharacterRarity.Epic,
                Role = CharacterRole.DPS,
                Power = 4300,
                Health = 6500,
                Attack = 1250,
                Defense = 700,
                ImageUrl = "/images/cyborg.png",
                Abilities = new List<string> { "Sonic Cannon", "Tech Upgrade", "Grid Connection" }
            },
            new Character
            {
                Id = "harleyquinn",
                Name = "Harley Quinn",
                Rarity = CharacterRarity.Epic,
                Role = CharacterRole.DPS,
                Power = 4100,
                Health = 6000,
                Attack = 1350,
                Defense = 550,
                ImageUrl = "/images/harleyquinn.png",
                Abilities = new List<string> { "Mallet Smash", "Explosive Surprise", "Acrobatic Dodge" }
            }
        };
    }

    public List<Character> GetAllCharacters() => _allCharacters;

    public Character? GetCharacterById(string id) => 
        _allCharacters.FirstOrDefault(c => c.Id == id);

    public List<Character> GetCharactersByRarity(CharacterRarity rarity) =>
        _allCharacters.Where(c => c.Rarity == rarity).ToList();

    public List<Character> GetCharactersByRole(CharacterRole role) =>
        _allCharacters.Where(c => c.Role == role).ToList();
}
