using UnityEngine;

public interface IBoundInScene
{
	bool Intersection(ref Bounds bound, bool tstY);

	bool Contains(Vector3 point, bool tstY);
}
