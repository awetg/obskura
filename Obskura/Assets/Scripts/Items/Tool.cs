using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tool the player can collect.
/// </summary>
public class Tool : ItemMessage {

	//Can contain a Torch, a Gun or both
	public LeftHandTool TorchType;
	public RightHandTool GunType;

	protected override void Action (Player player)
	{
		base.Action (player);
		player.CollectTool (torch: TorchType, gun: GunType);
	}
		
}
