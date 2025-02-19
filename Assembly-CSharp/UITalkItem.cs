using UnityEngine;

public class UITalkItem : MonoBehaviour
{
	public UILabel mText;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetText(string strtext)
	{
		if (!(mText == null))
		{
			mText.text = strtext;
		}
	}
}
