using System.Collections.Generic;

public static class LeastBox
{
	public static int SortSelBox(SelBox a, SelBox b)
	{
		return a.m_Val - b.m_Val;
	}

	public static void Calculate(Dictionary<int, byte> bitmap, ref List<SelBox> leastboxes)
	{
		leastboxes.Clear();
		int[] array = new int[bitmap.Count];
		bitmap.Keys.CopyTo(array, 0);
		int[] array2 = array;
		foreach (int num in array2)
		{
			Dictionary<int, byte> dictionary;
			Dictionary<int, byte> dictionary2 = (dictionary = bitmap);
			int key;
			int key2 = (key = num);
			byte b = dictionary[key];
			dictionary2[key2] = (byte)(b & 0xFE);
		}
		int[] array3 = array;
		foreach (int num2 in array3)
		{
			if ((bitmap[num2] & 1) != 0)
			{
				continue;
			}
			byte b2 = bitmap[num2];
			b2 = (byte)((b2 != 254) ? ((uint)(b2 & 0xE0)) : 254u);
			Dictionary<int, byte> dictionary3;
			Dictionary<int, byte> dictionary4 = (dictionary3 = bitmap);
			int key;
			int key3 = (key = num2);
			byte b = dictionary3[key];
			dictionary4[key3] = (byte)(b | 1);
			IntVector3 intVector = VCIsoData.KeyToIPos(num2);
			IntVector3 intVector2 = new IntVector3(intVector);
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			while (flag || flag2 || flag3)
			{
				if (flag)
				{
					bool flag4 = true;
					bool flag5 = true;
					List<int> list = new List<int>();
					List<int> list2 = new List<int>();
					for (int k = intVector.y; k <= intVector2.y; k++)
					{
						for (int l = intVector.z; l <= intVector2.z; l++)
						{
							int num3 = VCIsoData.IPosToKey(intVector2.x + 1, k, l);
							int num4 = VCIsoData.IPosToKey(intVector.x - 1, k, l);
							byte b3 = byte.MaxValue;
							byte b4 = byte.MaxValue;
							if (bitmap.ContainsKey(num3))
							{
								b3 = bitmap[num3];
							}
							if (bitmap.ContainsKey(num4))
							{
								b4 = bitmap[num4];
							}
							b3 = (byte)((b3 != 254) ? ((uint)(b3 & 0xE1)) : 254u);
							b4 = (byte)((b4 != 254) ? ((uint)(b4 & 0xE1)) : 254u);
							if (b3 != b2)
							{
								flag4 = false;
							}
							else
							{
								list.Add(num3);
							}
							if (b4 != b2)
							{
								flag5 = false;
							}
							else
							{
								list2.Add(num4);
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
						foreach (int item in list)
						{
							Dictionary<int, byte> dictionary5;
							Dictionary<int, byte> dictionary6 = (dictionary5 = bitmap);
							int key4 = (key = item);
							b = dictionary5[key];
							dictionary6[key4] = (byte)(b | 1);
						}
						intVector2.x++;
					}
					if (flag5)
					{
						foreach (int item2 in list2)
						{
							Dictionary<int, byte> dictionary7;
							Dictionary<int, byte> dictionary8 = (dictionary7 = bitmap);
							int key5 = (key = item2);
							b = dictionary7[key];
							dictionary8[key5] = (byte)(b | 1);
						}
						intVector.x--;
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
					List<int> list3 = new List<int>();
					List<int> list4 = new List<int>();
					for (int m = intVector.y; m <= intVector2.y; m++)
					{
						for (int n = intVector.x; n <= intVector2.x; n++)
						{
							int num5 = VCIsoData.IPosToKey(n, m, intVector2.z + 1);
							int num6 = VCIsoData.IPosToKey(n, m, intVector.z - 1);
							byte b5 = byte.MaxValue;
							byte b6 = byte.MaxValue;
							if (bitmap.ContainsKey(num5))
							{
								b5 = bitmap[num5];
							}
							if (bitmap.ContainsKey(num6))
							{
								b6 = bitmap[num6];
							}
							b5 = (byte)((b5 != 254) ? ((uint)(b5 & 0xE1)) : 254u);
							b6 = (byte)((b6 != 254) ? ((uint)(b6 & 0xE1)) : 254u);
							if (b5 != b2)
							{
								flag6 = false;
							}
							else
							{
								list3.Add(num5);
							}
							if (b6 != b2)
							{
								flag7 = false;
							}
							else
							{
								list4.Add(num6);
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
						foreach (int item3 in list3)
						{
							Dictionary<int, byte> dictionary9;
							Dictionary<int, byte> dictionary10 = (dictionary9 = bitmap);
							int key6 = (key = item3);
							b = dictionary9[key];
							dictionary10[key6] = (byte)(b | 1);
						}
						intVector2.z++;
					}
					if (flag7)
					{
						foreach (int item4 in list4)
						{
							Dictionary<int, byte> dictionary11;
							Dictionary<int, byte> dictionary12 = (dictionary11 = bitmap);
							int key7 = (key = item4);
							b = dictionary11[key];
							dictionary12[key7] = (byte)(b | 1);
						}
						intVector.z--;
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
				List<int> list5 = new List<int>();
				List<int> list6 = new List<int>();
				for (int num7 = intVector.x; num7 <= intVector2.x; num7++)
				{
					for (int num8 = intVector.z; num8 <= intVector2.z; num8++)
					{
						int num9 = VCIsoData.IPosToKey(num7, intVector2.y + 1, num8);
						int num10 = VCIsoData.IPosToKey(num7, intVector.y - 1, num8);
						byte b7 = byte.MaxValue;
						byte b8 = byte.MaxValue;
						if (bitmap.ContainsKey(num9))
						{
							b7 = bitmap[num9];
						}
						if (bitmap.ContainsKey(num10))
						{
							b8 = bitmap[num10];
						}
						b7 = (byte)((b7 != 254) ? ((uint)(b7 & 0xE1)) : 254u);
						b8 = (byte)((b8 != 254) ? ((uint)(b8 & 0xE1)) : 254u);
						if (b7 != b2)
						{
							flag8 = false;
						}
						else
						{
							list5.Add(num9);
						}
						if (b8 != b2)
						{
							flag9 = false;
						}
						else
						{
							list6.Add(num10);
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
					foreach (int item5 in list5)
					{
						Dictionary<int, byte> dictionary13;
						Dictionary<int, byte> dictionary14 = (dictionary13 = bitmap);
						int key8 = (key = item5);
						b = dictionary13[key];
						dictionary14[key8] = (byte)(b | 1);
					}
					intVector2.y++;
				}
				if (flag9)
				{
					foreach (int item6 in list6)
					{
						Dictionary<int, byte> dictionary15;
						Dictionary<int, byte> dictionary16 = (dictionary15 = bitmap);
						int key9 = (key = item6);
						b = dictionary15[key];
						dictionary16[key9] = (byte)(b | 1);
					}
					intVector.y--;
				}
				if (!flag8 && !flag9)
				{
					flag2 = false;
				}
				list5.Clear();
				list6.Clear();
			}
			SelBox selBox = new SelBox();
			selBox.m_Box.xMin = (short)intVector.x;
			selBox.m_Box.yMin = (short)intVector.y;
			selBox.m_Box.zMin = (short)intVector.z;
			selBox.m_Box.xMax = (short)intVector2.x;
			selBox.m_Box.yMax = (short)intVector2.y;
			selBox.m_Box.zMax = (short)intVector2.z;
			selBox.m_Val = (byte)(b2 | 7);
			leastboxes.Add(selBox);
		}
		leastboxes.Sort(SortSelBox);
		int[] array4 = array;
		foreach (int num12 in array4)
		{
			Dictionary<int, byte> dictionary17;
			Dictionary<int, byte> dictionary18 = (dictionary17 = bitmap);
			int key;
			int key10 = (key = num12);
			byte b = dictionary17[key];
			dictionary18[key10] = (byte)(b | 1);
		}
	}
}
