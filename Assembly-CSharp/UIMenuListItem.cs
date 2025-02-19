using PeUIEffect;
using UnityEngine;

public class UIMenuListItem : MonoBehaviour
{
	public delegate void BaseMsgEvent(object sender);

	public int KeyId;

	public UIOption.KeyCategory mCategory;

	public UIGameMenuCtrl.MenuItemFlag mMenuItemFlag;

	public UIMenuListItem Parent;

	public UILabel LbText;

	public UISlicedSprite ItemSelectedBg;

	public UISlicedSprite SpHaveChild;

	public UISprite mIcoSpr;

	[HideInInspector]
	public int Index = -1;

	private MenuParticleEffect mEffect;

	private string defoutIcoStr;

	private float time;

	private int Speed = 25;

	public string Text
	{
		get
		{
			return LbText.text;
		}
		set
		{
			LbText.text = value;
			base.gameObject.name = value;
			LbText.MakePixelPerfect();
		}
	}

	public bool IsHaveChild
	{
		get
		{
			return SpHaveChild.enabled;
		}
		set
		{
			SpHaveChild.enabled = value;
		}
	}

	public BoxCollider Box_Collider => GetComponent<BoxCollider>();

	public string icoName
	{
		set
		{
			if (mIcoSpr == null)
			{
				return;
			}
			if (value.Trim().Length > 0)
			{
				mIcoSpr.spriteName = value;
				string[] array = value.Split('_');
				if (array.Length >= 2)
				{
					defoutIcoStr = array[0] + "_" + array[1] + "_";
				}
				else
				{
					defoutIcoStr = string.Empty;
				}
			}
			else
			{
				mIcoSpr.spriteName = "null";
			}
		}
	}

	public event BaseMsgEvent e_OnMouseMoveIn;

	public event BaseMsgEvent e_OnMouseMoveOut;

	public event BaseMsgEvent e_OnClick;

	public void SetHotKeyContent(string str)
	{
		string text = LbText.text;
		if (str == "Escape")
		{
			LbText.text = text.Split('[')[0] + "[4169e1][Esc][-]";
		}
		else
		{
			LbText.text = text.Split('[')[0] + "[4169e1][" + str + "][-]";
		}
	}

	private void OnMouseMoveIn()
	{
		if (this.e_OnMouseMoveIn != null)
		{
			this.e_OnMouseMoveIn(this);
		}
	}

	private void OnMouseMoveOut()
	{
		if (this.e_OnMouseMoveOut != null)
		{
			this.e_OnMouseMoveOut(this);
		}
	}

	private void OnClickItem()
	{
		if (this.e_OnClick != null)
		{
			this.e_OnClick(this);
		}
	}

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/UIEffect/MenuParticleEffect")) as GameObject;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, -5f);
		gameObject.transform.localScale = Vector3.one;
		mEffect = gameObject.GetComponent<MenuParticleEffect>();
	}

	private void Update()
	{
		if (mEffect != null && ItemSelectedBg != null)
		{
			mEffect.gameObject.SetActive(ItemSelectedBg.enabled);
		}
		if (!(ItemSelectedBg != null) || defoutIcoStr.Length <= 0)
		{
			return;
		}
		if (ItemSelectedBg.enabled)
		{
			if (time < (float)Speed)
			{
				mIcoSpr.spriteName = defoutIcoStr + "1";
			}
			else if (time < (float)(2 * Speed))
			{
				mIcoSpr.spriteName = defoutIcoStr + "2";
			}
			else if (time < (float)(3 * Speed))
			{
				mIcoSpr.spriteName = defoutIcoStr + "3";
			}
			else if (time < (float)(4 * Speed))
			{
				mIcoSpr.spriteName = defoutIcoStr + "4";
			}
			else
			{
				time = 0f;
			}
			time += Time.deltaTime * 100f;
		}
		else
		{
			mIcoSpr.spriteName = defoutIcoStr + "1";
			time = 0f;
		}
	}
}
