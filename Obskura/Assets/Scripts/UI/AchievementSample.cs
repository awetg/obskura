using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementSample : MonoBehaviour {

	public Button button;
	public Text AchievementName;
	public Image IconImage;

	private Achievement a;
	private AchievementLister l;

	// Use this for initialization
	void Start () {
		
	}

	public void SetValues( Achievement currentA, AchievementLister currentSample){

		a = currentA;
		AchievementName.text = a.AchievementName;
		IconImage.sprite = a.icon;
	}

}
