using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public struct GameEntry {
	public string GameName;
	public int Difficulty;
	public int Level;
	public float Score;
	public int Ammo;
}

public struct AchievementEntry {
	public string name;
}

public static class FileDB
{
	public static char Separator = ';';
	const string baseDir = "Saved";
	const string gameDataExt = ".gamedata";
	const string achExt = ".ach";

	private static string getPath(string playerName, string ext){
		string filename = playerName + ext;
		string dir = System.IO.Path.Combine (Application.dataPath, baseDir);
		string path = System.IO.Path.Combine (dir, filename);

		return path;
	}

	public static void AddPlayerIfNotExist(string playerName){
		string gds = getPath (playerName, gameDataExt);
		string achs = getPath (playerName, achExt);

		if (!System.IO.File.Exists (gds))
			System.IO.File.Create (gds).Close ();
		
		if (!System.IO.File.Exists (achs))
			System.IO.File.Create (achs).Close ();
		
	}

	public static GameEntry[] ReadGameData(string playerName){

		string path = getPath (playerName, gameDataExt);

		string[] lines = System.IO.File.ReadAllLines (path);

		var result = new List<GameEntry> ();

		foreach (string line in lines) {
			string[] fields = line.Split (new char[]{ Separator }, StringSplitOptions.RemoveEmptyEntries);

			if (fields.Length != 5)
				continue;

			GameEntry gd = new GameEntry ();

			gd.GameName = fields [0];
			gd.Difficulty = Int32.Parse(fields [1]);
			gd.Level = Int32.Parse(fields [2]);
			gd.Score = float.Parse(fields [3], CultureInfo.InvariantCulture);
			gd.Ammo = Int32.Parse(fields [4]);

			result.Add (gd);
		}

		return result.ToArray ();
	}

	public static Achievement[] ReadAchievements(string playerName){

		string path = getPath (playerName, achExt);

		string[] lines = System.IO.File.ReadAllLines (path);

		var result = new List<Achievement> ();

		foreach (string line in lines) {
			string[] fields = line.Split (new char[]{ Separator }, StringSplitOptions.RemoveEmptyEntries);

			if (fields.Length != 1 || line == "")
				continue;

			Achievement ach = new Achievement ();

			ach.AchievementName = fields [0];

			result.Add (ach);
		}

		return result.ToArray ();

	}

	public static void WriteGameData(string playerName, GameEntry gd) {
		string path = getPath (playerName, gameDataExt);
		System.Text.StringBuilder result = new System.Text.StringBuilder();

		result.Append (Environment.NewLine);
		result.Append (gd.GameName);
		result.Append (Separator);
		result.Append (gd.Difficulty);
		result.Append (Separator);
		result.Append (gd.Level);
		result.Append (Separator);
		result.Append (gd.Score);
		result.Append (Separator);
		result.Append (gd.Ammo);


		System.IO.File.AppendAllText (path, result.ToString());
	}

	public static void WriteAchievement(string playerName, Achievement ach) {
		string path = getPath (playerName, achExt);
		System.Text.StringBuilder result = new System.Text.StringBuilder();

		result.Append (Environment.NewLine);
		result.Append (ach.AchievementName);


		System.IO.File.AppendAllText (path, result.ToString());
	}
}

