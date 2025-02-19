using UnityEngine;

public class UIMissionGoalItem : MonoBehaviour
{
	[SerializeField]
	private UITable goalItemRoot;

	[SerializeField]
	private UILabel goalItemTextLb;

	[SerializeField]
	private UISprite goalIconSprite;

	[SerializeField]
	private GameObject goalBoolRoot;

	[SerializeField]
	private UILabel goalBoolTextLb;

	[SerializeField]
	private UICheckbox goalBoolCb;

	public int index;

	public int value0;

	public int value1;

	public Color textColor
	{
		get
		{
			return goalItemTextLb.color;
		}
		set
		{
			goalItemTextLb.color = value;
			goalBoolTextLb.color = value;
		}
	}

	public string itemText
	{
		get
		{
			return goalItemTextLb.text;
		}
		set
		{
			goalItemTextLb.text = value;
		}
	}

	public void SetItemContent(string text, string sprite_name)
	{
		if (!goalItemRoot.gameObject.activeSelf)
		{
			goalItemRoot.gameObject.SetActive(value: true);
		}
		goalItemTextLb.text = text;
		if (!goalIconSprite.gameObject.activeSelf)
		{
			goalIconSprite.gameObject.SetActive(value: true);
		}
		goalIconSprite.spriteName = sprite_name;
		if (goalBoolRoot.activeSelf)
		{
			goalBoolRoot.SetActive(value: false);
		}
		goalItemRoot.Reposition();
	}

	public void SetItemContent(string text)
	{
		if (!goalItemRoot.gameObject.activeSelf)
		{
			goalItemRoot.gameObject.SetActive(value: true);
		}
		goalItemTextLb.text = text;
		if (goalIconSprite.gameObject.activeSelf)
		{
			goalIconSprite.gameObject.SetActive(value: false);
		}
		if (goalBoolRoot.activeSelf)
		{
			goalBoolRoot.SetActive(value: false);
		}
		goalItemRoot.Reposition();
	}

	public void SetBoolContent(string text, bool is_achived)
	{
		if (!goalBoolRoot.activeSelf)
		{
			goalBoolRoot.SetActive(value: true);
		}
		goalBoolTextLb.text = text;
		goalBoolCb.isChecked = is_achived;
		if (goalItemRoot.gameObject.activeSelf)
		{
			goalItemRoot.gameObject.SetActive(value: false);
		}
	}
}
