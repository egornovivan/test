using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CompountMiniMap : MonoBehaviour
{
	private int oldTexSize = 512;

	private int newTexSize = 2048;

	private int totalPixe = 18432;

	private int texOneSide;

	public int IndexStart;

	public int IndexEnd = 35;

	private bool mStart;

	private void Start()
	{
		texOneSide = totalPixe / newTexSize;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			mStart = true;
			if (!Directory.Exists(Application.dataPath + "../../newMiniMap)"))
			{
				Directory.CreateDirectory(Application.dataPath + "../../newMiniMap");
			}
		}
		if (Input.GetKeyDown(KeyCode.N))
		{
			mStart = false;
		}
		if (!mStart)
		{
			return;
		}
		Texture2D texture2D = new Texture2D(newTexSize, newTexSize, TextureFormat.RGB24, mipmap: false);
		Dictionary<int, Texture2D> dictionary = new Dictionary<int, Texture2D>();
		bool flag = false;
		for (int i = 0; i < newTexSize; i++)
		{
			for (int j = 0; j < newTexSize; j++)
			{
				if (Input.GetKeyDown(KeyCode.N))
				{
					return;
				}
				int num = IndexStart % (totalPixe / newTexSize) * newTexSize + i;
				int num2 = IndexStart / (totalPixe / newTexSize) * newTexSize + j;
				int num3 = num / oldTexSize + num2 / oldTexSize * (totalPixe / oldTexSize);
				if (!dictionary.ContainsKey(num3))
				{
					Texture2D texture2D2 = Resources.Load("MiniMaps/MiniMap" + num3) as Texture2D;
					if (!(texture2D2 != null))
					{
						flag = true;
						mStart = false;
						break;
					}
					dictionary.Add(num3, texture2D2);
				}
				if (!flag)
				{
					IntVec2 intVec = new IntVec2();
					intVec.x = num - num3 % (totalPixe / oldTexSize) * oldTexSize;
					intVec.y = num2 - num3 / (totalPixe / oldTexSize) * oldTexSize;
					texture2D.SetPixel(i, j, dictionary[num3].GetPixel(intVec.x, intVec.y));
				}
			}
			if (flag)
			{
				break;
			}
		}
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		dictionary.Clear();
		File.WriteAllBytes(Application.dataPath + "../../newMiniMap/MiniMap" + (IndexStart % texOneSide + (texOneSide - 1 - IndexStart / texOneSide) * texOneSide) + ".png", bytes);
		IndexStart++;
		if (IndexStart > IndexEnd)
		{
			mStart = false;
		}
		GC.Collect();
	}
}
