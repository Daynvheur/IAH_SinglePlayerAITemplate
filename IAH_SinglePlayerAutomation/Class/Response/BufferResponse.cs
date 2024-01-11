using System.Numerics;
using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class BufferResponse
{
	[JsonInclude]
	public List<WebBufferTile> tiles = new();
}

public class WebBufferTile
{
	[JsonInclude]
	public Vector3 position;
	[JsonInclude]
	public required string uniqueID;
}