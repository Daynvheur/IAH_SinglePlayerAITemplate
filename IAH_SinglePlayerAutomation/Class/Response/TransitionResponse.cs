using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation;

public class TransitionResponse
{
	[JsonInclude]
	public required string state;
}