using UnityEngine;

public class VCESelectMethod_Box : VCESelectMethod
{
	private IntVector3 m_Begin = new IntVector3(-1, -1, -1);

	private IntVector3 m_End = new IntVector3(-1, -1, -1);

	private float m_PlanePos = -1f;

	private ECoordPlane m_Coord = ECoordPlane.XZ;

	private int m_NormalSign;

	public int m_Depth = 1;

	public bool m_PlaneFeather = true;

	public int m_FeatherLength;

	public bool m_MaterialSelect;

	public bool m_MaterialSelectChange;

	public static int s_RecentDepth = 1;

	public static bool s_RecentPlaneFeather = true;

	public static int s_RecentFeatherLength;

	public static bool s_RecentMaterialSelect;

	private VCEUIBoxMethodInspector m_Inspector;

	private float m_GUIAlpha = 1f;

	private void Start()
	{
		m_Depth = s_RecentDepth;
		m_FeatherLength = s_RecentFeatherLength;
		m_PlaneFeather = s_RecentPlaneFeather;
		m_MaterialSelect = s_RecentMaterialSelect;
		m_Inspector = Object.Instantiate(m_Parent.m_BoxMethodInspectorRes);
		m_Inspector.gameObject.SetActive(value: false);
		m_Inspector.transform.parent = m_Parent.m_MainInspector.transform;
		m_Inspector.transform.localPosition = new Vector3(0f, -66f, 0f);
		m_Inspector.transform.localScale = Vector3.one;
		m_Inspector.m_SelectMethod = this;
		m_Inspector.gameObject.SetActive(value: true);
	}

	private void OnDestroy()
	{
		if (m_Inspector != null)
		{
			Object.Destroy(m_Inspector.gameObject);
			m_Inspector = null;
		}
	}

	private void Update()
	{
		s_RecentDepth = m_Depth;
		s_RecentFeatherLength = m_FeatherLength;
		s_RecentMaterialSelect = m_MaterialSelect;
		s_RecentPlaneFeather = m_PlaneFeather;
		VCEditor.Instance.m_NearVoxelIndicator.enabled = !m_Selecting;
		m_GUIAlpha = Mathf.Lerp(m_GUIAlpha, 0f, 0.05f);
	}

