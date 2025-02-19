using UnityEngine;

public class CSUI_WorkItem : MonoBehaviour
{
	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private CSUI_NPCGrid mNpcGrid;

	public void SetWorker(CSPersonnel personnel)
	{
		mLbName.text = personnel.m_Name;
		mNpcGrid.m_Npc = personnel;
	}
}
