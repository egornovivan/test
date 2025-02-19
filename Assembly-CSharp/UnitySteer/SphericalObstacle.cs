using UnityEngine;

namespace UnitySteer;

public class SphericalObstacle : Obstacle
{
	public float radius;

	public Vector3 center;

	public SphericalObstacle(float r, Vector3 c)
	{
		radius = r;
		center = c;
	}

	public SphericalObstacle()
	{
		radius = 1f;
		center = Vector3.zero;
	}

	public override string ToString()
	{
		return $"[SphericalObstacle {center} {radius}]";
	}

	public static Obstacle GetObstacle(GameObject gameObject)
	{
		int instanceID = gameObject.GetInstanceID();
		float num = 0f;
		if (!Obstacle.ObstacleCache.ContainsKey(instanceID))
		{
			SphericalObstacleData component = gameObject.GetComponent<SphericalObstacleData>();
			if (component != null)
			{
				Obstacle.ObstacleCache[instanceID] = new SphericalObstacle(component.Radius, gameObject.transform.position + component.Center);
			}
			else
			{
				Component[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
				if (componentsInChildren == null)
				{
					Debug.LogError("Obstacle '" + gameObject.name + "' has no colliders");
					return null;
				}
				Component[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					Collider collider = (Collider)array[i];
					if (!collider.isTrigger)
					{
						float num2 = Mathf.Max(Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y), collider.bounds.extents.z);
						float num3 = Vector3.Distance(gameObject.transform.position, collider.bounds.center);
						float num4 = num3 + num2;
						if (num4 > num)
						{
							num = num4;
						}
					}
				}
				Obstacle.ObstacleCache[instanceID] = new SphericalObstacle(num, gameObject.transform.position);
			}
		}
		return Obstacle.ObstacleCache[instanceID] as SphericalObstacle;
	}

	public void annotatePosition()
	{
		annotatePosition(Color.grey);
	}

	public void annotatePosition(Color color)
	{
		Debug.DrawRay(center, Vector3.up * radius, color);
		Debug.DrawRay(center, Vector3.forward * radius, color);
		Debug.DrawRay(center, Vector3.right * radius, color);
	}
}
