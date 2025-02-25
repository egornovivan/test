using System;
using UnityEngine;

[AddComponentMenu("Pathfinding/Navmesh/RecastTileUpdate")]
public class RecastTileUpdate : MonoBehaviour
{
	public static event Action<Bounds> OnNeedUpdates;

	private void Start()
	{
		ScheduleUpdate();
	}

	private void OnDestroy()
	{
		ScheduleUpdate();
	}

	public void ScheduleUpdate()
	{
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			if (RecastTileUpdate.OnNeedUpdates != null)
			{
				RecastTileUpdate.OnNeedUpdates(component.bounds);
			}
		}
		else if (RecastTileUpdate.OnNeedUpdates != null)
		{
			RecastTileUpdate.OnNeedUpdates(new Bounds(base.transform.position, Vector3.zero));
		}
	}
}
