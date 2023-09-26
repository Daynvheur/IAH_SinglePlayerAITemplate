using System.Numerics;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class BufferResponse
{
	public List<WebBufferTile> tiles = new();
}

public class WebBufferTile
{
	public Vector3 position;
	public required string uniqueID;
}