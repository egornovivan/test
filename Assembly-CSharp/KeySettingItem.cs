using System;
using InControl;
using UnityEngine;

public class KeySettingItem : MonoBehaviour
{
	private class KeyAccessor : IKeyJoyAccessor
	{
		public int FindInArray(KeySettingItem[] items, KeyCode key)
		{
			int num = items.Length;
			for (int i = 0; i < num; i++)
			{
				if (items[i]._keySetting._key == key)
				{
					return (!items[i]._keySetting._keyLock) ? (i + 1) : (-(i + 1));
				}
			}
			return 0;
		}

		public void Set(KeySettingItem item, KeyCode keyToSet)
		{
			item._keySetting._key = keyToSet;
			item.mKeyContent.text = keyToSet.ToStr();
		}

		public KeyCode Get(KeySettingItem item)
		{
			return item._keySetting._key;
		}
	}

	private class JoyAccessor : IKeyJoyAccessor
	{
		KeyCode IKeyJoyAccessor.Get(KeySettingItem item)
		{
			return KeyCode.None;
		}

		public int FindInArray(KeySettingItem[] items, InputControlType key)
		{
			int num = items.Length;
			for (int i = 0; i < num; i++)
			{
				if (items[i]._keySetting._joy == key)
				{
					return (!items[i]._keySetting._joyLock) ? (i + 1) : (-(i + 1));
				}
			}
			return 0;
		}

		public void Set(KeySettingItem item, InputControlType keyToSet)
		{
			item._keySetting._joy = keyToSet;
			item.mCtrlContent.text = keyToSet.ToString();
		}

		public int FindInArray(KeySettingItem[] items, KeyCode key)
		{
			return 0;
		}

		public void Set(KeySettingItem item, KeyCode keyToSet)
		{
		}

		public InputControlType Get(KeySettingItem item)
		{
			return item._keySetting._joy;
		}
	}

	public int _keySettingName;

	public PeInput.KeyJoySettingPair _keySetting;

	public UILabel mFunctionContent;

	public UILabel mKeyContent;

	public UILabel mCtrlContent;

	public UISprite mLockSpr;

	public UICheckbox mKeyCheckBox;

	public UICheckbox mCtrlCheckBox;

	[SerializeField]
	private GameObject m_OpKeyBtnsParent;

	[SerializeField]
	private UIButton m_ApplyKeyBtn;

	[SerializeField]
	private UIButton m_CancelKeyBtn;

	[SerializeField]
	private GameObject m_OpCtrlBtnsParent;

	[SerializeField]
	private UIButton m_ApplyCtrlBtn;

	[SerializeField]
	private UIButton m_CancelCtrlBtn;

	private KeyCode _keyToSet;

	private InputControlType _joyToSet;

	private bool _bHover;

	private bool _bApply;

	private KeyCode _ctrlKeyDown;

	private static KeyAccessor s_keyAccessor = new KeyAccessor();

	private static JoyAccessor s_joyAccessor = new JoyAccessor();

	private void Start()
	{
		mKeyCheckBox.radioButtonRoot = base.transform.parent;
		mCtrlCheckBox.radioButtonRoot = base.transform.parent;
		_keyToSet = KeyCode.None;
		_joyToSet = InputControlType.None;
		_bHover = false;
		_bApply = false;
		UICheckbox uICheckbox = mKeyCheckBox;
		uICheckbox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(uICheckbox.onStateChange, new UICheckbox.OnStateChange(SetKey));
		UICheckbox uICheckbox2 = mCtrlCheckBox;
		uICheckbox2.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(uICheckbox2.onStateChange, new UICheckbox.OnStateChange(SetJoy));
		mKeyCheckBox.IsUseSelfOnClick = false;
		mCtrlCheckBox.IsUseSelfOnClick = false;
		UIEventListener.Get(mKeyCheckBox.gameObject).onClick = delegate
		{
			OnClick(mKeyCheckBox);
		};
		UIEventListener.Get(mCtrlCheckBox.gameObject).onClick = delegate
		{
			OnClick(mCtrlCheckBox);
		};
		UIEventListener.Get(m_ApplyKeyBtn.gameObject).onClick = delegate
		{
			ApplyKeyChange();
		};
		UIEventListener.Get(m_CancelKeyBtn.gameObject).onClick = delegate
		{
			CancelKeyChange();
		};
		UIEventListener.Get(m_ApplyCtrlBtn.gameObject).onClick = delegate
		{
			ApplyCtrlChange();
		};
		UIEventListener.Get(m_CancelCtrlBtn.gameObject).onClick = delegate
		{
			CancelCtrlChange();
		};
		UIEventListener.Get(mKeyCheckBox.gameObject).onHover = delegate(GameObject go, bool bHover)
		{
			_bHover = bHover;
		};
	}

	private void OnClick(UICheckbox item)
	{
		if (item.enabled && Input.GetMouseButtonUp(0) && !item.isChecked)
		{
			item.isChecked = !item.isChecked;
		}
	}

