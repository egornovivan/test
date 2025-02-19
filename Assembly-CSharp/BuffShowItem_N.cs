using SkillAsset;
using UnityEngine;

public class BuffShowItem_N : MonoBehaviour
{
	private EffSkillBuffInst mBuffIns;

	public void SetBuff(EffSkillBuffInst buffIns)
	{
		mBuffIns = buffIns;
		if (mBuffIns != null)
		{
			GetComponent<UISprite>().spriteName = mBuffIns.m_buff.m_iconImgPath;
			GetComponent<UISprite>().MakePixelPerfect();
		}
		else
		{
			GetComponent<UISprite>().spriteName = "Null";
		}
	}

	private void OnTooltip(bool show)
	{
		if (mBuffIns != null)
		{
			if ("0" != mBuffIns.m_buff.m_buffHint)
			{
				ToolTipsMgr.ShowText(mBuffIns.m_buff.m_buffHint);
			}
		}
		else
		{
			ToolTipsMgr.ShowText(null);
		}
	}
}
