using Pathea;
using Railway;
using UnityEngine;

public class RailwayStation : MonoBehaviour
{
	public Transform mLinkPoint;

	public Transform mJointPoint;

	public CrossLine mLine;

	public int pointId = -1;

	[SerializeField]
	private RailwayStation m_LinkStation;

	public Point Point => PeSingleton<Manager>.Instance.GetPoint(pointId);

	public void SetPos(Vector3 pos)
	{
		base.transform.position = pos;
		UpdateLink();
	}

	public void SetRot(Vector3 rot)
	{
		base.transform.rotation = Quaternion.Euler(rot);
		UpdateLink();
	}

	public void LinkTo(RailwayStation targetStation)
	{
		m_LinkStation = targetStation;
		UpdateLink();
	}

	public void UpdateLink()
	{
		if (null == m_LinkStation)
		{
			BreakLink();
		}
		else
		{
			EstablishLink(m_LinkStation.mLinkPoint.position, mLinkPoint.position);
		}
	}

	private void EstablishLink(Vector3 pos1, Vector3 pos2)
	{
		mLine.m_Begin = pos1;
		mLine.m_End = pos2;
		if (!mLine.gameObject.activeSelf)
		{
			mLine.gameObject.SetActive(value: true);
		}
	}

	private void BreakLink()
	{
		if (mLine.gameObject.activeSelf)
		{
			mLine.gameObject.SetActive(value: false);
		}
	}
}
