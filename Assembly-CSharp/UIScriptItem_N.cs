using System;
using UnityEngine;

public class UIScriptItem_N : MonoBehaviour
{
	[SerializeField]
	private UISprite m_BgSprite;

	[SerializeField]
	private UILabel m_NameLabel;

	public Action<UIScriptItem_N> SelectEvent;

	public int ItemID { get; private set; }

	public int ScriptIndex { get; private set; }

	private void Awake()
	{
		ScriptIndex = -1;
		ItemID = -1;
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			SelectItem();
		});
	}

	public void UpdateInfo(int itemID, int scriptIndex)
	{
		ItemID = itemID;
		ScriptIndex = scriptIndex;
		UpdateName(scriptIndex);
	}

	public void SelectItem(bool execEvent = true)
	{
		UpdateBg("scriptItemBgSelect");
		if (execEvent && SelectEvent != null && ScriptIndex != -1)
		{
			SelectEvent(this);
		}
	}

	public void CanSelectItem()
	{
		UpdateBg("scriptItemBg");
	}

	public void Reset()
	{
		ItemID = -1;
		ScriptIndex = -1;
		CanSelectItem();
		SelectEvent = null;
	}

	private void UpdateBg(string spriteName)
	{
		m_BgSprite.spriteName = spriteName;
		m_BgSprite.MakePixelPerfect();
	}

	private void UpdateName(int index)
	{
		m_NameLabel.text = (index + 1).ToString();
	}
}
