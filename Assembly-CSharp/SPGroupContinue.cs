using UnityEngine;

public class SPGroupContinue : SPGroup
{
	public bool overlay;

	public int count;

	public float interval;

	public int[] pathIDs;

	private bool CheckOnTerrain()
	{
		Vector3 position = base.transform.position;
		if (AiUtil.CheckPositionOnGround(ref position, 10f, AiUtil.groundedLayer))
		{
			base.transform.position = position;
			return true;
		}
		return false;
	}
}
