using System;
using Pathea;
using UnityEngine;

public class L1Trigger : MonoBehaviour
{
	private int triggerNum = -1;

	private void Start()
	{
		triggerNum = Convert.ToInt32(base.gameObject.name.Substring(4, 1));
	}

	private void OnTriggerEnter(Collider target)
	{
		if (!(null == target.GetComponentInParent<MainPlayerCmpt>()))
		{
			if (L1Manager.instance.followers.Count == 0)
			{
				L1Manager.instance.FindFollowers(L1Manager.instance.followers);
			}
			L1Manager.instance.isTrigger[triggerNum] = true;
			FollowerDelivery(triggerNum);
		}
	}

	private void FollowerDelivery(int meNum)
	{
		switch (meNum)
		{
		case 2:
			if (L1Manager.instance.isTrigger[1])
			{
				L1Manager.instance.isTrigger[1] = false;
				L1Manager.instance.SetPosition(1);
			}
			break;
		case 4:
			if (L1Manager.instance.isTrigger[3])
			{
				L1Manager.instance.isTrigger[3] = false;
				L1Manager.instance.SetPosition(2);
			}
			break;
		case 6:
			if (L1Manager.instance.isTrigger[5])
			{
				L1Manager.instance.isTrigger[5] = false;
				L1Manager.instance.SetPosition(3);
			}
			break;
		case 1:
			if (L1Manager.instance.isTrigger[2])
			{
				L1Manager.instance.isTrigger[2] = false;
				L1Manager.instance.SetPosition(4);
			}
			break;
		case 3:
			if (L1Manager.instance.isTrigger[4])
			{
				L1Manager.instance.isTrigger[4] = false;
				L1Manager.instance.SetPosition(5);
			}
			break;
		case 5:
			if (L1Manager.instance.isTrigger[6])
			{
				L1Manager.instance.isTrigger[6] = false;
				L1Manager.instance.SetPosition(6);
			}
			break;
		}
	}
}
