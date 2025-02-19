using UnityEngine;

public class UILabelTishi : MonoBehaviour
{
	private UILabel mLabel;

	private int tempCount;

	private void Start()
	{
		mLabel = GetComponent<UILabel>();
	}

	public void ShowText(string text)
	{
		tempCount = 0;
		mLabel.enabled = true;
		mLabel.text = text;
	}

	private void Update()
	{
		if (!(mLabel == null))
		{
			if (mLabel.enabled)
			{
				tempCount++;
			}
			else
			{
				tempCount = 0;
			}
			if (tempCount > 100)
			{
				mLabel.enabled = false;
			}
		}
	}
}
