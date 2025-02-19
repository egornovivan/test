using UnityEngine;

public interface ISceneObject
{
	int Id { get; }

	int WorldId { get; }

	Vector3 Pos { get; }
}
