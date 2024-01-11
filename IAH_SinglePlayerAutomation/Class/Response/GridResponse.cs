using System.Numerics;
using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class GridResponse
{
	[JsonInclude]
	public List<GridNode> gridNodes = new();
}

public class GridNode
{
	[JsonInclude]
	public bool blocked;
	[JsonInclude]
	public Vector3 position;
}