	public override void MainMethod()
	{
		if (VCEInput.s_Increase && !VCEInput.s_Shift)
		{
			if (m_Depth < 300)
			{
				m_Depth++;
			}
			m_GUIAlpha = 5f;
		}
		else if (VCEInput.s_Decrease && !VCEInput.s_Shift)
		{
			if (m_Depth > 1)
			{
				m_Depth--;
			}
			m_GUIAlpha = 5f;
		}
		if (m_Selecting)
		{
			RaycastHit rch;
			if (VCEInput.s_Cancel)
			{
				ExitSelecting();
			}
			else if (VCEMath.RayCastCoordPlane(VCEInput.s_PickRay, m_Coord, m_PlanePos, out rch))
			{
				m_End.x = Mathf.FloorToInt(rch.point.x);
				m_End.y = Mathf.FloorToInt(rch.point.y);
				m_End.z = Mathf.FloorToInt(rch.point.z);
				switch (m_Coord)
				{
				case ECoordPlane.XY:
					m_End.z = m_Begin.z + (m_Depth - 1) * m_NormalSign;
					break;
				case ECoordPlane.XZ:
					m_End.y = ((m_Begin.y != 0 || VCEditor.s_Scene.m_IsoData.GetVoxel(VCIsoData.IPosToKey(m_Begin)).Volume >= 128) ? (m_Begin.y + (m_Depth - 1) * m_NormalSign) : (VCEditor.s_Scene.m_Setting.m_EditorSize.y - 1));
					break;
				case ECoordPlane.ZY:
					m_End.x = m_Begin.x + (m_Depth - 1) * m_NormalSign;
					break;
				default:
					ExitSelecting();
					return;
				}
				m_Iso.ClampPointI(m_Begin);
				m_Iso.ClampPointI(m_End);
				if (Input.GetMouseButtonUp(0) && m_Selecting)
				{
					Submit();
					m_Selecting = false;
					m_NeedUpdate = true;
				}
			}
			else
			{
				ExitSelecting();
			}
			return;
		}
		if (!VCEInput.s_MouseOnUI && Input.GetMouseButtonDown(0))
		{
			if (m_Parent.m_Target.snapto != null && m_Parent.m_Target.cursor != null)
			{
				VCEMath.DrawTarget target = m_Parent.m_Target;
				if (m_Iso.GetVoxel(VCIsoData.IPosToKey(target.snapto)).Volume > 0)
				{
					m_Begin.x = target.snapto.x;
					m_Begin.y = target.snapto.y;
					m_Begin.z = target.snapto.z;
				}
				else
				{
					m_Begin.x = target.cursor.x;
					m_Begin.y = target.cursor.y;
					m_Begin.z = target.cursor.z;
					target.rch.normal = -target.rch.normal;
				}
				m_End.x = m_Begin.x;
				m_End.y = m_Begin.y;
				m_End.z = m_Begin.z;
				if (Mathf.Abs(target.rch.normal.x) > 0.9f)
				{
					m_Coord = ECoordPlane.ZY;
					m_PlanePos = target.rch.point.x;
					m_Selecting = true;
					m_NormalSign = -Mathf.RoundToInt(Mathf.Sign(target.rch.normal.x));
				}
				else if (Mathf.Abs(target.rch.normal.y) > 0.9f)
				{
					m_Coord = ECoordPlane.XZ;
					m_PlanePos = target.rch.point.y;
					m_Selecting = true;
					m_NormalSign = -Mathf.RoundToInt(Mathf.Sign(target.rch.normal.y));
				}
				else if (Mathf.Abs(target.rch.normal.z) > 0.9f)
				{
					m_Coord = ECoordPlane.XY;
					m_PlanePos = target.rch.point.z;
					m_Selecting = true;
					m_NormalSign = -Mathf.RoundToInt(Mathf.Sign(target.rch.normal.z));
				}
				else
				{
					Debug.LogError("It's impossible !!");
					ExitSelecting();
				}
			}
			else
			{
				if (!VCEInput.s_Shift && !VCEInput.s_Alt && !VCEInput.s_Control)
				{
					m_Selection.Clear();
				}
				m_NeedUpdate = true;
			}
		}
		if (m_MaterialSelectChange)
		{
			Submit();
			m_Selecting = false;
			m_NeedUpdate = true;
		}
	}

