using Pathea;
using UnityEngine;

public class UIGraphNode
{
	public int mLever_v;

	public UIGraphNode mPartent;

	public int mChildCount;

	public int needCount;

	public int getCount;

	public int bagCount;

	public Replicator.Formula ms;

	public GameObject mObject;

	public int mIndexForPartent;

	public int mIndex;

	public UIGraphItemCtrl mCtrl;

	public UITreeGrid mTreeGrid;

	public UIComWndToolTipCtrl mTipCtrl;

	public UIGraphNode(int lever_v, UIGraphNode partent)
	{
		mLever_v = lever_v;
		mPartent = partent;
		mIndexForPartent = -1;
		mObject = null;
		mCtrl = null;
		ms = null;
		needCount = 0;
		getCount = 0;
		bagCount = 0;
	}

	public int GetItemID()
	{
		if (mTipCtrl != null)
		{
			return mTipCtrl.GetItemID();
		}
		return 0;
	}
}
