namespace IAH_SinglePlayerAutomation.Class
{
	[Flags]
	public enum APIAnswer
	{
		PasswordIsNull = 0,
		StateIsNull = 1 << 1,
		StateNotInGame = 1 << 2,
		StateNotTpScreen = 1 << 3,
		StateNotInGameNorTpScreen = StateNotInGame | StateNotTpScreen,
		ResponseIsNull = 1 << 4,
		RequestFailed = 1 << 5,
		RequestSuccess = 1 << 6,
		ContentIsEmpty = 1 << 7,
		NoTpCards = 1 << 8,
		NoChaosCards = 1 << 9,
		NoTpCardsNorChaosCards = NoTpCards | NoChaosCards,
		NoWebBufferTiles = 1 << 10,
		NoModem = 1 << 11,
		NoWebBufferTilesOrModem = NoWebBufferTiles | NoModem,
		NoEntities = 1 << 12,
		NoTiles = 1 << 13,
		NoTile = 1 << 14,
		TileIsBusy = 1 << 15,
		TileHasOccupied = 1 << 16,
		NoInitLeft = 1 << 17,
		StateNotMainMenuIntro = 1 << 18,
		StateNotMainMenu = 1 << 19,
		StateNotHackerSelectionMenu = 1 << 20,
		StateNotHackerSelected = 1 << 21,
		StateNotGameOverMenu = 1 << 22,
	}
}