using System.Collections.Generic;
using Pathea.Maths;
using UnityEngine;

namespace RedGrass;

public class RGSimpleEditor : MonoBehaviour
{
	public bool isAdd = true;

	public float radius = 20f;

	public int deleteHeight = 10;

	public float density = 1f;

	public Texture2D pattern;

	public MapProjectorAnyAxis mapProjector;

	public string prototypes = "0";

	public RGScene scene;

	private Dictionary<INTVECTOR3, RedGrassInstance> mAddGrasses;

	private Vector3[,] _normals;

	public bool isEmpty => mAddGrasses.Count == 0;

	public RedGrassInstance[] addGrasses
	{
		get
		{
			RedGrassInstance[] array = new RedGrassInstance[mAddGrasses.Count];
			int num = 0;
			foreach (RedGrassInstance value in mAddGrasses.Values)
			{
				array[num] = value;
				num++;
			}
			return array;
		}
	}

	public void Clear()
	{
		mAddGrasses.Clear();
	}

	private void Awake()
	{
		mAddGrasses = new Dictionary<INTVECTOR3, RedGrassInstance>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		int height = pattern.height;
		int width = pattern.width;
		if (height != width)
		{
			return;
		}
		int[] array = GetPrototypes(prototypes);
		if (isAdd && array.Length == 0)
		{
			return;
		}
		mapProjector.MapTex = pattern;
		mapProjector.Size = radius;
		mapProjector.NearClip = -50f;
		mapProjector.ColorIndex = ((!isAdd) ? 1 : 0);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (!Physics.Raycast(ray, out var hitInfo, 1000f, 4096))
		{
			return;
		}
		mapProjector.transform.position = hitInfo.point;
		if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
		{
			if (isAdd)
			{
				DrawGrass(hitInfo.point, hitInfo.normal, array);
			}
			else
			{
				DeleteGrass(hitInfo.point, hitInfo.normal);
			}
		}
	}

	private void DrawGrass(Vector3 point, Vector3 nml, int[] protos)
	{
		float num = radius * 2f;
		int num2 = Mathf.Clamp(Mathf.RoundToInt(num / 30f) + 1, 1, 4);
		float num3 = Mathf.Max(0f, point.x - radius - (float)num2);
		float num4 = point.x + radius + (float)(num2 * 2);
		float num5 = Mathf.Max(0f, point.z - radius - (float)num2);
		float num6 = point.z + radius + (float)(num2 * 2);
		bool flag = point.y >= -0.1f && Camera.main.transform.forward.y < 0.7f;
		int num7 = Mathf.CeilToInt(num / (float)num2) + 1;
		float[,] array = new float[num7 + 2, num7 + 2];
		Vector3 direction = ((!flag) ? Vector3.up : Vector3.down);
		float y = ((!flag) ? (point.y - 100f) : (point.y + 100f));
		int num8 = 0;
		float num9 = num3;
		while (num9 < num4)
		{
			int num10 = 0;
			float num11 = num5;
			while (num11 < num6)
			{
				Vector3 origin = new Vector3(num9, y, num11);
				Ray ray = new Ray(origin, direction);
				if (Physics.Raycast(ray, out var hitInfo, 1000f, 4096))
				{
					array[num8, num10] = hitInfo.point.y;
				}
				num11 += (float)num2;
				num10++;
			}
			num9 += (float)num2;
			num8++;
		}
		_normals = new Vector3[num7, num7];
		for (int i = 0; i < num7; i++)
		{
			for (int j = 0; j < num7; j++)
			{
				_normals[i, j] = CalculateNormal(array[i + 2, j + 1], array[i, j + 1], array[i + 1, j + 2], array[i + 1, j], num2 * 2);
			}
		}
		int num12 = Mathf.CeilToInt(radius);
		Color[] pixels = mapProjector.MapTex.GetPixels();
		int height = mapProjector.MapTex.height;
		int width = mapProjector.MapTex.width;
		for (int k = -num12; k <= num12; k++)
		{
			for (int l = -num12; l <= num12; l++)
			{
				Vector3 origin2 = point + new Vector3(k, y, l);
				Vector2 pos = new Vector2(((float)k + radius) / (radius * 2f) * (float)width, ((float)l + radius) / (radius * 2f) * (float)height);
				float num13 = CalcHeight(pos, pixels, width, height);
				float num14 = Mathf.Clamp01(num13 * density);
				if (num13 < 0.02f || num14 < 0.002f || !Physics.Raycast(origin2, direction, out var hitInfo2, 1000f, 4096))
				{
					continue;
				}
				INTVECTOR3 key = new INTVECTOR3((int)hitInfo2.point.x, (int)hitInfo2.point.y, (int)hitInfo2.point.z);
				RedGrassInstance redGrassInstance = scene.data.Read(key.x, key.y, key.z);
				if (redGrassInstance.Density < 0.001f)
				{
					RedGrassInstance redGrassInstance2 = default(RedGrassInstance);
					redGrassInstance2.Density = num14;
					redGrassInstance2.Position = hitInfo2.point;
					redGrassInstance2.Prototype = protos[Random.Range(0, protos.Length) % protos.Length];
					redGrassInstance2.ColorF = Color.white;
					if (scene.data.Write(redGrassInstance2))
					{
						mAddGrasses[key] = redGrassInstance2;
					}
					continue;
				}
				RedGrassInstance redGrassInstance3 = default(RedGrassInstance);
				redGrassInstance3.Density = Mathf.Clamp01(num14 + redGrassInstance.Density);
				redGrassInstance3.Position = redGrassInstance.Position;
				if (Random.value < density)
				{
					redGrassInstance3.Prototype = protos[Random.Range(0, protos.Length) % protos.Length];
				}
				else
				{
					redGrassInstance3.Prototype = redGrassInstance.Prototype;
				}
				redGrassInstance3.ColorF = redGrassInstance.ColorF;
				redGrassInstance3.Normal = redGrassInstance.Normal;
				if (scene.data.Write(redGrassInstance3))
				{
					mAddGrasses[key] = redGrassInstance3;
				}
			}
		}
	}

