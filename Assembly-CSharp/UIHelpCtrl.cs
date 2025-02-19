using System.Collections.Generic;
using System.Linq;
using Pathea;
using UnityEngine;

public class UIHelpCtrl : UIBaseWidget
{
	[SerializeField]
	private UIGrid mHelpGrid;

	[SerializeField]
	private TutorialItem_N mPerfab;

	[SerializeField]
	private UITexture mHelpTex;

	[SerializeField]
	private UILabel mTitle;

	[SerializeField]
	private UIPhoneWnd mPhoneWnd;

	private List<TutorialItem_N> mTitleList = new List<TutorialItem_N>();

	private TutorialItem_N mBackUpItem;

	private int mTabIndex;

	private int mCurrentID = -1;

	private bool UpdateHelpGrid;

	public override void OnCreate()
	{
		TutorialData.e_OnAddActiveID += OnAddActiveID;
		base.OnCreate();
	}

	public override void OnDelete()
	{
		TutorialData.e_OnAddActiveID -= OnAddActiveID;
		base.OnDelete();
	}

	private void AddAllTutorIDByGameMode()
	{
		if (PeGameMgr.sceneMode != 0 && TutorialData.s_tblTutorialData.Count > 0)
		{
			int[] array = TutorialData.s_tblTutorialData.Keys.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (!TutorialData.m_ActiveIDList.Contains(array[i]) && !TutorialData.SkillIDs.Contains(array[i]))
				{
					TutorialData.m_ActiveIDList.Add(array[i]);
				}
			}
		}
		if (!PeGameMgr.IsAdventure || !RandomMapConfig.useSkillTree)
		{
			return;
		}
		for (int j = 0; j < TutorialData.SkillIDs.Length; j++)
		{
			if (!TutorialData.m_ActiveIDList.Contains(TutorialData.SkillIDs[j]))
			{
				TutorialData.m_ActiveIDList.Add(TutorialData.SkillIDs[j]);
			}
		}
	}

	private void LateUpdate()
	{
		if (UpdateHelpGrid)
		{
			mHelpGrid.repositionNow = true;
		}
	}

	public override void Show()
	{
		base.Show();
		ResetTutorial();
	}

	protected override void InitWindow()
	{
		AddAllTutorIDByGameMode();
		base.InitWindow();
	}

	private void ResetTutorial()
	{
		List<int> list = new List<int>();
		foreach (TutorialItem_N mTitle in mTitleList)
		{
			mTitle.transform.parent = null;
			Object.Destroy(mTitle.gameObject);
		}
		mTitleList.Clear();
		foreach (int activeID in TutorialData.m_ActiveIDList)
		{
			if (TutorialData.s_tblTutorialData.ContainsKey(activeID) && TutorialData.s_tblTutorialData[activeID].mType == mTabIndex + 1)
			{
				list.Add(activeID);
			}
		}
		List<int> allId = TutorialData.s_tblTutorialData.Keys.ToList();
		list = list.OrderBy((int a) => allId.FindIndex((int b) => b == a)).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			TutorialItem_N tutorialItem_N = Object.Instantiate(mPerfab);
			tutorialItem_N.transform.parent = mHelpGrid.transform;
			tutorialItem_N.transform.localScale = Vector3.one;
			tutorialItem_N.transform.localPosition = Vector3.zero;
			tutorialItem_N.mCheckBox.radioButtonRoot = null;
			tutorialItem_N.mCheckBox.startsChecked = false;
			tutorialItem_N.mCheckBox.isChecked = false;
			tutorialItem_N.SetItem(list[i], TutorialData.s_tblTutorialData[list[i]].mContent);
			tutorialItem_N.e_OnClick += OnClickGridItem;
			mTitleList.Add(tutorialItem_N);
		}
		if (mTitleList.Count > 0)
		{
			mTitleList[0].mCheckBox.isChecked = true;
			OnClickGridItem(mTitleList[0]);
		}
		UpdateHelpGrid = true;
	}

	private void OnClickGridItem(object sender)
	{
		TutorialItem_N tutorialItem_N = sender as TutorialItem_N;
		if (!(tutorialItem_N == null))
		{
			ChangeSelect(tutorialItem_N.mID);
		}
	}

	public void ChangeSelect(int ID)
	{
		if (!TutorialData.s_tblTutorialData.ContainsKey(ID))
		{
			mHelpTex.enabled = false;
			return;
		}
		if (null != mBackUpItem)
		{
			mBackUpItem.mCheckBox.isChecked = false;
		}
		TutorialItem_N itemByID = GetItemByID(ID);
		if (null != itemByID)
		{
			itemByID.mCheckBox.isChecked = true;
		}
		mBackUpItem = itemByID;
		mHelpTex.enabled = true;
		mCurrentID = ID;
		mTitle.text = TutorialData.s_tblTutorialData[ID].mContent;
		if (null != mHelpTex.mainTexture)
		{
			Object.Destroy(mHelpTex.mainTexture);
		}
		string text = ((!SystemSettingData.Instance.IsChinese) ? "Texture2d/HelpTex_English/" : "Texture2d/HelpTex_Chinese/");
		text += TutorialData.s_tblTutorialData[ID].mTexName;
		Texture texture = Resources.Load(text) as Texture;
		mHelpTex.mainTexture = Object.Instantiate(texture);
		Resources.UnloadAsset(texture);
	}

	private TutorialItem_N GetItemByID(int id)
	{
		return mTitleList.Find((TutorialItem_N a) => a.mID == id);
	}

	private void OnAddActiveID(int id)
	{
		if (!(mPhoneWnd == null))
		{
			mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
			ChangeSelect(id);
		}
	}
}
