using System.Collections.Generic;

public class VCEAlterDecalMap : VCEModify
{
	public int m_Index;

	public ulong m_OldDecal;

	public ulong m_NewDecal;

	private static List<VCEAlterDecalMap> s_AllModify = new List<VCEAlterDecalMap>();

	public VCEAlterDecalMap(int index, ulong old_guid, ulong new_guid)
	{
		m_Index = index;
		m_OldDecal = old_guid;
		m_NewDecal = new_guid;
		s_AllModify.Add(this);
	}

	~VCEAlterDecalMap()
	{
		s_AllModify.Remove(this);
	}

	public override void Undo()
	{
		VCDecalAsset vCDecalAsset = VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index];
		VCDecalAsset decal = VCEAssetMgr.GetDecal(m_OldDecal);
		ulong num = vCDecalAsset?.m_Guid ?? 0;
		ulong num2 = decal?.m_Guid ?? 0;
		if (num != num2)
		{
			VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index] = decal;
		}
	}

	public override void Redo()
	{
		VCDecalAsset vCDecalAsset = VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index];
		VCDecalAsset decal = VCEAssetMgr.GetDecal(m_NewDecal);
		ulong num = vCDecalAsset?.m_Guid ?? 0;
		ulong num2 = decal?.m_Guid ?? 0;
		if (num != num2)
		{
			VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index] = decal;
		}
	}
}
