using System.Collections.Generic;
using UnityEngine;

public class VCEFreeAirbrush : VCEBrush
{
	public bool m_Eraser;

	public GLAirbrushCursor m_GLCursor;

	private Color m_TargetColor = Color.clear;

	public Color m_UIColor = Color.clear;

	private VCMeshMgr m_MeshMgr;

	private bool m_Drawing;

	private Vector3 m_lastHit;

	private Vector3 m_lastDraw;

	private float m_simDist = 1f;

	private float m_simStep = 1f;

	private float m_drawStep = 1f;

	private List<MeshFilter> m_NeedUpdateMfs = new List<MeshFilter>();

	public float m_Radius = 0.25f;

	public float m_Hardness = 1f;

	public float m_Strength = 1f;

	public static float s_RecentRadius = 4f;

	public static float s_RecentHardness;

	public static float s_RecentStrength = 1f;

	public VCEUIFreeAirbrushInspector m_InspectorRes;

	public VCEUIFreeAirbrushInspector m_Inspector;

	public bool DisplayCursor => m_Drawing || !VCEInput.s_MouseOnUI;

	private void Start()
	{
		m_Radius = s_RecentRadius;
		m_Hardness = s_RecentHardness;
		m_Strength = s_RecentStrength;
		m_GLCursor.m_Airbrush = this;
		m_MeshMgr = VCEditor.Instance.m_MeshMgr;
		m_simDist = VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 0.2f;
		m_lastHit = Vector3.zero;
		m_lastDraw = Vector3.one * -100f;
		m_Inspector = Object.Instantiate(m_InspectorRes);
		m_Inspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_Inspector.transform.localPosition = new Vector3(0f, -10f, 0f);
		m_Inspector.transform.localScale = Vector3.one;
		m_Inspector.m_ParentBrush = this;
		m_Inspector.gameObject.SetActive(value: true);
		if (m_Eraser)
		{
			VCEStatusBar.ShowText("Eraser - Erase color on voxel blocks".ToLocalizationString(), 7f, typeeffect: true);
		}
		else
		{
			VCEStatusBar.ShowText("Airbrush - Paint color on voxel blocks".ToLocalizationString(), 7f, typeeffect: true);
		}
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
		s_RecentRadius = m_Radius;
		s_RecentHardness = m_Hardness;
		s_RecentStrength = m_Strength;
		if (!VCEditor.s_Scene.m_MeshComputer.Computing && m_MeshMgr.m_ColliderDirty)
		{
			m_MeshMgr.PrepareMeshColliders();
		}
		if (m_MeshMgr.m_ColliderDirty)
		{
			return;
		}
		m_TargetColor = ((!m_Eraser) ? VCEditor.SelectedColor : VCIsoData.BLANK_COLOR);
		m_UIColor = m_TargetColor;
		m_UIColor.a = 1f;
		foreach (MeshFilter needUpdateMf in m_NeedUpdateMfs)
		{
			m_MeshMgr.UpdateMeshColor(needUpdateMf);
		}
		m_NeedUpdateMfs.Clear();
		if (m_Drawing)
		{
			if (VCEInput.s_Cancel)
			{
				m_Drawing = false;
				if (m_Action != null)
				{
					m_Action.Undo();
					m_Action = null;
				}
				else
				{
					Debug.LogError("There must be some problem");
				}
				return;
			}
			if (Input.GetMouseButtonUp(0))
			{
				m_Drawing = false;
				if (m_Action != null)
				{
					VCEUpdateColorSign item = new VCEUpdateColorSign(forward: true, back: false);
					m_Action.Modifies.Add(item);
					if (m_Action.Modifies.Count > 2)
					{
						m_Action.Do();
					}
					m_Action = null;
				}
				else
				{
					Debug.LogError("There must be some problem");
				}
				return;
			}
			Vector3 point;
			if (VCEMath.RayCastMesh(VCEInput.s_PickRay, out var rch))
			{
				point = rch.point;
				m_simDist = rch.distance;
			}
			else
			{
				point = VCEInput.s_PickRay.GetPoint(m_simDist);
			}
			Vector3 vector = point - m_lastHit;
			if (!(vector.magnitude > m_simStep))
			{
				return;
			}
			float num = m_simStep / vector.magnitude;
			float num2;
			for (num2 = num; num2 < 0.999f + num; num2 += num)
			{
				num2 = Mathf.Clamp01(num2);
				Vector3 vector2 = m_lastHit + num2 * vector;
				Ray ray = new Ray(VCEInput.s_PickRay.origin, (vector2 - VCEInput.s_PickRay.origin).normalized);
				if (VCEMath.RayCastMesh(ray, out rch))
				{
					m_simDist = rch.distance;
					if ((rch.point - m_lastDraw).magnitude > m_drawStep)
					{
						PaintPosition(rch.point, rch.normal);
						m_lastDraw = rch.point;
						Debug.DrawRay(ray.origin, vector2 - VCEInput.s_PickRay.origin, m_UIColor, 10f);
					}
					else
					{
						Debug.DrawRay(ray.origin, vector2 - VCEInput.s_PickRay.origin, new Color(m_UIColor.r, m_UIColor.g, m_UIColor.b, 0.2f), 10f);
					}
				}
				else
				{
					Debug.DrawRay(ray.origin, vector2 - VCEInput.s_PickRay.origin, new Color(m_UIColor.r, m_UIColor.g, m_UIColor.b, 0.1f), 10f);
				}
			}
			m_lastHit = point;
		}
		else if (VCEInput.s_Cancel)
		{
			Cancel();
		}
		else if (Input.GetMouseButtonDown(0) && !VCEInput.s_MouseOnUI)
		{
			m_Drawing = true;
			if (m_Action != null)
			{
				Debug.LogError("There must be some problem");
				m_Action = null;
			}
			m_Action = new VCEAction();
			VCEUpdateColorSign item2 = new VCEUpdateColorSign(forward: false, back: true);
			m_Action.Modifies.Add(item2);
			m_lastHit = Vector3.zero;
			m_lastDraw = Vector3.one * -100f;
			m_simStep = Mathf.Clamp(m_Radius * 0.07f, 0.2f, 100f) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
			m_drawStep = Mathf.Clamp(m_Radius * 0.5f, 0.2f, 100f) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
			m_simDist = VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 0.2f;
			if (VCEMath.RayCastMesh(VCEInput.s_PickRay, out var rch2))
			{
				PaintPosition(rch2.point, rch2.normal);
				m_lastHit = rch2.point;
				m_lastDraw = rch2.point;
				m_simDist = rch2.distance;
			}
			else
			{
				m_lastHit = VCEInput.s_PickRay.GetPoint(m_simDist);
			}
		}
	}

