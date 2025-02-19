using UnityEngine;

public class FollowPoint : MonoBehaviour
{
	private const float rowDistance = 1f;

	private const float lineDistance = 2f;

	public float mHeight;

	public static Transform CreateFollowTrans(Transform target, float height = 0f)
	{
		FollowPoint[] componentsInChildren = target.GetComponentsInChildren<FollowPoint>(includeInactive: true);
		int pointIndex = componentsInChildren.Length;
		GameObject gameObject = new GameObject("FollowPoint", typeof(FollowPoint));
		gameObject.transform.parent = target;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = GetLocalPos(pointIndex) + Vector3.up * height;
		FollowPoint component = gameObject.GetComponent<FollowPoint>();
		component.mHeight = height;
		return gameObject.transform;
	}

	private static Vector3 GetLocalPos(int pointIndex)
	{
		int num = pointIndex / 2;
		int num2 = pointIndex % 2;
		return Vector3.back * 1f * (num + 1) + Vector3.right * 2f * (num2 * 2 - 1);
	}

	private static void ResortPoint(Transform target)
	{
		FollowPoint[] componentsInChildren = target.GetComponentsInChildren<FollowPoint>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].transform.localPosition = GetLocalPos(i) + Vector3.up * componentsInChildren[i].mHeight;
		}
	}

	public static void DestroyFollowTrans(Transform followTrans)
	{
		if (!(null == followTrans) && !(null == followTrans.GetComponent<FollowPoint>()))
		{
			Transform parent = followTrans.parent;
			Object.DestroyImmediate(followTrans.gameObject);
			ResortPoint(parent);
		}
	}
}
