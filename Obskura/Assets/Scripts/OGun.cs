//Written by Manuel
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Plasma Gun the Player uses to fire Plasma Bullets (OBullet)
/// </summary>
public class OGun : MonoBehaviour {
	
	public AudioSource FiringSound; //Sound of firing

	public OBullet PlasmaBullet; //Bullet to be fired

	/// <summary>
	/// Fire a plasma bullet from one point to another.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="target">Target.</param>
	public void Fire(Vector2 from, Vector2 target){

		//Create a copy of the template bullet and fire itt
		GameObject bullet = GameObject.Instantiate (PlasmaBullet.gameObject);
		bullet.GetComponent<OBullet> ().Fire (from, target);

		if (FiringSound != null)
			FiringSound.PlayOneShot (FiringSound.clip);

	}

}
