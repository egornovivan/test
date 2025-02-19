using UnityEngine;

namespace Pathea;

public class FlagViewCmpt : ViewCmpt
{
	private DragItemLogicFlag mLogic;

	public DragItemLogicFlag FlagLogic
	{
		get
		{
			if (null == mLogic)
			{
				mLogic = GetComponent<DragItemLogicFlag>();
			}
			return mLogic;
		}
	}

	public override bool hasView => !(null == mLogic) && null != mLogic.FlagEntity;

	public override Transform centerTransform
	{
		get
		{
			if (!hasView)
			{
				return null;
			}
			return FlagLogic.transform;
		}
	}
}
