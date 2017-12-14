using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Score that the Player can pick up;
/// </summary>
public class Money : Item {

	public float Amount = 10.0f;

	protected override void Action (Player player)
	{
		player.Score += Amount;
		player.MakeCollectSound ();
	}
		
}
