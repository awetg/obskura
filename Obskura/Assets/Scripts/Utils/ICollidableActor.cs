using System;
using UnityEngine;

public interface ICollidableActor2D
{
	float GetSize();

	Vector2 GetPosition();

	GameObject GetGameObject();


}