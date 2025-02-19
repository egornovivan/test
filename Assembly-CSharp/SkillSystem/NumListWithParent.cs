using System;

namespace SkillSystem;

public class NumListWithParent : NumList
{
	private NumList _parent;

	private bool[] _useParentMasks;

	public NumListWithParent(NumList parent, bool[] useParentMasks, int cnt, Action<NumList, int, float> setter = null)
		: base(cnt, setter)
	{
		_parent = parent;
		_useParentMasks = useParentMasks;
	}

	public override float Get(int idx)
	{
		return (!_useParentMasks[idx]) ? base.Get(idx) : _parent.Get(idx);
	}

	public override void Set(int idx, float val)
	{
		if (_useParentMasks[idx])
		{
			_parent.Set(idx, val);
		}
		else
		{
			base.Set(idx, val);
		}
	}
}
