using UnityEngine;
using WhiteCat;

public class CreateObjectsAlongPath : MonoBehaviour
{
	public GameObject original;

	public int amount = 20;

	private void Awake()
	{
		Path component = GetComponent<Path>();
		int splineIndex = 0;
		float splineTime = 0f;
		for (int i = 0; i < amount; i++)
		{
			float pathLength = (float)i * component.pathTotalLength / (float)amount;
			component.GetPathPositionAtPathLength(pathLength, ref splineIndex, ref splineTime);
			Vector3 splinePoint = component.GetSplinePoint(splineIndex, splineTime);
			Quaternion splineRotation = component.GetSplineRotation(splineIndex, splineTime);
			Object.Instantiate(original, splinePoint, splineRotation);
		}
	}
}
