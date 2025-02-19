using Pathea;
using UnityEngine;

public class FBSafePlane : MonoBehaviour
{
	private static FBSafePlane mInstance;

	private static readonly float BorderLength = 100f;

	private BoxCollider m_Colldier;

	private Vector3 m_ResetPos;

	public static FBSafePlane instance
	{
		get
		{
			if (null == mInstance)
			{
				GameObject gameObject = new GameObject("FBSafePlane");
				mInstance = gameObject.AddComponent<FBSafePlane>();
			}
			return mInstance;
		}
	}

	private void Awake()
	{
		base.gameObject.layer = 12;
		m_Colldier = base.gameObject.AddComponent<BoxCollider>();
		m_Colldier.isTrigger = true;
		m_Colldier.center = Vector3.zero;
	}

	public void ResetCol(Vector3 min, Vector3 max, Vector3 resetPos)
	{
		Vector3 position = 0.5f * (min + max);
		position.y = min.y - 10f;
		base.transform.position = position;
		Vector3 size = max - min + 2f * BorderLength * Vector3.one;
		size.y = 5f;
		m_Colldier.size = size;
		m_ResetPos = resetPos;
	}

	public void DeleteCol()
	{
		if (null != m_Colldier)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PeEntity componentInParent = other.transform.GetComponentInParent<PeEntity>();
		if (null != componentInParent)
		{
			componentInParent.position = m_ResetPos;
		}
	}
}
