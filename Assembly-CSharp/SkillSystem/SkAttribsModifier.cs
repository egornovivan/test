using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem;

public class SkAttribsModifier
{
	internal List<ISkAttribsOp> _ops = new List<ISkAttribsOp>();

	internal List<ISkAttribsOp> _tmpOps = new List<ISkAttribsOp>();

	internal List<int> _tmpOpsIdxs = new List<int>();

	private SkAttribsModifier()
	{
	}

	public static SkAttribsModifier Create(string desc)
	{
		if (desc.Equals("0") || desc.Equals(string.Empty))
		{
			return null;
		}
		SkAttribsModifier skAttribsModifier = new SkAttribsModifier();
		string[] array = desc.Split('#');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(new char[1] { ',' }, 2);
			switch (array3[0].ToLower())
			{
			case "mat":
			{
				SkAttribsOpMAT skAttribsOpMAT = new SkAttribsOpMAT(array3[1]);
				skAttribsModifier._tmpOps.Add(skAttribsOpMAT);
				skAttribsModifier._tmpOpsIdxs.Add(skAttribsOpMAT._idx);
				break;
			}
			case "mad":
				skAttribsModifier._ops.Add(new SkAttribsOpMAD(array3[1]));
				break;
			case "exp":
			{
				SkAttribsOpEXP skAttribsOpEXP = new SkAttribsOpEXP(array3[1]);
				if (skAttribsOpEXP._bTmpOp)
				{
					skAttribsModifier._tmpOps.Add(skAttribsOpEXP);
					skAttribsModifier._tmpOpsIdxs.Add(skAttribsOpEXP._idx);
				}
				else
				{
					skAttribsModifier._ops.Add(skAttribsOpEXP);
				}
				break;
			}
			case "get":
				skAttribsModifier._ops.Add(new SkAttribsOpGET(array3[1]));
				break;
			case "rnd":
				skAttribsModifier._ops.Add(new SkAttribsOpRND(array3[1]));
				break;
			default:
				Debug.Log("[Error]:Unrecognized atttribModifier." + text);
				break;
			}
		}
		return skAttribsModifier;
	}

	public void ReqExecTmp(ISkAttribs dst)
	{
		int count = _tmpOpsIdxs.Count;
		for (int i = 0; i < count; i++)
		{
			dst.modflags[_tmpOpsIdxs[i]] = true;
		}
	}

	public void Exec(ISkAttribs dst, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
	{
		int count = _ops.Count;
		for (int i = 0; i < count; i++)
		{
			_ops[i].Exec(dst, paraCaster, paraTarget, para);
		}
		ReqExecTmp(dst);
	}

	public void TryExecTmp(ISkAttribs dst, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para, int idxToMod, int times = 1)
	{
		int count = _tmpOpsIdxs.Count;
		for (int i = 0; i < count; i++)
		{
			if (_tmpOpsIdxs[i] != idxToMod)
			{
				continue;
			}
			for (int j = 0; j < times; j++)
			{
				_tmpOps[i].Exec(dst, paraCaster, paraTarget, para);
				if (_tmpOps[i] is SkAttribsOpMAT skAttribsOpMAT)
				{
					dst.buffMul += skAttribsOpMAT._mul;
					dst.buffPreAdd += skAttribsOpMAT._preAdd;
					dst.buffPostAdd += skAttribsOpMAT._postAdd;
				}
			}
		}
	}
}