	protected override void Submit()
	{
		m_Iso.ClampPointI(m_Begin);
		m_Iso.ClampPointI(m_End);
		IntVector3 intVector = new IntVector3();
		IntVector3 intVector2 = new IntVector3();
		intVector.x = Mathf.Min(m_Begin.x, m_End.x);
		intVector.y = Mathf.Min(m_Begin.y, m_End.y);
		intVector.z = Mathf.Min(m_Begin.z, m_End.z);
		intVector2.x = Mathf.Max(m_Begin.x, m_End.x);
		intVector2.y = Mathf.Max(m_Begin.y, m_End.y);
		intVector2.z = Mathf.Max(m_Begin.z, m_End.z);
		IntVector3 intVector3 = new IntVector3();
		IntVector3 intVector4 = new IntVector3();
		intVector3.x = intVector.x - m_FeatherLength;
		intVector3.y = intVector.y - m_FeatherLength;
		intVector3.z = intVector.z - m_FeatherLength;
		intVector4.x = intVector2.x + m_FeatherLength;
		intVector4.y = intVector2.y + m_FeatherLength;
		intVector4.z = intVector2.z + m_FeatherLength;
		if (m_PlaneFeather)
		{
			switch (m_Coord)
			{
			case ECoordPlane.XY:
				intVector3.z = intVector.z;
				intVector4.z = intVector2.z;
				break;
			case ECoordPlane.XZ:
				intVector3.y = intVector.y;
				intVector4.y = intVector2.y;
				break;
			case ECoordPlane.ZY:
				intVector3.x = intVector.x;
				intVector4.x = intVector2.x;
				break;
			}
		}
		m_Iso.ClampPointI(intVector3);
		m_Iso.ClampPointI(intVector4);
		if (!VCEInput.s_Shift && !VCEInput.s_Alt && !VCEInput.s_Control)
		{
			m_Selection.Clear();
		}
		for (int i = intVector3.x; i <= intVector4.x; i++)
		{
			for (int j = intVector3.y; j <= intVector4.y; j++)
			{
				for (int k = intVector3.z; k <= intVector4.z; k++)
				{
					int num = VCIsoData.IPosToKey(i, j, k);
					if (m_Iso.GetVoxel(num).Volume < 1)
					{
						continue;
					}
					int num2 = 0;
					int num3 = ((m_FeatherLength != 0) ? ((int)(VCEMath.BoxFeather(new IntVector3(i, j, k), intVector, intVector2, m_FeatherLength) * 255f)) : 255);
					int num4 = 0;
					if (num3 < 1)
					{
						continue;
					}
					if (m_Selection.ContainsKey(num))
					{
						num2 = m_Selection[num];
					}
					num4 = (VCEInput.s_Shift ? (num2 + num3) : (VCEInput.s_Alt ? (num2 - num3) : ((!VCEInput.s_Control) ? num3 : Mathf.Abs(num2 - num3))));
					num4 = Mathf.Clamp(num4, 0, 255);
					if (num4 < 1)
					{
						m_Selection.Remove(num);
					}
					else if (num2 < 1)
					{
						m_Selection.Add(num, (byte)num4);
					}
					else
					{
						m_Selection[num] = (byte)num4;
					}
					if (m_MaterialSelect)
					{
						VCVoxel voxel = m_Iso.GetVoxel(num);
						if (m_Iso.m_Materials[voxel.Type] != VCEditor.SelectedMaterial)
						{
							m_Selection.Remove(num);
						}
					}
				}
			}
		}
	}

	private void ExitSelecting()
	{
		m_Begin.x = -1;
		m_Begin.y = -1;
		m_Begin.z = -1;
		m_End.x = -1;
		m_End.y = -1;
		m_End.z = -1;
		m_Coord = ECoordPlane.XZ;
		m_PlanePos = 0f;
		m_NormalSign = 0;
		m_Selecting = false;
	}

	public IntVector3 SelectingSize()
	{
		if (!m_Selecting)
		{
			return new IntVector3(0, 0, m_Depth);
		}
		IntVector3 zero = IntVector3.Zero;
		IntVector3 zero2 = IntVector3.Zero;
		zero.x = Mathf.Min(m_Begin.x, m_End.x);
		zero.y = Mathf.Min(m_Begin.y, m_End.y);
		zero.z = Mathf.Min(m_Begin.z, m_End.z);
		zero2.x = Mathf.Max(m_Begin.x, m_End.x);
		zero2.y = Mathf.Max(m_Begin.y, m_End.y);
		zero2.z = Mathf.Max(m_Begin.z, m_End.z);
		return m_Coord switch
		{
			ECoordPlane.ZY => new IntVector3(zero2.z - zero.z + 1, zero2.y - zero.y + 1, m_Depth), 
			ECoordPlane.XZ => new IntVector3(zero2.x - zero.x + 1, zero2.z - zero.z + 1, m_Depth), 
			ECoordPlane.XY => new IntVector3(zero2.x - zero.x + 1, zero2.y - zero.y + 1, m_Depth), 
			_ => new IntVector3(0, 0, m_Depth), 
		};
	}

