using System;

namespace SkillSystem;

public class SkAttribsOpGET : ISkAttribsOp
{
	internal int[] _ids;

	public SkAttribsOpGET(string strArgs)
	{
		string[] array = strArgs.Split(',');
		int num = array.Length;
		_ids = new int[num];
		for (int i = 0; i < num; i++)
		{
			_ids[i] = Convert.ToInt32(array[i]);
		}
	}

	public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
	{
		dstAttribs.pack += _ids;
	}
}
