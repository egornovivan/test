using UnityEngine;

public class RCGen : MonoBehaviour
{
	public int type = 1;

	private void Start()
	{
	}

	private void Update()
	{
		RoadSystem.DataSource.AlterRoadCell(base.transform.position, new RoadCell(type));
	}
}
