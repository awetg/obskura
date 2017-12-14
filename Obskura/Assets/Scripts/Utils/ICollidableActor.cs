using System;
using UnityEngine;

public enum CollisionType {
	PLASMA
}

/// <summary>
/// If an actor (enemy, door, player) implements this interface, they can be collided by bullets
/// </summary>
public interface ICollidableActor2D
{
	bool IsColliderActive();

	float GetSize();

	Vector2 GetPosition();

	GameObject GetGameObject();

	void CollidedBy(CollisionType type, float damage, Vector2 force, bool setOnFire = false);

}