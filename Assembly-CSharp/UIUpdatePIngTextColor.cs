using System;
using UnityEngine;

public class UIUpdatePIngTextColor : MonoBehaviour
{
	public UIListItemCtrl mListItemCtrl;

	private UILabel UIPingText;

	private void Start()
	{
		if (mListItemCtrl != null)
		{
			UIPingText = mListItemCtrl.mLabelList[6].GetComponent<UILabel>();
		}
	}

	private void Update()
	{
		if (!(UIPingText == null) && UIPingText.text.Length > 0)
		{
			int num = -1;
			try
			{
				num = Convert.ToInt32(UIPingText.text);
			}
			catch
			{
				return;
			}
			if (num < 151)
			{
				UIPingText.color = new Color(2f / 51f, 41f / 51f, 14f / 85f);
			}
			else if (num < 251)
			{
				UIPingText.color = new Color(1f, 73f / 85f, 36f / 85f);
			}
			else
			{
				UIPingText.color = Color.red;
			}
		}
	}
}
