using DCWC_TeamAssist.Models;

namespace DCWC_TeamAssist.Services;

public class CharacterDataService
{
    private List<Character> _allCharacters = new();

    public CharacterDataService()
    {
        InitializeCharacters();
    }

    private void InitializeCharacters()
    {
        // DC World's Collide Characters - IDs match portrait filenames
        _allCharacters = new List<Character>
        {
            new Character { Id = "aquaman", Name = "Aquaman", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Tank },
            new Character { Id = "batman", Name = "Batman", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "bizarro", Name = "Bizarro", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "black-canary", Name = "Black Canary", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "black-manta-", Name = "Black Manta", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "bloodsport", Name = "Bloodsport", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "blue-bettle-", Name = "Blue Beetle", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "captain-boomerang-1", Name = "Captain Boomerang", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "captain-cold-", Name = "Captain Cold", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "catwoman", Name = "Catwoman", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "clay-face", Name = "Clayface", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "constantine-", Name = "Constantine", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Support },
            new Character { Id = "cyborg-", Name = "Cyborg", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "deadshot", Name = "Deadshot", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "deathstroke-", Name = "Deathstroke", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "doctor-fate", Name = "Doctor Fate", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Support },
            new Character { Id = "doctor-phosphorus-", Name = "Doctor Phosphorus", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "frankenstein", Name = "Frankenstein", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "gorilla-", Name = "Gorilla Grodd", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Tank },
            new Character { Id = "green-arrow", Name = "Green Arrow", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "green-lantern-hal-", Name = "Green Lantern (Hal Jordan)", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "green-lantern-jessica", Name = "Green Lantern (Jessica Cruz)", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "green-lantern-john-", Name = "Green Lantern (John Stewart)", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "harley-quinn", Name = "Harley Quinn", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "huntress", Name = "Huntress", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "kid-flash-", Name = "Kid Flash", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "killer-croc", Name = "Killer Croc", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "king-shark-", Name = "King Shark", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "lex-luther", Name = "Lex Luthor", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Support },
            new Character { Id = "martian-", Name = "Martian Manhunter", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Tank },
            new Character { Id = "mera", Name = "Mera", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "mirror-master", Name = "Mirror Master", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "mr-freeze", Name = "Mr. Freeze", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "nightwing", Name = "Nightwing", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "ocean-master", Name = "Ocean Master", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "pandorra", Name = "Pandora", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Support },
            new Character { Id = "phantom-stranger", Name = "Phantom Stranger", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Support },
            new Character { Id = "poison-ivy-", Name = "Poison Ivy", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "raven-", Name = "Raven", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Support },
            new Character { Id = "red-hood", Name = "Red Hood", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "red-robin-", Name = "Red Robin", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "robin-", Name = "Robin", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "scarecrow", Name = "Scarecrow", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "sinestro-", Name = "Sinestro", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "star-sapphire", Name = "Star Sapphire", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "starfire", Name = "Starfire", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "superboy", Name = "Superboy", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "supergirl-", Name = "Supergirl", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "superman", Name = "Superman", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Tank },
            new Character { Id = "the-bridge-", Name = "The Bride", Rarity = CharacterRarity.Epic, Role = CharacterRole.Tank },
            new Character { Id = "the-cheetah", Name = "The Cheetah", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "the-flash-", Name = "The Flash", Rarity = CharacterRarity.Legendary, Role = CharacterRole.DPS },
            new Character { Id = "the-penguin-", Name = "The Penguin", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "the-question", Name = "The Question", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "two-face", Name = "Two-Face", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "vixen", Name = "Vixen", Rarity = CharacterRarity.Epic, Role = CharacterRole.DPS },
            new Character { Id = "weasel-", Name = "Weasel", Rarity = CharacterRarity.Epic, Role = CharacterRole.Support },
            new Character { Id = "wonder-woman", Name = "Wonder Woman", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Tank },
            new Character { Id = "zatanna", Name = "Zatanna", Rarity = CharacterRarity.Legendary, Role = CharacterRole.Support }
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
