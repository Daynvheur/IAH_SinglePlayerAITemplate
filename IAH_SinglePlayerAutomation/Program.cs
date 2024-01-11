using IAH_SinglePlayerAutomation.Class.Response;

namespace IAH_SinglePlayerAutomation;

internal static class Program
{
	static APIAnswer answer;
	/*
	 * When our currentPlayerState is INGAME we create gameState class to hold tiles and entities.
	 * This is rough example designed to be refactored or remade, you should create your own AI, you can do it in C#, JS,etc, use some DX or GDI Library to visualize output.
	 * Visit HTTPS://IAMHACKER.CC for more information or for support, we have discord server there,
	 * also don't forget to wishlist game on Steam: https://store.steampowered.com/app/304770/IAH_INTERNET_WAR/
	 */

	public static async Task Main(string[] args)
	{
		Console.WriteLine("Starting Singleplayer AI Template...");
		await Task.Delay(1000); // wait 1 second before attempting call player state.

		while (true)
		{
			await Task.Delay(1000); // run every 1 sec

			Console.Clear();

			TransitionResponse? requestResponse = await Requests.GetPlayerState();
			Console.WriteLine($"PlayerState: {requestResponse?.state}");
			if (requestResponse == null) continue;

			/* STARTING
				* LOADING
				* MAIN_MENU_INTRO
				* MAIN_MENU
				* MODE_SELECTION
				* HACKER_SELECTION
				* HACKER_SELECT
				* SANDBOX
				* INGAME
				* TPSCREEN
				* GAMEOVER
				*/

			// STEP 1: Do some Transitions in the Main Menu
			await Requests.MainMenuTransition(requestResponse);
			await Requests.ModeSelectionTransition(requestResponse);
			await Requests.HackerSelectionTransition(requestResponse);
			await Requests.HackerSelectTransition(requestResponse);

			// STEP2: These happen INGAME, we get some data.
			answer = await Requests.GetTiles(requestResponse);
			if (answer == APIAnswer.RequestSuccess) Console.WriteLine(Requests.GameState.Tiles);
			answer = await Requests.GetGrid(requestResponse);
			if (answer == APIAnswer.RequestSuccess) Console.WriteLine(Requests.GameState.GridNodes);
			answer = await Requests.GetEntities(requestResponse);
			if (answer == APIAnswer.RequestSuccess) Console.WriteLine(Requests.GameState.Entities);
			await Requests.GetSystemState(requestResponse);
			answer = await Requests.GetBufferTiles(requestResponse);
			if (answer == APIAnswer.RequestSuccess) Console.WriteLine(Requests.GameState.WebBufferTiles);
			answer = await Requests.GetGameState(requestResponse);
			if (answer == APIAnswer.RequestSuccess) Console.WriteLine(Requests.GameState);

			// STEP3: Get API Password that we need in order to perform bot AI actions.
			await Requests.GetApiPassword(requestResponse);

			// STEP4: When we Get Level Ups, select TP Perks or Chaos Cards.
			await Requests.TpScreen(requestResponse);

			// STEP5: Now when we are in the game we need to click game menus, like starting PC, selecting OS and connecting to the Internet.
			await Requests.InitialMenuSequence();

			// STEP6: Browse Internet, Create your Bots, and trigger Level Up Screen (TpScreen).
			if (Requests.GameState.CanPerformAction())
			{
				await Requests.BrowseInternet();
				await Requests.UseWWWBlock();
				await Requests.ClickLevelUp();
			}

			//STEP7: Use Framework to improve your bots.
			await Requests.UseFramework();

			//STEP8: Run AI Logic.
			await Requests.RunAiLogic();

			//STEP9: Check if we lost, and then go back to the main menu.
			await Requests.GameOverTransition(requestResponse);

			/* few other endpoints
				* /v1/cpubusy -> true or false -> tells if cpu is busy, this one is good if you want to know if you are performing some actions right now
				* /v1/time -> returns unity Time.time -> you can use this Entity-attackdelay to check when shoot delay is over, also do note, in competitive Time.time speed will change depending on tactical mode speed.
				*/
		}
	}
}