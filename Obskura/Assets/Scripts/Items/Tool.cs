using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : ItemMessage {

	public LeftHandTool TorchType;
	public RightHandTool GunType;

	protected override void Action (Player player)
	{
		base.Action (player);
		player.CollectTool (torch: TorchType, gun: GunType);
	}
		
}
