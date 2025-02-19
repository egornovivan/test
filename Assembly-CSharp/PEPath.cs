using UnityEngine;

public class PEPath : MonoBehaviour
{
	public EPathMode wrapMode;

	public bool isTerrain;

	private int m_layer;

	private void Awake()
	{
		m_layer = 6144;
	}

	private void Update()
	{
		if (!isTerrain)
		{
			return;
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (Physics.Raycast(child.position + Vector3.up * 256f, -Vector3.up, out var hitInfo, 512f, m_layer))
			{
				child.position = hitInfo.point;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		foreach (Transform item in base.transform)
		{
			Gizmos.DrawSphere(item.position, 0.5f);
		}
		int childCount = base.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			int num = i + 1;
			if (num < childCount)
			{
				Gizmos.DrawLine(base.transform.GetChild(i).position, base.transform.GetChild(num).position);
			}
			else if (wrapMode == EPathMode.Loop)
			{
				Gizmos.DrawLine(base.transform.GetChild(i).position, base.transform.GetChild(0).position);
			}
		}
	}
}
