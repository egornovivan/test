using UnityEngine;

public class UISkillTypeBtn : MonoBehaviour
{
	public delegate void DClickEvent(UISkillTypeBtn btn);

	[SerializeField]
	private UISlicedSprite uiSprite;

	public Color enableColor = Color.white;

	public Color disableColor = Color.white;

	public Color hoverColor = Color.blue;

	public int index = -1;

	private Color _color = Color.white;

	public string spriteName
	{
		get
		{
			return uiSprite.spriteName;
		}
		set
		{
			uiSprite.spriteName = value;
		}
	}

	public event DClickEvent onBtnClick;

	public void SetEnable(bool enable)
	{
		if (enable)
		{
			uiSprite.color = enableColor;
		}
		else
		{
			uiSprite.color = disableColor;
		}
		_color = uiSprite.color;
	}

	private void OnClick()
	{
		if (this.onBtnClick != null)
		{
			this.onBtnClick(this);
		}
		Debug.Log("Click Skill Type Button");
	}

	private void OnHover(bool isOver)
	{
		if (isOver)
		{
			uiSprite.color = Color.Lerp(_color, hoverColor, 0.2f);
		}
		else
		{
			uiSprite.color = _color;
		}
	}
}
