public class VCEBoxBrush : VCEFreeSizeBrush
{
	protected override string BrushDesc()
	{
		return "Box Brush - Draw free size voxel cube";
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
		for (int i = min.x; i <= max.x; i++)
		{
			for (int j = min.y; j <= max.y; j++)
			{
				for (int k = min.z; k <= max.z; k++)
				{
					if (VCEditor.s_Mirror.Enabled_Masked)
					{
						VCEditor.s_Mirror.MirrorVoxel(new IntVector3(i, j, k));
						for (int l = 0; l < VCEditor.s_Mirror.OutputCnt; l++)
						{
							if (VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[l]))
							{
								int num2 = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[l]);
								VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(num2);
								VCVoxel vCVoxel = new VCVoxel(byte.MaxValue, (byte)VCEditor.SelectedVoxelType);
								if ((int)voxel != (int)vCVoxel)
								{
									VCEAlterVoxel item2 = new VCEAlterVoxel(num2, voxel, vCVoxel);
									m_Action.Modifies.Add(item2);
								}
							}
						}
					}
					else
					{
						int num3 = VCIsoData.IPosToKey(new IntVector3(i, j, k));
						VCVoxel voxel2 = VCEditor.s_Scene.m_IsoData.GetVoxel(num3);
						VCVoxel vCVoxel2 = new VCVoxel(byte.MaxValue, (byte)VCEditor.SelectedVoxelType);
						if ((int)voxel2 != (int)vCVoxel2)
						{
							VCEAlterVoxel item3 = new VCEAlterVoxel(num3, voxel2, vCVoxel2);
							m_Action.Modifies.Add(item3);
						}
					}
				}
			}
		}
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.Do();
		}
		ResetDrawing();
	}
}
