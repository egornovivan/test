using UnityEngine;

public class BSGLNearVoxelIndicator : GLBehaviour
{
	public IntVector3 m_Center = InvalidPos;

	public int m_Expand = 4;

	public Gradient m_BoxColors;

	public bool ShowBlock = true;

	public int minVol = 128;

	public static IntVector3 InvalidPos => new IntVector3(-100, -100, -100);

	private void Start()
	{
		GlobalGLs.AddGL(this);
	}

	private void Update()
	{
		BSMath.DrawTarget target;
		if (ShowBlock)
		{
			if (BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay(Input.mousePosition), BuildingMan.Voxels, out target, minVol, ignoreDiagonal: true, BuildingMan.Datas))
			{
				m_Center = target.snapto;
			}
			else
			{
				m_Center = InvalidPos;
			}
		}
		else if (BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay(Input.mousePosition), BuildingMan.Voxels, out target, minVol, true))
		{
			m_Center = target.snapto;
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
		for (int i = -m_Expand; i <= m_Expand; i++)
		{
			for (int j = -m_Expand; j <= m_Expand; j++)
			{
				for (int k = -m_Expand; k <= m_Expand; k++)
				{
					IntVector3 intVector = new IntVector3(i + m_Center.x, j + m_Center.y, k + m_Center.z);
					VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z);
					bool flag = false;
					if (ShowBlock)
					{
						for (float num = (float)intVector.x - 0.5f; num < (float)intVector.x + 0.1f; num += 0.5f)
						{
							for (float num2 = (float)intVector.y - 0.5f; num2 < (float)intVector.y + 0.1f; num2 += 0.5f)
							{
								for (float num3 = (float)intVector.z - 0.5f; num3 < (float)intVector.z + 0.1f; num3 += 0.5f)
								{
									B45Block b45Block = Block45Man.self.DataSource.SafeRead(Mathf.FloorToInt(num * 2f), Mathf.FloorToInt(num2 * 2f), Mathf.FloorToInt(num3 * 2f));
									if (b45Block.blockType != 0 || b45Block.materialType != 0)
									{
										flag = true;
										break;
									}
								}
							}
						}
					}
					Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
					bounds.SetMinMax(new Vector3((float)intVector.x - 0.5f, (float)intVector.y - 0.5f, (float)intVector.z - 0.5f), new Vector3((float)intVector.x + 0.5f, (float)intVector.y + 0.5f, (float)intVector.z + 0.5f));
					float time = (float)Mathf.Max(Mathf.Abs(i), Mathf.Abs(j), Mathf.Abs(k)) / (float)m_Expand;
					Color color = m_BoxColors.Evaluate(time);
					bool flag2 = false;
					if (vFVoxel.Volume > 1)
					{
						Color color2 = m_BoxColors.Evaluate((float)(int)vFVoxel.Volume / 255f);
						color.r = color2.r;
						color.g = color2.g;
						color.b = color2.b;
						flag2 = true;
					}
					else if (flag)
					{
						flag2 = true;
					}
					if (flag2)
					{
						Color c = color;
						color.a *= 1.5f;
						float num4 = 0.04f;
						if (Camera.main != null)
						{
							float magnitude = (Camera.main.transform.position - bounds.min).magnitude;
							float magnitude2 = (Camera.main.transform.position - bounds.max).magnitude;
							magnitude2 = ((!(magnitude2 > magnitude)) ? magnitude : magnitude2);
							num4 = Mathf.Clamp(magnitude2 * 0.001f, 0.04f, 0.2f);
						}
						bounds.min -= new Vector3(num4, num4, num4);
						bounds.max += new Vector3(num4, num4, num4);
						GL.Begin(1);
						GL.Color(color);
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
						GL.End();
						GL.Begin(7);
						GL.Color(c);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.End();
					}
				}
			}
		}
		GL.PopMatrix();
	}
}
