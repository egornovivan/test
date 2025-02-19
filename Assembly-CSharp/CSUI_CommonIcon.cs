using UnityEngine;

public class CSUI_CommonIcon : MonoBehaviour
{
	public delegate void OnClickItemEvent(CSEntity entity);

	public UISprite mSpState;

	public UISprite mIcoSpr;

	public UICheckbox mCheckBox;

	public ShowToolTipItem_N mShowToolTip;

	private CSCommon m_Common;

	private int m_Type;

	private int m_TipID;

	[HideInInspector]
	public CSCommon Common
	{
		get
		{
			return m_Common;
		}
		set
		{
			m_Common = value;
			if (m_Common != null)
			{
				IcoType = m_Common.m_Type;
			}
		}
	}

	private int IcoType
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
			switch (m_Type)
			{
			case 33:
				mIcoSpr.spriteName = "building_powerplant_coal";
				m_TipID = 82210007;
				break;
			case 2:
				mIcoSpr.spriteName = "element_building_020201";
				m_TipID = 82210002;
				break;
			case 35:
				mIcoSpr.spriteName = "fusion_plant";
				m_TipID = 82210017;
				break;
			case 4:
				mIcoSpr.spriteName = "element_building_030301";
				m_TipID = 82210017;
				break;
			case 5:
				mIcoSpr.spriteName = "element_building_030101";
				m_TipID = 82210017;
				break;
			case 6:
				mIcoSpr.spriteName = "element_building_030201";
				m_TipID = 82210017;
				break;
			default:
				m_TipID = 0;
				break;
			}
			if (null != mShowToolTip && m_TipID != 0)
			{
				mShowToolTip.mStrID = m_TipID;
			}
		}
	}

	public event OnClickItemEvent e_OnClickIco;

	private void OnClickIco()
	{
		if (this.e_OnClickIco != null && Common != null)
		{
			this.e_OnClickIco(Common);
		}
	}

	private void Start()
	{
		mSpState.enabled = false;
		mCheckBox.radioButtonRoot = base.transform.parent;
	}

	private void Update()
	{
		if (Common != null)
		{
			if (Common.Assembly == null)
			{
				mSpState.color = new Color(1f, 1f, 1f, 0.3f);
				mSpState.enabled = true;
			}
			else if (!Common.IsRunning)
			{
				mSpState.color = new Color(1f, 0.3f, 0.3f, 0.3f);
				mSpState.enabled = true;
			}
			else
			{
				mSpState.enabled = false;
			}
		}
	}
}
