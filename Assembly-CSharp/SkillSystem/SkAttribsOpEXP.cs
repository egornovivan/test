using System;

namespace SkillSystem;

public class SkAttribsOpEXP : ICompilableExp, ISkAttribsOp
{
	public bool _bTmpOp;

	public int _idx;

	public Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> _deleOpExp;

	public SkAttribsOpEXP(string strArgs)
	{
		string[] array = strArgs.Split(new char[1] { ',' }, 2);
		_idx = Convert.ToInt32(array[0]);
		_bTmpOp = array[1].Contains("buff");
		SkInst.s_ExpCompiler.AddExpString(this, array[1]);
	}

	public void OnCompiled(Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> action)
	{
		_deleOpExp = action;
	}

	public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
	{
		_deleOpExp(paraCaster, paraTarget, para);
	}
}
