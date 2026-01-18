using DCWC_TeamAssist.Models;

namespace DCWC_TeamAssist.Services;

public class TeamBuilderService
{
    private readonly CharacterDataService _characterData;
    private readonly UserCharacterService _userCharacterService;

    public TeamBuilderService(CharacterDataService characterData, UserCharacterService userCharacterService)
    {
        _characterData = characterData;
        _userCharacterService = userCharacterService;
    }

    public Team BuildBestTeam(GameMode gameMode, int teamSize = 5)
    {
        var ownedCharacters = _userCharacterService.GetOwnedCharactersWithData();
        
        if (ownedCharacters.Count == 0)
        {
            return new Team { Name = "No Characters Available", GameMode = gameMode };
        }

        // Strategy based on game mode
        List<(Character character, UserCharacter userCharacter)> selectedCharacters;

        switch (gameMode)
        {
            case GameMode.Arena:
            case GameMode.PvP:
                // PvP: Balance of DPS and Tank
                selectedCharacters = SelectBalancedTeam(ownedCharacters, teamSize);
                break;
            
            case GameMode.Raid:
            case GameMode.Boss:
                // Raid/Boss: High DPS with some support
                selectedCharacters = SelectHighDpsTeam(ownedCharacters, teamSize);
                break;
            
            case GameMode.Campaign:
                // Campaign: Balanced with survivability
                selectedCharacters = SelectSurvivalTeam(ownedCharacters, teamSize);
                break;
            
            default:
                selectedCharacters = SelectTopPowerTeam(ownedCharacters, teamSize);
                break;
        }

        var team = new Team
        {
            Name = $"Best {gameMode} Team",
            GameMode = gameMode,
            CharacterIds = selectedCharacters.Select(c => c.character.Id).ToList(),
            TotalPower = selectedCharacters.Sum(c => c.character.Power + c.userCharacter.CalculatedPower)
        };

        return team;
    }

    private List<(Character character, UserCharacter userCharacter)> SelectBalancedTeam(
        List<(Character character, UserCharacter userCharacter)> characters, int teamSize)
    {
        var result = new List<(Character character, UserCharacter userCharacter)>();
        
        // Get best tank
        var tanks = characters
            .Where(c => c.character.Role == CharacterRole.Tank)
            .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
            .Take(2);
        result.AddRange(tanks);

        // Get best DPS
        var dps = characters
            .Where(c => c.character.Role == CharacterRole.DPS)
            .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
            .Take(teamSize - 2);
        result.AddRange(dps);

        // Fill remaining with highest power
        if (result.Count < teamSize)
        {
            var remaining = characters
                .Where(c => !result.Any(r => r.character.Id == c.character.Id))
                .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
                .Take(teamSize - result.Count);
            result.AddRange(remaining);
        }

        return result.Take(teamSize).ToList();
    }

    private List<(Character character, UserCharacter userCharacter)> SelectHighDpsTeam(
        List<(Character character, UserCharacter userCharacter)> characters, int teamSize)
    {
        var result = new List<(Character character, UserCharacter userCharacter)>();
        
        // Get 1 tank for survivability
        var tank = characters
            .Where(c => c.character.Role == CharacterRole.Tank)
            .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
            .Take(1);
        result.AddRange(tank);

        // Get mostly DPS
        var dps = characters
            .Where(c => c.character.Role == CharacterRole.DPS)
            .OrderByDescending(c => c.character.Attack + c.userCharacter.CalculatedPower)
            .Take(teamSize - 1);
        result.AddRange(dps);

        // Fill with highest power if needed
        if (result.Count < teamSize)
        {
            var remaining = characters
                .Where(c => !result.Any(r => r.character.Id == c.character.Id))
                .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
                .Take(teamSize - result.Count);
            result.AddRange(remaining);
        }

        return result.Take(teamSize).ToList();
    }

    private List<(Character character, UserCharacter userCharacter)> SelectSurvivalTeam(
        List<(Character character, UserCharacter userCharacter)> characters, int teamSize)
    {
        var result = new List<(Character character, UserCharacter userCharacter)>();
        
        // Get 2 tanks
        var tanks = characters
            .Where(c => c.character.Role == CharacterRole.Tank)
            .OrderByDescending(c => c.character.Health + c.userCharacter.CalculatedPower)
            .Take(2);
        result.AddRange(tanks);

        // Get support/healer if available
        var support = characters
            .Where(c => c.character.Role == CharacterRole.Support || c.character.Role == CharacterRole.Healer)
            .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
            .Take(1);
        result.AddRange(support);

        // Fill with DPS
        var remaining = characters
            .Where(c => !result.Any(r => r.character.Id == c.character.Id))
            .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
            .Take(teamSize - result.Count);
        result.AddRange(remaining);

        return result.Take(teamSize).ToList();
    }

    private List<(Character character, UserCharacter userCharacter)> SelectTopPowerTeam(
        List<(Character character, UserCharacter userCharacter)> characters, int teamSize)
    {
        return characters
            .OrderByDescending(c => c.character.Power + c.userCharacter.CalculatedPower)
            .Take(teamSize)
            .ToList();
    }
}
