using System;
using UnityEngine;

public class VCEMovingGizmo : GLBehaviour
{
	public delegate void DNotify();

	public delegate void DMovingNotify(Vector3 ofs);

	public VCESelectComponent m_ParentBrush;

	private bool m_MaterialReplaced;

	private float m_VoxelSize;

	public float m_Length = 10f;

	public BoxCollider m_XCollider;

	public BoxCollider m_YCollider;

	public BoxCollider m_ZCollider;

	private bool m_XFocused;

	private bool m_YFocused;

	private bool m_ZFocused;

	private bool m_XDragging;

	private bool m_YDragging;

	private bool m_ZDragging;

	private Vector3 m_LastPos;

	private Vector3 m_NowPos;

	private Vector3 m_MovingOffset;

	private bool m_LastDragging;

	public DNotify OnDragBegin;

	public DNotify OnDrop;

	public DMovingNotify OnMoving;

	public bool Dragging => m_XDragging || m_YDragging || m_ZDragging;

	public Vector3 MovingOffset => (!Dragging) ? Vector3.zero : m_MovingOffset;

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
			m_XCollider.size = new Vector3(m_Length * 2f + 6f, 1.2f, 1.2f);
			m_YCollider.size = new Vector3(1.2f, m_Length * 2f + 6f, 1.2f);
			m_ZCollider.size = new Vector3(1.2f, 1.2f, m_Length * 2f + 6f);
			if (vector.magnitude < m_VoxelSize)
			{
				m_XCollider.gameObject.SetActive(value: false);
				m_YCollider.gameObject.SetActive(value: false);
				m_ZCollider.gameObject.SetActive(value: false);
			}
			else
			{
				vector.Normalize();
				m_XCollider.gameObject.SetActive(Mathf.Abs(vector.x) < 0.93f);
				m_YCollider.gameObject.SetActive(Mathf.Abs(vector.y) < 0.93f);
				m_ZCollider.gameObject.SetActive(Mathf.Abs(vector.z) < 0.93f);
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
				if (flag2 && hitInfo2.distance <= hitInfo.distance && hitInfo2.distance <= hitInfo3.distance)
				{
					m_YFocused = true;
				}
				if (flag && hitInfo3.distance <= hitInfo2.distance && hitInfo3.distance <= hitInfo.distance)
				{
					m_ZFocused = true;
				}
				m_ParentBrush.m_MouseOnGizmo = true;
				if (Input.GetMouseButtonDown(0))
				{
					if (m_XFocused)
					{
						m_XDragging = true;
					}
					if (m_YFocused)
					{
						m_YDragging = true;
					}
					if (m_ZFocused)
					{
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
			m_XCollider.size = new Vector3(m_Length * 2f + 6f, 1.2f, 1.2f);
			m_YCollider.size = new Vector3(1.2f, m_Length * 2f + 6f, 1.2f);
			m_ZCollider.size = new Vector3(1.2f, 1.2f, m_Length * 2f + 6f);
		}
		if (Dragging && !m_LastDragging)
		{
			float num = m_Length * 500f;
			if (m_XDragging)
			{
				if (Mathf.Abs(vector.y) < Mathf.Abs(vector.z))
				{
					m_XCollider.size = new Vector3(num, num, 1.2f);
				}
				else
				{
					m_XCollider.size = new Vector3(num, 1.2f, num);
				}
			}
			else
			{
				m_XCollider.size = new Vector3(m_Length * 2f + 6f, 1.2f, 1.2f);
			}
			if (m_YDragging)
			{
				if (Mathf.Abs(vector.x) < Mathf.Abs(vector.z))
				{
					m_YCollider.size = new Vector3(num, num, 1.2f);
				}
				else
				{
					m_YCollider.size = new Vector3(1.2f, num, num);
				}
			}
			else
			{
				m_YCollider.size = new Vector3(1.2f, m_Length * 2f + 6f, 1.2f);
			}
			if (m_ZDragging)
			{
				if (Mathf.Abs(vector.x) < Mathf.Abs(vector.y))
				{
					m_ZCollider.size = new Vector3(num, 1.2f, num);
				}
				else
				{
					m_ZCollider.size = new Vector3(1.2f, num, num);
				}
			}
			else
			{
				m_ZCollider.size = new Vector3(1.2f, 1.2f, m_Length * 2f + 6f);
			}
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
			RaycastHit hitInfo4 = default(RaycastHit);
			if (m_XDragging && m_XCollider.Raycast(VCEInput.s_PickRay, out hitInfo4, 500f))
			{
				m_NowPos = hitInfo4.point;
			}
			if (m_YDragging && m_YCollider.Raycast(VCEInput.s_PickRay, out hitInfo4, 500f))
			{
				m_NowPos = hitInfo4.point;
			}
			if (m_ZDragging && m_ZCollider.Raycast(VCEInput.s_PickRay, out hitInfo4, 500f))
			{
				m_NowPos = hitInfo4.point;
			}
			if (m_LastDragging)
			{
				m_MovingOffset = m_NowPos - m_LastPos;
				if (!m_XDragging)
				{
					m_MovingOffset.x = 0f;
				}
				if (!m_YDragging)
				{
					m_MovingOffset.y = 0f;
				}
				if (!m_ZDragging)
				{
					m_MovingOffset.z = 0f;
				}
				if (MovingOffset.magnitude > 0f && OnMoving != null)
				{
					OnMoving(MovingOffset);
				}
			}
			else
			{
				m_MovingOffset = Vector3.zero;
			}
			m_LastPos = m_NowPos;
		}
		m_LastDragging = Dragging;
	}

	public override void OnGL()
	{
		float num = m_Length * m_VoxelSize;
		float num2 = (m_Length - 2f) * m_VoxelSize;
		float num3 = (m_Length + 2f) * m_VoxelSize;
		Vector3 normalized = (VCEditor.Instance.m_MainCamera.transform.position - base.transform.position).normalized;
		float num4 = Mathf.Clamp01(1f - Mathf.Pow((Mathf.Abs(normalized.x) - 0.9204f) * 100f, 3f));
		float num5 = Mathf.Clamp01(1f - Mathf.Pow((Mathf.Abs(normalized.y) - 0.9204f) * 100f, 3f));
		float num6 = Mathf.Clamp01(1f - Mathf.Pow((Mathf.Abs(normalized.z) - 0.9204f) * 100f, 3f));
		if (m_XDragging)
		{
			num4 = 1f;
		}
		if (m_YDragging)
		{
			num5 = 1f;
		}
		if (m_ZDragging)
		{
			num6 = 1f;
		}
		for (float num7 = 0f; num7 < 1.99f; num7 += 0.25f)
		{
			float f = num7 * (float)Math.PI;
			Vector3 vector = new Vector3(0f, m_VoxelSize * Mathf.Cos(f), m_VoxelSize * Mathf.Sin(f)) * 1.5f;
			GL.Begin(7);
			GL.Color(Color.white * ((!m_XFocused && !m_XDragging) ? (0.2f * num4) : 0.8f));
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position - base.transform.right * num - vector);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position - base.transform.right * num + vector);
			GL.TexCoord2(1f, 1f);
			GL.Vertex(base.transform.position + base.transform.right * num + vector);
			GL.TexCoord2(1f, 0f);
			GL.Vertex(base.transform.position + base.transform.right * num - vector);
			GL.End();
			GL.Begin(4);
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position - base.transform.right * num2 - vector * 1.2f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position - base.transform.right * num2 + vector * 1f);
			GL.TexCoord2(1f, 0.5f);
			GL.Vertex(base.transform.position - base.transform.right * num3);
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position + base.transform.right * num2 - vector * 1.2f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position + base.transform.right * num2 + vector * 1f);
			GL.TexCoord2(1f, 0.5f);
			GL.Vertex(base.transform.position + base.transform.right * num3);
			GL.End();
		}
		for (float num8 = 0f; num8 < 1.99f; num8 += 0.25f)
		{
			float f2 = num8 * (float)Math.PI;
			Vector3 vector2 = new Vector3(m_VoxelSize * Mathf.Cos(f2), 0f, m_VoxelSize * Mathf.Sin(f2)) * 1.5f;
			GL.Begin(7);
			GL.Color(Color.white * ((!m_YFocused && !m_YDragging) ? (0.2f * num5) : 0.8f));
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position - base.transform.up * num - vector2);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position - base.transform.up * num + vector2);
			GL.TexCoord2(1f, 1f);
			GL.Vertex(base.transform.position + base.transform.up * num + vector2);
			GL.TexCoord2(1f, 0f);
			GL.Vertex(base.transform.position + base.transform.up * num - vector2);
			GL.End();
			GL.Begin(4);
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position - base.transform.up * num2 - vector2 * 1.2f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position - base.transform.up * num2 + vector2 * 1f);
			GL.TexCoord2(1f, 0.5f);
			GL.Vertex(base.transform.position - base.transform.up * num3);
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position + base.transform.up * num2 - vector2 * 1.2f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position + base.transform.up * num2 + vector2 * 1f);
			GL.TexCoord2(1f, 0.5f);
			GL.Vertex(base.transform.position + base.transform.up * num3);
			GL.End();
		}
		for (float num9 = 0f; num9 < 1.99f; num9 += 0.25f)
		{
			float f3 = num9 * (float)Math.PI;
			Vector3 vector3 = new Vector3(m_VoxelSize * Mathf.Cos(f3), m_VoxelSize * Mathf.Sin(f3), 0f) * 1.5f;
			GL.Begin(7);
			GL.Color(Color.white * ((!m_ZFocused && !m_ZDragging) ? (0.2f * num6) : 0.8f));
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position - base.transform.forward * num - vector3);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position - base.transform.forward * num + vector3);
			GL.TexCoord2(1f, 1f);
			GL.Vertex(base.transform.position + base.transform.forward * num + vector3);
			GL.TexCoord2(1f, 0f);
			GL.Vertex(base.transform.position + base.transform.forward * num - vector3);
			GL.End();
			GL.Begin(4);
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position - base.transform.forward * num2 - vector3 * 1.2f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position - base.transform.forward * num2 + vector3 * 1f);
			GL.TexCoord2(1f, 0.5f);
			GL.Vertex(base.transform.position - base.transform.forward * num3);
			GL.TexCoord2(0f, 0f);
			GL.Vertex(base.transform.position + base.transform.forward * num2 - vector3 * 1.2f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex(base.transform.position + base.transform.forward * num2 + vector3 * 1f);
			GL.TexCoord2(1f, 0.5f);
			GL.Vertex(base.transform.position + base.transform.forward * num3);
			GL.End();
		}
	}
}
