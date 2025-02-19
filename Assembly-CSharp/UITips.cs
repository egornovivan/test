using Pathea;
using UnityEngine;

public class UITips : MonoBehaviour
{
	public UITipsWndCtrl tipWnds;

	public UITipListControl tipList;

	private void Awake()
	{
		PeSingleton<PeTipsMsgMan>.Instance.onAddTipMsg += OnAddNewTipsMsg;
	}

	private void OnDestroy()
	{
		PeSingleton<PeTipsMsgMan>.Instance.onAddTipMsg -= OnAddNewTipsMsg;
	}

	private void OnAddNewTipsMsg(PeTipMsg tipMsg)
	{
		tipList.AddMsg(tipMsg);
	}
}
