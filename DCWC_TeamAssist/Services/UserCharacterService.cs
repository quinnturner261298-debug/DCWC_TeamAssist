using DCWC_TeamAssist.Models;

namespace DCWC_TeamAssist.Services;

public class UserCharacterService
{
    private readonly LocalStorageService _localStorage;
    private readonly CharacterDataService _characterData;
    private const string STORAGE_KEY = "dcwc_user_characters";
    private List<UserCharacter> _userCharacters = new();

    public UserCharacterService(LocalStorageService localStorage, CharacterDataService characterData)
    {
        _localStorage = localStorage;
        _characterData = characterData;
    }

    public async Task InitializeAsync()
    {
        var stored = await _localStorage.GetItemAsync<List<UserCharacter>>(STORAGE_KEY);
        _userCharacters = stored ?? new List<UserCharacter>();
    }

    public async Task SaveAsync()
    {
        await _localStorage.SetItemAsync(STORAGE_KEY, _userCharacters);
    }

    public List<UserCharacter> GetAllUserCharacters() => _userCharacters;

    public UserCharacter? GetUserCharacter(string characterId) =>
        _userCharacters.FirstOrDefault(uc => uc.CharacterId == characterId);

    public async Task AddOrUpdateCharacterAsync(UserCharacter userCharacter)
    {
        var existing = _userCharacters.FirstOrDefault(uc => uc.CharacterId == userCharacter.CharacterId);
        
        if (existing != null)
        {
            existing.Rank = userCharacter.Rank;
            existing.BadgeLevel = userCharacter.BadgeLevel;
            existing.IsOwned = userCharacter.IsOwned;
        }
        else
        {
            _userCharacters.Add(userCharacter);
        }

        await SaveAsync();
    }

    public async Task RemoveCharacterAsync(string characterId)
    {
        var character = _userCharacters.FirstOrDefault(uc => uc.CharacterId == characterId);
        if (character != null)
        {
            _userCharacters.Remove(character);
            await SaveAsync();
        }
    }

    public List<(Character character, UserCharacter userCharacter)> GetOwnedCharactersWithData()
    {
        return _userCharacters
            .Where(uc => uc.IsOwned)
            .Select(uc => (_characterData.GetCharacterById(uc.CharacterId), uc))
            .Where(tuple => tuple.Item1 != null)
            .Select(tuple => (tuple.Item1!, tuple.uc))
            .ToList();
    }

    public int GetMaxBadgeLevel(CharacterRarity rarity) =>
        rarity == CharacterRarity.Epic ? 30 : 40;
}
