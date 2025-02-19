using UnityEngine;

public class GridItem : MonoBehaviour
{
	public delegate void BaseMsgEvent(object sender);

	public delegate void OnToolTipEvent(bool show, object sender);

	public UILabel mNumCount;

	public UIFilledSprite mSkillCooldown;

	public UITexture mTexContent;

	public UISprite mSpContent_0;

	public UISprite mSpContent_1;

	public UISprite mSpContent_2;

	public UISprite mSpContent_Bg;

	public UISprite mSpBg;

	public UISprite mNewMark;

	[HideInInspector]
	public object mData;

	public BaseMsgEvent e_OnClick;

	public BaseMsgEvent e_BeginDrag;

	public BaseMsgEvent e_Drag;

	public BaseMsgEvent e_Drop;

	public BaseMsgEvent e_OnGetDrag;

	public OnToolTipEvent e_OnToolTip;
}
