using System.Collections.Generic;
using UnityEngine;

public class MouseActionWnd_N : MonoBehaviour
{
	private const float BorderLength = 12f;

	private static MouseActionWnd_N mInstance;

	public Transform mWnd;

	public UILabel mText;

	public UISlicedSprite mBgSpr;

	public UISprite mSprPrefab;

	private List<UISprite> mSprList;

	private string mCurrentContent = string.Empty;

	public static MouseActionWnd_N Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mSprList = new List<UISprite>();
	}

	private void Update()
	{
		mWnd.localPosition = PeCamera.mousePos + 10f * Vector3.forward;
	}

	public void SetText(string content)
	{
		if (string.IsNullOrEmpty(content) || !(mCurrentContent != content))
		{
			return;
		}
		Clear();
		mCurrentContent = content;
		mText.enabled = true;
		mBgSpr.enabled = true;
		float num = 0f;
		string[] array = content.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Contains("$"))
			{
				string text = array[i].Split('$')[1];
				array[i] = array[i].Replace("$" + text + "$", string.Empty);
				float num2 = mText.font.CalculatePrintedSize(array[i], encoding: true, UIFont.SymbolStyle.None).x * (float)mText.font.size;
				UISprite uISprite = Object.Instantiate(mSprPrefab);
				uISprite.transform.parent = mWnd;
				uISprite.transform.localScale = Vector3.zero;
				uISprite.transform.localPosition = new Vector3(12f + num2 + 2f, -12f - (float)(i * mText.font.size) - 2f);
				uISprite.spriteName = text;
				uISprite.MakePixelPerfect();
				num2 += uISprite.transform.localScale.x + 2f;
				mSprList.Add(uISprite);
				if (num2 > num)
				{
					num = num2;
				}
			}
			else
			{
				float num3 = mText.font.CalculatePrintedSize(array[i], encoding: true, UIFont.SymbolStyle.None).x * (float)mText.font.size;
				if (num3 > num)
				{
					num = num3;
				}
			}
		}
		mBgSpr.transform.localScale = new Vector3(num + 24f, (float)(array.Length * mText.font.size) + 24f, 1f);
		string text2 = array[0];
		for (int j = 1; j < array.Length; j++)
		{
			text2 = text2 + "\n" + array[j];
		}
		mText.text = text2;
	}

	public void Clear()
	{
		foreach (UISprite mSpr in mSprList)
		{
			Object.Destroy(mSpr.gameObject);
		}
		mSprList.Clear();
		mText.enabled = false;
		mBgSpr.enabled = false;
		mCurrentContent = string.Empty;
	}
}
