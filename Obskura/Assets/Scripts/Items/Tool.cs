using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item {

	public LeftHandTool TorchType;
	public RightHandTool GunType;

	protected override void Action (Player player)
	{
		player.CollectTool (torch: TorchType, gun: GunType);
	}
		
}
