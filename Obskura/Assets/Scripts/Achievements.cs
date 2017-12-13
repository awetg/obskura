using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct GameAchievement {
	public string name;
	public string description;
	public int points;
	public bool unlocked;

	public GameAchievement(string title, string desc, int ps, bool isUnlocked){
		name = title;
		description = desc;
		points = ps;
		unlocked = isUnlocked;
	}

	public void Unlock(){
		unlocked = true;
	}

}

public class Achievements : MonoBehaviour {

	Dictionary<string, GameAchievement> achs= new Dictionary<string, GameAchievement>();

	private int kills = 0;
	private int coins = 0;
	private bool hasShot = false;

	// Use this for initialization
	void Start () {
		achs.Add ("Pacifist", new GameAchievement ("Pacifist", "Finish the game without shooting", 100, false));
		achs.Add ("Greedy", new GameAchievement ("Greedy", "Collect more than 5 coins", 10, false));
		achs.Add ("Terminator", new GameAchievement ("Terminator", "Kill more than 20 enemies", 10, false));
	}

	public void Unlock (string name) {
		if (achs.ContainsKey (name))
			achs [name].Unlock ();
	}

	public List<GameAchievement> GetUnlockedAchievements(){
		return achs.Values.Where (v => v.unlocked).ToList();
	}

	public void CheckAndUnlock(){
		if (kills >= 20)
			Unlock("Terminator");
		
		if (!hasShot)
			Unlock ("Pacifist");

		if (coins >= 5)
			Unlock ("Greedy");
	}

	public void GunFired(){
		hasShot = true;
	}

	public void EnemyDied(){
		kills += 1;
	}

	public void CoinCollected(){
		coins += 1;
	}
}