	private void OnGUI()
	{
		if (!VCEInput.s_MouseOnUI)
		{
			GUI.skin = VCEditor.Instance.m_GUISkin;
			GUI.color = Color.white;
			if (m_Selecting)
			{
				GUI.color = Color.white;
			}
			else
			{
				GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(m_GUIAlpha));
			}
			if (m_Depth > 1)
			{
				GUI.Label(new Rect(Input.mousePosition.x - 105f, (float)Screen.height - Input.mousePosition.y - 50f, 100f, 100f), "Depth x " + m_Depth, "CursorText1");
			}
			else if (m_Begin.y > 0)
			{
				GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 26f, 100f, 100f), "Use [Up]/[Down] arrow to set depth", "CursorText1");
			}
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			if (VCEInput.s_Shift)
			{
				GUI.Label(new Rect(Input.mousePosition.x - 105f, (float)Screen.height - Input.mousePosition.y - 75f, 100f, 100f), "ADD", "CursorText1");
			}
			else if (VCEInput.s_Alt)
			{
				GUI.Label(new Rect(Input.mousePosition.x - 105f, (float)Screen.height - Input.mousePosition.y - 75f, 100f, 100f), "SUBTRACT", "CursorText1");
			}
			else if (VCEInput.s_Control)
			{
				GUI.Label(new Rect(Input.mousePosition.x - 105f, (float)Screen.height - Input.mousePosition.y - 75f, 100f, 100f), "CROSS", "CursorText1");
			}
		}
	}

	public override void OnGL()
	{
		if (m_Selecting)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			zero.x = Mathf.Min(m_Begin.x, m_End.x);
			zero.y = Mathf.Min(m_Begin.y, m_End.y);
			zero.z = Mathf.Min(m_Begin.z, m_End.z);
			zero2.x = Mathf.Max(m_Begin.x, m_End.x);
			zero2.y = Mathf.Max(m_Begin.y, m_End.y);
			zero2.z = Mathf.Max(m_Begin.z, m_End.z);
			zero -= Vector3.one * 0.07f;
			zero2 += Vector3.one * 1.07f;
			Vector3[] array = new Vector3[8]
			{
				new Vector3(zero2.x, zero2.y, zero2.z),
				new Vector3(zero.x, zero2.y, zero2.z),
				new Vector3(zero.x, zero.y, zero2.z),
				new Vector3(zero2.x, zero.y, zero2.z),
				new Vector3(zero2.x, zero2.y, zero.z),
				new Vector3(zero.x, zero2.y, zero.z),
				new Vector3(zero.x, zero.y, zero.z),
				new Vector3(zero2.x, zero.y, zero.z)
			};
			for (int i = 0; i < 8; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = array[i] * VCEditor.s_Scene.m_Setting.m_VoxelSize;
			}
			Color c = new Color(0f, 0.2f, 0.5f, 1f);
			Color c2 = new Color(0f, 0.2f, 0.5f, 1f);
			c.a = 1f;
			c2.a *= 0.4f + Mathf.Sin(Time.time * 6f) * 0.1f;
			GL.Begin(1);
			GL.Color(c);
			GL.Vertex(array[0]);
			GL.Vertex(array[1]);
			GL.Vertex(array[1]);
			GL.Vertex(array[2]);
			GL.Vertex(array[2]);
			GL.Vertex(array[3]);
			GL.Vertex(array[3]);
			GL.Vertex(array[0]);
			GL.Vertex(array[4]);
			GL.Vertex(array[5]);
			GL.Vertex(array[5]);
			GL.Vertex(array[6]);
			GL.Vertex(array[6]);
			GL.Vertex(array[7]);
			GL.Vertex(array[7]);
			GL.Vertex(array[4]);
			GL.Vertex(array[0]);
			GL.Vertex(array[4]);
			GL.Vertex(array[1]);
			GL.Vertex(array[5]);
			GL.Vertex(array[2]);
			GL.Vertex(array[6]);
			GL.Vertex(array[3]);
			GL.Vertex(array[7]);
			GL.End();
			GL.Begin(7);
			GL.Color(c2);
			GL.Vertex(array[0]);
			GL.Vertex(array[1]);
			GL.Vertex(array[2]);
			GL.Vertex(array[3]);
			GL.Vertex(array[4]);
			GL.Vertex(array[5]);
			GL.Vertex(array[6]);
			GL.Vertex(array[7]);
			GL.Vertex(array[0]);
			GL.Vertex(array[4]);
			GL.Vertex(array[5]);
			GL.Vertex(array[1]);
			GL.Vertex(array[1]);
			GL.Vertex(array[5]);
			GL.Vertex(array[6]);
			GL.Vertex(array[2]);
			GL.Vertex(array[2]);
			GL.Vertex(array[6]);
			GL.Vertex(array[7]);
			GL.Vertex(array[3]);
			GL.Vertex(array[3]);
			GL.Vertex(array[7]);
			GL.Vertex(array[4]);
			GL.Vertex(array[0]);
			GL.End();
		}
		else
		{
			if (VCEInput.s_MouseOnUI)
			{
				return;
			}
			VCEMath.DrawTarget target = m_Parent.m_Target;
			if (target.cursor != null && target.snapto != null && m_Iso.GetVoxel(VCIsoData.IPosToKey(target.snapto)).Volume != 0)
			{
				Vector3 point = target.rch.point;
				Vector3 point2 = target.rch.point;
				point += target.rch.normal * 0.03f;
				point2 += target.rch.normal * 0.06f;
				Color color = Color.white;
				if (Mathf.Abs(target.rch.normal.x) > 0.9f)
				{
					point.y = Mathf.Floor(point.y);
					point.z = Mathf.Floor(point.z);
					point2.y = Mathf.Floor(point2.y) + 1f;
					point2.z = Mathf.Floor(point2.z) + 1f;
					color = new Color(0.9f, 0.1f, 0.2f, 1f);
				}
				else if (Mathf.Abs(target.rch.normal.y) > 0.9f)
				{
					point.x = Mathf.Floor(point.x);
					point.z = Mathf.Floor(point.z);
					point2.x = Mathf.Floor(point2.x) + 1f;
					point2.z = Mathf.Floor(point2.z) + 1f;
					color = new Color(0.5f, 1f, 0.1f, 1f);
				}
				else if (Mathf.Abs(target.rch.normal.z) > 0.9f)
				{
					point.y = Mathf.Floor(point.y);
					point.x = Mathf.Floor(point.x);
					point2.y = Mathf.Floor(point2.y) + 1f;
					point2.x = Mathf.Floor(point2.x) + 1f;
					color = new Color(0.1f, 0.6f, 1f, 1f);
				}
				Vector3[] array2 = new Vector3[8]
				{
					new Vector3(point2.x, point2.y, point2.z),
					new Vector3(point.x, point2.y, point2.z),
					new Vector3(point.x, point.y, point2.z),
					new Vector3(point2.x, point.y, point2.z),
					new Vector3(point2.x, point2.y, point.z),
					new Vector3(point.x, point2.y, point.z),
					new Vector3(point.x, point.y, point.z),
					new Vector3(point2.x, point.y, point.z)
				};
				for (int j = 0; j < 8; j++)
				{
					ref Vector3 reference2 = ref array2[j];
					reference2 = array2[j] * VCEditor.s_Scene.m_Setting.m_VoxelSize;
				}
				Color c3 = color;
				Color c4 = color;
				c3.a = 1f;
				c4.a *= 0.7f + Mathf.Sin(Time.time * 6f) * 0.1f;
				GL.Begin(1);
				GL.Color(c3);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[7]);
				GL.End();
				GL.Begin(7);
				GL.Color(c4);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[0]);
				GL.End();
			}
		}
	}
}
