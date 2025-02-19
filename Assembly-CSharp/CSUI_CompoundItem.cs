using System;
using UnityEngine;

public class CSUI_CompoundItem : MonoBehaviour
{
	public delegate void ClickEvent(CSUI_CompoundItem ci);

	[SerializeField]
	private UISprite m_IconUI;

	[SerializeField]
	private UILabel m_CountUI;

	[SerializeField]
	private UISlider m_Slider;

	[SerializeField]
	private GameObject m_DeleteBtn;

	[SerializeField]
	private UICheckbox m_CheckBox;

	public object m_RefObj;

	public ClickEvent RightBtnClickEvent;

	public string IcomName
	{
		get
		{
			return m_IconUI.spriteName;
		}
		set
		{
			m_IconUI.spriteName = value;
		}
	}

	public int Count
	{
		get
		{
			if (CSUtils.IsNumber(m_CountUI.text))
			{
				return int.Parse(m_CountUI.text);
			}
			m_IconUI.gameObject.SetActive(value: false);
			return 0;
		}
		set
		{
			if (value == 0)
			{
				m_CountUI.gameObject.SetActive(value: false);
			}
			else
			{
				m_CountUI.gameObject.SetActive(value: true);
			}
			m_CountUI.text = value.ToString();
		}
	}

	public float SliderValue
	{
		get
		{
			return m_Slider.sliderValue;
		}
		set
		{
			m_Slider.sliderValue = value;
		}
	}

	public bool ShowSlider
	{
		get
		{
			return m_Slider.gameObject.activeInHierarchy;
		}
		set
		{
			m_Slider.gameObject.SetActive(value);
		}
	}

	public event Action<CSUI_CompoundItem> onDeleteBtnClick;

	private void CheckDeleteBtnState()
	{
		if (m_CheckBox.isChecked && IcomName != string.Empty && IcomName != "Null")
		{
			if (!m_DeleteBtn.gameObject.activeSelf)
			{
				m_DeleteBtn.gameObject.SetActive(value: true);
			}
		}
		else if (m_DeleteBtn.gameObject.activeSelf)
		{
			m_DeleteBtn.gameObject.SetActive(value: false);
		}
	}

	private void ItemOnClick()
	{
		if (Input.GetMouseButton(1) && RightBtnClickEvent != null)
		{
			RightBtnClickEvent(this);
		}
	}

	private void OnDeleteBtnClick()
	{
		if (Input.GetMouseButton(0) && this.onDeleteBtnClick != null)
		{
			this.onDeleteBtnClick(this);
		}
	}

	private void Update()
	{
		CheckDeleteBtnState();
	}
}
