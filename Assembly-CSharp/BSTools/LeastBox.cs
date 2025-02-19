using System.Collections.Generic;

namespace BSTools;

public static class LeastBox
{
	public static int SortSelBox(SelBox a, SelBox b)
	{
		return a.m_Val - b.m_Val;
	}

	public static void Calculate(Dictionary<IntVector3, byte> bitmap, ref List<SelBox> leastboxes)
	{
		leastboxes.Clear();
		IntVector3[] array = new IntVector3[bitmap.Count];
		int num = 0;
		foreach (IntVector3 key11 in bitmap.Keys)
		{
			array[num] = new IntVector3(key11);
			num++;
		}
		IntVector3[] array2 = array;
		foreach (IntVector3 intVector in array2)
		{
			Dictionary<IntVector3, byte> dictionary;
			Dictionary<IntVector3, byte> dictionary2 = (dictionary = bitmap);
			IntVector3 key;
			IntVector3 key2 = (key = intVector);
			byte b = dictionary[key];
			dictionary2[key2] = (byte)(b & 0xFE);
		}
		IntVector3[] array3 = array;
		foreach (IntVector3 intVector2 in array3)
		{
			if ((bitmap[intVector2] & 1) != 0)
			{
				continue;
			}
			byte b2 = bitmap[intVector2];
			b2 = (byte)((b2 != 254) ? ((uint)(b2 & 0xE0)) : 254u);
			Dictionary<IntVector3, byte> dictionary3;
			Dictionary<IntVector3, byte> dictionary4 = (dictionary3 = bitmap);
			IntVector3 key;
			IntVector3 key3 = (key = intVector2);
			byte b = dictionary3[key];
			dictionary4[key3] = (byte)(b | 1);
			IntVector3 intVector3 = intVector2;
			IntVector3 intVector4 = new IntVector3(intVector3);
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			while (flag || flag2 || flag3)
			{
				if (flag)
				{
					bool flag4 = true;
					bool flag5 = true;
					List<IntVector3> list = new List<IntVector3>();
					List<IntVector3> list2 = new List<IntVector3>();
					for (int k = intVector3.y; k <= intVector4.y; k++)
					{
						for (int l = intVector3.z; l <= intVector4.z; l++)
						{
							IntVector3 intVector5 = new IntVector3(intVector4.x + 1, k, l);
							IntVector3 intVector6 = new IntVector3(intVector3.x - 1, k, l);
							byte b3 = byte.MaxValue;
							byte b4 = byte.MaxValue;
							if (bitmap.ContainsKey(intVector5))
							{
								b3 = bitmap[intVector5];
							}
							if (bitmap.ContainsKey(intVector6))
							{
								b4 = bitmap[intVector6];
							}
							b3 = (byte)((b3 != 254) ? ((uint)(b3 & 0xE1)) : 254u);
							b4 = (byte)((b4 != 254) ? ((uint)(b4 & 0xE1)) : 254u);
							if (b3 != b2)
							{
								flag4 = false;
							}
							else
							{
								list.Add(intVector5);
							}
							if (b4 != b2)
							{
								flag5 = false;
							}
							else
							{
								list2.Add(intVector6);
							}
							if (!flag4 && !flag5)
							{
								break;
							}
						}
						if (!flag4 && !flag5)
						{
							break;
						}
					}
					if (flag4)
					{
						foreach (IntVector3 item in list)
						{
							Dictionary<IntVector3, byte> dictionary5;
							Dictionary<IntVector3, byte> dictionary6 = (dictionary5 = bitmap);
							IntVector3 key4 = (key = item);
							b = dictionary5[key];
							dictionary6[key4] = (byte)(b | 1);
						}
						intVector4.x++;
					}
					if (flag5)
					{
						foreach (IntVector3 item2 in list2)
						{
							Dictionary<IntVector3, byte> dictionary7;
							Dictionary<IntVector3, byte> dictionary8 = (dictionary7 = bitmap);
							IntVector3 key5 = (key = item2);
							b = dictionary7[key];
							dictionary8[key5] = (byte)(b | 1);
						}
						intVector3.x--;
					}
					if (!flag4 && !flag5)
					{
						flag = false;
					}
					list.Clear();
					list2.Clear();
				}
				if (flag3)
				{
					bool flag6 = true;
					bool flag7 = true;
					List<IntVector3> list3 = new List<IntVector3>();
					List<IntVector3> list4 = new List<IntVector3>();
					for (int m = intVector3.y; m <= intVector4.y; m++)
					{
						for (int n = intVector3.x; n <= intVector4.x; n++)
						{
							IntVector3 intVector7 = new IntVector3(n, m, intVector4.z + 1);
							IntVector3 intVector8 = new IntVector3(n, m, intVector3.z - 1);
							byte b5 = byte.MaxValue;
							byte b6 = byte.MaxValue;
							if (bitmap.ContainsKey(intVector7))
							{
								b5 = bitmap[intVector7];
							}
							if (bitmap.ContainsKey(intVector8))
							{
								b6 = bitmap[intVector8];
							}
							b5 = (byte)((b5 != 254) ? ((uint)(b5 & 0xE1)) : 254u);
							b6 = (byte)((b6 != 254) ? ((uint)(b6 & 0xE1)) : 254u);
							if (b5 != b2)
							{
								flag6 = false;
							}
							else
							{
								list3.Add(intVector7);
							}
							if (b6 != b2)
							{
								flag7 = false;
							}
							else
							{
								list4.Add(intVector8);
							}
							if (!flag6 && !flag7)
							{
								break;
							}
						}
						if (!flag6 && !flag7)
						{
							break;
						}
					}
					if (flag6)
					{
						foreach (IntVector3 item3 in list3)
						{
							Dictionary<IntVector3, byte> dictionary9;
							Dictionary<IntVector3, byte> dictionary10 = (dictionary9 = bitmap);
							IntVector3 key6 = (key = item3);
							b = dictionary9[key];
							dictionary10[key6] = (byte)(b | 1);
						}
						intVector4.z++;
					}
					if (flag7)
					{
						foreach (IntVector3 item4 in list4)
						{
							Dictionary<IntVector3, byte> dictionary11;
							Dictionary<IntVector3, byte> dictionary12 = (dictionary11 = bitmap);
							IntVector3 key7 = (key = item4);
							b = dictionary11[key];
							dictionary12[key7] = (byte)(b | 1);
						}
						intVector3.z--;
					}
					if (!flag6 && !flag7)
					{
						flag3 = false;
					}
					list3.Clear();
					list4.Clear();
				}
				if (!flag2)
				{
					continue;
				}
				bool flag8 = true;
				bool flag9 = true;
				List<IntVector3> list5 = new List<IntVector3>();
				List<IntVector3> list6 = new List<IntVector3>();
				for (int num2 = intVector3.x; num2 <= intVector4.x; num2++)
				{
					for (int num3 = intVector3.z; num3 <= intVector4.z; num3++)
					{
						IntVector3 intVector9 = new IntVector3(num2, intVector4.y + 1, num3);
						IntVector3 intVector10 = new IntVector3(num2, intVector3.y - 1, num3);
						byte b7 = byte.MaxValue;
						byte b8 = byte.MaxValue;
						if (bitmap.ContainsKey(intVector9))
						{
							b7 = bitmap[intVector9];
						}
						if (bitmap.ContainsKey(intVector10))
						{
							b8 = bitmap[intVector10];
						}
						b7 = (byte)((b7 != 254) ? ((uint)(b7 & 0xE1)) : 254u);
						b8 = (byte)((b8 != 254) ? ((uint)(b8 & 0xE1)) : 254u);
						if (b7 != b2)
						{
							flag8 = false;
						}
						else
						{
							list5.Add(intVector9);
						}
						if (b8 != b2)
						{
							flag9 = false;
						}
						else
						{
							list6.Add(intVector10);
						}
						if (!flag8 && !flag9)
						{
							break;
						}
					}
					if (!flag8 && !flag9)
					{
						break;
					}
				}
				if (flag8)
				{
					foreach (IntVector3 item5 in list5)
					{
						Dictionary<IntVector3, byte> dictionary13;
						Dictionary<IntVector3, byte> dictionary14 = (dictionary13 = bitmap);
						IntVector3 key8 = (key = item5);
						b = dictionary13[key];
						dictionary14[key8] = (byte)(b | 1);
					}
					intVector4.y++;
				}
				if (flag9)
				{
					foreach (IntVector3 item6 in list6)
					{
						Dictionary<IntVector3, byte> dictionary15;
						Dictionary<IntVector3, byte> dictionary16 = (dictionary15 = bitmap);
						IntVector3 key9 = (key = item6);
						b = dictionary15[key];
						dictionary16[key9] = (byte)(b | 1);
					}
					intVector3.y--;
				}
				if (!flag8 && !flag9)
				{
					flag2 = false;
				}
				list5.Clear();
				list6.Clear();
			}
			SelBox selBox = new SelBox();
			selBox.m_Box.xMin = (short)intVector3.x;
			selBox.m_Box.yMin = (short)intVector3.y;
			selBox.m_Box.zMin = (short)intVector3.z;
			selBox.m_Box.xMax = (short)intVector4.x;
			selBox.m_Box.yMax = (short)intVector4.y;
			selBox.m_Box.zMax = (short)intVector4.z;
			selBox.m_Val = (byte)(b2 | 7);
			leastboxes.Add(selBox);
		}
		leastboxes.Sort(SortSelBox);
		IntVector3[] array4 = array;
		foreach (IntVector3 intVector11 in array4)
		{
			Dictionary<IntVector3, byte> dictionary17;
			Dictionary<IntVector3, byte> dictionary18 = (dictionary17 = bitmap);
			IntVector3 key;
			IntVector3 key10 = (key = intVector11);
			byte b = dictionary17[key];
			dictionary18[key10] = (byte)(b | 1);
		}
	}
}
