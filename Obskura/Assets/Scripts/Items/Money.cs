using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : ItemMessage {

	public float Amount = 10.0f;

	protected override void Action (Player player)
	{
		base.Action (player);
		player.Score += Amount;
	}
		
}
