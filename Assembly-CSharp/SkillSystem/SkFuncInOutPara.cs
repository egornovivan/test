using System.Collections.Generic;

namespace SkillSystem;

public class SkFuncInOutPara
{
	public SkInst _inst;

	public object _para;

	public bool _ret;

	public SkFuncInOutPara(SkInst inst, object para, bool ret = false)
	{
		_inst = inst;
		_para = para;
		_ret = ret;
	}

	public SkCondColDesc GetColDesc()
	{
		string colDesc = _para as string;
		return new SkCondColDesc(colDesc);
	}

	public List<SkEntity> GetTargets()
	{
		return _para as List<SkEntity>;
	}
}
