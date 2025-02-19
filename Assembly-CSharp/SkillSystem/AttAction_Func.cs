using UnityEngine;

namespace SkillSystem;

public class AttAction_Func : AttAction
{
	private string mFuncName;

	public AttAction_Func(SkEntity skEntity, string[] para)
		: base(skEntity)
	{
		mFuncName = para[1];
	}

	public override void Do()
	{
		mSkEntity.SendMessage(mFuncName, SendMessageOptions.DontRequireReceiver);
	}
}
