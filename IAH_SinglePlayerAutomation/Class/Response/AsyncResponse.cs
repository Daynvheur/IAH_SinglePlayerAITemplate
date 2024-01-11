using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class;

public class AsyncResponse
{
	[JsonInclude]
	public string? ResponseString { get; set; }

	[JsonInclude]
	public bool IsSuccessStatusCode { get; set; }
}