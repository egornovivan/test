public class VCEDelComponent : VCEModify
{
	public int m_Index;

	public VCComponentData m_Data;

	public VCEDelComponent(int index, VCComponentData data)
	{
		m_Index = index;
		m_Data = data.Copy();
	}

	public override void Undo()
	{
		VCComponentData vCComponentData = m_Data.Copy();
		vCComponentData.CreateEntity(for_editor: true, null);
		VCEditor.s_Scene.m_IsoData.m_Components.Insert(m_Index, vCComponentData);
	}

	public override void Redo()
	{
		VCEditor.s_Scene.m_IsoData.m_Components[m_Index].DestroyEntity();
		VCEditor.s_Scene.m_IsoData.m_Components.RemoveAt(m_Index);
	}
}
