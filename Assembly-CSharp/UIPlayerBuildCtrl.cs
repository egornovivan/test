using System;
using System.Collections.Generic;
using AppearBlendShape;
using CustomCharactor;
using Pathea;
using UnityEngine;

public class UIPlayerBuildCtrl : MonoBehaviour
{
	public class ScrollItem
	{
		public UIScrollBar mScrollBar;

		public EMorphItem mType;

		public ScrollItem(UIScrollBar bar, EMorphItem type)
		{
			mScrollBar = bar;
			mType = type;
		}
	}

	public enum ESex
	{
		Def,
		Female,
		Male
	}

	[SerializeField]
	private UIInput mNameInput;

	[SerializeField]
	private UIPageGridBox mHeadGridBox;

	[SerializeField]
	private UIPageGridBox mFaceGridBox;

	[SerializeField]
	private Palette_N mSkinColor;

	[SerializeField]
	private UIPageGridBox mHairGridBox;

	[SerializeField]
	private Palette_N mHairColor;

	[SerializeField]
	private UIScrollBar mFaceWidth;

	[SerializeField]
	private UIScrollBar mFaceThickness;

	[SerializeField]
	private UIScrollBar mChinWidth;

	[SerializeField]
	private UIScrollBar mJawWidth;

	[SerializeField]
	private UIScrollBar mEyebrowsLocation;

	[SerializeField]
	private UIScrollBar mEyebrowsDirection;

	[SerializeField]
	private UIScrollBar mEyesDirection;

	[SerializeField]
	private UIScrollBar mEyesSize;

	[SerializeField]
	private Palette_N mEyeballsColor;

	[SerializeField]
	private UIScrollBar mNoseLocation;

	[SerializeField]
	private UIScrollBar mNoseHeight;

	[SerializeField]
	private UIScrollBar mNoseSize;

	[SerializeField]
	private UIScrollBar mMouthLocation;

	[SerializeField]
	private UIScrollBar mMouthSize;

	[SerializeField]
	private UIScrollBar mMouthShap;

	[SerializeField]
	private UIScrollBar mShoulder;

	[SerializeField]
	private UIScrollBar mBreast;

	[SerializeField]
	private UIScrollBar mUpperArm;

	[SerializeField]
	private UIScrollBar mLowerArm;

	[SerializeField]
	private UIScrollBar mBelly;

	[SerializeField]
	private UIScrollBar mWaist;

	[SerializeField]
	private UIScrollBar mUpperLeg;

	[SerializeField]
	private UIScrollBar mLowerLeg;

	[SerializeField]
	private Palette_N mSkinColor2;

	[SerializeField]
	private UIPageGridBox mSaveGrodBox;

	[SerializeField]
	private UIBtnTouZi mBtnRandom;

	[SerializeField]
	private UILabelTishi mLbTishi;

	[SerializeField]
	private GameObject mMalePrefab;

	[SerializeField]
	private GameObject mFemalePrefab;

	[SerializeField]
	private UISprite mBtnMaleBg;

	[SerializeField]
	private UISprite mBtnFemaleBg;

	[SerializeField]
	private BoxCollider mBtnMaleCollider;

	[SerializeField]
	private BoxCollider mBtnFemaleCollider;

	[SerializeField]
	private Camera mUICamera;

	[SerializeField]
	private GameObject mUIMapSelect;

	[HideInInspector]
	public bool haschanged;

	private List<PlayerBuildGirdItem> mHeadList;

	private List<PlayerBuildGirdItem> mFaceList;

	private List<PlayerBuildGirdItem> mHairList;

	private List<PlayerBuildGirdItem> mSaveList;

	private List<ScrollItem> mScrollItemList;

	private bool mInitEnd;

	private Vector3 mBodyCamPos;

	private int mCameraState;

	private UIPlayerBuildMoveCtrl mMoveCtrl;

	private PlayerModel mMaleInfo;

	private PlayerModel mFemaleInfo;

	private PlayerModel mCurrent;

	private CustomMetaData mMetaData;

	private Vector3 mTargetCamPos;

	private float mRotSpeed = 90f;

