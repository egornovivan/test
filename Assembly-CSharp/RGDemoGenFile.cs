using System.Collections.Generic;
using System.IO;
using RedGrass;
using UnityEngine;

public class RGDemoGenFile : MonoBehaviour
{
	private const int tile = 32;

	private const int xcount = 192;

	private const int zcount = 192;

	private const int xzcount = 36864;

	private const int xstart = -3072;

	private const int zstart = -3072;

	private const int xend = 3072;

	private const int zend = 3072;

	private const string filename = "D:/grassinst_test.dat";

	private List<RedGrassInstance>[] allGrasses;

	private int[] offsets;

	private int[] lens;

	private FileStream fs;

	private BinaryWriter w;

	private int x = -3072;

	private int z = -3072;

	private int idx;

	private void Start()
	{
		allGrasses = new List<RedGrassInstance>[36864];
		offsets = new int[36864];
		lens = new int[36864];
		for (int i = 0; i < 36864; i++)
		{
			allGrasses[i] = new List<RedGrassInstance>();
		}
		fs = new FileStream("D:/grassinst_test.dat", FileMode.OpenOrCreate, FileAccess.Write);
		w = new BinaryWriter(fs);
		w.Seek(294912, SeekOrigin.Begin);
	}

	private void Update()
	{
		GenOneTile();
		GenOneTile();
		GenOneTile();
		GenOneTile();
	}

	private void GenOneTile()
	{
		if (idx == 36864)
		{
			w.Seek(0, SeekOrigin.Begin);
			for (int i = 0; i < 36864; i++)
			{
				w.Write(offsets[i]);
				w.Write(lens[i]);
			}
			w.Close();
			fs.Close();
			Object.Destroy(this);
			return;
		}
		offsets[idx] = (int)fs.Position;
		SimplexNoise simplexNoise = new SimplexNoise();
		for (int j = 0; j < 32; j++)
		{
			for (int k = 0; k < 32; k++)
			{
				Vector3 origin = new Vector3((float)(x + j) + 0.5f, 1043f, (float)(z + k) + 0.5f);
				float num = (float)simplexNoise.Noise(origin.x / 64f, origin.y / 64f, origin.z / 64f);
				float num2 = (float)simplexNoise.Noise(origin.y / 100f, origin.z / 100f, origin.x / 100f);
				if (num < -0.48f)
				{
					continue;
				}
				float density = Mathf.Clamp01(num + 0.5f);
				if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1000f, 4096))
				{
					RedGrassInstance item = default(RedGrassInstance);
					item.Position = hitInfo.point;
					item.Density = density;
					item.Normal = hitInfo.normal;
					item.ColorF = Color.white;
					if (Random.value > Mathf.Sqrt(hitInfo.normal.y))
					{
						item.Prototype = ((!(num2 > 0f)) ? 65 : 64);
						allGrasses[idx].Add(item);
					}
					item.Prototype = ((!(num2 > 0f)) ? 1 : 0);
					allGrasses[idx].Add(item);
					Debug.DrawRay(hitInfo.point, hitInfo.normal, new Color(hitInfo.normal.x * 0.5f + 0.5f, hitInfo.normal.y * 0.5f + 0.5f, hitInfo.normal.z * 0.5f + 0.5f), 1f);
				}
			}
		}
		lens[idx] = allGrasses[idx].Count;
		foreach (RedGrassInstance item2 in allGrasses[idx])
		{
			item2.WriteToStream(w);
		}
		allGrasses[idx].Clear();
		x += 32;
		if (x > 3071)
		{
			z += 32;
			x = -3072;
		}
		idx++;
		Debug.Log(idx.ToString());
	}
}
