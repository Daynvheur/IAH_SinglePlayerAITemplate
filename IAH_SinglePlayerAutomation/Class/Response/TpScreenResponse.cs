using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class TpScreenResponse
{
	[JsonInclude]
	public List<TpCard> chaosCards = new();
	[JsonInclude]
	public List<TpCard> tpCards = new();
}

public class TpCard
{
	[JsonInclude]
	public string? type;
}