	private bool mRotLeft;

	private bool mRotRight;

	private float starDis;

	private Vector3 OffSetPos = new Vector3(0f, 0.12f, 0.6f);

	public bool actionOk = true;

	private PlayerBuildGirdItem mHeadGridSelectedItem;

	private PlayerBuildGirdItem mHairGridSelectedItem;

	private PlayerBuildGirdItem mSaveGridSelectedItem;

	private int mSaveGridSelectedIndex = -1;

	private ESex Sex { get; set; }

	private void Awake()
	{
		InitPlayer();
		mBodyCamPos = Camera.main.transform.position;
		CustomDataMgr.Instance.LoadAllData();
		InitGirdBox(mHeadGridBox, out mHeadList, PlayerBuildGirdItem.Type.Type_Head);
		InitGirdBox(mFaceGridBox, out mFaceList, PlayerBuildGirdItem.Type.Type_Face);
		InitGirdBox(mHairGridBox, out mHairList, PlayerBuildGirdItem.Type.Type_Hair);
		InitGirdBox(mSaveGrodBox, out mSaveList, PlayerBuildGirdItem.Type.Type_Save);
		mHeadGridBox.e_RefalshGrid += ReflashHeadGridBox;
		mFaceGridBox.e_RefalshGrid += ReflashFaceGridBox;
		mHairGridBox.e_RefalshGrid += ReflashHairGridBox;
		mSaveGrodBox.e_RefalshGrid += ReflashSaveGridBox;
		mSkinColor.e_ChangeColor += SetSkinColor;
		mSkinColor2.e_ChangeColor += SetSkinColor;
		mEyeballsColor.e_ChangeColor += SetEyeColor;
		mHairColor.e_ChangeColor += SetHairColor;
		mBtnRandom.e_EndRun += BtnRandomOnClick;
		mMoveCtrl = base.gameObject.GetComponent<UIPlayerBuildMoveCtrl>();
		InitScrolleBar();
		foreach (ScrollItem mScrollItem in mScrollItemList)
		{
			UIEventListener uIEventListener = UIEventListener.Get(mScrollItem.mScrollBar.background.gameObject);
			UIEventListener uIEventListener2 = uIEventListener;
			uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnScrollBarPress));
			uIEventListener = UIEventListener.Get(mScrollItem.mScrollBar.foreground.gameObject);
			UIEventListener uIEventListener3 = uIEventListener;
			uIEventListener3.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onPress, new UIEventListener.BoolDelegate(OnScrollBarPress));
			UIEventListener uIEventListener4 = uIEventListener;
			uIEventListener4.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener4.onDrag, new UIEventListener.VectorDelegate(OnScrollBarDrag));
		}
	}

	private void OnScrollBarPress(GameObject go, bool isPressed)
	{
		if (CheckNeedSaveTip())
		{
			haschanged = true;
		}
	}

	private void OnScrollBarDrag(GameObject go, Vector2 delta)
	{
		if (CheckNeedSaveTip())
		{
			haschanged = true;
		}
	}

	private void Start()
	{
		BtnMaleOnClick();
		ChangeCameraPos();
		ResetSaveGridBox();
	}

	private void Update()
	{
		if (ChangeAppearData())
		{
			RebuildModel();
		}
		if (mMoveCtrl != null && mMoveCtrl.mCameraState != mCameraState)
		{
			ChangeCameraPos();
		}
		if (Input.GetMouseButton(1))
		{
			if (starDis != 0f)
			{
				mCurrent.mMode.transform.rotation *= Quaternion.AngleAxis((starDis - Input.mousePosition.x) * 20f * Time.deltaTime, mCurrent.mMode.transform.up);
			}
			starDis = Input.mousePosition.x;
		}
		if (Input.GetMouseButtonUp(1))
		{
			starDis = 0f;
		}
		if (isMouseOnClider(mBtnMaleCollider))
		{
			mBtnMaleBg.color = Color.white;
		}
		else
		{
			mBtnMaleBg.color = ((Sex != ESex.Male) ? new Color(0.8f, 0.8f, 0.8f, 1f) : Color.white);
		}
		if (isMouseOnClider(mBtnFemaleCollider))
		{
			mBtnFemaleBg.color = Color.white;
		}
		else
		{
			mBtnFemaleBg.color = ((Sex != ESex.Female) ? new Color(0.8f, 0.8f, 0.8f, 1f) : Color.white);
		}
		if (mRotLeft)
		{
			mCurrent.mMode.transform.rotation *= Quaternion.Euler(0f, mRotSpeed * Time.deltaTime, 0f);
		}
		if (mRotRight)
		{
			mCurrent.mMode.transform.rotation *= Quaternion.Euler(0f, (0f - mRotSpeed) * Time.deltaTime, 0f);
		}
		Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, mTargetCamPos, 6f * Time.deltaTime);
	}

	private bool isMouseOnClider(BoxCollider collider)
	{
		Ray ray = mUICamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		return collider.Raycast(ray, out hitInfo, 100f);
	}

	private void ChangeCameraPos()
	{
		mCameraState = mMoveCtrl.mCameraState;
		switch (mCameraState)
		{
		case 0:
			mTargetCamPos = mBodyCamPos;
			break;
		case 1:
			mTargetCamPos = mCurrent.mHeadTran.position + OffSetPos;
			break;
		}
	}

	private void InitGirdBox(UIPageGridBox gridBox, out List<PlayerBuildGirdItem> gridList, PlayerBuildGirdItem.Type _type)
	{
		gridBox.InitGrid();
		gridList = new List<PlayerBuildGirdItem>();
		for (int i = 0; i < gridBox.mItemsObject.Count; i++)
		{
			PlayerBuildGirdItem component = gridBox.mItemsObject[i].GetComponent<PlayerBuildGirdItem>();
			component.InitItem(i, _type);
			component.SetItemInfo("Null");
			component.e_ClickItem += GridBoxItemOnClick;
			component.canSelected = true;
			if (component != null)
			{
				gridList.Add(component);
			}
		}
	}

	private void InitScrolleBar()
	{
		mScrollItemList = new List<ScrollItem>();
		mScrollItemList.Add(new ScrollItem(mFaceWidth, EMorphItem.Min));
		mScrollItemList.Add(new ScrollItem(mFaceThickness, EMorphItem.FaceThickness));
		mScrollItemList.Add(new ScrollItem(mChinWidth, EMorphItem.ChinWidth));
		mScrollItemList.Add(new ScrollItem(mJawWidth, EMorphItem.JawWidth));
		mScrollItemList.Add(new ScrollItem(mEyebrowsLocation, EMorphItem.EyebrowLocation));
		mScrollItemList.Add(new ScrollItem(mEyebrowsDirection, EMorphItem.EyebrowDirection));
		mScrollItemList.Add(new ScrollItem(mEyesDirection, EMorphItem.EyeDirection));
		mScrollItemList.Add(new ScrollItem(mEyesSize, EMorphItem.EyeSize));
		mScrollItemList.Add(new ScrollItem(mNoseLocation, EMorphItem.NoseLocation));
		mScrollItemList.Add(new ScrollItem(mNoseHeight, EMorphItem.NoseHeight));
		mScrollItemList.Add(new ScrollItem(mNoseSize, EMorphItem.NoseSize));
		mScrollItemList.Add(new ScrollItem(mMouthLocation, EMorphItem.MouthLocation));
		mScrollItemList.Add(new ScrollItem(mMouthSize, EMorphItem.MouthSize));
		mScrollItemList.Add(new ScrollItem(mMouthShap, EMorphItem.MouthShape));
		mScrollItemList.Add(new ScrollItem(mShoulder, EMorphItem.MaxFace));
		mScrollItemList.Add(new ScrollItem(mBreast, EMorphItem.Breast));
		mScrollItemList.Add(new ScrollItem(mUpperArm, EMorphItem.UpperArm));
		mScrollItemList.Add(new ScrollItem(mLowerArm, EMorphItem.LowerArm));
		mScrollItemList.Add(new ScrollItem(mBelly, EMorphItem.Belly));
		mScrollItemList.Add(new ScrollItem(mWaist, EMorphItem.Waist));
		mScrollItemList.Add(new ScrollItem(mUpperLeg, EMorphItem.UpperLeg));
		mScrollItemList.Add(new ScrollItem(mLowerLeg, EMorphItem.LowerLeg));
	}

	private void InitPlayer()
	{
		GameObject gameObject = GameObject.Find("VirtualObjManager");
		GameObject gameObject2 = UnityEngine.Object.Instantiate(mMalePrefab);
		gameObject2.transform.position = gameObject.transform.position;
		gameObject2.transform.rotation = Quaternion.Euler(0f, -10f, 0f);
		mMaleInfo = gameObject2.GetComponent<PlayerModel>();
		mMaleInfo.mAppearData = new AppearData();
		mMaleInfo.mClothed = new AvatarData();
		mMaleInfo.mNude = new AvatarData();
		gameObject2.name = "Male";
		gameObject2.tag = "Player";
		gameObject2.SetActive(value: false);
		gameObject2 = UnityEngine.Object.Instantiate(mFemalePrefab);
		gameObject2.transform.position = gameObject.transform.position;
		gameObject2.transform.rotation = Quaternion.Euler(0f, -10f, 0f);
		mFemaleInfo = gameObject2.GetComponent<PlayerModel>();
		mFemaleInfo.mAppearData = new AppearData();
		mFemaleInfo.mClothed = new AvatarData();
		mFemaleInfo.mNude = new AvatarData();
		gameObject2.name = "Female";
		gameObject2.tag = "Player";
		gameObject2.SetActive(value: false);
	}

	private bool ChangeAppearData()
	{
		bool result = false;
		for (int i = 0; i < mScrollItemList.Count; i++)
		{
			float weight = mCurrent.mAppearData.GetWeight(mScrollItemList[i].mType);
			float num = mScrollItemList[i].mScrollBar.scrollValue * 2f - 1f;
			if (!Mathf.Approximately(weight, num))
			{
				result = true;
				mCurrent.mAppearData.SetWeight(mScrollItemList[i].mType, num);
			}
		}
		return result;
	}

	private void ResetBuildUIValue()
	{
		for (int i = 0; i < mScrollItemList.Count; i++)
		{
			float weight = mCurrent.mAppearData.GetWeight(mScrollItemList[i].mType);
			float b = mScrollItemList[i].mScrollBar.scrollValue * 2f - 1f;
			if (!Mathf.Approximately(weight, b))
			{
				mScrollItemList[i].mScrollBar.scrollValue = (weight + 1f) / 2f;
			}
		}
		ResetHeadGridSelected();
		ResetHairGridSelected();
	}

	private void ResetHeadGridSelected()
	{
		int headIndex = mMetaData.GetHeadIndex(mCurrent.mNude[AvatarData.ESlot.Head]);
		int num = headIndex - mHeadGridBox.mStartIndex;
		if (num < mHeadList.Count && num >= 0 && headIndex != -1)
		{
			if (mHeadGridSelectedItem != mHeadList[num])
			{
				if (mHeadGridSelectedItem != null)
				{
					mHeadGridSelectedItem.isSelected = false;
				}
				mHeadGridSelectedItem = mHeadList[num];
				mHeadGridSelectedItem.isSelected = true;
			}
		}
		else
		{
			if (mHeadGridSelectedItem != null)
			{
				mHeadGridSelectedItem.isSelected = false;
			}
			mHeadGridSelectedItem = null;
		}
	}

	private void ResetHairGridSelected()
	{
		int hairIndex = mMetaData.GetHairIndex(mCurrent.mNude[AvatarData.ESlot.HairF]);
		int num = hairIndex - mHairGridBox.mStartIndex;
		if (num < mHairList.Count && num >= 0 && hairIndex != -1)
		{
			if (mHairGridSelectedItem != mHairList[num])
			{
				if (mHairGridSelectedItem != null)
				{
					mHairGridSelectedItem.isSelected = false;
				}
				mHairGridSelectedItem = mHairList[num];
				mHairGridSelectedItem.isSelected = true;
			}
		}
		else
		{
			if (mHairGridSelectedItem != null)
			{
				mHairGridSelectedItem.isSelected = false;
			}
			mHairGridSelectedItem = null;
		}
	}

	private void ResetBodyPartGridBox()
	{
		int headCount = mMetaData.GetHeadCount();
		mHeadGridBox.ResetItemCount(headCount);
		mHeadGridBox.ReflashGridCotent();
		headCount = mMetaData.GetHairCount();
		mHairGridBox.ResetItemCount(headCount);
		mHairGridBox.ReflashGridCotent();
		BtnResetOnClick();
	}

	private void ResetSaveGridBox()
	{
		int dataCount = CustomDataMgr.Instance.dataCount;
		mSaveGrodBox.ResetItemCount(dataCount);
		mSaveGrodBox.ReflashGridCotent();
	}

	private void RebuildModel()
	{
		mCurrent.BuildModel();
		SkinnedMeshRenderer componentInChildren = mCurrent.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren != null)
		{
			Bounds localBounds = componentInChildren.localBounds;
			localBounds.center = Vector3.zero;
			componentInChildren.localBounds = localBounds;
		}
	}

	private void ApplyColor()
	{
		mCurrent.ApplyColor();
	}

	private void ReflashHeadGridBox(int start)
	{
		for (int i = 0; i < mHeadList.Count; i++)
		{
			if (i + start < mHeadGridBox.mItemCount)
			{
				mHeadList[i].SetItemInfo(mMetaData.GetHead(i + start).icon);
			}
			else
			{
				mHeadList[i].SetItemInfo("Null");
			}
		}
		ResetHeadGridSelected();
	}

	private void ReflashHairGridBox(int start)
	{
		for (int i = 0; i < mHairList.Count; i++)
		{
			if (i + start < mHairGridBox.mItemCount)
			{
				mHairList[i].SetItemInfo(mMetaData.GetHair(i + start).icon);
			}
			else
			{
				mHairList[i].SetItemInfo("Null");
			}
		}
		ResetHairGridSelected();
	}

	private void ReflashFaceGridBox(int start)
	{
	}

	private void ReflashSaveGridBox(int start)
	{
		for (int i = 0; i < mSaveList.Count; i++)
		{
			if (i + start < CustomDataMgr.Instance.dataCount)
			{
				mSaveList[i].SetItemInfo(CustomDataMgr.Instance.GetDataHeadIco(i + start));
			}
			else
			{
				mSaveList[i].SetItemInfo("Null");
			}
		}
		if (mSaveGridSelectedItem != null)
		{
			mSaveGridSelectedItem.isSelected = false;
			mSaveGridSelectedItem = null;
			mSaveGridSelectedIndex = -1;
		}
	}

	private void SetHairColor(Color col)
	{
		mCurrent.mAppearData.mHairColor = col;
		ApplyColor();
		if (CheckNeedSaveTip())
		{
			haschanged = true;
		}
	}

	private void SetSkinColor(Color col)
	{
		mCurrent.mAppearData.mSkinColor = col;
		ApplyColor();
		haschanged = true;
	}

	private void SetEyeColor(Color col)
	{
		mCurrent.mAppearData.mEyeColor = col;
		ApplyColor();
		if (CheckNeedSaveTip())
		{
			haschanged = true;
		}
	}

	public void FirstToTutorial()
	{
		TutorialExit.type = ((PeGameMgr.playerType == PeGameMgr.EPlayerType.Multiple) ? TutorialExit.TutorialType.MultiLobby : TutorialExit.TutorialType.Story);
		SystemSettingData.Instance.Tutorialed = true;
		string text = mNameInput.text;
		if (string.IsNullOrEmpty(text))
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000051));
			return;
		}
		CustomDataMgr.Instance.Current = CreateCustomData();
		CustomDataMgr.Instance.Current.charactorName = text;
		PeGameMgr.playerType = PeGameMgr.EPlayerType.Tutorial;
		PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Tutorial;
		PeGameMgr.tutorialMode = PeGameMgr.ETutorialMode.DigBuild;
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
	}

	public static void MainmenuToTutorial()
	{
		TutorialExit.type = TutorialExit.TutorialType.Mainmenu;
		SystemSettingData.Instance.Tutorialed = true;
		SingleGameStory.curType = SingleGameStory.StoryScene.TrainingShip;
		PeGameMgr.gameLevel = PeGameMgr.EGameLevel.Easy;
		PeGameMgr.playerType = PeGameMgr.EPlayerType.Tutorial;
		PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Tutorial;
		PeGameMgr.tutorialMode = PeGameMgr.ETutorialMode.DigBuild;
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
	}

	private void CreatePlayer()
	{
		string text = mNameInput.text;
		if (string.IsNullOrEmpty(text))
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000051));
			return;
		}
		int sex = (int)Sex;
		if (PeGameMgr.IsSingle)
		{
			CustomDataMgr.Instance.Current = CreateCustomData();
			CustomDataMgr.Instance.Current.charactorName = text;
			if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Custom)
			{
				PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
				PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
				PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Custom;
				PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
			}
			else if (PeGameMgr.randomMap)
			{
				SeedSetGui_N.Instance.Show();
			}
			else
			{
				IntroRunner.movieEnd = delegate
				{
					Debug.Log("<color=aqua>intro movie end.</color>");
					PeSceneCtrl.Instance.GotoGameSence();
				};
				PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.Intro);
			}
		}
		else if (PeGameMgr.IsMulti)
		{
			if (actionOk)
			{
				byte[] array = mCurrent.mAppearData.Serialize();
				byte[] array2 = mCurrent.mNude.Serialize();
				LobbyInterface.LobbyRPC(ELobbyMsgType.RoleCreate, text, (byte)sex, array, array2);
				actionOk = false;
				Invoke("ResetActionOK", 2f);
			}
		}
		else
		{
			Debug.LogError("error player mode.");
		}
	}

	public void ResetActionOK()
	{
		actionOk = true;
	}

	private bool SaveAppearData()
	{
		CustomCharactor.CustomData appearData = CreateCustomData();
		return CustomDataMgr.Instance.SaveData(appearData);
	}

	private CustomCharactor.CustomData CreateCustomData()
	{
		CustomCharactor.CustomData customData = new CustomCharactor.CustomData();
		customData.headIcon = PeViewStudio.TakePhoto(mCurrent.gameObject, 64, 64, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
		byte[] data = mCurrent.mAppearData.Serialize();
		customData.appearData = new AppearData();
		customData.appearData.Deserialize(data);
		customData.sex = ((Sex != ESex.Male) ? CustomCharactor.ESex.Female : CustomCharactor.ESex.Male);
		customData.nudeAvatarData = new AvatarData(mCurrent.mNude);
		return customData;
	}

	private void LoadAppearData(CustomCharactor.CustomData mCustomData)
	{
		if (mCustomData != null)
		{
			Sex = ((mCustomData.sex != 0) ? ESex.Female : ESex.Male);
			ChangeSex();
			mCurrent.mAppearData = mCustomData.appearData;
			mCurrent.mNude = mCustomData.nudeAvatarData;
			ResetBuildUIValue();
			RebuildModel();
		}
	}

	private void ChangeSex()
	{
		if (Sex == ESex.Male)
		{
			mMaleInfo.mMode.SetActive(value: true);
			mFemaleInfo.mMode.SetActive(value: false);
			mCurrent = mMaleInfo;
			mMetaData = CustomMetaData.InstanceMale;
		}
		else
		{
			mMaleInfo.mMode.SetActive(value: false);
			mFemaleInfo.mMode.SetActive(value: true);
			mCurrent = mFemaleInfo;
			mMetaData = CustomMetaData.InstanceFemale;
		}
		ChangeCameraPos();
		ResetBodyPartGridBox();
		SetDefaultColor();
	}

	private void SetDefaultColor()
	{
		mSkinColor.CurCol = mCurrent.mAppearData.mSkinColor;
		mSkinColor2.CurCol = mCurrent.mAppearData.mSkinColor;
		mEyeballsColor.CurCol = mCurrent.mAppearData.mEyeColor;
		mHairColor.CurCol = mCurrent.mAppearData.mHairColor;
	}

	private void BtnEnterOnClick()
	{
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (false)
		{
			if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Multiple)
			{
				if (mNameInput.text == "tutorial")
				{
					SystemSettingData.Instance.Tutorialed = false;
				}
				CreatePlayer();
			}
			else if (mNameInput.text == "tutorial")
			{
				FirstToTutorial();
			}
			else
			{
				CreatePlayer();
			}
		}
		else if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Multiple)
		{
			CreatePlayer();
		}
		else if (SystemSettingData.Instance.Tutorialed || PeGameMgr.sceneMode != 0)
		{
			CreatePlayer();
		}
		else
		{
			FirstToTutorial();
		}
	}

	private void BtnBackOnClick()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000052), PeSceneCtrl.Instance.GotoMainMenuScene);
	}

	private void BtnMaleOnClick()
	{
		Sex = ESex.Male;
		ChangeSex();
	}

	private void BtnFemaleOnClick()
	{
		Sex = ESex.Female;
		ChangeSex();
	}

	private void BtnRandomOnClick()
	{
		if (haschanged)
		{
			haschanged = !haschanged;
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000173), BtnSaveOnClick, BtnRandomOnClick);
			return;
		}
		int headCount = mMetaData.GetHeadCount();
		int index = UnityEngine.Random.Range(0, headCount);
		mCurrent.mNude.SetPart(AvatarData.ESlot.Head, mMetaData.GetHead(index).modelPath);
		int hairCount = mMetaData.GetHairCount();
		int index2 = UnityEngine.Random.Range(0, hairCount);
		string[] modelPath = mMetaData.GetHair(index2).modelPath;
		mCurrent.mNude.SetPart(AvatarData.ESlot.HairF, modelPath[0]);
		mCurrent.mNude.SetPart(AvatarData.ESlot.HairT, modelPath[1]);
		mCurrent.mNude.SetPart(AvatarData.ESlot.HairB, modelPath[2]);
		mCurrent.mAppearData.mHairColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
		mCurrent.mAppearData.mEyeColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
		mCurrent.mAppearData.mSkinColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
		mCurrent.mAppearData.RandomMorphWeight();
		ResetBuildUIValue();
		RebuildModel();
	}

	private void BtnResetOnClick()
	{
		if (Sex == ESex.Male)
		{
			mCurrent.mNude.SetMaleBody();
		}
		else
		{
			mCurrent.mNude.SetFemaleBody();
		}
		mCurrent.mAppearData.Default();
		ResetBuildUIValue();
		RebuildModel();
		haschanged = false;
	}

	private void BtnSaveOnClick()
	{
		SaveAppearData();
		ResetSaveGridBox();
		mLbTishi.ShowText(PELocalization.GetString(10064));
		haschanged = false;
	}

	private void BtnLoadOnClick()
	{
		if (mSaveGridSelectedIndex >= 0 && mSaveGridSelectedIndex < CustomDataMgr.Instance.dataCount)
		{
			LoadAppearData(CustomDataMgr.Instance.GetCustomData(mSaveGridSelectedIndex));
		}
	}

	private void BtnDeleteOnClick()
	{
		if (mSaveGridSelectedIndex >= 0 && mSaveGridSelectedIndex < CustomDataMgr.Instance.dataCount)
		{
			CustomDataMgr.Instance.DeleteData(mSaveGridSelectedIndex);
			ResetSaveGridBox();
		}
	}

	private void GridBoxItemOnClick(int uiIndex, PlayerBuildGirdItem.Type _type)
	{
		switch (_type)
		{
		case PlayerBuildGirdItem.Type.Type_Head:
		{
			int num2 = uiIndex + mHeadGridBox.mStartIndex;
			if (num2 < mHeadGridBox.mItemCount)
			{
				if (mHeadGridSelectedItem != null)
				{
					mHeadGridSelectedItem.isSelected = false;
				}
				mHeadGridSelectedItem = mHeadList[uiIndex];
				mHeadGridSelectedItem.isSelected = true;
				HeadGridBoxOnClick(num2);
			}
			break;
		}
		case PlayerBuildGirdItem.Type.Type_Hair:
		{
			int num3 = uiIndex + mHairGridBox.mStartIndex;
			if (num3 < mHairGridBox.mItemCount)
			{
				if (mHairGridSelectedItem != null)
				{
					mHairGridSelectedItem.isSelected = false;
				}
				mHairGridSelectedItem = mHairList[uiIndex];
				mHairGridSelectedItem.isSelected = true;
				HairGridBoxOnClick(num3);
			}
			break;
		}
		case PlayerBuildGirdItem.Type.Type_Face:
			FaceGridBoxOnClick(uiIndex + mFaceGridBox.mStartIndex);
			break;
		case PlayerBuildGirdItem.Type.Type_Save:
		{
			int num = uiIndex + mSaveGrodBox.mStartIndex;
			if (num < mSaveGrodBox.mItemCount)
			{
				if (mSaveGridSelectedItem != null)
				{
					mSaveGridSelectedItem.isSelected = false;
				}
				mSaveGridSelectedItem = mSaveList[uiIndex];
				mSaveGridSelectedItem.isSelected = true;
				SaveGridBoxOnClick(num);
			}
			break;
		}
		}
	}

	private void HeadGridBoxOnClick(int index)
	{
		mCurrent.mNude.SetPart(AvatarData.ESlot.Head, mMetaData.GetHead(index).modelPath);
		RebuildModel();
		haschanged = true;
	}

	private void HairGridBoxOnClick(int index)
	{
		string[] modelPath = mMetaData.GetHair(index).modelPath;
		mCurrent.mNude.SetPart(AvatarData.ESlot.HairF, modelPath[0]);
		mCurrent.mNude.SetPart(AvatarData.ESlot.HairT, modelPath[1]);
		mCurrent.mNude.SetPart(AvatarData.ESlot.HairB, modelPath[2]);
		RebuildModel();
		haschanged = true;
	}

	private void FaceGridBoxOnClick(int index)
	{
	}

	private void SaveGridBoxOnClick(int index)
	{
		mSaveGridSelectedIndex = index;
	}

	private void OnRotLeftBtnDown()
	{
		mRotLeft = true;
	}

	private void OnRotLeftBtnUp()
	{
		mRotLeft = false;
	}

	private void OnRotRightBtnDown()
	{
		mRotRight = true;
	}

	private void OnRotRightBtnUp()
	{
		mRotRight = false;
	}

	private bool CheckNeedSaveTip()
	{
		bool result = false;
		CustomCharactor.CustomData @new = CreateCustomData();
		List<CustomCharactor.CustomData> customDataList = CustomDataMgr.Instance.CustomDataList;
		if (customDataList.Count != 0)
		{
			foreach (CustomCharactor.CustomData item in customDataList)
			{
				if (CompareCustomData(@new, item))
				{
					result = true;
					break;
				}
			}
		}
		else
		{
			result = true;
		}
		return result;
	}

	private bool CompareCustomData(CustomCharactor.CustomData _new, CustomCharactor.CustomData _old)
	{
		bool result = false;
		if (_new != null && _new.appearData != null && _old != null && _old.appearData != null)
		{
			AppearData appearData = _new.appearData;
			AppearData appearData2 = _old.appearData;
			if (appearData.mEyeColor != appearData2.mEyeColor)
			{
				result = true;
			}
			else if (appearData.mLipColor != appearData2.mLipColor)
			{
				result = true;
			}
			else if (appearData.mSkinColor != appearData2.mSkinColor)
			{
				result = true;
			}
			else if (appearData.mHairColor != appearData2.mHairColor)
			{
				result = true;
			}
			if (mScrollItemList != null && mScrollItemList.Count > 0)
			{
				for (int i = 0; i < mScrollItemList.Count; i++)
				{
					float weight = appearData.GetWeight(mScrollItemList[i].mType);
					float weight2 = appearData2.GetWeight(mScrollItemList[i].mType);
					if (!Mathf.Approximately(weight, weight2))
					{
						result = true;
						break;
					}
				}
			}
			if (_new.sex != _old.sex)
			{
				result = true;
			}
		}
		return result;
	}
}
