using System;

/// <summary>
/// Game data shared by all the scenes.
/// </summary>
public static class GameData
{
	const float difficultyFactor = 0.5f;
	private static int difficulty = 2;


	public static void SetDifficulty(int diff){
		difficulty = diff;
	}

	public static int GetDifficulty(){
		return difficulty;
	}

	public static float GetDifficultyMultiplier(){
		return difficulty * difficultyFactor;
	}
}

