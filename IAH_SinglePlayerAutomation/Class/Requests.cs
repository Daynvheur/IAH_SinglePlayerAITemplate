using IAH_SinglePlayerAutomation.Class;
using IAH_SinglePlayerAutomation.Class.Response;
using System.Text;
using System.Text.Json;
using static IAH_SinglePlayerAutomation.Class.APIAnswer;

namespace IAH_SinglePlayerAutomation;

public abstract class Requests
{
	private const string remoteip = "127.0.0.1";  // in campaign mode this will be always 127.0.0.1 for your RemoteUserBot even if you operate your AI from some other PC.
	private const string yourkey = "yourkey"; // HTTPS://IAMHACKER.CC -> Get API Key
	private static HttpClient _httpClient = new() { BaseAddress = new($"http://{remoteip}:6800") }; // you can specify different port in the game launch parameters (default is 6800) -> for example: -apiPort 6900
	private static GameState gameState = new();

	public static string GameState { get => gameState.ToString(); }

	public static bool CanPerformAction() => gameState.CanPerformAction();

	public static async Task<TransitionResponse?> GetPlayerState()
	{
		AsyncResponse response = await SendGetRequestAsync("/v1/playerstate");
		return !response.IsSuccessStatusCode || response.ResponseString == null
			? default
			: JsonSerializer.Deserialize<TransitionResponse>(response.ResponseString);
	}

