using System.Numerics;
using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation;

public class TilesResponse
{
	[JsonInclude]
	public List<Tile> tiles = new();
}

public class Tile
{
	[JsonInclude]
	public string? equipType;
	[JsonInclude]
	public string? frameworkType;
	[JsonInclude]
	public bool isBusy;
	[JsonInclude]
	public bool isOpen;
	[JsonInclude]
	public string? mainType;
	[JsonInclude]
	public Vector3 position;
	[JsonInclude]
	public string? type;
	[JsonInclude]
	public required string uniqueID;
}