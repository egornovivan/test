using System.Collections.Generic;
using UnityEngine;

namespace BSTools;

public class SelBox
{
	public IntBox m_Box;

	public byte m_Val;

	public SelBox()
	{
	}

	public SelBox(long hash, byte val)
	{
		m_Box.Hash = hash;
		m_Val = val;
	}

	public static IntBox CalculateBound(List<SelBox> boxes)
	{
		int count = boxes.Count;
		int[] array = new int[count];
		int[] array2 = new int[count];
		int[] array3 = new int[count];
		int[] array4 = new int[count];
		int[] array5 = new int[count];
		int[] array6 = new int[count];
		for (int i = 0; i < count; i++)
		{
			IntBox box = boxes[i].m_Box;
			array[i] = box.xMin;
			array2[i] = box.yMin;
			array3[i] = box.zMin;
			array4[i] = box.xMax;
			array5[i] = box.yMax;
			array6[i] = box.zMax;
		}
		IntBox result = default(IntBox);
		result.xMin = (short)Mathf.Min(array);
		result.yMin = (short)Mathf.Min(array2);
		result.zMin = (short)Mathf.Min(array3);
		result.xMax = (short)Mathf.Max(array4);
		result.yMax = (short)Mathf.Max(array5);
		result.zMax = (short)Mathf.Max(array6);
		return result;
	}
}
