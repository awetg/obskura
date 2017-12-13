using System;

public static class GameData
{
	const float difficultyFactor = 0.5f;
	private static string playerName = "";
	private static GameEntry selectedNewGamePlus;
	private static int difficulty = 2;

	public static void SetPlayer(string name){
		FileDB.AddPlayerIfNotExist (name);
	}

	public static void SetNewGamePlus(string name, int level){
		var gds = FileDB.ReadGameData (playerName);

		foreach (GameEntry gd in gds) {
			if (gd.GameName == name && gd.Level == level)
				selectedNewGamePlus = gd;
		}
	}

	public static void SetDifficulty(int diff){
		difficulty = diff;
	}

	public static string GetPlayerName(){
		return playerName;
	}

	public static GameEntry GetSelectedNewGamePlus(){
		return selectedNewGamePlus;
	}

	public static int GetDifficulty(){
		return difficulty;
	}

	public static float GetDifficultyMultiplier(){
		return difficulty * difficultyFactor;
	}
}

