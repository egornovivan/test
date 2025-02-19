using ItemAsset;
using UnityEngine;

public class QuickBarItem_N : Grid_N
{
	[SerializeField]
	private UISprite m_QuickBarSprite;

	private bool m_isKeyGrid;

	private void Awake()
	{
		m_isKeyGrid = false;
		UpdateQuickBarKeyState();
	}

	public override void SetItem(ItemSample itemGrid, bool showNew = false)
	{
		base.SetItem(itemGrid, showNew);
		UpdateQuickBarKeyState();
	}

	public void UpdateKeyInfo(int key)
	{
		if (key == -1)
		{
			mScriptIco.spriteName = "itemhand";
			mScriptIco.MakePixelPerfect();
			m_isKeyGrid = false;
		}
		else
		{
			mScriptIco.spriteName = "num_" + key;
			mScriptIco.MakePixelPerfect();
			m_QuickBarSprite.spriteName = "QuickKey_" + key;
			m_QuickBarSprite.MakePixelPerfect();
			m_isKeyGrid = true;
		}
	}

	private void UpdateQuickBarKeyState()
	{
		m_QuickBarSprite.enabled = base.Item != null && m_isKeyGrid;
	}
}
