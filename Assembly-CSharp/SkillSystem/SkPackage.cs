using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem;

public class SkPackage : PackBase
{
	internal static List<SkBuffInst> _tmpBuffs = new List<SkBuffInst>();

	internal SkAttribs _parentAttribs;

	internal List<SkBuffInst> _buffs = new List<SkBuffInst>();

	public SkPackage(SkAttribs parentAttribs)
	{
		_parentAttribs = parentAttribs;
	}

	public void ExecBuffs()
	{
		int count = _buffs.Count;
		if (count <= 0)
		{
			return;
		}
		_tmpBuffs.Clear();
		_tmpBuffs.AddRange(_buffs);
		for (int num = count - 1; num >= 0; num--)
		{
			int num2 = ((_buffs[num] != _tmpBuffs[num]) ? _buffs.IndexOf(_tmpBuffs[num]) : num);
			if (num2 >= 0 && !_tmpBuffs[num].Exec(_parentAttribs) && _tmpBuffs[num].OnDiscard(_parentAttribs))
			{
				_buffs.RemoveAt(num2);
			}
		}
	}

	public void ExecTmpBuffs(int idxToExec)
	{
		int count = _buffs.Count;
		if (count <= 0)
		{
			return;
		}
		_tmpBuffs.Clear();
		_tmpBuffs.AddRange(_buffs);
		for (int num = count - 1; num >= 0; num--)
		{
			int num2 = ((_buffs[num] != _tmpBuffs[num]) ? _buffs.IndexOf(_tmpBuffs[num]) : num);
			if (num2 >= 0)
			{
				_tmpBuffs[num].TryExecTmp(_parentAttribs, idxToExec);
			}
		}
	}

	protected override void PushIn(params int[] ids)
	{
		List<ItemToPack> list = new List<ItemToPack>();
		int num = ids.Length;
		int num2 = 0;
		while (num2 < num)
		{
			if (IsBuff(ids[num2]))
			{
				SkBuffInst.Mount(this, SkBuffInst.Create(ids[num2], null, _parentAttribs));
				num2 += 2;
			}
			else
			{
				list.Add(new ItemToPack(ids[num2], ids[num2 + 1]));
				num2 += 2;
			}
		}
		if (list.Count > 0)
		{
			_parentAttribs.OnPutInPakAttribs(list);
		}
	}

	protected override void PopOut(params int[] ids)
	{
		int num = ids.Length;
		int i = 0;
		while (i < num)
		{
			if (IsBuff(ids[i]))
			{
				SkBuffInst.Unmount(this, (SkBuffInst buffInst) => buffInst.MatchID(ids[i]));
				i++;
			}
			else
			{
				Debug.LogError("[SkPackablAttribs]Unsupport id to minus:" + ids[i]);
				i += 2;
			}
		}
	}

	internal static bool IsBuff(int id)
	{
		return id > 10000000;
	}
}
