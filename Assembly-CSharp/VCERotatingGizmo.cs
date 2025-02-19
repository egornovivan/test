using System;
using UnityEngine;

public class VCERotatingGizmo : GLBehaviour
{
	public delegate void DNotify();

	public delegate void DRotatingNotify(Vector3 axis, float angle);

	public VCESelectComponent m_ParentBrush;

	public int m_AxisMask = 7;

	private bool m_MaterialReplaced;

	private float m_VoxelSize;

	public float m_Radius = 10f;

	public MeshCollider m_XCollider;

	public MeshCollider m_YCollider;

	public MeshCollider m_ZCollider;

	private bool m_XFocused;

	private bool m_YFocused;

	private bool m_ZFocused;

	private bool m_XDragging;

	private bool m_YDragging;

	private bool m_ZDragging;

	private Vector3 m_DragStartPos;

	private Vector3 m_DragScreenPos;

	private float m_DragStartAngle;

	private Vector3 m_DragIdentityDir;

	private Vector3 m_RotatingOffset;

	private bool m_LastDragging;

	public DNotify OnDragBegin;

	public DNotify OnDrop;

	public DRotatingNotify OnRotating;

	public bool Dragging => m_XDragging || m_YDragging || m_ZDragging;

	public Vector3 RotatingOffset => (!Dragging) ? Vector3.zero : m_RotatingOffset;

	public void ReplaceMat()
	{
		if (!m_MaterialReplaced)
		{
			m_Material = UnityEngine.Object.Instantiate(m_Material);
			m_MaterialReplaced = true;
		}
	}

	private void Start()
	{
		m_VoxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		base.transform.localScale = Vector3.one * m_VoxelSize;
		m_XFocused = false;
		m_YFocused = false;
		m_ZFocused = false;
		m_XDragging = false;
		m_YDragging = false;
		m_ZDragging = false;
		m_LastDragging = false;
	}

	private void OnEnable()
	{
		m_VoxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		base.transform.localScale = Vector3.one * m_VoxelSize;
		m_XFocused = false;
		m_YFocused = false;
		m_ZFocused = false;
		m_XDragging = false;
		m_YDragging = false;
		m_ZDragging = false;
		m_LastDragging = false;
	}

	private void OnDisable()
	{
		m_ParentBrush.m_MouseOnGizmo = false;
		VCECamera.Instance.FreeView();
	}

