using UnityEngine;

public class VCEAlterComponentTransform : VCEModify
{
	public int m_Index;

	public Vector3 m_OldPosition;

	public Vector3 m_OldRotation;

	public Vector3 m_OldScale;

	public Vector3 m_NewPosition;

	public Vector3 m_NewRotation;

	public Vector3 m_NewScale;

	public VCEAlterComponentTransform(int index, Vector3 old_pos, Vector3 old_rot, Vector3 old_scl, Vector3 new_pos, Vector3 new_rot, Vector3 new_scl)
	{
		m_Index = index;
		m_OldPosition = old_pos;
		m_OldRotation = old_rot;
		m_OldScale = old_scl;
		m_NewPosition = new_pos;
		m_NewRotation = new_rot;
		m_NewScale = new_scl;
	}

	public override void Undo()
	{
		VCComponentData vCComponentData = VCEditor.s_Scene.m_IsoData.m_Components[m_Index];
		vCComponentData.m_Position = m_OldPosition;
		vCComponentData.m_Rotation = m_OldRotation;
		vCComponentData.m_Scale = m_OldScale;
		vCComponentData.UpdateEntity(for_editor: true);
	}

	public override void Redo()
	{
		VCComponentData vCComponentData = VCEditor.s_Scene.m_IsoData.m_Components[m_Index];
		vCComponentData.m_Position = m_NewPosition;
		vCComponentData.m_Rotation = m_NewRotation;
		vCComponentData.m_Scale = m_NewScale;
		vCComponentData.UpdateEntity(for_editor: true);
	}
}
