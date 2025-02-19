using UnityEngine;

public class GLGridPlane : GLBehaviour
{
	public int m_MajorGridInterval = 10;

	public int m_MinorGridInterval = 5;

	public ECoordPlane m_CoordPlane = ECoordPlane.XZ;

	public Vector3 m_CellSize = Vector3.one;

	public IntVector3 m_CellCount = IntVector3.One;

	public Color m_PlaneColor;

	public Color m_MajorLineColor;

	public Color m_MinorLineColor;

	public Color m_CellLineColor;

	public bool m_ShowGrid = true;

	public bool m_Fdisk;

	public GameObject m_DirGroup;

	public GameObject m_DirFront;

	public GameObject m_DirBack;

	public GameObject m_DirLeft;

	public GameObject m_DirRight;

	public Projector m_LaserGrid;

	private void Start()
	{
		if (m_LaserGrid != null)
		{
			m_LaserGrid.material = Object.Instantiate(m_LaserGrid.material);
		}
	}

	private void Update()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		if (component != null)
		{
			Vector3 vector = new Vector3(m_CellSize.x * (float)m_CellCount.x, m_CellSize.y * (float)m_CellCount.y, m_CellSize.z * (float)m_CellCount.z);
			Vector3 center = vector * 0.5f;
			Vector3 size = vector;
			if (m_CoordPlane == ECoordPlane.XY)
			{
				center.z = 0f;
				size.z = m_CellSize.z * 0.1f;
			}
			else if (m_CoordPlane == ECoordPlane.XZ)
			{
				center.y = 0f;
				size.y = m_CellSize.y * 0.1f;
			}
			else if (m_CoordPlane == ECoordPlane.ZY)
			{
				center.x = 0f;
				size.x = m_CellSize.x * 0.1f;
			}
			component.center = center;
			component.size = size;
		}
		if (m_LaserGrid != null)
		{
			m_LaserGrid.orthographicSize = (float)Mathf.Max(m_CellCount.x, m_CellCount.z) * 0.5f * m_CellSize.x;
			m_LaserGrid.nearClipPlane = 0f;
			m_LaserGrid.farClipPlane = (float)m_CellCount.y * m_CellSize.y;
			m_LaserGrid.transform.position = new Vector3(m_CellSize.x * (float)m_CellCount.x * 0.5f, m_CellSize.y * (float)m_CellCount.y, m_CellSize.z * (float)m_CellCount.z * 0.5f);
			m_LaserGrid.material.SetFloat("_MajorGrid", (float)m_MajorGridInterval * m_CellSize.x);
			m_LaserGrid.material.SetFloat("_MinorGrid", (float)m_MinorGridInterval * m_CellSize.x);
			m_LaserGrid.material.SetFloat("_CellSize", m_CellSize.x);
			m_LaserGrid.gameObject.SetActive(VCEditor.Instance.m_UI.m_ShowLaserGrid && VCEditor.Instance.m_UI.m_MaterialTab.isChecked);
		}
		UpdateDirGroup();
	}

	private void OnEnable()
	{
		UpdateDirGroup();
	}

	public void UpdateDirGroup()
	{
		if (m_DirGroup != null)
		{
			m_DirGroup.transform.localScale = m_CellSize;
			m_DirFront.transform.localPosition = new Vector3((float)m_CellCount.x * 0.5f, 0f, m_CellCount.z + 3);
			m_DirBack.transform.localPosition = new Vector3((float)m_CellCount.x * 0.5f, 0f, -3f);
			m_DirLeft.transform.localPosition = new Vector3(-3f, 0f, (float)m_CellCount.z * 0.5f);
			m_DirRight.transform.localPosition = new Vector3(m_CellCount.x + 3, 0f, (float)m_CellCount.z * 0.5f);
			m_DirGroup.SetActive(!VCEditor.Instance.m_UI.m_ISOTab.isChecked);
		}
	}

	public override void OnGL()
	{
		Vector3 vector = new Vector3(m_CellSize.x * (float)m_CellCount.x, m_CellSize.y * (float)m_CellCount.y, m_CellSize.z * (float)m_CellCount.z);
		Vector3 position = base.transform.position;
		Vector3 vector2 = position + vector;
		if (m_Fdisk)
		{
			float num = m_CellSize.x * 5f;
			float num2 = 0.5f;
			GL.Begin(7);
			GL.Color(m_PlaneColor);
			ECoordPlane coordPlane = m_CoordPlane;
			if (coordPlane == ECoordPlane.XZ)
			{
				GL.Vertex3(position.x, position.y, position.z);
				GL.Vertex3(num2 * vector2.x - num, position.y, position.z);
				GL.Vertex3(num2 * vector2.x - num, position.y, vector2.z);
				GL.Vertex3(position.x, position.y, vector2.z);
			}
			GL.End();
			GL.Begin(7);
			GL.Color(m_PlaneColor);
			coordPlane = m_CoordPlane;
			if (coordPlane == ECoordPlane.XZ)
			{
				GL.Vertex3(num2 * vector2.x + num, position.y, position.z);
				GL.Vertex3(vector2.x, position.y, position.z);
				GL.Vertex3(vector2.x, position.y, vector2.z);
				GL.Vertex3(num2 * vector2.x + num, position.y, vector2.z);
			}
			GL.End();
		}
		else
		{
			GL.Begin(7);
			GL.Color(m_PlaneColor);
			switch (m_CoordPlane)
			{
			case ECoordPlane.XZ:
				GL.Vertex3(position.x, position.y, position.z);
				GL.Vertex3(vector2.x, position.y, position.z);
				GL.Vertex3(vector2.x, position.y, vector2.z);
				GL.Vertex3(position.x, position.y, vector2.z);
				break;
			case ECoordPlane.XY:
				GL.Vertex3(position.x, position.y, position.z);
				GL.Vertex3(vector2.x, position.y, position.z);
				GL.Vertex3(vector2.x, vector2.y, position.z);
				GL.Vertex3(position.x, vector2.y, position.z);
				break;
			case ECoordPlane.ZY:
				GL.Vertex3(position.x, position.y, position.z);
				GL.Vertex3(position.x, position.y, vector2.z);
				GL.Vertex3(position.x, vector2.y, vector2.z);
				GL.Vertex3(position.x, vector2.y, position.z);
				break;
			}
			GL.End();
		}
		if (!m_ShowGrid)
		{
			return;
		}
		GL.Begin(1);
		switch (m_CoordPlane)
		{
		case ECoordPlane.XZ:
		{
			if (m_Fdisk)
			{
				float num3 = m_CellSize.x * 5f;
				float num4 = 0.5f;
				for (int k = 0; k <= Mathf.CeilToInt((float)m_CellCount.x * 0.5f - 500f * m_CellSize.x); k++)
				{
					if (k % m_MajorGridInterval == 0)
					{
						GL.Color(m_MajorLineColor);
					}
					else if (k % m_MinorGridInterval == 0)
					{
						GL.Color(m_MinorLineColor);
					}
					else if (Mathf.CeilToInt((float)m_CellCount.x * 0.5f - 500f * m_CellSize.x) == k)
					{
						GL.Color(m_MajorLineColor);
					}
					else
					{
						GL.Color(m_CellLineColor);
					}
					float x = position.x + (float)k * m_CellSize.x;
					GL.Vertex3(x, position.y, position.z);
					GL.Vertex3(x, position.y, vector2.z);
				}
				for (int l = Mathf.CeilToInt((float)m_CellCount.x * 0.5f + 500f * m_CellSize.x); l <= m_CellCount.x; l++)
				{
					if ((l - Mathf.CeilToInt((float)m_CellCount.x * 0.5f + 500f * m_CellSize.x)) % m_MajorGridInterval == 0)
					{
						GL.Color(m_MajorLineColor);
					}
					else if ((l - Mathf.CeilToInt((float)m_CellCount.x * 0.5f + 500f * m_CellSize.x)) % m_MinorGridInterval == 0)
					{
						GL.Color(m_MinorLineColor);
					}
					else
					{
						GL.Color(m_CellLineColor);
					}
					float x2 = position.x + (float)l * m_CellSize.x;
					GL.Vertex3(x2, position.y, position.z);
					GL.Vertex3(x2, position.y, vector2.z);
				}
				for (int m = 0; m <= m_CellCount.z; m++)
				{
					if (m % m_MajorGridInterval == 0)
					{
						GL.Color(m_MajorLineColor);
					}
					else if (m % m_MinorGridInterval == 0)
					{
						GL.Color(m_MinorLineColor);
					}
					else
					{
						GL.Color(m_CellLineColor);
					}
					float z2 = position.z + (float)m * m_CellSize.z;
					GL.Vertex3(position.x, position.y, z2);
					GL.Vertex3(num4 * vector2.x - num3, position.y, z2);
					GL.Vertex3(num4 * vector2.x + num3, position.y, z2);
					GL.Vertex3(vector2.x, position.y, z2);
				}
				break;
			}
			for (int n = 0; n <= m_CellCount.x; n++)
			{
				if (n % m_MajorGridInterval == 0)
				{
					GL.Color(m_MajorLineColor);
				}
				else if (n % m_MinorGridInterval == 0)
				{
					GL.Color(m_MinorLineColor);
				}
				else
				{
					GL.Color(m_CellLineColor);
				}
				float x3 = position.x + (float)n * m_CellSize.x;
				GL.Vertex3(x3, position.y, position.z);
				GL.Vertex3(x3, position.y, vector2.z);
			}
			for (int num5 = 0; num5 <= m_CellCount.z; num5++)
			{
				if (num5 % m_MajorGridInterval == 0)
				{
					GL.Color(m_MajorLineColor);
				}
				else if (num5 % m_MinorGridInterval == 0)
				{
					GL.Color(m_MinorLineColor);
				}
				else
				{
					GL.Color(m_CellLineColor);
				}
				float z3 = position.z + (float)num5 * m_CellSize.z;
				GL.Vertex3(position.x, position.y, z3);
				GL.Vertex3(vector2.x, position.y, z3);
			}
			break;
		}
		case ECoordPlane.XY:
		{
			for (int num6 = 0; num6 <= m_CellCount.x; num6++)
			{
				if (num6 % m_MajorGridInterval == 0)
				{
					GL.Color(m_MajorLineColor);
				}
				else if (num6 % m_MinorGridInterval == 0)
				{
					GL.Color(m_MinorLineColor);
				}
				else
				{
					GL.Color(m_CellLineColor);
				}
				float x4 = position.x + (float)num6 * m_CellSize.x;
				GL.Vertex3(x4, position.y, position.z);
				GL.Vertex3(x4, vector2.y, position.z);
			}
			for (int num7 = 0; num7 <= m_CellCount.y; num7++)
			{
				if (num7 % m_MajorGridInterval == 0)
				{
					GL.Color(m_MajorLineColor);
				}
				else if (num7 % m_MinorGridInterval == 0)
				{
					GL.Color(m_MinorLineColor);
				}
				else
				{
					GL.Color(m_CellLineColor);
				}
				float y2 = position.y + (float)num7 * m_CellSize.y;
				GL.Vertex3(position.x, y2, position.z);
				GL.Vertex3(vector2.x, y2, position.z);
			}
			break;
		}
		case ECoordPlane.ZY:
		{
			for (int i = 0; i <= m_CellCount.z; i++)
			{
				if (i % m_MajorGridInterval == 0)
				{
					GL.Color(m_MajorLineColor);
				}
				else if (i % m_MinorGridInterval == 0)
				{
					GL.Color(m_MinorLineColor);
				}
				else
				{
					GL.Color(m_CellLineColor);
				}
				float z = position.z + (float)i * m_CellSize.z;
				GL.Vertex3(position.x, position.y, z);
				GL.Vertex3(position.x, vector2.y, z);
			}
			for (int j = 0; j <= m_CellCount.y; j++)
			{
				if (j % m_MajorGridInterval == 0)
				{
					GL.Color(m_MajorLineColor);
				}
				else if (j % m_MinorGridInterval == 0)
				{
					GL.Color(m_MinorLineColor);
				}
				else
				{
					GL.Color(m_CellLineColor);
				}
				float y = position.y + (float)j * m_CellSize.y;
				GL.Vertex3(position.x, y, position.z);
				GL.Vertex3(position.x, y, vector2.z);
			}
			break;
		}
		}
		GL.End();
	}
}
