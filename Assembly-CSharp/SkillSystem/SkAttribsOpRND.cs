using System;
using System.Collections.Generic;

namespace SkillSystem;

public class SkAttribsOpRND : ISkAttribsOp
{
	internal int _cntMin;

	internal int _cntMax;

	internal int[] _ids;

	internal float[] _odds;

	public SkAttribsOpRND(string strArgs)
	{
		string[] array = strArgs.Split(',');
		_cntMin = Convert.ToInt32(array[0]);
		_cntMax = Convert.ToInt32(array[1]);
		int num = (array.Length - 2) / 2;
		_ids = new int[num];
		_odds = new float[num];
		for (int i = 0; i < num; i++)
		{
			_ids[i] = Convert.ToInt32(array[2 + i * 2]);
			_odds[i] = Convert.ToSingle(array[3 + i * 2]);
		}
	}

	public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
	{
		int num = SkInst.s_rand.Next(_cntMin, _cntMax + 1);
		int num2 = _ids.Length;
		int[] array = new int[num2];
		for (int i = 0; i < num; i++)
		{
			float num3 = (float)SkInst.s_rand.NextDouble();
			for (int j = 0; j < num2; j++)
			{
				if (num3 <= _odds[j])
				{
					array[j]++;
					break;
				}
			}
		}
		List<int> list = new List<int>(num2 * 2);
		for (int k = 0; k < num2; k++)
		{
			if (array[k] > 0)
			{
				list.Add(_ids[k]);
				list.Add(array[k]);
			}
		}
		dstAttribs.pack += list.ToArray();
	}
}
