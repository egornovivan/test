using System.Collections.Generic;
using UnityEngine;

public class VCEConeBrush : VCEFreeSizeBrush
{
	public VCEConeGizmoMesh m_ConeGizmo;

	private static ECoordAxis m_Direction = ECoordAxis.Y;

	private static float m_PositiveScale;

	private static float m_NegativeScale = 1f;

	protected override string BrushDesc()
	{
		return "Cone Brush - Draw free size voxel cone";
	}

	protected override void ExtraAdjust()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (m_Direction == ECoordAxis.Y)
			{
				m_Direction = ECoordAxis.X;
			}
			else if (m_Direction == ECoordAxis.Z)
			{
				m_Direction = ECoordAxis.Y;
			}
			else if (m_Direction == ECoordAxis.X)
			{
				m_Direction = ECoordAxis.Z;
			}
			else
			{
				m_Direction = ECoordAxis.Y;
			}
		}
		Transform transform = null;
		if (m_GizmoCube.m_ShapeGizmo != null)
		{
			transform = m_GizmoCube.m_ShapeGizmo.GetChild(0);
		}
		switch (m_Direction)
		{
		case ECoordAxis.X:
			if (transform != null)
			{
				transform.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
			}
			break;
		case ECoordAxis.Y:
			if (transform != null)
			{
				transform.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			}
			break;
		case ECoordAxis.Z:
			if (transform != null)
			{
				transform.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
			}
			break;
		default:
			if (transform != null)
			{
				transform.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			}
			break;
		}
		if ((m_Direction == ECoordAxis.Y && Input.GetKey(KeyCode.UpArrow)) || (m_Direction == ECoordAxis.X && Input.GetKey(VCEInput.s_RightKeyCode)) || (m_Direction == ECoordAxis.Z && Input.GetKey(VCEInput.s_ForwardKeyCode)))
		{
			m_PositiveScale += 0.05f;
			if (m_PositiveScale > 1f)
			{
				m_NegativeScale -= m_PositiveScale - 1f;
				m_PositiveScale = 1f;
			}
			if (m_NegativeScale < 0f)
			{
				m_NegativeScale = 0f;
			}
		}
		if ((m_Direction == ECoordAxis.Y && Input.GetKey(KeyCode.DownArrow)) || (m_Direction == ECoordAxis.X && Input.GetKey(VCEInput.s_LeftKeyCode)) || (m_Direction == ECoordAxis.Z && Input.GetKey(VCEInput.s_BackKeyCode)))
		{
			m_NegativeScale += 0.05f;
			if (m_NegativeScale > 1f)
			{
				m_PositiveScale -= m_NegativeScale - 1f;
				m_NegativeScale = 1f;
			}
			if (m_PositiveScale < 0f)
			{
				m_PositiveScale = 0f;
			}
		}
		m_ConeGizmo.m_NegativeScale = m_NegativeScale;
		m_ConeGizmo.m_PositiveScale = m_PositiveScale;
	}

	protected override void Do()
	{
		m_Action = new VCEAction();
		ulong num = VCEditor.s_Scene.m_IsoData.MaterialGUID(VCEditor.SelectedVoxelType);
		ulong guid = VCEditor.SelectedMaterial.m_Guid;
		if (num != guid)
		{
			VCEAlterMaterialMap item = new VCEAlterMaterialMap(VCEditor.SelectedVoxelType, num, guid);
			m_Action.Modifies.Add(item);
		}
		float num2 = ((m_Direction != ECoordAxis.X) ? 1f : 0f);
		float num3 = ((m_Direction != ECoordAxis.Y) ? 1f : 0f);
		float num4 = ((m_Direction != ECoordAxis.Z) ? 1f : 0f);
		int num5 = ((m_Direction != ECoordAxis.X) ? 1 : 0);
		int num6 = ((m_Direction != ECoordAxis.Y) ? 1 : 0);
		int num7 = ((m_Direction != ECoordAxis.Z) ? 1 : 0);
		float num8 = ((m_Direction != ECoordAxis.X) ? 1f : 1000000f);
		float num9 = ((m_Direction != ECoordAxis.Y) ? 1f : 1000000f);
		float num10 = ((m_Direction != ECoordAxis.Z) ? 1f : 1000000f);
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		IntVector3 min = base.Min;
		IntVector3 max = base.Max;
		Vector3 vector = base.Size.ToVector3() * 0.5f;
		Vector3 vector2 = min.ToVector3() + vector;
		Vector3 one = Vector3.one;
		float num11 = Mathf.Max(vector.x * num2, vector.y * num3, vector.z * num4);
		float num12 = Mathf.Max(Mathf.Min(vector.x * num8, vector.y * num9, vector.z * num10) - 0.5f, 1f);
		one = ((vector.x * num2 >= vector.y * num3 && vector.x * num2 >= vector.z * num4) ? new Vector3(1f, vector.x / vector.y, vector.x / vector.z) : ((!(vector.y * num3 >= vector.x * num2) || !(vector.y * num3 >= vector.z * num4)) ? new Vector3(vector.z / vector.x, vector.z / vector.y, 1f) : new Vector3(vector.y / vector.x, 1f, vector.y / vector.z)));
		float num13 = (float)(max.x - min.x) + 0.5f;
		float num14 = (float)(max.y - min.y) + 0.5f;
		float num15 = (float)(max.z - min.z) + 0.5f;
		for (int i = min.x - 2 * num5; i <= max.x + 2 * num5; i++)
		{
			for (int j = min.y - 2 * num6; j <= max.y + 2 * num6; j++)
			{
				for (int k = min.z - 2 * num7; k <= max.z + 2 * num7; k++)
				{
					float t = 1f;
					if (m_Direction == ECoordAxis.X)
					{
						t = (float)(i - min.x) / num13;
					}
					else if (m_Direction == ECoordAxis.Y)
					{
						t = (float)(j - min.y) / num14;
					}
					else if (m_Direction == ECoordAxis.Z)
					{
						t = (float)(k - min.z) / num15;
					}
					float num16 = Mathf.Lerp(m_NegativeScale, m_PositiveScale, t);
					if (num16 * num11 < 0.01f)
					{
						num16 = 0.01f / num11;
					}
					IntVector3 intVector = new IntVector3(i, j, k);
					if (VCEditor.s_Mirror.Enabled_Masked)
					{
						Vector3 vector3 = intVector.ToVector3() + Vector3.one * 0.5f - vector2;
						vector3.x *= one.x;
						vector3.y *= one.y;
						vector3.z *= one.z;
						vector3.x *= num2;
						vector3.y *= num3;
						vector3.z *= num4;
						float num17 = num16 * num11;
						float num18 = (num17 - vector3.magnitude) / num17;
						float num19 = Mathf.Clamp(num18 * num12 * num16 * 127.5f + 127.5f, 0f, 255.49f);
						VCEditor.s_Mirror.MirrorVoxel(intVector);
						for (int l = 0; l < VCEditor.s_Mirror.OutputCnt; l++)
						{
							intVector = VCEditor.s_Mirror.Output[l];
							if (VCEditor.s_Scene.m_IsoData.IsPointIn(intVector))
							{
								int pos = VCIsoData.IPosToKey(intVector);
								VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(pos);
								VCVoxel vCVoxel = 0;
								vCVoxel = ((voxel.Volume == 0) ? new VCVoxel((byte)num19, (byte)VCEditor.SelectedVoxelType) : ((voxel.Volume < 128) ? ((!(num19 < 128f)) ? new VCVoxel((byte)num19, (byte)VCEditor.SelectedVoxelType) : voxel) : ((!(num19 < 128f)) ? new VCVoxel((byte)num19, (byte)VCEditor.SelectedVoxelType) : voxel)));
								VCVoxel voxel2 = VCEditor.s_Scene.m_Stencil.GetVoxel(pos);
								vCVoxel.Volume = ((vCVoxel.Volume <= voxel2.Volume) ? voxel2.Volume : vCVoxel.Volume);
								VCEditor.s_Scene.m_Stencil.SetVoxel(pos, vCVoxel);
							}
						}
					}
					else if (VCEditor.s_Scene.m_IsoData.IsPointIn(intVector))
					{
						Vector3 vector4 = intVector.ToVector3() + Vector3.one * 0.5f - vector2;
						vector4.x *= one.x;
						vector4.y *= one.y;
						vector4.z *= one.z;
						vector4.x *= num2;
						vector4.y *= num3;
						vector4.z *= num4;
						float num20 = num16 * num11;
						float num21 = (num20 - vector4.magnitude) / num20;
						float num22 = Mathf.Clamp(num21 * num12 * num16 * 127.5f + 127.5f, 0f, 255.49f);
						int pos2 = VCIsoData.IPosToKey(intVector);
						VCVoxel voxel3 = VCEditor.s_Scene.m_IsoData.GetVoxel(pos2);
						VCVoxel vCVoxel2 = 0;
						vCVoxel2 = ((voxel3.Volume == 0) ? new VCVoxel((byte)num22, (byte)VCEditor.SelectedVoxelType) : ((voxel3.Volume < 128) ? ((!(num22 < 128f)) ? new VCVoxel((byte)num22, (byte)VCEditor.SelectedVoxelType) : voxel3) : ((!(num22 < 128f)) ? new VCVoxel((byte)num22, (byte)VCEditor.SelectedVoxelType) : voxel3)));
						VCEditor.s_Scene.m_Stencil.SetVoxel(pos2, vCVoxel2);
					}
				}
			}
		}
		VCEditor.s_Scene.m_Stencil.NormalizeAllVoxels();
		foreach (KeyValuePair<int, VCVoxel> voxel6 in VCEditor.s_Scene.m_Stencil.m_Voxels)
		{
			VCVoxel voxel4 = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel6.Key);
			VCVoxel voxel5 = VCEditor.s_Scene.m_Stencil.GetVoxel(voxel6.Key);
			if ((int)voxel4 != (int)voxel5)
			{
				VCEAlterVoxel item2 = new VCEAlterVoxel(voxel6.Key, voxel4, voxel5);
				m_Action.Modifies.Add(item2);
			}
		}
		VCEditor.s_Scene.m_Stencil.Clear();
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.Do();
		}
		ResetDrawing();
	}

	protected override void ExtraGUI()
	{
		if (m_Phase != 0)
		{
			string text = "Use          to change direction\r\nUse               to change shape";
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 56f, 100f, 100f), text, "CursorText2");
			GUI.color = new Color(1f, 1f, 0f, 0.9f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 56f, 100f, 100f), "       TAB\r\n       Arrows", "CursorText2");
		}
	}
}
