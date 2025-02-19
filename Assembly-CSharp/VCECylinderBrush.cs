using System.Collections.Generic;
using UnityEngine;

public class VCECylinderBrush : VCEFreeSizeBrush
{
	private static ECoordAxis m_Direction = ECoordAxis.Y;

	protected override string BrushDesc()
	{
		return "Cylinder Brush - Draw free size voxel cylinder";
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
				transform.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
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
		for (int i = min.x - 2 * num5; i <= max.x + 2 * num5; i++)
		{
			for (int j = min.y - 2 * num6; j <= max.y + 2 * num6; j++)
			{
				for (int k = min.z - 2 * num7; k <= max.z + 2 * num7; k++)
				{
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
						float num13 = (num11 - vector3.magnitude) / num11;
						float num14 = Mathf.Clamp(num13 * num12 * 127.5f + 127.5f, 0f, 255.49f);
						VCEditor.s_Mirror.MirrorVoxel(intVector);
						for (int l = 0; l < VCEditor.s_Mirror.OutputCnt; l++)
						{
							intVector = VCEditor.s_Mirror.Output[l];
							if (VCEditor.s_Scene.m_IsoData.IsPointIn(intVector))
							{
								int pos = VCIsoData.IPosToKey(intVector);
								VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(pos);
								VCVoxel vCVoxel = 0;
								vCVoxel = ((voxel.Volume == 0) ? new VCVoxel((byte)num14, (byte)VCEditor.SelectedVoxelType) : ((voxel.Volume < 128) ? ((!(num14 < 128f)) ? new VCVoxel((byte)num14, (byte)VCEditor.SelectedVoxelType) : voxel) : ((!(num14 < 128f)) ? new VCVoxel((byte)num14, (byte)VCEditor.SelectedVoxelType) : voxel)));
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
						float num15 = (num11 - vector4.magnitude) / num11;
						float num16 = Mathf.Clamp(num15 * num12 * 127.5f + 127.5f, 0f, 255.49f);
						int pos2 = VCIsoData.IPosToKey(intVector);
						VCVoxel voxel3 = VCEditor.s_Scene.m_IsoData.GetVoxel(pos2);
						VCVoxel vCVoxel2 = 0;
						vCVoxel2 = ((voxel3.Volume == 0) ? new VCVoxel((byte)num16, (byte)VCEditor.SelectedVoxelType) : ((voxel3.Volume < 128) ? ((!(num16 < 128f)) ? new VCVoxel((byte)num16, (byte)VCEditor.SelectedVoxelType) : voxel3) : ((!(num16 < 128f)) ? new VCVoxel((byte)num16, (byte)VCEditor.SelectedVoxelType) : voxel3)));
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
			string text = "Use          to change direction";
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 56f, 100f, 100f), text, "CursorText2");
			GUI.color = new Color(1f, 1f, 0f, 0.9f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 56f, 100f, 100f), "       TAB", "CursorText2");
		}
	}
}
