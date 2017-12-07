using System;
using UnityEngine;

public enum CollisionType {
	PLASMA
}

public interface ICollidableActor2D
{
	bool IsColliderActive();

	float GetSize();

	Vector2 GetPosition();

	GameObject GetGameObject();

	void CollidedBy(CollisionType type, float damage, Vector2 force, bool setOnFire = false);

}