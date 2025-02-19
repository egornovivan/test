using System.Collections.Generic;
using UnityEngine;

public class VCESphereBrush : VCEFreeSizeBrush
{
	protected override string BrushDesc()
	{
		return "Sphere Brush - Draw free size voxel sphere or ellipsoid";
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
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		IntVector3 min = base.Min;
		IntVector3 max = base.Max;
		Vector3 vector = base.Size.ToVector3() * 0.5f;
		Vector3 vector2 = min.ToVector3() + vector;
		Vector3 one = Vector3.one;
		float num2 = Mathf.Max(vector.x, vector.y, vector.z);
		float num3 = Mathf.Max(Mathf.Min(vector.x, vector.y, vector.z) - 0.5f, 1f);
		one = ((vector.x >= vector.y && vector.x >= vector.z) ? new Vector3(1f, vector.x / vector.y, vector.x / vector.z) : ((!(vector.y >= vector.x) || !(vector.y >= vector.z)) ? new Vector3(vector.z / vector.x, vector.z / vector.y, 1f) : new Vector3(vector.y / vector.x, 1f, vector.y / vector.z)));
		for (int i = min.x - 2; i <= max.x + 2; i++)
		{
			for (int j = min.y - 2; j <= max.y + 2; j++)
			{
				for (int k = min.z - 2; k <= max.z + 2; k++)
				{
					IntVector3 intVector = new IntVector3(i, j, k);
					if (VCEditor.s_Mirror.Enabled_Masked)
					{
						Vector3 vector3 = intVector.ToVector3() + Vector3.one * 0.5f - vector2;
						vector3.x *= one.x;
						vector3.y *= one.y;
						vector3.z *= one.z;
						float num4 = (num2 - vector3.magnitude) / num2;
						float num5 = Mathf.Clamp(num4 * num3 * 127.5f + 127.5f, 0f, 255.49f);
						VCEditor.s_Mirror.MirrorVoxel(intVector);
						for (int l = 0; l < VCEditor.s_Mirror.OutputCnt; l++)
						{
							intVector = VCEditor.s_Mirror.Output[l];
							if (VCEditor.s_Scene.m_IsoData.IsPointIn(intVector))
							{
								int pos = VCIsoData.IPosToKey(intVector);
								VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(pos);
								VCVoxel vCVoxel = 0;
								vCVoxel = ((voxel.Volume == 0) ? new VCVoxel((byte)num5, (byte)VCEditor.SelectedVoxelType) : ((voxel.Volume < 128) ? ((!(num5 < 128f)) ? new VCVoxel((byte)num5, (byte)VCEditor.SelectedVoxelType) : voxel) : ((!(num5 < 128f)) ? new VCVoxel((byte)num5, (byte)VCEditor.SelectedVoxelType) : voxel)));
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
						float num6 = (num2 - vector4.magnitude) / num2;
						float num7 = Mathf.Clamp(num6 * num3 * 127.5f + 127.5f, 0f, 255.49f);
						int pos2 = VCIsoData.IPosToKey(intVector);
						VCVoxel voxel3 = VCEditor.s_Scene.m_IsoData.GetVoxel(pos2);
						VCVoxel vCVoxel2 = 0;
						vCVoxel2 = ((voxel3.Volume == 0) ? new VCVoxel((byte)num7, (byte)VCEditor.SelectedVoxelType) : ((voxel3.Volume < 128) ? ((!(num7 < 128f)) ? new VCVoxel((byte)num7, (byte)VCEditor.SelectedVoxelType) : voxel3) : ((!(num7 < 128f)) ? new VCVoxel((byte)num7, (byte)VCEditor.SelectedVoxelType) : voxel3)));
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
}