	private void DeleteGrass(Vector3 point, Vector3 nml)
	{
		int num = Mathf.CeilToInt(radius / 30f) * 2;
		bool flag = nml.y >= -0.1f && Camera.main.transform.forward.y < 0.7f;
		Vector3 direction = ((!flag) ? Vector3.up : Vector3.down);
		float y = ((!flag) ? (point.y - radius) : (point.y + radius));
		int num2 = num / 2;
		Color[] pixels = mapProjector.MapTex.GetPixels();
		int height = mapProjector.MapTex.height;
		int width = mapProjector.MapTex.width;
		for (float num3 = 0f - radius; num3 < radius; num3 += 1f)
		{
			for (float num4 = 0f - radius; num4 < radius; num4 += 1f)
			{
				Vector3 origin = point + new Vector3(num3, y, num4);
				Ray ray = new Ray(origin, direction);
				if (!Physics.Raycast(ray, out var hitInfo, 1000f, 4096))
				{
					continue;
				}
				for (int i = -num2; i <= num2; i++)
				{
					for (int j = -num2; j <= num2; j++)
					{
						Vector2 pos = new Vector2(((num3 + (float)i) / radius + 1f) * 0.5f * (float)width, ((num4 + (float)j) / radius + 1f) * 0.5f * (float)height);
						float num5 = CalcHeight(pos, pixels, width, height) * density;
						List<RedGrassInstance> list = scene.data.Read((int)origin.x + i, (int)origin.z + j, (int)hitInfo.point.y - num2 - 1, (int)hitInfo.point.y + num2 + 1 + Mathf.Max(0, deleteHeight));
						if (num5 < 0.0001f)
						{
							continue;
						}
						foreach (RedGrassInstance item in list)
						{
							RedGrassInstance redGrassInstance = default(RedGrassInstance);
							redGrassInstance.Position = item.Position;
							redGrassInstance.Normal = item.Normal;
							redGrassInstance.Density = Mathf.Max(0f, item.Density - num5);
							redGrassInstance.ColorF = item.ColorF;
							redGrassInstance.Prototype = item.Prototype;
							if (scene.data.Write(redGrassInstance))
							{
								Vector3 position = redGrassInstance.Position;
								mAddGrasses[new INTVECTOR3((int)position.x, (int)position.y, (int)position.z)] = redGrassInstance;
							}
						}
					}
				}
			}
		}
	}

	private Vector3 CalculateNormal(float xForwad, float xBack, float zForwad, float zBack, int detla)
	{
		float value = (xBack - xForwad) / (float)detla;
		float value2 = (zBack - zForwad) / (float)detla;
		return new Vector3(Mathf.Clamp(value, -0.6f, 0.6f), 1f, Mathf.Clamp(value2, -0.6f, 0.6f)).normalized;
	}

	private float CalcHeight(Vector2 pos, Color[] pixels, int tex_w, int tex_h)
	{
		if (pos.x < 0f || pos.y < 0f)
		{
			return 0f;
		}
		float t = pos.x - (float)(int)pos.x;
		float t2 = pos.y - (float)(int)pos.y;
		int num = Mathf.Min(Mathf.CeilToInt(pos.x), tex_w - 1);
		int num2 = Mathf.Min(Mathf.FloorToInt(pos.x), tex_w - 1);
		int num3 = Mathf.Min(Mathf.CeilToInt(pos.y), tex_h - 1);
		int num4 = Mathf.Min(Mathf.FloorToInt(pos.y), tex_h - 1);
		float a = pixels[tex_w * num4 + num2].a;
		float a2 = pixels[tex_w * num4 + num].a;
		float a3 = pixels[tex_w * num3 + num2].a;
		float a4 = pixels[tex_w * num3 + num].a;
		return Mathf.Lerp(Mathf.Lerp(a, a2, t), Mathf.Lerp(a3, a4, t), t2);
	}

	private int[] GetPrototypes(string str)
	{
		try
		{
			string[] array = str.Split(',');
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = int.Parse(array[i]);
			}
			return array2;
		}
		catch
		{
			return new int[0];
		}
	}
}
