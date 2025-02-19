using UnityEngine;

namespace WhiteCat;

public interface IProjectileData
{
	Transform emissionTransform { get; }

	Vector3 targetPosition { get; }
}