	private void Update()
	{
		_ctrlKeyDown = KeyCode.None;
		for (KeyCode keyCode = KeyCode.JoystickButton0; keyCode <= KeyCode.JoystickButton19; keyCode++)
		{
			if (Input.GetKeyDown(keyCode))
			{
				_ctrlKeyDown = keyCode;
				break;
			}
		}
	}

	private void OnGUI()
	{
		if (_keySetting == null)
		{
			return;
		}
		if (mKeyCheckBox.isChecked && _keySetting._keyLock)
		{
			mKeyCheckBox.isChecked = false;
		}
		if (mCtrlCheckBox.isChecked && _keySetting._joyLock)
		{
			mCtrlCheckBox.isChecked = false;
		}
		if (!mLockSpr.enabled && _keySetting._keyLock)
		{
			mLockSpr.enabled = true;
		}
		if (_keySettingName != 0)
		{
			mFunctionContent.text = PELocalization.GetString(_keySettingName);
		}
		if (!mCtrlCheckBox.isChecked && !mKeyCheckBox.isChecked)
		{
			return;
		}
		KeyCode keyCode = _ctrlKeyDown;
		if (Event.current != null)
		{
			if (Event.current.type == EventType.KeyDown)
			{
				keyCode = Event.current.keyCode;
			}
			else if (Event.current.type == EventType.MouseDown && _bHover)
			{
				keyCode = (KeyCode)(323 + Event.current.button);
			}
		}
		if (mCtrlCheckBox.isChecked)
		{
			for (InputControlType inputControlType = InputControlType.Action1; inputControlType < InputControlType.Options; inputControlType++)
			{
				if (InputManager.ActiveDevice.GetControl(inputControlType).WasPressed)
				{
					_joyToSet = inputControlType;
					mCtrlContent.text = _joyToSet.ToString();
					break;
				}
			}
		}
		else if (mKeyCheckBox.isChecked && keyCode != 0 && keyCode > KeyCode.Escape && keyCode < KeyCode.JoystickButton0)
		{
			_keyToSet = keyCode;
			if (Event.current.alt && keyCode != KeyCode.LeftAlt && keyCode != KeyCode.RightAlt)
			{
				_keyToSet = PeInput.AltKey(_keyToSet);
			}
			if (Event.current.shift && keyCode != KeyCode.LeftShift && keyCode != KeyCode.RightShift)
			{
				_keyToSet = PeInput.ShiftKey(_keyToSet);
			}
			if (Event.current.control && keyCode != KeyCode.LeftControl && keyCode != KeyCode.RightControl)
			{
				_keyToSet = PeInput.CtrlKey(_keyToSet);
			}
			mKeyContent.text = _keyToSet.ToStr();
		}
	}

	private void ApplyKeyChange()
	{
		_bApply = true;
		mKeyCheckBox.isChecked = false;
		_bApply = false;
	}

	private void CancelKeyChange()
	{
		_keyToSet = _keySetting._key;
		mKeyCheckBox.isChecked = false;
	}

	private void ApplyCtrlChange()
	{
		_bApply = true;
		mCtrlCheckBox.isChecked = false;
		_bApply = false;
	}

	private void CancelCtrlChange()
	{
		_joyToSet = _keySetting._joy;
		mCtrlCheckBox.isChecked = false;
	}

	private void SetKey(bool state)
	{
		m_OpKeyBtnsParent.SetActive(state);
		if (!state && _bApply)
		{
			if (_keyToSet != 0 && _keyToSet != _keySetting._key)
			{
				KeySettingItem keySettingItem = UIOption.Instance.TestConflict(this, _keyToSet, s_keyAccessor);
				if (keySettingItem != null)
				{
					MessageBox_N.ShowYNBox(PELocalization.GetString(8000178), delegate
					{
						s_keyAccessor.Set(this, _keyToSet);
					});
				}
				else
				{
					s_keyAccessor.Set(this, _keyToSet);
				}
			}
		}
		else
		{
			_keyToSet = _keySetting._key;
		}
		mKeyContent.text = _keySetting._key.ToStr();
	}

	private void SetJoy(bool state)
	{
		m_OpCtrlBtnsParent.SetActive(state);
		if (!state && _bApply)
		{
			if (_joyToSet != 0 && _joyToSet != _keySetting._joy)
			{
				KeySettingItem keySettingItem = null;
				if (keySettingItem != null)
				{
					MessageBox_N.ShowYNBox(PELocalization.GetString(8000178), delegate
					{
						s_joyAccessor.Set(this, _joyToSet);
					});
				}
				else
				{
					s_joyAccessor.Set(this, _joyToSet);
				}
			}
		}
		else
		{
			_joyToSet = _keySetting._joy;
		}
		mCtrlContent.text = _keySetting._joy.ToString();
	}
}
