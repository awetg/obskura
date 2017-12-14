using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SupplyType {MedPack, Ammo};

public class Supply : Item {

	public SupplyType Type;
	public float Amount;

	protected override void Action ( Player player)
	{
		if (Type == SupplyType.MedPack) {
			player.HP += Amount;

			if (player.HP > player.MaxHP)
				player.HP = player.MaxHP;

		} else if (Type == SupplyType.Ammo) {
			player.Ammo += (int)Amount;
		}

		player.MakeCollectSound ();
	}
}
