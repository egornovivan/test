using System.Collections.Generic;
using UnityEngine;

public class BSGLNearBlockIndicator : GLBehaviour
{
	public Vector3 m_Center = InvalidPos;

	public int m_Expand = 4;

	public Gradient m_BoxColors;

	private Material _LineMaterial;

	public static Vector3 InvalidPos => new Vector3(-100f, -100f, -100f);

	private void Start()
	{
		GlobalGLs.AddGL(this);
	}

	private void Update()
	{
		if (BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay(Input.mousePosition), BuildingMan.Blocks, out var target, 128, ignoreDiagonal: true, BuildingMan.Datas))
		{
			m_Center = new Vector3((float)Mathf.FloorToInt(target.snapto.x * 2f) * 0.5f, (float)Mathf.FloorToInt(target.snapto.y * 2f) * 0.5f, (float)Mathf.FloorToInt(target.snapto.z * 2f) * 0.5f);
		}
		else
		{
			m_Center = InvalidPos;
		}
	}

	private void OnDisable()
	{
		m_Center = InvalidPos;
	}

	public override void OnGL()
	{
		if (!base.enabled || !base.gameObject.activeInHierarchy || BSInput.s_MouseOnUI)
		{
			return;
		}
		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Unlit/Transparent Colored"));
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}
		GL.PushMatrix();
		m_Material.SetPass(0);
		float num = (float)m_Expand * 0.5f;
		List<Bounds> list = new List<Bounds>();
		List<Color> list2 = new List<Color>();
		for (float num2 = 0f - num; num2 <= num; num2 += 0.5f)
		{
			for (float num3 = 0f - num; num3 <= num; num3 += 0.5f)
			{
				for (float num4 = 0f - num; num4 <= num; num4 += 0.5f)
				{
					Vector3 vector = new Vector3(num2 + m_Center.x, num3 + m_Center.y, num4 + m_Center.z);
					B45Block b45Block = Block45Man.self.DataSource.SafeRead(Mathf.FloorToInt(vector.x * 2f), Mathf.FloorToInt(vector.y * 2f), Mathf.FloorToInt(vector.z * 2f));
					bool flag = false;
					if (b45Block.blockType != 0 || b45Block.materialType != 0)
					{
						flag = true;
					}
					VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.SafeRead(Mathf.FloorToInt(vector.x + 0.5f), Mathf.FloorToInt(vector.y + 0.5f), Mathf.FloorToInt(vector.z + 0.5f));
					Bounds item = new Bounds(Vector3.zero, Vector3.zero);
					float time = Mathf.Max(Mathf.Abs(num2), Mathf.Abs(num3), Mathf.Abs(num4)) / num;
					Color item2 = m_BoxColors.Evaluate(time);
					if (vFVoxel.Volume > 1)
					{
						item.SetMinMax(new Vector3(vector.x, vector.y, vector.z), new Vector3(vector.x + 0.5f, vector.y + 0.5f, vector.z + 0.5f));
						float num5 = 0.02f;
						if (Camera.main != null)
						{
							float magnitude = (Camera.main.transform.position - item.min).magnitude;
							float magnitude2 = (Camera.main.transform.position - item.max).magnitude;
							magnitude2 = ((!(magnitude2 > magnitude)) ? magnitude : magnitude2);
							num5 = Mathf.Clamp(magnitude2 * 0.002f, 0.02f, 0.1f);
						}
						item.min -= new Vector3(num5, num5, num5);
						item.max += new Vector3(num5, num5, num5);
						Color color = m_BoxColors.Evaluate((float)(int)vFVoxel.Volume / 255f);
						item2.r = color.r;
						item2.g = color.g;
						item2.b = color.b;
						list.Add(item);
						list2.Add(item2);
					}
					else if (flag)
					{
						item.SetMinMax(new Vector3(vector.x, vector.y, vector.z), new Vector3(vector.x + 0.5f, vector.y + 0.5f, vector.z + 0.5f));
						float num6 = 0.02f;
						if (Camera.main != null)
						{
							float magnitude3 = (Camera.main.transform.position - item.min).magnitude;
							float magnitude4 = (Camera.main.transform.position - item.max).magnitude;
							magnitude4 = ((!(magnitude4 > magnitude3)) ? magnitude3 : magnitude4);
							num6 = Mathf.Clamp(magnitude4 * 0.002f, 0.02f, 0.1f);
						}
						item.min -= new Vector3(num6, num6, num6);
						item.max += new Vector3(num6, num6, num6);
						list.Add(item);
						list2.Add(item2);
					}
				}
			}
		}
		GL.Begin(1);
		for (int i = 0; i < list.Count; i++)
		{
			Color color2 = list2[i];
			color2.a *= 1.5f;
			if (color2.a > 0.01f)
			{
				Bounds bounds = list[i];
				GL.Color(list2[i]);
				GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
				GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
				GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
				GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
				GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
				GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
				GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
				GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
				GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
				GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
				GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
				GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
				GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
				GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
				GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
				GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
				GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
				GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
				GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
				GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
				GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
				GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
				GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
				GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
			}
		}
		GL.End();
		GL.Begin(7);
		for (int j = 0; j < list.Count; j++)
		{
			if (list2[j].a > 0.01f)
			{
				Bounds bounds2 = list[j];
				GL.Color(list2[j]);
				GL.Vertex3(bounds2.min.x, bounds2.min.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.min.y, bounds2.max.z);
				GL.Vertex3(bounds2.min.x, bounds2.max.y, bounds2.max.z);
				GL.Vertex3(bounds2.min.x, bounds2.max.y, bounds2.min.z);
				GL.Vertex3(bounds2.max.x, bounds2.min.y, bounds2.min.z);
				GL.Vertex3(bounds2.max.x, bounds2.min.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.max.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.max.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.min.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.min.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.min.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.min.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.max.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.max.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.max.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.max.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.min.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.max.y, bounds2.min.z);
				GL.Vertex3(bounds2.max.x, bounds2.max.y, bounds2.min.z);
				GL.Vertex3(bounds2.max.x, bounds2.min.y, bounds2.min.z);
				GL.Vertex3(bounds2.min.x, bounds2.min.y, bounds2.max.z);
				GL.Vertex3(bounds2.min.x, bounds2.max.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.max.y, bounds2.max.z);
				GL.Vertex3(bounds2.max.x, bounds2.min.y, bounds2.max.z);
			}
		}
		GL.End();
		GL.PopMatrix();
	}
}