	private void PaintPosition(Vector3 wpos, Vector3 normal)
	{
		float voxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		VCEditor.s_Mirror.CalcPrepare(voxelSize);
		base.transform.rotation = Quaternion.LookRotation(-normal);
		Vector3 right = base.transform.right;
		Vector3 up = base.transform.up;
		base.transform.rotation = Quaternion.identity;
		float num = Mathf.Clamp(m_Radius, 0.25f, 16f);
		Vector3 vector = wpos + normal * voxelSize;
		for (float num2 = 0f - num; num2 <= num + 0.001f; num2 += 0.25f)
		{
			for (float num3 = 0f - num; num3 <= num + 0.001f; num3 += 0.25f)
			{
				if (!(num2 * num2 + num3 * num3 > num * num))
				{
					float num4 = 1f;
					if (num > 0.75f && m_Hardness < 1f)
					{
						num4 = (1f - Mathf.Sqrt(num2 * num2 + num3 * num3) / num) / (1f - m_Hardness);
						num4 = Mathf.Pow(num4, 2f);
						num4 = Mathf.Clamp01(num4);
					}
					Vector3 origin = vector + (num2 * right + num3 * up) * voxelSize;
					Ray ray = new Ray(origin, -normal);
					if (VCEMath.RayCastMesh(ray, out var rch, voxelSize * 2f))
					{
						Vector3 iso_pos = rch.point / VCEditor.s_Scene.m_Setting.m_VoxelSize;
						MeshFilter component = rch.collider.GetComponent<MeshFilter>();
						ColorPosition(iso_pos, component, num4 * m_Strength);
					}
				}
			}
		}
	}

	private void ColorPosition(Vector3 iso_pos, MeshFilter mf, float strength)
	{
		if (m_Action == null || !mf)
		{
			return;
		}
		int num = VCIsoData.IPosToColorKey(iso_pos);
		int num2 = num & 0x3FF;
		int num3 = (num >> 20) & 0x3FF;
		int num4 = (num >> 10) & 0x3FF;
		if ((num2 % 2 == 1 && num3 % 2 == 1) || (num3 % 2 == 1 && num4 % 2 == 1) || (num4 % 2 == 1 && num2 % 2 == 1))
		{
			return;
		}
		if (VCEditor.s_Mirror.Enabled_Masked)
		{
			IntVector3 ipos = VCIsoData.IPosToColorPos(iso_pos);
			VCEditor.s_Mirror.MirrorColor(ipos);
			for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
			{
				if (!VCEditor.s_Scene.m_IsoData.IsColorPosIn(VCEditor.s_Mirror.Output[i]))
				{
					continue;
				}
				int num5 = VCIsoData.ColorPosToColorKey(VCEditor.s_Mirror.Output[i]);
				Color32 color = VCEditor.s_Scene.m_IsoData.GetColor(num5);
				Color32 new_color = Color32.Lerp(color, m_TargetColor, strength);
				if (color.r != new_color.r || color.g != new_color.g || color.b != new_color.b || color.a != new_color.a)
				{
					VCEAlterColor vCEAlterColor = new VCEAlterColor(num5, color, new_color);
					m_Action.Modifies.Add(vCEAlterColor);
					vCEAlterColor.Redo();
					if (!m_NeedUpdateMfs.Contains(mf))
					{
						m_NeedUpdateMfs.Add(mf);
					}
				}
			}
			return;
		}
		Color32 color2 = VCEditor.s_Scene.m_IsoData.GetColor(num);
		Color32 new_color2 = Color32.Lerp(color2, m_TargetColor, strength);
		if (color2.r != new_color2.r || color2.g != new_color2.g || color2.b != new_color2.b || color2.a != new_color2.a)
		{
			VCEAlterColor vCEAlterColor2 = new VCEAlterColor(num, color2, new_color2);
			m_Action.Modifies.Add(vCEAlterColor2);
			vCEAlterColor2.Redo();
			if (!m_NeedUpdateMfs.Contains(mf))
			{
				m_NeedUpdateMfs.Add(mf);
			}
		}
	}

	protected override void Do()
	{
	}

	public override void Cancel()
	{
		if (VCEditor.DocumentOpen())
		{
			VCEditor.Instance.m_UI.m_DefaultBrush_Color.isChecked = true;
		}
	}
}
