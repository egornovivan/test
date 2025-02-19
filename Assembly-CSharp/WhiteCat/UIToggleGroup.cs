using UnityEngine;

namespace WhiteCat;

public class UIToggleGroup : MonoBehaviour
{
	[SerializeField]
	private UIButton[] _buttons;

	[HideInInspector]
	[SerializeField]
	private int _selected = -1;

	private static Color normalColor = new Color(0.69f, 0.69f, 0.69f, 1f);

	private static Color selectedColor = new Color(0f, 0.9f, 1f, 1f);

	public int selected
	{
		get
		{
			return _selected;
		}
		set
		{
			if (_selected != value)
			{
				if (_selected >= 0)
				{
					_buttons[_selected].tweenTarget.GetComponent<UIWidget>().color = normalColor;
					_buttons[_selected].defaultColor = normalColor;
				}
				_selected = value;
				if (_selected >= 0)
				{
					_buttons[_selected].tweenTarget.GetComponent<UIWidget>().color = selectedColor;
					_buttons[_selected].defaultColor = selectedColor;
				}
			}
		}
	}
}