	public static async Task<APIAnswer> GetApiPassword(TransitionResponse transition)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state != "INGAME") return StateNotInGame;

		// API Password is used to authenticate your APILicense with the game and provide
		// you with a password that you have to protect during your gameplay session.
		// API Password gives you ownership of the bots that are associated with your RemoteUserBot.
		// When you perform Bot Functions you need always supply API Password.
		// API Password is always regenerated when your RemoteUserBot respawns. if this function is not called every 15-24 seconds game will exit AI Mode.
		// In multiplayer certain bots can crack API Passwords giving you or enemies ability to hack bots.
		// API Key and API Password are not the same thing, never reveal your API Key.

		APIAnswer response = await PostApply<APIPasswordResponse>("/v1/apipassword", JsonSerializer.Serialize(new Dictionary<string, object>
		{
			{ "ip", remoteip },
			{ "apiKey", yourkey }
		}), gameState.Apply);

		if (response == RequestSuccess) Console.WriteLine("Password acquired."); //Console.WriteLine("Buffer Item Placed on The Map...");
		await Task.Delay(2000);

		return response;
	}

	public static async Task<APIAnswer> BotAction(string entityUniqueID, string action, object actionValue) => await gameState.BotAction(entityUniqueID, action, actionValue);

	public static async Task RunAiLogic()
	{
		gameState.RunAiLogic();

		// 0.2 sec delay..
		await Task.Delay(200);
	}

	public static async Task<APIAnswer> TpScreen(TransitionResponse transition)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state != "TPSCREEN") return StateNotTpScreen;

		return await GetApply<TpScreenResponse>("/v1/tpscreen", async (data) =>
		{
			if (data.tpCards.Count > 0)
			{
				// we got TP CARDS to choose.... time to choose one.. lets pick first -> index 0.
				// https://i.gyazo.com/8e375b3f859fd92626b783c364f39b7b.png
				return await DoPost("/v1/tpscreen", new Dictionary<string, object>
					{
						{"action", 0}
					});
			}
			else if (data.chaosCards.Count > 0)
			{
				// same thing as above but only for chaos cards
				return await DoPost("/v1/tpscreen", new Dictionary<string, object>
					{
						{"action", 0}
					});
			}
			else
				return NoTpCardsNorChaosCards;
		});
	}

	public static async Task<APIAnswer> GetGameState(TransitionResponse transition)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state is not "INGAME" and not "TPSCREEN") return StateNotInGameNorTpScreen;

		return await GetApply<GameStateResponse>("/v1/gameState", gameState.Apply);
	}

	public static async Task<APIAnswer> GetSystemState(TransitionResponse transition) => await GetApply<SystemResponse>("/v1/system", gameState.Apply);

	public static async Task<APIAnswer> ClickLevelUp() => await gameState.LevelUp();

	public static async Task<APIAnswer> UseFramework() => await gameState.LesserHeal();

	public static async Task<APIAnswer> UseWWWBlock() => await gameState.TileAction();

	public static async Task<APIAnswer> BrowseInternet() => await gameState.BrowseInternet();

	public static async Task<APIAnswer> InitialMenuSequence() => await gameState.InitiateMenuSequence();

	public static async Task<APIAnswer> MainMenuTransition(TransitionResponse transition) => await Transition(transition, "MAIN_MENU_INTRO", StateNotMainMenuIntro, "MAIN_MENU", "Enter Main Menu...");

	public static async Task<APIAnswer> ModeSelectionTransition(TransitionResponse transition) => await Transition(transition, "MAIN_MENU", StateNotMainMenu, "MODE_SELECTION", "Enter Mode Selection Menu...");

	public static async Task<APIAnswer> HackerSelectionTransition(TransitionResponse transition) => await Transition(transition, "MODE_SELECTION", StateNotHackerSelectionMenu, "HACKER_SELECTION", "Enter Hacker Selection Menu...");

	public static async Task<APIAnswer> HackerSelectTransition(TransitionResponse transition)
	{
		APIAnswer response = await Transition(transition, "HACKER_SELECTION", StateNotHackerSelected, new Dictionary<string, object>
			{
				{"transition", "HACKER_SELECT"},
				{"transitionValue", 0}
			}, "Select Hacker...");

		if (response == RequestSuccess)
			gameState = new(); // reset internal state
		return response;
	}

	public static async Task<APIAnswer> GameOverTransition(TransitionResponse transition) => await Transition(transition, "GAMEOVER", StateNotGameOverMenu, "GAMEOVER", "Game Over...");

	public static async Task<APIAnswer> GetTiles(TransitionResponse transition)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state is not "INGAME" and not "TPSCREEN") return StateNotInGameNorTpScreen;

		return await GetApply<TilesResponse>("/v1/tiles", gameState.Apply);
	}

	/// <summary>very expensive function -> only returns success once for every grid change. store well.
	/// <br/>you need this if you want to make advanced movement logic for your bots.</summary>
	public static async Task<APIAnswer> GetGrid(TransitionResponse transition)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state is not "INGAME" and not "TPSCREEN") return StateNotInGameNorTpScreen;

		return await GetApply<GridResponse>("/v1/grid", gameState.Apply);
	}

	public static async Task<APIAnswer> GetEntities(TransitionResponse transition)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state is not "INGAME" and not "TPSCREEN") return StateNotInGameNorTpScreen;

		return await GetApply<EntitiesResponse>("/v1/entities", gameState.Apply);
	}

	public static async Task<APIAnswer> GetBufferTiles(TransitionResponse transition)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state is not "INGAME") return StateNotInGame;
		var response = await GetApply<BufferResponse>("/v1/buffer", gameState.Apply);
		if (response != RequestSuccess) return response;

		return await gameState.BufferItem();
	}

	public static async Task<bool?> RayCast(string uniqueID, string targetUniqueID)
	{
		var response = await SendPostRequestAsync("/v1/raycast", JsonSerializer.Serialize(new Dictionary<string, object> { { "uniqueID", uniqueID }, { "targetUniqueID", targetUniqueID } }));
		if (!response.IsSuccessStatusCode) return default;
		if (response.ResponseString == null) return default;

		return JsonSerializer.Deserialize<bool>(response.ResponseString);
	}

	public static async Task<APIAnswer> DoPost<T>(string endpoint, Dictionary<string, T> jsonData) => await DoMeta(() => SendPostRequestAsync(endpoint, JsonSerializer.Serialize(jsonData)));

	public static async Task<APIAnswer> DoGet(string endpoint) => await DoMeta(() => SendGetRequestAsync(endpoint));

	private static async Task<APIAnswer> DoMeta(Func<Task<AsyncResponse>> func) => (await func()).IsSuccessStatusCode ? RequestSuccess : RequestFailed;

	public static async Task<AsyncResponse> SendPostRequestAsync(string endpoint, string jsonData) => await DoRequestAsync(_httpClient.SendAsync, new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = new StringContent(jsonData, Encoding.UTF8, "application/json") });

	public static async Task<AsyncResponse> SendGetRequestAsync(string endpoint) => await DoRequestAsync(_httpClient.GetAsync, endpoint);

	private static async Task<AsyncResponse> DoRequestAsync<T>(Func<T, Task<HttpResponseMessage>> httpFunc, T request)
	{
		HttpResponseMessage response;
		try
		{
			response = await httpFunc(request).ConfigureAwait(false);
		}
		catch (HttpRequestException ex)
		{
			if (ex.HResult == -2147467259)
				return new AsyncResponse { IsSuccessStatusCode = true, ResponseString = JsonSerializer.Serialize(new TransitionResponse() { state = "STARTING" }) }; // Mock.
			return new AsyncResponse { IsSuccessStatusCode = false };
		}

		return new AsyncResponse { ResponseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false), IsSuccessStatusCode = response.IsSuccessStatusCode };
	}

	private static async Task<APIAnswer> PostApply<T>(string endpoint, string jsonData, Action<T> apply) => await SendApply(() => SendPostRequestAsync(endpoint, jsonData), apply);

	private static async Task<APIAnswer> GetApply<T>(string endpoint, Action<T> apply) => await SendApply(() => SendGetRequestAsync(endpoint), apply);

	private static async Task<APIAnswer> SendApply<T>(Func<Task<AsyncResponse>> send, Action<T> apply)
	{
		AsyncResponse response = await send();
		if (!response.IsSuccessStatusCode) return RequestFailed;
		if (response.ResponseString == null) return ResponseIsNull;

		T? data = JsonSerializer.Deserialize<T>(response.ResponseString);
		if (data == null) return ContentIsEmpty;
		apply(data);

		return RequestSuccess;
	}

	private static async Task<APIAnswer> GetApply<T>(string endpoint, Func<T, Task<APIAnswer>> apply) => await SendApply(() => SendGetRequestAsync(endpoint), apply);

	private static async Task<APIAnswer> SendApply<T>(Func<Task<AsyncResponse>> send, Func<T, Task<APIAnswer>> apply)
	{
		AsyncResponse response = await send();
		if (!response.IsSuccessStatusCode) return RequestFailed;
		if (response.ResponseString == null) return ResponseIsNull;

		T? data = JsonSerializer.Deserialize<T>(response.ResponseString);
		if (data == null) return ContentIsEmpty;
		return await apply(data);
	}

	private static async Task<APIAnswer> Transition<T>(TransitionResponse transition, string expectedState, APIAnswer stateIsIncorrect, T nextMenu, string greetingsLine) => await Transition(transition, expectedState, stateIsIncorrect, new Dictionary<string, T> { { "transition", nextMenu } }, greetingsLine);

	private static async Task<APIAnswer> Transition<T>(TransitionResponse transition, string expectedState, APIAnswer stateIsIncorrect, Dictionary<string, T> jsonData, string greetingsLine)
	{
		if (transition.state == null) return StateIsNull;
		if (transition.state != expectedState) return stateIsIncorrect;

		var response = await DoPost("/v1/playerstate", jsonData);

		if (response == RequestSuccess)
		{
			Console.WriteLine(greetingsLine);
			transition.state = ""; // consume state.
		}
		return response;
	}
}