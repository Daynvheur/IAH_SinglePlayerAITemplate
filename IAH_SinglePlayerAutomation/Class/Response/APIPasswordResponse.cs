using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class APIPasswordResponse
{
	[JsonInclude]
	public required string apiPassword;

	[JsonInclude]
	public required string ip;
}