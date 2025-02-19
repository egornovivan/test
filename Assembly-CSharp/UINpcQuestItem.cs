using System;
using UnityEngine;

public class UINpcQuestItem : MonoBehaviour
{
	[SerializeField]
	private UILabel textLabel;

	[SerializeField]
	private UISprite titleIcon;

	public int index;

	public Action<UINpcQuestItem> onClick;

	public string test
	{
		get
		{
			return textLabel.text;
		}
		set
		{
			textLabel.text = value;
		}
	}

	private void OnBtnClick()
	{
		if (onClick != null)
		{
			onClick(this);
		}
	}
}
