using System;

namespace SkillSystem;

public class AttFilter_Event : AttFilter
{
	private SkEntity mSke;

	private int mTargetAtt;

	private Action mCheckFunc;

	public AttFilter_Event(SkEntity ske, int targetAtt, Action checkFunc)
	{
		mSke = ske;
		mCheckFunc = checkFunc;
		mSke._attribs._OnAlterNumAttribs += OnAttChange;
	}

	public void OnAttChange(int att, float oldValue, float newValue)
	{
		if (att == mTargetAtt && mCheckFunc != null)
		{
			mCheckFunc();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		mSke._attribs._OnAlterNumAttribs -= OnAttChange;
	}
}
