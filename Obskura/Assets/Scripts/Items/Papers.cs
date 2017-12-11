using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Papers : Item {

//	public string Title;
//	public string Text;

	protected override void Action ( Player player)
	{
		base.Action (player);
		//FIXME: Show title and text
//		Debug.Log(Title + ":");
//		Debug.Log (Text);
	}
		
}
