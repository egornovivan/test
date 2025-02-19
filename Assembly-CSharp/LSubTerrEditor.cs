using System;
using UnityEngine;
using WhiteCat;

public class LSubTerrEditor : MonoBehaviour
{
	public const int MAXPOSITION = 2048;

	public GUISkin GSkin;

	public int BeginIndex;

	public int EndIndex;

	public float Density = 3f;

	public float Radius = 5f;

	public float MinWidthScale = 1f;

	public float MaxWidthScale = 1f;

	public float MinHeightScale = 1f;

	public float MaxHeightScale = 1f;

	public bool Eraser;

	public int EraserHeightUBB = 2;

	public int EraserHeightLBB = -3;

	public int EraserFilterBegin;

	public int EraserFilterEnd = 1023;

	private Vector3 Focus = Vector3.zero;

	private Vector3 Normal = Vector3.zero;

	private Vector2[] TreePositions;

	private int TreeCount;

	private int lastBeginIndex;

	private void Start()
	{
		TreePositions = new Vector2[2048];
		RandOnce();
	}

	private void Update()
	{
		if (BeginIndex != lastBeginIndex && Mathf.Abs(BeginIndex - EndIndex) > 3)
		{
			EndIndex = BeginIndex;
		}
		lastBeginIndex = BeginIndex;
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			Radius *= 1.15f;
			if (Radius > 32f)
			{
				Radius = 32f;
			}
			RandOnce();
		}
		else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			Radius *= 0.82f;
			if (Radius < 1f)
			{
				Radius = 1f;
			}
			RandOnce();
		}
		if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
		{
			Density *= 0.9f;
			RandOnce();
		}
		if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			Density *= 1.18f;
			RandOnce();
		}
		float num = 2f;
		if (Density > 36f)
		{
			Density = 36f;
		}
		if (Density < num)
		{
			Density = num;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hitInfo, 500f, 4096))
		{
			Focus = hitInfo.point;
			Normal = hitInfo.normal;
		}
		else
		{
			Focus = Vector3.zero;
			Normal = Vector3.zero;
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			RandOnce();
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			Eraser = !Eraser;
		}
		bool flag = false;
		for (int i = 0; i < LSubTerrainMgr.Instance.LayerCreators.Length; i++)
		{
			if (LSubTerrainMgr.Instance.LayerCreators[i].bProcessing)
			{
				flag = true;
				break;
			}
		}
		if (!Input.GetKeyDown(KeyCode.N) || flag)
		{
			return;
		}
		if (Eraser)
		{
			for (float num2 = Mathf.Floor(Focus.x - Radius); num2 <= Mathf.Ceil(Focus.x + Radius); num2 += 1f)
			{
				for (float num3 = Mathf.Floor(Focus.z - Radius); num3 <= Mathf.Ceil(Focus.z + Radius); num3 += 1f)
				{
					if (Normal.y > 0f)
					{
						Vector3 origin = new Vector3(num2, Focus.y + Radius, num3);
						if (Physics.Raycast(origin, Vector3.down, out hitInfo, 2f * Radius, 4096) && (hitInfo.point - Focus).magnitude <= Radius + 0.5f)
						{
							for (float num4 = hitInfo.point.y + (float)EraserHeightLBB; num4 <= hitInfo.point.y + (float)EraserHeightUBB; num4 += 1f)
							{
								LSubTerrainMgr.DeleteTreesAtPos(new IntVector3(hitInfo.point.x, num4, hitInfo.point.z), EraserFilterBegin, EraserFilterEnd);
							}
						}
						continue;
					}
					Vector3 origin2 = new Vector3(num2, Focus.y - Radius, num3);
					if (Physics.Raycast(origin2, Vector3.up, out hitInfo, 2f * Radius, 4096) && (hitInfo.point - Focus).magnitude <= Radius + 0.5f)
					{
						for (float num5 = hitInfo.point.y + (float)EraserHeightLBB; num5 <= hitInfo.point.y + (float)EraserHeightUBB; num5 += 1f)
						{
							LSubTerrainMgr.DeleteTreesAtPos(new IntVector3(hitInfo.point.x, num5, hitInfo.point.z), EraserFilterBegin, EraserFilterEnd);
						}
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < TreeCount; j++)
			{
				int prototype = (int)(UnityEngine.Random.value * (Mathf.Abs((float)EndIndex - (float)BeginIndex) + 0.99999f) + (float)Mathf.Min(BeginIndex, EndIndex));
				float num6 = UnityEngine.Random.value * (MaxWidthScale - MinWidthScale) + MinWidthScale;
				float num7 = UnityEngine.Random.value * (MaxHeightScale - MinHeightScale) + MinHeightScale;
				if (num6 < 0.1f)
				{
					num6 = 0.1f;
				}
				if (num6 > 3f)
				{
					num6 = 3f;
				}
				if (num7 < 0.1f)
				{
					num7 = 0.1f;
				}
				if (num7 > 3f)
				{
					num7 = 3f;
				}
				if (Normal.y > 0f)
				{
					Vector3 origin3 = Focus + Vector3.up * Radius;
					origin3.x += TreePositions[j].x;
					origin3.z += TreePositions[j].y;
					if (Physics.Raycast(origin3, Vector3.down, out hitInfo, 2f * Radius, 4096))
					{
						LSubTerrainMgr.AddTree(hitInfo.point, prototype, num6, num7);
					}
				}
				else
				{
					Vector3 origin4 = Focus - Vector3.up * Radius;
					origin4.x += TreePositions[j].x;
					origin4.z += TreePositions[j].y;
					if (Physics.Raycast(origin4, Vector3.up, out hitInfo, 2f * Radius, 4096))
					{
						LSubTerrainMgr.AddTree(hitInfo.point, prototype, num6, num7);
					}
				}
			}
		}
		LSubTerrainMgr.RefreshAllLayerTerrains();
		LSubTerrainMgr.CacheAllNodes();
	}

	private void RandOnce()
	{
		ref Vector2 reference = ref TreePositions[0];
		reference = Vector2.zero;
		TreeCount = 1;
		for (int i = 1; i < 2048; i++)
		{
			bool flag = true;
			int num = 0;
			while (flag && num < 64)
			{
				ref Vector2 reference2 = ref TreePositions[i];
				reference2 = UnityEngine.Random.insideUnitCircle * Radius;
				flag = false;
				for (int j = 0; j < i; j++)
				{
					if ((TreePositions[i] - TreePositions[j]).magnitude < Density)
					{
						flag = true;
						break;
					}
				}
				num++;
			}
			if (num > 60)
			{
				break;
			}
			TreeCount++;
		}
	}

	public void DoGL()
	{
		if (Focus.sqrMagnitude < 1f)
		{
			return;
		}
		Material lineMaterial = PEVCConfig.instance.lineMaterial;
		lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		GL.PushMatrix();
		lineMaterial.SetPass(0);
		for (float num = 0f; num < 359.5f; num += 4f)
		{
			Vector3 focus = Focus;
			focus.x += Radius * Mathf.Cos(num * ((float)Math.PI / 180f));
			focus.z += Radius * Mathf.Sin(num * ((float)Math.PI / 180f));
			Vector3 focus2 = Focus;
			focus2.x += Radius * Mathf.Cos((num + 4f) * ((float)Math.PI / 180f));
			focus2.z += Radius * Mathf.Sin((num + 4f) * ((float)Math.PI / 180f));
			RaycastHit hitInfo;
			if (Normal.y > 0f)
			{
				if (Physics.Raycast(focus + Vector3.up * Radius, Vector3.down, out hitInfo, 2f * Radius, 4096))
				{
					focus = hitInfo.point;
					focus += Vector3.up * 0.1f;
					if (Physics.Raycast(focus2 + Vector3.up * Radius, Vector3.down, out hitInfo, 2f * Radius, 4096))
					{
						focus2 = hitInfo.point;
						focus2 += Vector3.up * 0.1f;
						GL.Begin(1);
						GL.Color((!Eraser) ? new Color(0.2f, 0.5f, 0f, 0.15f) : new Color(0.3f, 0.5f, 1f, 0.15f));
						GL.Vertex3(focus.x, focus.y, focus.z);
						GL.Vertex3(focus2.x, focus2.y, focus2.z);
						GL.End();
						GL.Begin(7);
						GL.Color((!Eraser) ? new Color(0.03f, 0.1f, 0f, 0.15f) : new Color(0f, 0f, 0f, 0f));
						GL.Vertex3(focus.x, focus.y, focus.z);
						GL.Vertex3(focus2.x, focus2.y, focus2.z);
						GL.Vertex3(focus2.x, focus2.y + 0.7f, focus2.z);
						GL.Vertex3(focus.x, focus.y + 0.7f, focus.z);
						GL.End();
					}
				}
			}
			else if (Physics.Raycast(focus + Vector3.down * Radius, Vector3.up, out hitInfo, 2f * Radius, 4096))
			{
				focus = hitInfo.point;
				focus -= Vector3.up * 0.1f;
				if (Physics.Raycast(focus2 + Vector3.down * Radius, Vector3.up, out hitInfo, 2f * Radius, 4096))
				{
					focus2 = hitInfo.point;
					focus2 -= Vector3.up * 0.1f;
					GL.Begin(1);
					GL.Color((!Eraser) ? new Color(0.2f, 0.5f, 0f, 0.15f) : new Color(0.3f, 0.5f, 1f, 0.15f));
					GL.Vertex3(focus.x, focus.y, focus.z);
					GL.Vertex3(focus2.x, focus2.y, focus2.z);
					GL.End();
					GL.Begin(7);
					GL.Color((!Eraser) ? new Color(0.03f, 0.1f, 0f, 0.15f) : new Color(0f, 0f, 0f, 0f));
					GL.Vertex3(focus.x, focus.y, focus.z);
					GL.Vertex3(focus2.x, focus2.y, focus2.z);
					GL.Vertex3(focus2.x, focus2.y - 0.7f, focus2.z);
					GL.Vertex3(focus.x, focus.y - 0.7f, focus.z);
					GL.End();
				}
			}
		}
		if (!Eraser)
		{
			for (int i = 0; i < TreeCount; i++)
			{
				RaycastHit hitInfo2;
				if (Normal.y > 0f)
				{
					Vector3 origin = Focus + Vector3.up * Radius;
					origin.x += TreePositions[i].x;
					origin.z += TreePositions[i].y;
					if (Physics.Raycast(origin, Vector3.down, out hitInfo2, 2f * Radius, 4096))
					{
						Vector3 vector = hitInfo2.point + Vector3.up * 0.3f;
						GL.Begin(1);
						GL.Color(new Color(0.4f, 1f, 0f, 0.5f));
						GL.Vertex3(vector.x - 0.5f, vector.y, vector.z);
						GL.Vertex3(vector.x + 0.5f, vector.y, vector.z);
						GL.Vertex3(vector.x, vector.y, vector.z - 0.5f);
						GL.Vertex3(vector.x, vector.y, vector.z + 0.5f);
						GL.Vertex3(vector.x, vector.y - 0.5f, vector.z);
						GL.Vertex3(vector.x, vector.y + 2f, vector.z);
						GL.Vertex3(vector.x, vector.y + 2f, vector.z);
						GL.Vertex3(vector.x + 0.2f, vector.y + 1.5f, vector.z);
						GL.Vertex3(vector.x, vector.y + 2f, vector.z);
						GL.Vertex3(vector.x - 0.2f, vector.y + 1.5f, vector.z);
						GL.Vertex3(vector.x, vector.y + 2f, vector.z);
						GL.Vertex3(vector.x, vector.y + 1.5f, vector.z + 0.2f);
						GL.Vertex3(vector.x, vector.y + 2f, vector.z);
						GL.Vertex3(vector.x, vector.y + 1.5f, vector.z - 0.2f);
						GL.End();
					}
				}
				else
				{
					Vector3 origin2 = Focus - Vector3.up * Radius;
					origin2.x += TreePositions[i].x;
					origin2.z += TreePositions[i].y;
					if (Physics.Raycast(origin2, Vector3.up, out hitInfo2, 2f * Radius, 4096))
					{
						Vector3 vector2 = hitInfo2.point - Vector3.up * 0.3f;
						GL.Begin(1);
						GL.Color(new Color(0.4f, 1f, 0f, 0.5f));
						GL.Vertex3(vector2.x - 0.5f, vector2.y, vector2.z);
						GL.Vertex3(vector2.x + 0.5f, vector2.y, vector2.z);
						GL.Vertex3(vector2.x, vector2.y, vector2.z - 0.5f);
						GL.Vertex3(vector2.x, vector2.y, vector2.z + 0.5f);
						GL.Vertex3(vector2.x, vector2.y - 2f, vector2.z);
						GL.Vertex3(vector2.x, vector2.y + 0.5f, vector2.z);
						GL.Vertex3(vector2.x, vector2.y - 2f, vector2.z);
						GL.Vertex3(vector2.x + 0.2f, vector2.y - 1.5f, vector2.z);
						GL.Vertex3(vector2.x, vector2.y - 2f, vector2.z);
						GL.Vertex3(vector2.x - 0.2f, vector2.y - 1.5f, vector2.z);
						GL.Vertex3(vector2.x, vector2.y - 2f, vector2.z);
						GL.Vertex3(vector2.x, vector2.y - 1.5f, vector2.z + 0.2f);
						GL.Vertex3(vector2.x, vector2.y - 2f, vector2.z);
						GL.Vertex3(vector2.x, vector2.y - 1.5f, vector2.z - 0.2f);
						GL.End();
					}
				}
			}
		}
		GL.PopMatrix();
	}
}
