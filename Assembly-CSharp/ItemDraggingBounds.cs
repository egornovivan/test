using UnityEngine;

public class ItemDraggingBounds : MonoBehaviour
{
	[SerializeField]
	private Bounds m_Bounds;

	public bool showBounds;

	public bool activeState;

	private ViewBounds mViewBounds;

	public Bounds selfBounds => m_Bounds;

	public Bounds worldBounds
	{
		get
		{
			Vector3 size = base.transform.rotation * m_Bounds.size;
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			size.z = Mathf.Abs(size.z);
			return new Bounds(base.transform.position + base.transform.rotation * m_Bounds.center, size);
		}
	}

	public void ResetBounds(Vector3 center, Vector3 size)
	{
		m_Bounds.center = center;
		m_Bounds.size = size;
	}

	private void Update()
	{
		if (showBounds)
		{
			if (null == mViewBounds)
			{
				mViewBounds = ViewBoundsMgr.Instance.Get();
			}
			Bounds bounds = worldBounds;
			mViewBounds.SetPos(bounds.min);
			mViewBounds.SetSize(bounds.size);
			mViewBounds.SetColor((!activeState) ? Color.red : Color.green);
		}
		else if (null != mViewBounds)
		{
			ViewBoundsMgr.Instance.Recycle(mViewBounds);
			mViewBounds = null;
		}
	}

	private void OnDestroy()
	{
		if (null != mViewBounds)
		{
			ViewBoundsMgr.Instance.Recycle(mViewBounds);
		}
	}
}
