using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UISaveLoad : UIStaticWnd
{
	private const int AutoSaveNum = 3;

	private const int CustomDataNum = 20;

	private static UISaveLoad mInstance;

	public UILabel mSaveLoadBtn;

	public N_ImageButton mDealetBtn;

	public N_ImageButton mSaveBtn;

	public UICheckbox mSaveTabBtn;

	public UICheckbox mLoadTabBtn;

	public UITexture mSaveTex;

	public UIGrid mInfoGrid;

	public UILabel mGametypeText;

	public GameObject mSeedRoot;

	public UILabel mSeedIDText;

	public UILabel mSeedTitleText;

	public UILabel mPlayTimeText;

	public UILabel mGameTimeText;

	public bool mIsSave;

	private int mIndex;

	private Texture2D mAutoTex;

	public SaveDateItem_N mDataPrefab;

	public UIGrid mDataGrid;

	private List<SaveDateItem_N> mDataList;

	public static UISaveLoad Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mIndex = 0;
		mSaveTex.enabled = false;
		mAutoTex = Resources.Load("Texture2d/Tex/AutoSave") as Texture2D;
		mDataList = new List<SaveDateItem_N>();
		int num = 23;
		for (int i = 0; i < num; i++)
		{
			SaveDateItem_N saveDateItem_N = Object.Instantiate(mDataPrefab);
			saveDateItem_N.transform.parent = mDataGrid.transform;
			saveDateItem_N.transform.localScale = Vector3.one;
			saveDateItem_N.transform.localPosition = Vector3.back;
			if (i < 3)
			{
				saveDateItem_N.mIndexTex.text = "Auto" + (i + 1);
			}
			else
			{
				saveDateItem_N.mIndexTex.text = (i - 3 + 1).ToString();
			}
			saveDateItem_N.mCheckbox.radioButtonRoot = mDataGrid.transform;
			saveDateItem_N.Init(i, delegate(int index, PeGameSummary summary)
			{
				mIndex = index;
				ChangeArvhive(summary);
			});
			mDataList.Add(saveDateItem_N);
		}
		mDataGrid.Reposition();
	}

	private void Update()
	{
		if (mWndCenter.activeSelf && Input.GetKeyDown(KeyCode.Escape))
		{
			OnQuitBtn();
		}
		if (mIsSave && mIndex < 3)
		{
			mSaveBtn.isEnabled = false;
			mDealetBtn.isEnabled = false;
			return;
		}
		if (mIndex >= 0 && mIndex < mDataList.Count)
		{
			mDealetBtn.isEnabled = null != mDataList[mIndex].Summary;
		}
		mSaveBtn.isEnabled = true;
	}

	public void ToSaveWnd()
	{
		mLoadTabBtn.startsChecked = false;
		UICheckbox uICheckbox = mSaveTabBtn;
		bool flag = true;
		mSaveTabBtn.isChecked = flag;
		uICheckbox.startsChecked = flag;
		OnSaveTabBtn();
	}

	public void ToLoadWnd()
	{
		mSaveTabBtn.startsChecked = false;
		UICheckbox uICheckbox = mLoadTabBtn;
		bool flag = true;
		mLoadTabBtn.isChecked = flag;
		uICheckbox.startsChecked = flag;
		OnLoadTabBtn();
	}

	private void OnSaveTabBtn()
	{
		mIsSave = true;
		mSaveLoadBtn.text = PELocalization.GetString(2000058);
		mDealetBtn.gameObject.SetActive(value: true);
		UpdateArchiveList();
	}

	private void OnLoadTabBtn()
	{
		mIsSave = false;
		mSaveLoadBtn.text = PELocalization.GetString(8000419);
		mDealetBtn.gameObject.SetActive(value: false);
		UpdateArchiveList();
	}

	public void HideSaveTabBtn()
	{
		mSaveTabBtn.gameObject.SetActive(value: false);
		mLoadTabBtn.gameObject.SetActive(value: true);
		mLoadTabBtn.transform.localPosition = mSaveTabBtn.transform.localPosition;
	}

	private void UpdateArchiveList()
	{
		for (int i = 0; i < 23; i++)
		{
			string value = PeSingleton<ArchiveMgr>.Instance.Load((ArchiveMgr.ESave)i);
			PeGameSummary archive = null;
			if (!string.IsNullOrEmpty(value))
			{
				archive = PeSingleton<PeGameSummary.Mgr>.Instance.Get();
			}
			mDataList[i].SetArchive(archive);
		}
		ChangeArvhive(mDataList[mIndex].Summary);
	}

	private void OnSaveLoadBtn()
	{
		if (mIndex >= mDataList.Count)
		{
			return;
		}
		if (mIsSave)
		{
			if (mIndex >= 3)
			{
				SaveData();
			}
		}
		else if (!(mDataList[mIndex] == null) && mDataList[mIndex].Summary != null)
		{
			LoadData();
		}
	}

	private void SaveData()
	{
		if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial || (PeGameMgr.IsSingleAdventure && PeGameMgr.yirdName == AdventureScene.Dungen.ToString()))
		{
			new PeTipMsg("[C8C800]" + PELocalization.GetString(8000852), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
			return;
		}
		PeSingleton<ArchiveMgr>.Instance.Save((ArchiveMgr.ESave)mIndex);
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000029));
		UpdateArchiveList();
	}

	private void OnDeletBtn()
	{
		PeSingleton<ArchiveMgr>.Instance.Delete((ArchiveMgr.ESave)mIndex);
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000030));
		UpdateArchiveList();
	}

	private void OnQuitBtn()
	{
		Hide();
		if (null != GameUI.Instance)
		{
			Hide();
		}
		else
		{
			TitleMenuGui_N.Instance.Show();
		}
	}

	private void ChangeArvhive(PeGameSummary summary)
	{
		if (summary != null)
		{
			switch (summary.sceneMode)
			{
			case PeGameMgr.ESceneMode.Story:
				mGametypeText.text = PELocalization.GetString(10007);
				mSeedRoot.SetActive(value: false);
				break;
			case PeGameMgr.ESceneMode.Adventure:
				mGametypeText.text = PELocalization.GetString(10008);
				mSeedIDText.text = summary.seed;
				mSeedTitleText.text = PELocalization.GetString(8000361) + ":";
				mSeedRoot.SetActive(value: true);
				break;
			case PeGameMgr.ESceneMode.Build:
				mGametypeText.text = PELocalization.GetString(10009);
				mSeedIDText.text = summary.seed;
				mSeedTitleText.text = PELocalization.GetString(8000361) + ":";
				mSeedRoot.SetActive(value: true);
				break;
			case PeGameMgr.ESceneMode.Custom:
				mGametypeText.text = PELocalization.GetString(10222);
				mSeedIDText.text = PELocalization.GetString(8000558);
				mSeedTitleText.text = PELocalization.GetString(8000557);
				mSeedRoot.SetActive(value: false);
				break;
			}
			mInfoGrid.repositionNow = true;
			UTimer uTimer = new UTimer();
			uTimer.Second = summary.playTime;
			if (uTimer.Day < 1.0)
			{
				mPlayTimeText.text = uTimer.FormatString("hh:mm:ss");
			}
			else
			{
				mPlayTimeText.text = uTimer.FormatString("D days hh:mm:ss");
			}
			PETimer tmpTimer = PETimerUtil.GetTmpTimer();
			tmpTimer.Second = summary.gameTime;
			mGameTimeText.text = tmpTimer.FormatString("hh:mm:ss AP");
			mSaveTex.enabled = true;
			mSaveTex.mainTexture = ((!(summary.screenshot != null)) ? mAutoTex : summary.screenshot);
		}
		else
		{
			mGametypeText.text = string.Empty;
			mSeedIDText.text = string.Empty;
			mPlayTimeText.text = string.Empty;
			mGameTimeText.text = string.Empty;
			mSaveTex.enabled = false;
			mSaveTex.mainTexture = null;
		}
	}

	private void LoadData()
	{
		ArchiveMgr.ESave eSave = (ArchiveMgr.ESave)mIndex;
		if (!string.IsNullOrEmpty(PeSingleton<ArchiveMgr>.Instance.Load(eSave)))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
			PeGameMgr.loadArchive = eSave;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene, eSave != ArchiveMgr.ESave.Min);
		}
	}
}
