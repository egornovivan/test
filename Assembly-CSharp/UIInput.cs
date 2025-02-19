using System.Windows.Forms;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Input (Basic)")]
public class UIInput : MonoBehaviour
{
	public enum KeyboardType
	{
		Default,
		ASCIICapable,
		NumbersAndPunctuation,
		URL,
		NumberPad,
		PhonePad,
		NamePhonePad,
		EmailAddress
	}

	public delegate char Validator(string currentText, char nextChar);

	public delegate void OnSubmit(string inputString);

	public bool bUseClipboard;

	public static UIInput current;

	public UILabel label;

	public int maxChars;

	public string caratChar = "|";

	public Validator validator;

	public KeyboardType type;

	public bool isPassword;

	public Color activeColor = Color.white;

	public GameObject eventReceiver;

	public string functionName = "OnSubmit";

	public OnSubmit onSubmit;

	public int mMainGroupID;

	public int mSubGroupID;

	public int mIndex;

	[HideInInspector]
	public int mSortScore;

	private string mText = string.Empty;

	private string mDefaultText = string.Empty;

	private Color mDefaultColor = Color.white;

	private UIWidget.Pivot mPivot = UIWidget.Pivot.Left;

	private float mPosition;

	private string mLastIME = string.Empty;

	private bool mDoInit = true;

