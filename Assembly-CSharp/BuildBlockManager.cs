using UnityEngine;

public class BuildBlockManager : GLBehaviour
{
	public static Vector3 BestMatchPosition(Vector3 pos)
	{
		Vector3 vector = (pos + 0.001f * Vector3.one) / 0.5f;
		vector.x = Mathf.RoundToInt(vector.x);
		vector.y = Mathf.RoundToInt(vector.y);
		vector.z = Mathf.RoundToInt(vector.z);
		return vector * 0.5f + 0.001f * Vector3.one;
	}

	public override void OnGL()
	{
	}
}
