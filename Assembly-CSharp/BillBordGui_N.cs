using System.Collections.Generic;
using UnityEngine;

public class BillBordGui_N : MonoBehaviour
{
	private static BillBordGui_N mInstance;

	public Transform mBillbordWnd;

	public NpcHeadInfo_N mPerfab;

	public HeadMask_N mHeadPerfab;

	private Dictionary<Transform, NpcHeadInfo_N> mNpcHeadInfoList = new Dictionary<Transform, NpcHeadInfo_N>();

	private Dictionary<Transform, HeadMask_N> mHeadMaskList = new Dictionary<Transform, HeadMask_N>();

	public float hideDistance = 15f;

	private float distanceMagnitude;

	public static BillBordGui_N Instance => mInstance;

	private void Awake()
	{
		distanceMagnitude = hideDistance * hideDistance;
		mInstance = this;
	}

	public void AddNpcHeadInfo(Transform targetTran, string npcName, string iconName = null)
	{
		if (!mNpcHeadInfoList.ContainsKey(targetTran))
		{
			if (iconName == null)
			{
				iconName = "Null";
			}
			NpcHeadInfo_N npcHeadInfo_N = Object.Instantiate(mPerfab);
			npcHeadInfo_N.transform.parent = mBillbordWnd;
			npcHeadInfo_N.transform.localPosition = 100f * Vector3.down;
			npcHeadInfo_N.transform.localScale = Vector3.one;
			npcHeadInfo_N.SetInfo(npcName, iconName);
			mNpcHeadInfoList.Add(targetTran, npcHeadInfo_N);
		}
	}

	public void RemoveNpcHeadInfo(Transform targetTran)
	{
		if (mNpcHeadInfoList.ContainsKey(targetTran) && null != targetTran && null != mNpcHeadInfoList[targetTran])
		{
			Object.Destroy(mNpcHeadInfoList[targetTran].gameObject);
			mNpcHeadInfoList.Remove(targetTran);
		}
	}

	public HeadMask_N AddHeadMask(Transform targetTran)
	{
		HeadMask_N headMask_N = Object.Instantiate(mHeadPerfab);
		headMask_N.transform.parent = mBillbordWnd;
		headMask_N.transform.localPosition = 100f * Vector3.down;
		headMask_N.transform.localScale = Vector3.one;
		mHeadMaskList.Add(targetTran, headMask_N);
		return mHeadMaskList[targetTran];
	}

	public void RemoveHeadMask(Transform targetTran)
	{
		if (mHeadMaskList.ContainsKey(targetTran) && null != targetTran && null != mHeadMaskList[targetTran])
		{
			Object.Destroy(mHeadMaskList[targetTran].gameObject);
			mHeadMaskList.Remove(targetTran);
		}
	}

	private void OnPreRender()
	{
		foreach (Transform key in mNpcHeadInfoList.Keys)
		{
			if (null == key || null == mNpcHeadInfoList[key])
			{
				continue;
			}
			Vector3 zero = Vector3.zero;
			if ((zero - key.position).sqrMagnitude > distanceMagnitude)
			{
				mNpcHeadInfoList[key].gameObject.SetActive(value: false);
				continue;
			}
			mNpcHeadInfoList[key].gameObject.SetActive(value: true);
			Vector3 localPosition = Camera.main.WorldToScreenPoint(key.position);
			if (localPosition.z < 0.5f)
			{
				mNpcHeadInfoList[key].gameObject.SetActive(value: false);
				continue;
			}
			localPosition.z = 0f;
			mNpcHeadInfoList[key].transform.localPosition = localPosition;
		}
		foreach (Transform key2 in mHeadMaskList.Keys)
		{
			if (!(null == key2) && !(null == mHeadMaskList[key2]))
			{
				Vector3 localPosition2 = Camera.main.WorldToScreenPoint(key2.position);
				bool flag = localPosition2.z > 0.5f && localPosition2.z < 50f;
				if (mHeadMaskList[key2].gameObject.activeSelf != flag)
				{
					mHeadMaskList[key2].gameObject.SetActive(flag);
				}
				localPosition2.z = 0f;
				mHeadMaskList[key2].transform.localPosition = localPosition2;
			}
		}
	}
}
