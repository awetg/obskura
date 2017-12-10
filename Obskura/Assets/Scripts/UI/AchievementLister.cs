using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Achievement {
	public string AchievementName;
	public Sprite icon;
}

public class AchievementLister : MonoBehaviour {

	public List<Achievement> achievementList;
	public GameObject achievementPrefab;

	// Use this for initialization
	void Start () {

		AddAchievement ();
		
	}
	
//	// Update is called once per frame
//	void Update () {
//		
//	}

	private void AddAchievement(){
		for (int i = 0; i < achievementList.Count; i++) {
			Achievement a = achievementList [i];
			GameObject newAchievement = (GameObject)GameObject.Instantiate (achievementPrefab);
			newAchievement.transform.SetParent (transform,false);
			AchievementSample newSample = newAchievement.GetComponent<AchievementSample> ();
			newSample.SetValues (a, this);
		}
	}
}