	public bool m_NoSubmit;

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (mDoInit)
			{
				Init();
			}
			mText = value;
			if (label != null)
			{
				if (string.IsNullOrEmpty(value))
				{
					value = mDefaultText;
				}
				label.supportEncoding = false;
				label.text = ((!selected) ? value : (value + caratChar));
				label.showLastPasswordChar = selected;
				label.color = ((!selected && !(value != mDefaultText)) ? mDefaultColor : activeColor);
			}
		}
	}

	public bool selected
	{
		get
		{
			return UICamera.selectedObject == base.gameObject;
		}
		set
		{
			if (!value && UICamera.selectedObject == base.gameObject)
			{
				UICamera.selectedObject = null;
			}
			else if (value)
			{
				UICamera.selectedObject = base.gameObject;
			}
		}
	}

	private void Awake()
	{
		mSortScore = (mSubGroupID << 16) | mIndex;
		InputFoceManager.Instance.AddInput(this);
	}

	private void OnDestroy()
	{
		InputFoceManager.Instance.RemoveInput(this);
	}

	protected void Init()
	{
		if (mDoInit)
		{
			mDoInit = false;
			if (label == null)
			{
				label = GetComponentInChildren<UILabel>();
			}
			if (label != null)
			{
				mDefaultText = label.text;
				mDefaultColor = label.color;
				label.supportEncoding = false;
				mPivot = label.pivot;
				mPosition = label.cachedTransform.localPosition.x;
			}
			else
			{
				base.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		if (UICamera.IsHighlighted(base.gameObject))
		{
			OnSelect(isSelected: true);
		}
	}

	private void OnDisable()
	{
		if (UICamera.IsHighlighted(base.gameObject))
		{
			OnSelect(isSelected: false);
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (mDoInit)
		{
			Init();
		}
		if (!(label != null) || !base.enabled || !NGUITools.GetActive(base.gameObject))
		{
			return;
		}
		if (isSelected)
		{
			mText = ((!(label.text == mDefaultText)) ? label.text : string.Empty);
			label.color = activeColor;
			if (isPassword)
			{
				label.password = true;
			}
			Input.imeCompositionMode = IMECompositionMode.On;
			Transform cachedTransform = label.cachedTransform;
			Vector3 position = label.pivotOffset;
			position.y += label.relativeSize.y;
			position = cachedTransform.TransformPoint(position);
			Input.compositionCursorPos = UICamera.currentCamera.WorldToScreenPoint(position);
			UpdateLabel();
			return;
		}
		if (string.IsNullOrEmpty(mText))
		{
			label.text = mDefaultText;
			label.color = mDefaultColor;
			if (isPassword)
			{
				label.password = false;
			}
		}
		else
		{
			label.text = mText;
		}
		label.showLastPasswordChar = false;
		Input.imeCompositionMode = IMECompositionMode.Off;
		RestoreLabel();
	}

	private void Update()
	{
		if (selected && mLastIME != Input.compositionString)
		{
			mLastIME = Input.compositionString;
			UpdateLabel();
		}
		if (selected && Input.GetKeyDown(KeyCode.Tab))
		{
			InputFoceManager.Instance.MoveNext(this);
			Input.ResetInputAxes();
		}
		if (!Input.GetKeyDown(KeyCode.LeftControl) || !bUseClipboard)
		{
			return;
		}
		string text = Clipboard.GetText();
		if (text == null || text.Length <= 0)
		{
			return;
		}
		string text2 = string.Empty;
		string text3 = text;
		foreach (char c in text3)
		{
			switch (c)
			{
			case '\t':
				text2 += ' ';
				break;
			default:
				text2 += c;
				break;
			case '\b':
			case '\n':
			case '\r':
				break;
			}
		}
		OnInput(text2);
	}

	private void OnInput(string input)
	{
		if (mDoInit)
		{
			Init();
		}
		if (input == null)
		{
			Debug.Log("UIInput.OnInput: string is null! from " + base.gameObject.name);
		}
		else
		{
			if (!selected || !base.enabled || !NGUITools.GetActive(base.gameObject) || UnityEngine.Application.platform == RuntimePlatform.Android || UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer)
			{
				return;
			}
			int i = 0;
			for (int length = input.Length; i < length; i++)
			{
				char c = input[i];
				if (c == '\b')
				{
					if (mText.Length > 0)
					{
						mText = mText.Substring(0, mText.Length - 1);
						SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (c == '\r' || c == '\n')
				{
					if (!m_NoSubmit && (UICamera.current.submitKey0 == KeyCode.Return || UICamera.current.submitKey1 == KeyCode.Return) && (!label.multiLine || (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))))
					{
						current = this;
						if (onSubmit != null)
						{
							onSubmit(mText);
						}
						if (eventReceiver == null)
						{
							eventReceiver = base.gameObject;
						}
						eventReceiver.SendMessage(functionName, mText, SendMessageOptions.DontRequireReceiver);
						current = null;
						selected = false;
						return;
					}
					if (validator != null)
					{
						c = validator(mText, c);
					}
					if (c == '\0')
					{
						continue;
					}
					if (c == '\n' || c == '\r')
					{
						if (label.multiLine)
						{
							mText += "\n";
						}
					}
					else
					{
						mText += c;
					}
					SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
				}
				else if (c >= ' ')
				{
					if (validator != null)
					{
						c = validator(mText, c);
					}
					if (c != 0)
					{
						mText += c;
						SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			UpdateLabel();
		}
	}

	private void UpdateLabel()
	{
		if (mDoInit)
		{
			Init();
		}
		if (maxChars > 0 && mText.Length > maxChars)
		{
			mText = mText.Substring(0, maxChars);
		}
		if (!(label.font != null))
		{
			return;
		}
		string text = ((!selected) ? mText : (mText + Input.compositionString + caratChar));
		label.supportEncoding = false;
		if (label.multiLine)
		{
			text = label.font.WrapText(text, (float)label.lineWidth / label.cachedTransform.localScale.x, 0, encoding: false, UIFont.SymbolStyle.None);
		}
		else
		{
			string endOfLineThatFits = label.font.GetEndOfLineThatFits(text, (float)label.lineWidth / label.cachedTransform.localScale.x, encoding: false, UIFont.SymbolStyle.None);
			if (endOfLineThatFits != text)
			{
				text = endOfLineThatFits;
				Vector3 localPosition = label.cachedTransform.localPosition;
				localPosition.x = mPosition + (float)label.lineWidth;
				label.cachedTransform.localPosition = localPosition;
				if (mPivot == UIWidget.Pivot.Left)
				{
					label.pivot = UIWidget.Pivot.Right;
				}
				else if (mPivot == UIWidget.Pivot.TopLeft)
				{
					label.pivot = UIWidget.Pivot.TopRight;
				}
				else if (mPivot == UIWidget.Pivot.BottomLeft)
				{
					label.pivot = UIWidget.Pivot.BottomLeft;
				}
			}
			else
			{
				RestoreLabel();
			}
		}
		label.text = text;
		label.showLastPasswordChar = selected;
	}

	private void RestoreLabel()
	{
		if (label != null)
		{
			label.pivot = mPivot;
			Vector3 localPosition = label.cachedTransform.localPosition;
			localPosition.x = mPosition;
			label.cachedTransform.localPosition = localPosition;
		}
	}
}
