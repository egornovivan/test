using System.Collections.Generic;

public class VCEAlterMaterialMap : VCEModify
{
	public int m_Index;

	public ulong m_OldMat;

	public ulong m_NewMat;

	private static List<VCEAlterMaterialMap> s_AllModify = new List<VCEAlterMaterialMap>();

	public VCEAlterMaterialMap(int index, ulong old_guid, ulong new_guid)
	{
		m_Index = index;
		m_OldMat = old_guid;
		m_NewMat = new_guid;
		s_AllModify.Add(this);
	}

	~VCEAlterMaterialMap()
	{
		s_AllModify.Remove(this);
	}

	public override void Undo()
	{
		VCMaterial vCMaterial = VCEditor.s_Scene.m_IsoData.m_Materials[m_Index];
		VCMaterial material = VCEAssetMgr.GetMaterial(m_OldMat);
		ulong num = vCMaterial?.m_Guid ?? 0;
		ulong num2 = material?.m_Guid ?? 0;
		if (num != num2)
		{
			VCEditor.s_Scene.m_IsoData.m_Materials[m_Index] = material;
			VCEditor.s_Scene.GenerateIsoMat();
		}
	}

	public override void Redo()
	{
		VCMaterial vCMaterial = VCEditor.s_Scene.m_IsoData.m_Materials[m_Index];
		VCMaterial material = VCEAssetMgr.GetMaterial(m_NewMat);
		ulong num = vCMaterial?.m_Guid ?? 0;
		ulong num2 = material?.m_Guid ?? 0;
		if (num != num2)
		{
			VCEditor.s_Scene.m_IsoData.m_Materials[m_Index] = material;
			VCEditor.s_Scene.GenerateIsoMat();
		}
	}

	public static void MatChange(ulong old_guid, ulong new_guid)
	{
		foreach (VCEAlterMaterialMap item in s_AllModify)
		{
			if (item.m_OldMat == old_guid)
			{
				item.m_OldMat = new_guid;
			}
			if (item.m_NewMat == old_guid)
			{
				item.m_NewMat = new_guid;
			}
		}
	}
}
