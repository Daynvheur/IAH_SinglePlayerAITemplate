using IAH_SinglePlayerAutomation.Class.Response;
using static IAH_SinglePlayerAutomation.Class.APIAnswer;

namespace IAH_SinglePlayerAutomation.Class
{
	public class GameState
	{
		private int _money;
		private int _score;
		private int _relativeDifficulty; // Typo on relativeDificulty
		private int _actionTurn;
		private bool _modemConnected;
		private bool _pcStarted;
		private string? _osSelected;
		private int _chaosCards;
		private int _tpCards; // Typo on TPCards

		private List<Entity> _entities = new();
		private List<Tile> _tiles = new();
		private List<GridNode> _gridNodes = new();
		private List<WebBufferTile> _webBufferTiles = new();

		private string? _apiPassword;
		private string? _remoteBotIp;
		private float _timeRunning; // in seconds
		private int _fps;
		private string? _version;
		private int _level;
		private int _wantedLevel;

		private long _lastPerformedActionTick;

		//private Tile? GetTileByType(string mainType, string type) => _tiles.FirstOrDefault(t => t.type == type && t.mainType == mainType);

		//private List<Tile> GetTilesByType(string mainType, string type) => _tiles.Where(e => e.type == type && e.mainType == mainType).ToList();

		private void PerformedAction() => _lastPerformedActionTick = DateTime.Now.Ticks;

		//public List<Entity> GetEntitiesByFlag(string team) => _entities.Where(e => e.team == team).ToList();

		//public List<Entity> GetEntitiesByType(string type) => _entities.Where(e => e.type == type).ToList();

		/// <summary>actions trigger hostile response, we need to be careful when we perform actions otherwise many enemies will spawn.</summary>
		public bool CanPerformAction() => _lastPerformedActionTick + (TimeSpan.TicksPerSecond * 5) <= DateTime.Now.Ticks;

		public override string ToString()
			=> "Money: " + _money
				+ " | Score: " + _score
				+ " | Relative Difficulty: " + _relativeDifficulty
				+ " | Action Turn: " + _actionTurn
				+ " | Tp Cards: " + _tpCards // _tpCards was missing previously
				+ " | Chaos Cards: " + _chaosCards // _chaosCards was missing previously
				+ " | Level: " + _level // _level was missing previously
				+ " | Wanted Level: " + _wantedLevel // _wantedLevel was missing previously
			+ Environment.NewLine
				+ "Entities: " + _entities.Count
				+ " | Tiles: " + _tiles.Count
				+ " | Grid Nodes: " + _gridNodes.Count
				+ " | Web Buffer Tiles: " + _webBufferTiles.Count
			+ Environment.NewLine
				+ "API Password: " + _apiPassword + " | RemoteBot IP: " + _remoteBotIp
			+ Environment.NewLine
				+ "System FPS: " + _fps + " | Version: " + _version + " | Uptime: " + _timeRunning;  // _timeRunning was missing previously

		public async void RunAiLogic()
		{
			var hostiles = _entities.Where(e => e.team == "HOSTILE" && !e.tags.Contains("CREEP") && !e.tags.Contains("NON-COMBAT")).ToList(); //remove bots that have creep flag or non-combat
			foreach (Entity entity in _entities.Where(e => e.ip == _remoteBotIp))
				await entity.RunAi(hostiles, BotAction);
		}

		public void Apply(GameStateResponse data)
		{
			_money = data.money;
			_score = data.score;
			_relativeDifficulty = data.relativeDificulty;
			_actionTurn = data.actionTurn;
			_modemConnected = data.modemConnected;
			_pcStarted = data.pcStarted;
			_osSelected = data.osSelected;
			_chaosCards = data.chaosCards;
			_tpCards = data.TPCards;
			_level = data.level; // level was missing previously
			_wantedLevel = data.wantedLevel; // wantedLevel was missing previously
		}

		public void Apply(EntitiesResponse data) => _entities = data.entities;

		public void Apply(TilesResponse data) => _tiles = data.tiles;

		/// <summary>very expensive function -> only returns success once for every grid change. store well.
		/// <br/>you need this if you want to make advanced movement logic for your bots.</summary>
		public void Apply(GridResponse data) => _gridNodes = data.gridNodes;

		public void Apply(BufferResponse data) => _webBufferTiles = data.tiles;

		public void Apply(APIPasswordResponse data)
		{
			_apiPassword = data.apiPassword;
			_remoteBotIp = data.ip;
		}

		public void Apply(SystemResponse data)
		{
			_fps = data.fps;
			_version = data.version;
			_timeRunning = data.timeRunning;
		}

		public async Task<APIAnswer> LesserHeal()
		{
			// if we have LESSERHEAL framework item in our inventory we will use it on our remote bot when it is low and heal ourselves.
			if (_webBufferTiles.Count <= 0 || !_modemConnected) return NoWebBufferTilesOrModem;

			//do note that this will use first remoteuserbot, it could be enemy, but for multiplayer you would want to write own code.
			List<Entity> entities = _entities.Where(e => e.type == "REMOTEUSERBOT").ToList();
			if (entities.Count <= 0) return NoEntities;
			List<Tile> tiles = _tiles.Where(t => t.type == "OSTILE" && t.mainType == "FRAMEWORK" && t.frameworkType == "LESSERHEAL").ToList();
			if (tiles.Count <= 0) return NoTiles;

			return await FrameworkAction(tiles[0].uniqueID, entities[0].uniqueID);
		}

