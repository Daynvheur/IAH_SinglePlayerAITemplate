using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class SystemResponse
{
	[JsonInclude]
	public int fps;
	[JsonInclude]
	public float timeRunning; // in seconds
	[JsonInclude]
	public string? version;
}