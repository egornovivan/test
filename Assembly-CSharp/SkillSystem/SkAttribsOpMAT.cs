using System;

namespace SkillSystem;

public class SkAttribsOpMAT : ISkAttribsOp
{
	internal int _idx;

	internal int _idxParaMul = -1;

	internal int _idxParaPreAdd = -1;

	internal int _idxParaPostAdd = -1;

	internal int _idxMul = -1;

	internal int _idxPreAdd = -1;

	internal int _idxPostAdd = -1;

	internal float _mul;

	internal float _preAdd;

	internal float _postAdd;

	public SkAttribsOpMAT(string strArgs)
	{
		string[] array = strArgs.Split(',');
		_idx = Convert.ToInt32(array[0]);
		try
		{
			_mul = Convert.ToSingle(array[1]);
		}
		catch
		{
			string[] array2 = array[1].Split(':');
			_idxParaMul = Convert.ToInt32(array2[0]);
			_idxMul = Convert.ToInt32(array2[1]);
		}
		try
		{
			_preAdd = Convert.ToSingle(array[2]);
		}
		catch
		{
			string[] array3 = array[1].Split(':');
			_idxParaPreAdd = Convert.ToInt32(array3[0]);
			_idxPreAdd = Convert.ToInt32(array3[1]);
		}
		try
		{
			_postAdd = Convert.ToSingle(array[3]);
		}
		catch
		{
			string[] array4 = array[1].Split(':');
			_idxParaPostAdd = Convert.ToInt32(array4[0]);
			_idxPostAdd = Convert.ToInt32(array4[1]);
		}
	}

	public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
	{
		if (_idxParaMul >= 0 || _idxParaPreAdd >= 0 || _idxParaPostAdd >= 0)
		{
			if (_idxParaMul == 0)
			{
				_mul = paraCaster.sums[_idxMul];
			}
			if (_idxParaMul == 1)
			{
				_mul = paraTarget.sums[_idxMul];
			}
			_idxParaMul = -1;
			if (_idxParaPreAdd == 0)
			{
				_preAdd = paraCaster.sums[_idxPreAdd];
			}
			if (_idxParaPreAdd == 1)
			{
				_preAdd = paraTarget.sums[_idxPreAdd];
			}
			_idxParaPreAdd = -1;
			if (_idxParaPostAdd == 0)
			{
				_postAdd = paraCaster.sums[_idxPostAdd];
			}
			if (_idxParaPostAdd == 1)
			{
				_postAdd = paraTarget.sums[_idxPostAdd];
			}
			_idxParaPostAdd = -1;
		}
	}
}
