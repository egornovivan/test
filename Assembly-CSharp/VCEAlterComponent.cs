public class VCEAlterComponent : VCEModify
{
	public int m_Index;

	public VCComponentData m_OldData;

	public VCComponentData m_NewData;

	public VCEAlterComponent(int index, VCComponentData old_data, VCComponentData new_data)
	{
		m_Index = index;
		m_OldData = old_data.Copy();
		m_NewData = new_data.Copy();
	}

	public override void Undo()
	{
		VCComponentData vCComponentData = VCEditor.s_Scene.m_IsoData.m_Components[m_Index];
		vCComponentData.Import(m_OldData.Export());
		vCComponentData.UpdateEntity(for_editor: true);
	}

	public override void Redo()
	{
		VCComponentData vCComponentData = VCEditor.s_Scene.m_IsoData.m_Components[m_Index];
		vCComponentData.Import(m_NewData.Export());
		vCComponentData.UpdateEntity(for_editor: true);
	}
}
