using System;

namespace SkillSystem;

public class SkAttribsOpMAD : ISkAttribsOp
{
	internal int _idx;

	internal float _mul;

	internal float _add;

	public SkAttribsOpMAD(string strArgs)
	{
		string[] array = strArgs.Split(',');
		_idx = Convert.ToInt32(array[0]);
		_mul = Convert.ToSingle(array[1]);
		_add = Convert.ToSingle(array[2]);
	}

	public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
	{
		float num = dstAttribs.raws[_idx];
		num = num * _mul + _add;
		dstAttribs.raws[_idx] = num;
	}
}
