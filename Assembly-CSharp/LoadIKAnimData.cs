using UnityEngine;

public class LoadIKAnimData : MonoBehaviour
{
	public int m_monsterID = 1009;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private Transform RecursionFindTransform(Transform _trans, string _strName)
	{
		Transform transform = _trans.Find(_strName);
		if (transform != null)
		{
			return transform;
		}
		for (int i = 0; i != _trans.childCount; i++)
		{
			transform = RecursionFindTransform(_trans.GetChild(i), _strName);
			if (transform != null)
			{
				break;
			}
		}
		return transform;
	}

	public bool OnLoadIKAnimData()
	{
		GetComponent<Animation>().clip = GetComponent<Animation>().GetClip("idle");
		LegController component = GetComponent<LegController>();
		component.groundPlaneHeight = 0f;
		component.groundedPose = GetComponent<Animation>().GetClip("idle");
		component.rootBone = base.transform.Find("Bone02");
		component.legs = new LegInfo[4];
		LegInfo legInfo = new LegInfo();
		legInfo.hip = RecursionFindTransform(base.transform, "Bone83");
		legInfo.ankle = RecursionFindTransform(base.transform, "Bone90");
		legInfo.toe = RecursionFindTransform(base.transform, "Bone90");
		legInfo.footLength = 0.5f;
		legInfo.footWidth = 0.5f;
		legInfo.footOffset = new Vector2(0f, 0f);
		component.legs[0] = legInfo;
		legInfo = new LegInfo();
		legInfo.hip = RecursionFindTransform(base.transform, "Bone43");
		legInfo.ankle = RecursionFindTransform(base.transform, "Bone50");
		legInfo.toe = RecursionFindTransform(base.transform, "Bone50");
		legInfo.footLength = 0.5f;
		legInfo.footWidth = 0.5f;
		legInfo.footOffset = new Vector2(0f, 0f);
		component.legs[1] = legInfo;
		legInfo = new LegInfo();
		legInfo.hip = RecursionFindTransform(base.transform, "Bone26");
		legInfo.ankle = RecursionFindTransform(base.transform, "Bone33");
		legInfo.toe = RecursionFindTransform(base.transform, "Bone33");
		legInfo.footLength = 0.5f;
		legInfo.footWidth = 0.5f;
		legInfo.footOffset = new Vector2(0f, 0f);
		component.legs[2] = legInfo;
		legInfo = new LegInfo();
		legInfo.hip = RecursionFindTransform(base.transform, "Bone34");
		legInfo.ankle = RecursionFindTransform(base.transform, "Bone41");
		legInfo.toe = RecursionFindTransform(base.transform, "Bone41");
		legInfo.footLength = 0.5f;
		legInfo.footWidth = 0.5f;
		legInfo.footOffset = new Vector2(0f, 0f);
		component.legs[3] = legInfo;
		return true;
	}
}
