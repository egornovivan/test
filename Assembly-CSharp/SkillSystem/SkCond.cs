using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem;

public class SkCond
{
	public enum CondType
	{
		TypeNormal,
		TypeRTCol
	}

	internal CondType _type;

	internal object _para;

	internal List<SkEntity> _retTars;

	internal Func<SkInst, object, SkFuncInOutPara> _cond;

	private SkCond()
	{
	}

	private static SkCond CreateSub(string desc)
	{
		SkCond skCond = new SkCond();
		try
		{
			string[] strCond = desc.Split(new char[1] { ',' }, 2);
			switch (strCond[0].ToLower())
			{
			case "loopcnt":
				skCond._cond = (SkInst inst, object y) => new SkFuncInOutPara(inst, y, inst.GuideCnt > 0 && inst.GuideCnt < Convert.ToInt32(strCond[1]));
				break;
			case "key":
				skCond._cond = (SkInst inst, object y) => new SkFuncInOutPara(inst, y, Input.GetKey(KeyCode.Q));
				break;
			case "lasthit":
				skCond._cond = (SkInst inst, object y) => new SkFuncInOutPara(inst, y, inst.LastHit);
				break;
			case "col":
				skCond._para = strCond[1];
				skCond._cond = TstCol;
				skCond._type = CondType.TypeRTCol;
				break;
			case "colname":
				skCond._para = strCond[1];
				skCond._cond = TstColName;
				skCond._type = CondType.TypeRTCol;
				break;
			case "collayer":
				skCond._para = strCond[1];
				skCond._cond = TstColLayer;
				skCond._type = CondType.TypeRTCol;
				break;
			case "colinfo":
				skCond._cond = TstColInfo;
				skCond._type = CondType.TypeRTCol;
				break;
			case "range":
				skCond._para = strCond[1];
				skCond._cond = TstInRange;
				break;
			case "true":
				skCond._cond = (SkInst inst, object y) => new SkFuncInOutPara(inst, y, ret: true);
				break;
			case "false":
				skCond._cond = (SkInst inst, object y) => new SkFuncInOutPara(inst, y);
				break;
			default:
				skCond._para = strCond;
				skCond._cond = TstExternalFunc;
				break;
			}
		}
		catch
		{
			Debug.LogError("[SKERR]Unrecognized sk cond:" + desc);
		}
		return skCond;
	}

	public static SkCond Create(string desc)
	{
		string[] array = desc.Split(';');
		List<SkCond> list = new List<SkCond>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			try
			{
				int num = Convert.ToInt32(text);
				if (list.Count == 0)
				{
					list.Add(CreateSub("true"));
				}
				for (int j = 1; j < num; j++)
				{
					list.Add(list[list.Count - 1]);
				}
			}
			catch
			{
				list.Add(CreateSub(text));
			}
		}
		SkCond skCond;
		if (list.Count == 1)
		{
			skCond = list[0];
		}
		else
		{
			skCond = new SkCond();
			skCond._para = list;
			skCond._cond = delegate(SkInst inst, object y)
			{
				List<SkCond> list2 = y as List<SkCond>;
				return (inst.GuideCnt > 0 && inst.GuideCnt <= list2.Count && list2[inst.GuideCnt - 1].Tst(inst)) ? new SkFuncInOutPara(inst, list2[inst.GuideCnt - 1]._retTars, ret: true) : new SkFuncInOutPara(inst, y);
			};
		}
		return skCond;
	}

	public bool Tst(SkInst inst)
	{
		SkFuncInOutPara skFuncInOutPara = _cond(inst, _para);
		_retTars = ((!skFuncInOutPara._ret) ? null : (skFuncInOutPara._para as List<SkEntity>));
		return skFuncInOutPara._ret;
	}

	internal static SkFuncInOutPara TstCol(SkInst inst, object strColDesc)
	{
		SkFuncInOutPara skFuncInOutPara = new SkFuncInOutPara(inst, strColDesc);
		if (inst._colInfo == null)
		{
			return skFuncInOutPara;
		}
		skFuncInOutPara._ret = false;
		SkCondColDesc colDesc = skFuncInOutPara.GetColDesc();
		if (colDesc.Contain(inst._colInfo))
		{
			SkEntity skEntity = (SkEntity)inst._colInfo.hitTrans;
			if (skEntity != null)
			{
				skFuncInOutPara._para = new List<SkEntity> { skEntity };
			}
			skFuncInOutPara._ret = true;
		}
		return skFuncInOutPara;
	}

	internal static SkFuncInOutPara TstColName(SkInst inst, object strColDesc)
	{
		SkFuncInOutPara skFuncInOutPara = new SkFuncInOutPara(inst, strColDesc);
		if (inst._colInfo == null)
		{
			return skFuncInOutPara;
		}
		SkCondColDesc colDesc = skFuncInOutPara.GetColDesc();
		colDesc.colTypes[0] = (colDesc.colTypes[1] = SkCondColDesc.ColType.ColName);
		colDesc.colStrings[1] = new string[1] { string.Empty };
		if (colDesc.Contain(inst._colInfo))
		{
			SkEntity skEntity = (SkEntity)inst._colInfo.hitTrans;
			if (skEntity != null)
			{
				skFuncInOutPara._para = new List<SkEntity> { skEntity };
			}
			skFuncInOutPara._ret = true;
		}
		return skFuncInOutPara;
	}

	internal static SkFuncInOutPara TstColLayer(SkInst inst, object strColDesc)
	{
		SkFuncInOutPara skFuncInOutPara = new SkFuncInOutPara(inst, strColDesc);
		if (inst._colInfo == null)
		{
			return skFuncInOutPara;
		}
		SkCondColDesc colDesc = skFuncInOutPara.GetColDesc();
		colDesc.colTypes[0] = (colDesc.colTypes[1] = SkCondColDesc.ColType.ColLayer);
		if (colDesc.Contain(inst._colInfo))
		{
			SkEntity skEntity = (SkEntity)inst._colInfo.hitTrans;
			if (skEntity != null)
			{
				skFuncInOutPara._para = new List<SkEntity> { skEntity };
			}
			skFuncInOutPara._ret = true;
		}
		return skFuncInOutPara;
	}

	internal static SkFuncInOutPara TstColInfo(SkInst inst, object para)
	{
		SkFuncInOutPara skFuncInOutPara = new SkFuncInOutPara(inst, para);
		if (inst._colInfo == null)
		{
			return skFuncInOutPara;
		}
		SkEntity skEntity = (SkEntity)inst._colInfo.hitTrans;
		if (skEntity != null)
		{
			skFuncInOutPara._para = new List<SkEntity> { skEntity };
			skFuncInOutPara._ret = true;
		}
		return skFuncInOutPara;
	}

	internal static SkFuncInOutPara TstInRange(SkInst inst, object para)
	{
		return new SkFuncInOutPara(inst, para);
	}

	internal static SkFuncInOutPara TstExternalFunc(SkInst inst, object para)
	{
		string[] array = para as string[];
		SkFuncInOutPara skFuncInOutPara = new SkFuncInOutPara(inst, (array.Length >= 2) ? array[1] : null);
		inst._caster.SendMessage(array[0], skFuncInOutPara, SendMessageOptions.DontRequireReceiver);
		return skFuncInOutPara;
	}
}
