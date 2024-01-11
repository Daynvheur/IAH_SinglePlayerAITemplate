using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class GameStateResponse
{
	[JsonInclude]
	public int actionTurn;
	[JsonInclude]
	public int chaosCards;
	[JsonInclude]
	public int level;
	[JsonInclude]
	public bool modemConnected;

	[JsonInclude]
	public int money;
	[JsonInclude]
	public string? osSelected;
	[JsonInclude]
	public bool pcStarted;
	[JsonInclude]
	public int relativeDificulty; // Typo on relativeDificulty
	[JsonInclude]
	public int score;
	[JsonInclude]
	public int TPCards;
	[JsonInclude]
	public int wantedLevel; // Typo on TPCards
}