		public async Task<APIAnswer> BotAction(string entityUniqueID, string action, object actionValue)
			=> _apiPassword == null || _remoteBotIp == null
				? PasswordIsNull
				: await Requests.DoPost("/v1/botaction", new Dictionary<string, object>
				{
					{"ip", _remoteBotIp},
					{"apiPassword", _apiPassword},
					{"entityUniqueID", entityUniqueID},
					{"actionType", action},
					{"actionValue", actionValue}
				});

		public async Task<APIAnswer> LevelUp() => _tpCards <= 0 ? NoTpCards : await Requests.DoGet("/v1/levelup");

		public async Task<APIAnswer> TileAction()
		{
			if (_webBufferTiles.Count <= 0 || !_modemConnected) return NoWebBufferTilesOrModem;
			Tile? tile = _tiles.FirstOrDefault(t => t.type == "OSTILE" && t.mainType == "WWW_BLOCK");
			if (tile == null) return NoTile;
			if (tile.isBusy) return TileIsBusy;

			await TileAction(tile.uniqueID, -1);
			await Task.Delay(500);
			PerformedAction();
			return await TileAction(tile.uniqueID, 0); // lets spawn first unit from our www block.. in windowsOS for example first one (zero index) is Good Bot
		}

		public async Task<APIAnswer> BrowseInternet()
		{
			// we could later expand this by checking that there are no enemies. etc
			if (_webBufferTiles.Count <= 0 || !_modemConnected) return NoWebBufferTilesOrModem;

			// for this example. we only browse net once... if we have occupied Tile somewhere dont browse net.
			if (_tiles.Any(t => t.type == "MAPTILE" && t.mainType == "OCUPIED")) return TileHasOccupied;

			Tile? tile = _tiles.FirstOrDefault(t => t.type == "MAPTILE" && t.mainType == "PLAYERPC");
			if (tile == null) return NoTile;
			if (tile.isBusy) return TileIsBusy;

			await TileAction(tile.uniqueID, -1);
			await Task.Delay(500);
			PerformedAction();
			return await TileAction(tile.uniqueID, 0);
		}

		public async Task<APIAnswer> InitiateMenuSequence()
		{
			if (_tiles.Count <= 0) return NoTiles;

			Tile? tile = _tiles.FirstOrDefault(t => t.type == "MAPTILE" && t.mainType == "PLAYERPC");
			if (tile == null) return NoTile;
			if (tile.isBusy) return TileIsBusy;

			if (!_pcStarted)
			{
				await TileAction(tile.uniqueID, -1);
				await Task.Delay(500);
				return await TileAction(tile.uniqueID, 0);
			}
			else if (string.IsNullOrEmpty(_osSelected))
			{
				await TileAction(tile.uniqueID, -1);
				await Task.Delay(500);
				return await TileAction(tile.uniqueID, 0); // 0 windows, 1 linux, 2 love
			}
			else if (!_modemConnected)
			{
				// you could make function in the GameState class that check that no Tile is busy.
				Tile? modemTile = _tiles.FirstOrDefault(t => t.type == "ITEMTILE" && t.mainType == "MODEM");
				if (modemTile == null) return NoTile;
				if (modemTile.isBusy) return TileIsBusy;

				await TileAction(modemTile.uniqueID, -1);
				await Task.Delay(500);
				return await TileAction(modemTile.uniqueID, 0);
			}
			else return NoInitLeft;
		}

		public async Task<APIAnswer> BufferItem()
		{
			if (_webBufferTiles.Count <= 0) return NoWebBufferTiles;

			// we need empty Tile, it's where we want to place.
			Tile? tile = _tiles.FirstOrDefault(t => t.type == "MAPTILE" && t.mainType == "EMPTY");
			if (tile == null) return NoTile;
			if (tile.isBusy) return TileIsBusy;

			PerformedAction();

			Console.WriteLine("Place Buffer Item on the Map...");

			APIAnswer response = await Requests.DoPost("/v1/buffer", new Dictionary<string, object>
						{
							// we have websites in a buffer, lets try place website from the buffer on the map.
							{"bufferUniqueID", _webBufferTiles[0].uniqueID},
							{"tileUniqueID", tile.uniqueID}
						});

			if (response == RequestSuccess)
			{
				Console.WriteLine("Buffer Item Placed on The Map...");
				await Task.Delay(2000);
			}
			return response;
		}


		/// <param name="action">-1: just open or close,
		/// <br/>0-4 Button Index -> which button to press, you can send button index without opening Tile, game will open Tile for you and click button.</param>
		private static async Task<APIAnswer> TileAction(string tileUniqueID, int action) => await Requests.DoPost("/v1/tileaction", new Dictionary<string, object> { { "uniqueID", tileUniqueID }, { "action", action } });

		private static async Task<APIAnswer> FrameworkAction(string tileUniqueID, string entityUniqueID) => await Requests.DoPost("/v1/frameworkaction", new Dictionary<string, object> { { "uniqueID", tileUniqueID }, { "entityUniqueID", entityUniqueID } });
	}
}