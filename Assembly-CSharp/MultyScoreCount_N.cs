using System.Collections.Generic;
using UnityEngine;

public class MultyScoreCount_N : UIBaseWnd
{
	public UILabel mKillGroupTitle;

	public UILabel mScoreGroupTitle;

	public UILabel mKillNumContent;

	public UILabel mScoreNumContent;

	public UILabel mConditionContent;

	public UILabel mScoreRule;

	public UILabel mMeatRule;

	public UISprite mResultSpr;

	public UISprite mTopBg;

	public UILabel mTopTitle;

	public PersonalWnd_N mPerfab;

	public UIGrid mGrid;

	private List<PersonalWnd_N> mWndList = new List<PersonalWnd_N>();

	public bool ReAwake;

	public static string EncodeColor(Color32 col32)
	{
		Color c = new Color((float)(int)col32.r / 255f, (float)(int)col32.g / 255f, (float)(int)col32.b / 255f, (float)(int)col32.a / 255f);
		int num = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
		return NGUIMath.DecimalToHex(num);
	}

	public override void Show()
	{
		base.Show();
		mScoreRule.text = BattleConstData._scoreInfo;
		mMeatRule.text = BattleConstData._meatInfo;
		mConditionContent.text = "Score: " + BattleConstData.Instance._win_point + "     Kills: " + BattleConstData.Instance._win_kill + "     Hold: " + BattleConstData.Instance._win_site + "Field";
		for (int num = mWndList.Count - 1; num >= 0; num--)
		{
			mWndList[num].transform.parent = null;
			Object.Destroy(mWndList[num].gameObject);
		}
		mWndList.Clear();
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		foreach (int key in BattleManager.CampList.Keys)
		{
			string text4 = text;
			text = text4 + "[" + EncodeColor(Singleton<ForceSetting>.Instance.GetForceColor(key)) + "]TEAM" + (key + 1) + "[-] / ";
			text4 = text2;
			text2 = text4 + "[" + EncodeColor(Singleton<ForceSetting>.Instance.GetForceColor(key)) + "]" + BattleManager.CampList[key]._killCount + "[-] / ";
			text4 = text3;
			text3 = text4 + "[" + EncodeColor(Singleton<ForceSetting>.Instance.GetForceColor(key)) + "]" + (int)BattleManager.CampList[key]._point + "[-] / ";
		}
		if (text != string.Empty)
		{
			text = text.Remove(text.Length - 3);
			text2 = text2.Remove(text2.Length - 3);
			text3 = text3.Remove(text3.Length - 3);
		}
		mKillGroupTitle.text = text;
		mScoreGroupTitle.text = text;
		mKillNumContent.text = text2;
		mScoreNumContent.text = text3;
		foreach (KeyValuePair<int, List<PlayerBattleInfo>> item in BattleManager.BattleInfoDict)
		{
			if (item.Value != null)
			{
				PersonalWnd_N personalWnd_N = Object.Instantiate(mPerfab);
				personalWnd_N.transform.parent = mGrid.transform;
				personalWnd_N.transform.localPosition = Vector3.zero;
				personalWnd_N.transform.localScale = Vector3.one;
				personalWnd_N.mGroupName.text = "[" + EncodeColor(Singleton<ForceSetting>.Instance.GetForceColor(item.Key)) + "]TEAM" + (item.Key + 1) + "[-]";
				personalWnd_N.Init();
				personalWnd_N.SetInfo(item.Value);
				mWndList.Add(personalWnd_N);
			}
		}
		mGrid.Reposition();
	}

	public void SetGameResult(bool isWinner)
	{
		mResultSpr.gameObject.SetActive(value: true);
		if (isWinner)
		{
			mResultSpr.spriteName = "Title_Victory";
		}
		else
		{
			mResultSpr.spriteName = "Title_Defeat";
		}
		mTopBg.gameObject.SetActive(value: false);
		mTopTitle.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (ReAwake)
		{
			ReAwake = false;
			Show();
		}
	}
}
