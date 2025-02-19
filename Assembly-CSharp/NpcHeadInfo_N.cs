using UnityEngine;

public class NpcHeadInfo_N : MonoBehaviour
{
	public UILabel mNameText;

	public UISprite mIconSpr;

	public void SetInfo(string name, string iconName)
	{
		mNameText.text = name;
		mNameText.MakePixelPerfect();
		mIconSpr.spriteName = iconName;
		mIconSpr.MakePixelPerfect();
		float num = mNameText.font.CalculatePrintedSize(name, encoding: true, UIFont.SymbolStyle.None).x * (float)mNameText.font.size;
		float x = mIconSpr.transform.localScale.x;
		float num2 = num + x;
		mIconSpr.transform.localPosition = new Vector3((0f - num2) / 2f - 2f, 0f, 0f);
		mNameText.transform.localPosition = new Vector3(x - num2 / 2f + 2f, 0f, 0f);
	}
}