	private void Update()
	{
		ReplaceMat();
		m_VoxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		Vector3 vector = VCEditor.Instance.m_MainCamera.transform.position - base.transform.position;
		if (!Dragging)
		{
			if (vector.magnitude < m_VoxelSize * m_Radius * 1.2f)
			{
				m_XCollider.gameObject.SetActive(value: false);
				m_YCollider.gameObject.SetActive(value: false);
				m_ZCollider.gameObject.SetActive(value: false);
			}
			else
			{
				m_XCollider.gameObject.SetActive((m_AxisMask & 1) > 0);
				m_YCollider.gameObject.SetActive((m_AxisMask & 2) > 0);
				m_ZCollider.gameObject.SetActive((m_AxisMask & 4) > 0);
			}
			m_XFocused = false;
			m_YFocused = false;
			m_ZFocused = false;
			RaycastHit hitInfo = default(RaycastHit);
			RaycastHit hitInfo2 = default(RaycastHit);
			RaycastHit hitInfo3 = default(RaycastHit);
			bool flag3;
			bool flag2;
			bool flag;
			if (VCEInput.s_MouseOnUI)
			{
				flag3 = (flag2 = (flag = false));
			}
			else
			{
				flag3 = m_XCollider.Raycast(VCEInput.s_PickRay, out hitInfo, 100f);
				flag2 = m_YCollider.Raycast(VCEInput.s_PickRay, out hitInfo2, 100f);
				flag = m_ZCollider.Raycast(VCEInput.s_PickRay, out hitInfo3, 100f);
			}
			if (flag3 || flag2 || flag)
			{
				if (!flag3)
				{
					hitInfo.distance = 100000f;
				}
				if (!flag2)
				{
					hitInfo2.distance = 100000f;
				}
				if (!flag)
				{
					hitInfo3.distance = 100000f;
				}
				if (flag3 && hitInfo.distance <= hitInfo2.distance && hitInfo.distance <= hitInfo3.distance)
				{
					m_XFocused = true;
				}
				else if (flag2 && hitInfo2.distance <= hitInfo.distance && hitInfo2.distance <= hitInfo3.distance)
				{
					m_YFocused = true;
				}
				else if (flag && hitInfo3.distance <= hitInfo2.distance && hitInfo3.distance <= hitInfo.distance)
				{
					m_ZFocused = true;
				}
				m_ParentBrush.m_MouseOnGizmo = true;
				if (Input.GetMouseButtonDown(0))
				{
					if (m_XFocused)
					{
						m_DragStartPos = hitInfo.point;
						m_DragIdentityDir = Vector3.Cross(Vector3.right, m_DragStartPos - base.transform.position).normalized * m_VoxelSize * 3f;
						Vector3 vector2 = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos);
						Vector3 vector3 = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos + m_DragIdentityDir);
						m_DragIdentityDir = (vector3 - vector2).normalized;
						m_DragScreenPos = Input.mousePosition;
						Vector3 vector4 = m_DragStartPos - base.transform.position;
						m_DragStartAngle = Mathf.Atan2(vector4.z, vector4.y) * 57.29578f;
						if (m_DragStartAngle < 0f)
						{
							m_DragStartAngle += 360f;
						}
						m_XDragging = true;
					}
					else if (m_YFocused)
					{
						m_DragStartPos = hitInfo2.point;
						m_DragIdentityDir = Vector3.Cross(Vector3.down, m_DragStartPos - base.transform.position).normalized * m_VoxelSize * 3f;
						Vector3 vector5 = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos);
						Vector3 vector6 = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos + m_DragIdentityDir);
						m_DragIdentityDir = (vector6 - vector5).normalized;
						m_DragScreenPos = Input.mousePosition;
						Vector3 vector7 = m_DragStartPos - base.transform.position;
						m_DragStartAngle = Mathf.Atan2(vector7.z, vector7.x) * 57.29578f;
						if (m_DragStartAngle < 0f)
						{
							m_DragStartAngle += 360f;
						}
						m_YDragging = true;
					}
					else if (m_ZFocused)
					{
						m_DragStartPos = hitInfo3.point;
						m_DragIdentityDir = Vector3.Cross(Vector3.forward, m_DragStartPos - base.transform.position).normalized * m_VoxelSize * 3f;
						Vector3 vector8 = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos);
						Vector3 vector9 = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos + m_DragIdentityDir);
						m_DragIdentityDir = (vector9 - vector8).normalized;
						m_DragScreenPos = Input.mousePosition;
						Vector3 vector10 = m_DragStartPos - base.transform.position;
						m_DragStartAngle = Mathf.Atan2(vector10.y, vector10.x) * 57.29578f;
						if (m_DragStartAngle < 0f)
						{
							m_DragStartAngle += 360f;
						}
						m_ZDragging = true;
					}
				}
			}
			else
			{
				m_ParentBrush.m_MouseOnGizmo = false;
			}
		}
		if (!Input.GetMouseButton(0))
		{
			m_XDragging = false;
			m_YDragging = false;
			m_ZDragging = false;
		}
		if (Dragging && !m_LastDragging)
		{
			if (OnDragBegin != null)
			{
				OnDragBegin();
			}
			VCECamera.Instance.FixView();
		}
		if (!Dragging && m_LastDragging)
		{
			if (OnDrop != null)
			{
				OnDrop();
			}
			VCECamera.Instance.FreeView();
		}
		if (Dragging)
		{
			m_ParentBrush.m_MouseOnGizmo = true;
			Vector3 mousePosition = Input.mousePosition;
			Vector3 lhs = mousePosition - m_DragScreenPos;
			float num = Mathf.RoundToInt(Vector3.Dot(lhs, m_DragIdentityDir) * 0.3f);
			if (m_XDragging)
			{
				m_RotatingOffset = new Vector3(num, 0f, 0f);
			}
			if (m_YDragging)
			{
				m_RotatingOffset = new Vector3(0f, num, 0f);
			}
			if (m_ZDragging)
			{
				m_RotatingOffset = new Vector3(0f, 0f, num);
			}
			if (m_XDragging)
			{
				OnRotating(Vector3.right, m_RotatingOffset.x);
			}
			if (m_YDragging)
			{
				OnRotating(Vector3.up, 0f - m_RotatingOffset.y);
			}
			if (m_ZDragging)
			{
				OnRotating(Vector3.forward, m_RotatingOffset.z);
			}
		}
		m_LastDragging = Dragging;
	}

	private void OnGUI()
	{
		if (!Dragging)
		{
			return;
		}
		GUI.skin = VCEditor.Instance.m_GUISkin;
		GUI.color = new Color(1f, 1f, 1f, 0.7f);
		Vector3 vector = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(base.transform.position);
		if (!(m_RotatingOffset.magnitude > 0.01f))
		{
			return;
		}
		string empty = string.Empty;
		if (m_XDragging)
		{
			empty = m_RotatingOffset.x.ToString("0");
			if (m_RotatingOffset.x > 0f)
			{
				empty = "+" + empty;
			}
			GUI.Label(new Rect(vector.x - 50f, (float)Screen.height - vector.y - 50f, 100f, 100f), empty, "CursorText3");
		}
		if (m_YDragging)
		{
			empty = (0f - m_RotatingOffset.y).ToString("0");
			if (m_RotatingOffset.y < 0f)
			{
				empty = "+" + empty;
			}
			GUI.Label(new Rect(vector.x - 50f, (float)Screen.height - vector.y - 50f, 100f, 100f), empty, "CursorText3");
		}
		if (m_ZDragging)
		{
			empty = m_RotatingOffset.z.ToString("0");
			if (m_RotatingOffset.z > 0f)
			{
				empty = "+" + empty;
			}
			GUI.Label(new Rect(vector.x - 50f, (float)Screen.height - vector.y - 50f, 100f, 100f), empty, "CursorText3");
		}
	}

	public override void OnGL()
	{
		float num = m_Radius * m_VoxelSize;
		float voxelSize = m_VoxelSize;
		float num2 = 5f;
		if ((VCEditor.Instance.m_MainCamera.transform.position - base.transform.position).magnitude < m_VoxelSize * m_Radius * 1.2f)
		{
			return;
		}
		GL.Begin(7);
		if (m_XCollider.gameObject.activeInHierarchy)
		{
			for (float num3 = 0f; num3 < 359.5f; num3 += num2)
			{
				bool flag = false;
				if (m_XDragging)
				{
					float dragStartAngle = m_DragStartAngle;
					float num4 = dragStartAngle + m_RotatingOffset.x;
					while (num4 - dragStartAngle > 360f)
					{
						num4 -= 360f;
					}
					for (; dragStartAngle - num4 > 360f; num4 += 360f)
					{
					}
					float num5 = Mathf.Min(dragStartAngle, num4);
					float num6 = Mathf.Max(dragStartAngle, num4);
					float num7 = num3 + num2 * 0.5f;
					if ((num5 <= num7 && num7 <= num6) || (num5 <= num7 + 360f && num7 + 360f <= num6) || (num5 <= num7 - 360f && num7 - 360f <= num6))
					{
						flag = true;
					}
					if (flag)
					{
						GL.Color(Color.white);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position + num * new Vector3(0f, Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f))));
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position + num * new Vector3(0f, Mathf.Cos((num3 + num2) * ((float)Math.PI / 180f)), Mathf.Sin((num3 + num2) * ((float)Math.PI / 180f))));
					}
				}
				for (float num8 = 0f; num8 < 0.99f; num8 += 0.25f)
				{
					float f = num3 * ((float)Math.PI / 180f);
					float f2 = (num3 + num2) * ((float)Math.PI / 180f);
					float f3 = num8 * (float)Math.PI;
					float num9 = voxelSize * Mathf.Cos(f3);
					float num10 = voxelSize * Mathf.Sin(f3);
					Vector3 v = new Vector3(num10, (num + num9) * Mathf.Cos(f), (num + num9) * Mathf.Sin(f));
					Vector3 v2 = new Vector3(0f - num10, (num - num9) * Mathf.Cos(f), (num - num9) * Mathf.Sin(f));
					Vector3 v3 = new Vector3(num10, (num + num9) * Mathf.Cos(f2), (num + num9) * Mathf.Sin(f2));
					Vector3 v4 = new Vector3(0f - num10, (num - num9) * Mathf.Cos(f2), (num - num9) * Mathf.Sin(f2));
					v += base.transform.position;
					v2 += base.transform.position;
					v3 += base.transform.position;
					v4 += base.transform.position;
					GL.Color(Color.white * ((!m_XFocused && !m_XDragging) ? 0.2f : 0.8f));
					if (m_YDragging || m_ZDragging)
					{
						GL.Color(Color.white * 0.05f);
					}
					GL.TexCoord2((!flag) ? 0f : 0.5f, 0f);
					GL.Vertex(v);
					GL.TexCoord2((!flag) ? 0f : 0.5f, 1f);
					GL.Vertex(v2);
					GL.TexCoord2((!flag) ? 1f : 0.5f, 1f);
					GL.Vertex(v4);
					GL.TexCoord2((!flag) ? 1f : 0.5f, 0f);
					GL.Vertex(v3);
				}
			}
		}
		if (m_YCollider.gameObject.activeInHierarchy)
		{
			for (float num11 = 0f; num11 < 360f; num11 += num2)
			{
				bool flag2 = false;
				if (m_YDragging)
				{
					float dragStartAngle2 = m_DragStartAngle;
					float num12 = dragStartAngle2 + m_RotatingOffset.y;
					while (num12 - dragStartAngle2 > 360f)
					{
						num12 -= 360f;
					}
					for (; dragStartAngle2 - num12 > 360f; num12 += 360f)
					{
					}
					float num13 = Mathf.Min(dragStartAngle2, num12);
					float num14 = Mathf.Max(dragStartAngle2, num12);
					float num15 = num11 + num2 * 0.5f;
					if ((num13 <= num15 && num15 <= num14) || (num13 <= num15 + 360f && num15 + 360f <= num14) || (num13 <= num15 - 360f && num15 - 360f <= num14))
					{
						flag2 = true;
					}
					if (flag2)
					{
						GL.Color(Color.white);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position + num * new Vector3(Mathf.Cos(num11 * ((float)Math.PI / 180f)), 0f, Mathf.Sin(num11 * ((float)Math.PI / 180f))));
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position + num * new Vector3(Mathf.Cos((num11 + num2) * ((float)Math.PI / 180f)), 0f, Mathf.Sin((num11 + num2) * ((float)Math.PI / 180f))));
					}
				}
				for (float num16 = 0f; num16 < 0.99f; num16 += 0.25f)
				{
					float f4 = num11 * ((float)Math.PI / 180f);
					float f5 = (num11 + num2) * ((float)Math.PI / 180f);
					float f6 = num16 * (float)Math.PI;
					float num17 = voxelSize * Mathf.Cos(f6);
					float num18 = voxelSize * Mathf.Sin(f6);
					Vector3 v5 = new Vector3((num + num17) * Mathf.Cos(f4), num18, (num + num17) * Mathf.Sin(f4));
					Vector3 v6 = new Vector3((num - num17) * Mathf.Cos(f4), 0f - num18, (num - num17) * Mathf.Sin(f4));
					Vector3 v7 = new Vector3((num + num17) * Mathf.Cos(f5), num18, (num + num17) * Mathf.Sin(f5));
					Vector3 v8 = new Vector3((num - num17) * Mathf.Cos(f5), 0f - num18, (num - num17) * Mathf.Sin(f5));
					v5 += base.transform.position;
					v6 += base.transform.position;
					v7 += base.transform.position;
					v8 += base.transform.position;
					GL.Color(Color.white * ((!m_YFocused && !m_YDragging) ? 0.2f : 0.8f));
					if (m_XDragging || m_ZDragging)
					{
						GL.Color(Color.white * 0.05f);
					}
					GL.TexCoord2((!flag2) ? 0f : 0.5f, 0f);
					GL.Vertex(v5);
					GL.TexCoord2((!flag2) ? 0f : 0.5f, 1f);
					GL.Vertex(v6);
					GL.TexCoord2((!flag2) ? 1f : 0.5f, 1f);
					GL.Vertex(v8);
					GL.TexCoord2((!flag2) ? 1f : 0.5f, 0f);
					GL.Vertex(v7);
				}
			}
		}
		if (m_ZCollider.gameObject.activeInHierarchy)
		{
			for (float num19 = 0f; num19 < 360f; num19 += num2)
			{
				bool flag3 = false;
				if (m_ZDragging)
				{
					float dragStartAngle3 = m_DragStartAngle;
					float num20 = dragStartAngle3 + m_RotatingOffset.z;
					while (num20 - dragStartAngle3 > 360f)
					{
						num20 -= 360f;
					}
					for (; dragStartAngle3 - num20 > 360f; num20 += 360f)
					{
					}
					float num21 = Mathf.Min(dragStartAngle3, num20);
					float num22 = Mathf.Max(dragStartAngle3, num20);
					float num23 = num19 + num2 * 0.5f;
					if ((num21 <= num23 && num23 <= num22) || (num21 <= num23 + 360f && num23 + 360f <= num22) || (num21 <= num23 - 360f && num23 - 360f <= num22))
					{
						flag3 = true;
					}
					if (flag3)
					{
						GL.Color(Color.white);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position);
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position + num * new Vector3(Mathf.Cos(num19 * ((float)Math.PI / 180f)), Mathf.Sin(num19 * ((float)Math.PI / 180f)), 0f));
						GL.TexCoord2(0.55f, 0.55f);
						GL.Vertex(base.transform.position + num * new Vector3(Mathf.Cos((num19 + num2) * ((float)Math.PI / 180f)), Mathf.Sin((num19 + num2) * ((float)Math.PI / 180f)), 0f));
					}
				}
				for (float num24 = 0f; num24 < 0.99f; num24 += 0.25f)
				{
					float f7 = num19 * ((float)Math.PI / 180f);
					float f8 = (num19 + num2) * ((float)Math.PI / 180f);
					float f9 = num24 * (float)Math.PI;
					float num25 = voxelSize * Mathf.Cos(f9);
					float num26 = voxelSize * Mathf.Sin(f9);
					Vector3 v9 = new Vector3((num + num25) * Mathf.Cos(f7), (num + num25) * Mathf.Sin(f7), num26);
					Vector3 v10 = new Vector3((num - num25) * Mathf.Cos(f7), (num - num25) * Mathf.Sin(f7), 0f - num26);
					Vector3 v11 = new Vector3((num + num25) * Mathf.Cos(f8), (num + num25) * Mathf.Sin(f8), num26);
					Vector3 v12 = new Vector3((num - num25) * Mathf.Cos(f8), (num - num25) * Mathf.Sin(f8), 0f - num26);
					v9 += base.transform.position;
					v10 += base.transform.position;
					v11 += base.transform.position;
					v12 += base.transform.position;
					GL.Color(Color.white * ((!m_ZFocused && !m_ZDragging) ? 0.2f : 0.8f));
					if (m_XDragging || m_YDragging)
					{
						GL.Color(Color.white * 0.05f);
					}
					GL.TexCoord2((!flag3) ? 0f : 0.5f, 0f);
					GL.Vertex(v9);
					GL.TexCoord2((!flag3) ? 0f : 0.5f, 1f);
					GL.Vertex(v10);
					GL.TexCoord2((!flag3) ? 1f : 0.5f, 1f);
					GL.Vertex(v12);
					GL.TexCoord2((!flag3) ? 1f : 0.5f, 0f);
					GL.Vertex(v11);
				}
			}
		}
		GL.End();
	}
}
