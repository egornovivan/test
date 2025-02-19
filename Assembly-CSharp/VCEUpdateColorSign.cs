public class VCEUpdateColorSign : VCEModify
{
	public bool m_Forward;

	public bool m_Back;

	public VCEUpdateColorSign(bool forward, bool back)
	{
		m_Forward = forward;
		m_Back = back;
	}

	public override void Undo()
	{
		if (m_Back)
		{
			VCEditor.Instance.m_MeshMgr.UpdateAllMeshColor();
		}
	}

	public override void Redo()
	{
		if (m_Forward)
		{
			VCEditor.Instance.m_MeshMgr.UpdateAllMeshColor();
		}
	}
}
