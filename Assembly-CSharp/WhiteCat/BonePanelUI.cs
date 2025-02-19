using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat;

public class BonePanelUI : MonoBehaviour
{
	[Serializable]
	private class ButtonGroup
	{
		[SerializeField]
		private UIButton[] _buttonGroup;

		private UIWidget[] _widgets;

		public bool disabled
		{
			set
			{
				UIButton[] buttonGroup = _buttonGroup;
				foreach (UIButton uIButton in buttonGroup)
				{
					uIButton.isEnabled = !value;
				}
			}
		}

		public void SetColor(int index, Color color)
		{
			_buttonGroup[index].defaultColor = color;
			_widgets[index].color = color;
			_buttonGroup[index].UpdateColor(_buttonGroup[index].isEnabled, immediate: true);
		}

		public void Init(BonePanelUI panel)
		{
			_widgets = new UIWidget[_buttonGroup.Length];
			for (int i = 0; i < _buttonGroup.Length; i++)
			{
				_widgets[i] = _buttonGroup[i].GetComponentInChildren<UIWidget>();
				UIEventListener uIEventListener = _buttonGroup[i].gameObject.AddComponent<UIEventListener>();
				int index = i;
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
				{
					panel.SwitchBone(index);
				});
			}
		}
	}

	[SerializeField]
	private GameObject _maleUI;

	[SerializeField]
	private GameObject _femaleUI;

	[SerializeField]
	private Color _maleNormalColor;

	[SerializeField]
	private Color _femaleNormalColor;

	[SerializeField]
	private Color _hilightColor;

	[SerializeField]
	private List<ButtonGroup> _maleButtons;

	[SerializeField]
	private List<ButtonGroup> _femaleButtons;

	private bool _awaked;

	private VCPArmorPivot _part;

	public int ArmorPartIndex => (_part != null) ? _part.showIndex : 0;

	private void SwitchBone(int index)
	{
		_maleButtons[(int)_part.armorType].SetColor(_part.showIndex, _maleNormalColor);
		_femaleButtons[(int)_part.armorType].SetColor(_part.showIndex, _femaleNormalColor);
		_part.showIndex = index;
		_maleButtons[(int)_part.armorType].SetColor(_part.showIndex, _hilightColor);
		_femaleButtons[(int)_part.armorType].SetColor(_part.showIndex, _hilightColor);
	}

	private void SwitchGender()
	{
		_part.isMale = !_part.isMale;
		_maleUI.SetActive(_part.isMale);
		_femaleUI.SetActive(!_part.isMale);
	}

	public void Show(VCPArmorPivot part)
	{
		if (!base.gameObject.activeSelf && part.armorType != ArmorType.Decoration)
		{
			_part = part;
			base.transform.parent.gameObject.SetActive(value: true);
			base.gameObject.SetActive(value: true);
			_maleUI.SetActive(_part.isMale);
			_femaleUI.SetActive(!_part.isMale);
			for (int i = 0; i < 4; i++)
			{
				_maleButtons[i].disabled = part.armorType != (ArmorType)i;
				_femaleButtons[i].disabled = part.armorType != (ArmorType)i;
			}
			_maleButtons[(int)part.armorType].SetColor(_part.showIndex, _hilightColor);
			_femaleButtons[(int)part.armorType].SetColor(_part.showIndex, _hilightColor);
		}
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
			_maleButtons[(int)_part.armorType].SetColor(_part.showIndex, _maleNormalColor);
			_femaleButtons[(int)_part.armorType].SetColor(_part.showIndex, _femaleNormalColor);
		}
	}

	private void Awake()
	{
		for (int i = 0; i < 4; i++)
		{
			_maleButtons[i].Init(this);
			_femaleButtons[i].Init(this);
		}
	}
}
