using System.Collections.Generic;

public abstract class VCESelectMethod : GLBehaviour
{
	public VCESelectVoxel m_Parent;

	public bool m_Selecting;

	protected Dictionary<int, byte> m_Selection;

	public bool m_NeedUpdate;

	protected VCIsoData m_Iso;

	public virtual void Init(VCESelectVoxel parent)
	{
		m_Parent = parent;
		m_Selecting = false;
		m_Selection = parent.m_SelectionMgr.m_Selection;
		m_Iso = VCEditor.s_Scene.m_IsoData;
	}

	public abstract void MainMethod();

	protected abstract void Submit();
}
