using System;
using System.Collections;
using RedGrass;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class UIOption : UIStaticWnd
{
	public enum KeyCategory
	{
		Common = 10179,
		Character,
		Construct,
		Carrier
	}

	private static UIOption mInstance;

	public UIStaticWnd mParentWnd;

	public GameObject mVideoWnd;

	public GameObject mAudioWnd;

	public GameObject mShortCutsWnd;

	public GameObject mMiscWnd;

	public GameObject mControlWnd;

	public GameObject mDefaultBtn;

	public UICheckbox mVideoBtn;

	public UICheckbox mAudioBtn;

	public UICheckbox mShortCutsBtn;

	public UIScrollBar mKeyScrollBar;

	public OptionItem_N mLightCountItem;

	public OptionItem_N mAnisotropicFilteringItem;

	public OptionItem_N mAntiAliasingItem;

	public OptionItem_N mShadowProjectionItem;

	public OptionItem_N mShadowDistanceItem;

	public OptionItem_N mShadowCascadesItem;

	public OptionItem_N mTerrainLevel;

	public OptionItem_N mRandomTerrainLevel;

	public OptionItem_N mTreeLevel;

	public OptionItem_N mGrassDensity;

	public OptionItem_N mGrassDistance;

	public OptionItem_N mWaterReflection;

	public OptionItem_N mWaterRefraction;

	public OptionItem_N mWaterDepth;

	public OptionItem_N mSSAO;

	public OptionItem_N mSyncCount;

	public OptionItem_N mDepthBlur;

	public OptionItem_N mHDR;

	public OptionItem_N mLightShadows;

	public ScrollItem mQuality;

	public ScrollItem mSoundVolume;

	public ScrollItem mMusicVolume;

	public ScrollItem mDialogVolume;

	public ScrollItem mEffectVolume;

	public ScrollItem mHoldGunCameraSensitivity;

	public ScrollItem mCameraSensitivity;

	public UILabel mCamS;

	public UILabel mHoldGunCamS;

	public UILabel mCamSMin;

	public UILabel mCamSMax;

	private float CamSMin = 0.3f;

	private float CamSMax = 4f;

	public ScrollItem mCameraFOVScroll;

	public UILabel mCameraFOV;

	public UILabel mCameraFOVMin;

	public UILabel mCameraFOVMax;

	private float CamFOVMin = 20f;

	private float CamFOVMax = 90f;

	public ScrollItem mCameraInertia;

	public UILabel mCamInertiaValue;

	public UILabel mCamInertiaMin;

	public UILabel mCamInertiaMax;

	private float CamInertiaMin;

	private float CamInertiaMax = 10f;

	public ScrollItem DriveCameraInertiaScroll;

	public UILabel DriveCamInertiaValueLabel;

	public UILabel DriveCamInertiaMinLabel;

	public UILabel DriveCamInertiaMaxLabel;

	private float m_DriveCamInertiaMin;

	private float m_DriveCamInertiaMax = 50f;

	public UICheckbox mCamHorizontal;

	public UICheckbox mCamVertical;

	public UICheckbox mAttacMode;

	public UICheckbox mHideHeadgear;

	public UICheckbox mHPNumbers;

	public UICheckbox mLockCursor;

	public UICheckbox mMonsterIK;

	public UICheckbox mVoxelCache;

	public UICheckbox mFixFontBlurry;

	public UICheckbox mAndyGuidance;

	public UICheckbox mMouseStateTip;

	public UICheckbox mUseController;

	public UICheckbox mHidePlayerOverHeadInfo;

	public static int[][] DefaultIndex = new int[2][]
	{
		new int[5],
		new int[5]
	};

	public float SoundVolume = 1f;

	public float MusicVolume = 1f;

	public float DialogVolume = 1f;

	public float EffectVolume = 1f;

	public Action VolumeChangeEvent;

	private int mTabIndex;

	public UIGrid mKeysSetGrid;

	public KeySettingItem mPerfab;

	public KeyCategoryItem mCategoryPrefab;

	private KeySettingItem[][] mKeySettingLists = new KeySettingItem[4][];

	private int LastQualityLevel = 6;

	private int[] tmpIdx = new int[4];

	public static UIOption Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		SoundVolume = SystemSettingData.Instance.SoundVolume;
		MusicVolume = SystemSettingData.Instance.MusicVolume;
		DialogVolume = SystemSettingData.Instance.DialogVolume;
		EffectVolume = SystemSettingData.Instance.EffectVolume;
		mCamSMin.text = CamSMin.ToString();
		mCamSMax.text = CamSMax.ToString();
		mCameraFOVMin.text = CamFOVMin.ToString();
		mCameraFOVMax.text = CamFOVMax.ToString();
		mCamInertiaMin.text = CamInertiaMin.ToString();
		mCamInertiaMax.text = CamInertiaMax.ToString();
		OnVideoBtn();
	}

	public void HideWindow()
	{
		Hide();
		SystemSettingData.Instance.SoundVolume = SoundVolume;
		SystemSettingData.Instance.MusicVolume = MusicVolume;
		SystemSettingData.Instance.DialogVolume = DialogVolume;
		SystemSettingData.Instance.EffectVolume = EffectVolume;
		if (mParentWnd != null)
		{
			mParentWnd.Show();
			mParentWnd = null;
		}
	}

	public void OnVideoBtn()
	{
		mTabIndex = 0;
		mDefaultBtn.SetActive(value: false);
		mVideoWnd.SetActive(value: true);
		mAudioWnd.SetActive(value: false);
		mShortCutsWnd.SetActive(value: false);
		mMiscWnd.SetActive(value: false);
		mControlWnd.SetActive(value: false);
		LastQualityLevel = SystemSettingData.Instance.mQualityLevel;
		ResetVideo(LastQualityLevel);
		mVideoBtn.isChecked = true;
		SystemSettingData.Instance.SoundVolume = SoundVolume;
		SystemSettingData.Instance.MusicVolume = MusicVolume;
		SystemSettingData.Instance.DialogVolume = DialogVolume;
		SystemSettingData.Instance.EffectVolume = EffectVolume;
	}

	private void OnAudioBrn()
	{
		if (mTabIndex != 1)
		{
			mTabIndex = 1;
			mDefaultBtn.SetActive(value: false);
			mAudioWnd.SetActive(value: true);
			mVideoWnd.SetActive(value: false);
			mShortCutsWnd.SetActive(value: false);
			mMiscWnd.SetActive(value: false);
			mControlWnd.SetActive(value: false);
			ResetAudio();
		}
	}

	private void OnShortCutsBtn()
	{
		if (mTabIndex != 2)
		{
			mTabIndex = 2;
			mDefaultBtn.SetActive(value: true);
			mShortCutsWnd.SetActive(value: true);
			mVideoWnd.SetActive(value: false);
			mAudioWnd.SetActive(value: false);
			mMiscWnd.SetActive(value: false);
			mControlWnd.SetActive(value: false);
			ResetkeySetting();
			SystemSettingData.Instance.SoundVolume = SoundVolume;
			SystemSettingData.Instance.MusicVolume = MusicVolume;
			SystemSettingData.Instance.DialogVolume = DialogVolume;
			SystemSettingData.Instance.EffectVolume = EffectVolume;
		}
	}

	private void OnControlBtn()
	{
		if (mTabIndex != 3)
		{
			mTabIndex = 3;
			mDefaultBtn.SetActive(value: false);
			mControlWnd.SetActive(value: true);
			mShortCutsWnd.SetActive(value: false);
			mVideoWnd.SetActive(value: false);
			mAudioWnd.SetActive(value: false);
			mMiscWnd.SetActive(value: false);
			ResetControl();
			SystemSettingData.Instance.SoundVolume = SoundVolume;
			SystemSettingData.Instance.MusicVolume = MusicVolume;
			SystemSettingData.Instance.DialogVolume = DialogVolume;
			SystemSettingData.Instance.EffectVolume = EffectVolume;
		}
	}

	private void OnMiscBtn()
	{
		if (mTabIndex != 4)
		{
			mTabIndex = 4;
			mDefaultBtn.SetActive(value: false);
			mMiscWnd.SetActive(value: true);
			mShortCutsWnd.SetActive(value: false);
			mVideoWnd.SetActive(value: false);
			mAudioWnd.SetActive(value: false);
			mControlWnd.SetActive(value: false);
			ResetMisc();
			SystemSettingData.Instance.SoundVolume = SoundVolume;
			SystemSettingData.Instance.MusicVolume = MusicVolume;
			SystemSettingData.Instance.DialogVolume = DialogVolume;
			SystemSettingData.Instance.EffectVolume = EffectVolume;
		}
	}

	private void OnClearVoxelCacheBtnClick()
	{
		VFDataRTGenFileCache.ClearAllCache();
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000913));
	}

	private void OnCtrlType0()
	{
	}

	private void OnCtrlType1()
	{
	}

	private void OnApplyBtn()
	{
		switch (mTabIndex)
		{
		case 0:
		{
			SystemSettingData.Instance.mQualityLevel = mQuality.mIndex;
			SystemSettingData.Instance.mLightCount = mLightCountItem.mIndex;
			SystemSettingData.Instance.mAnisotropicFiltering = mAnisotropicFilteringItem.mIndex;
			SystemSettingData.Instance.mAntiAliasing = mAntiAliasingItem.mIndex;
			SystemSettingData.Instance.mShadowProjection = mShadowProjectionItem.mIndex;
			SystemSettingData.Instance.mShadowDistance = mShadowDistanceItem.mIndex;
			SystemSettingData.Instance.mShadowCascades = mShadowCascadesItem.mIndex;
			SystemSettingData.Instance.mWaterReflection = mWaterReflection.mIndex;
			SystemSettingData.Instance.WaterRefraction = mWaterRefraction.mIndex == 1;
			SystemSettingData.Instance.WaterDepth = mWaterDepth.mIndex == 1;
			SystemSettingData.Instance.TerrainLevel = (byte)mTerrainLevel.mIndex;
			SystemSettingData.Instance.RandomTerrainLevel = (byte)mRandomTerrainLevel.mIndex;
			SystemSettingData.Instance.mTreeLevel = mTreeLevel.mIndex + 1;
			Antialiasing component = Camera.main.GetComponent<Antialiasing>();
			if (null != component)
			{
				component.enabled = mAntiAliasingItem.mIndex > 0;
			}
			SystemSettingData.Instance.GrassDensity = 1f * (float)mGrassDensity.mIndex / (float)(mGrassDensity.mSelections.Count - 1);
			SystemSettingData.Instance.mGrassLod = ConvertIndexToGrassLod(mGrassDistance.mIndex);
			SystemSettingData.Instance.mDepthBlur = mDepthBlur.mIndex == 1;
			SystemSettingData.Instance.mSSAO = mSSAO.mIndex == 1;
			SystemSettingData.Instance.SyncCount = mSyncCount.mIndex == 1;
			SystemSettingData.Instance.HDREffect = mHDR.mIndex == 1;
			SystemSettingData.Instance.mFastLightingMode = mLightShadows.mIndex != 1;
			SystemSettingData.Instance.ApplyVideo();
			break;
		}
		case 1:
			SoundVolume = SystemSettingData.Instance.SoundVolume;
			MusicVolume = SystemSettingData.Instance.MusicVolume;
			DialogVolume = SystemSettingData.Instance.DialogVolume;
			EffectVolume = SystemSettingData.Instance.EffectVolume;
			SystemSettingData.Instance.ApplyAudio();
			if (VolumeChangeEvent != null)
			{
				VolumeChangeEvent();
			}
			break;
		case 2:
		{
			for (int i = 0; i < PeInput.SettingsAll.Length; i++)
			{
				for (int j = 0; j < PeInput.SettingsAll[i].Length; j++)
				{
					PeInput.SettingsAll[i][j].Clone(mKeySettingLists[i][j]._keySetting);
				}
			}
			SystemSettingData.Instance.ApplyKeySetting();
			if (UIMenuList.Instance != null)
			{
				UIMenuList.Instance.RefreshHotKeyName();
			}
			break;
		}
		case 3:
			SystemSettingData.Instance.cameraSensitivity = CamSMin + (CamSMax - CamSMin) * mCameraSensitivity.mScroll.scrollValue;
			SystemSettingData.Instance.holdGunCameraSensitivity = CamSMin + (CamSMax - CamSMin) * mHoldGunCameraSensitivity.mScroll.scrollValue;
			SystemSettingData.Instance.CameraFov = CamFOVMin + (CamFOVMax - CamFOVMin) * mCameraFOVScroll.mScroll.scrollValue;
			SystemSettingData.Instance.CamInertia = CamInertiaMin + (CamInertiaMax - CamInertiaMin) * mCameraInertia.mScroll.scrollValue;
			SystemSettingData.Instance.DriveCamInertia = m_DriveCamInertiaMin + (m_DriveCamInertiaMax - m_DriveCamInertiaMin) * DriveCameraInertiaScroll.mScroll.scrollValue;
			SystemSettingData.Instance.CameraHorizontalInverse = mCamHorizontal.isChecked;
			SystemSettingData.Instance.CameraVerticalInverse = mCamVertical.isChecked;
			SystemSettingData.Instance.AttackWhithMouseDir = mAttacMode.isChecked;
			SystemSettingData.Instance.UseController = mUseController.isChecked;
			SystemSettingData.Instance.ApplyControl();
			break;
		case 4:
			SystemSettingData.Instance.HideHeadgear = mHideHeadgear.isChecked;
			SystemSettingData.Instance.HPNumbers = mHPNumbers.isChecked;
			SystemSettingData.Instance.ClipCursor = mLockCursor.isChecked;
			SystemSettingData.Instance.ApplyMonsterIK = mMonsterIK.isChecked;
			SystemSettingData.Instance.VoxelCacheEnabled = mVoxelCache.isChecked;
			SystemSettingData.Instance.FixBlurryFont = mFixFontBlurry.isChecked;
			SystemSettingData.Instance.AndyGuidance = mAndyGuidance.isChecked;
			SystemSettingData.Instance.MouseStateTip = mMouseStateTip.isChecked;
			SystemSettingData.Instance.HidePlayerOverHeadInfo = mHidePlayerOverHeadInfo.isChecked;
			SystemSettingData.Instance.ApplyMisc();
			break;
		}
	}

	private int ConvertGrassLodToIndex(ELodType loadType)
	{
		return loadType switch
		{
			ELodType.LOD_1_TYPE_1 => 0, 
			ELodType.LOD_2_TYPE_2 => 1, 
			ELodType.LOD_3_TYPE_1 => 2, 
			_ => 2, 
		};
	}

	private ELodType ConvertIndexToGrassLod(int index)
	{
		return index switch
		{
			0 => ELodType.LOD_1_TYPE_1, 
			1 => ELodType.LOD_2_TYPE_2, 
			2 => ELodType.LOD_3_TYPE_1, 
			_ => ELodType.LOD_3_TYPE_1, 
		};
	}

	private void OnOkbtn()
	{
		OnApplyBtn();
		OnCanncelBtn();
	}

	private void OnCanncelBtn()
	{
		OnVideoBtn();
		HideWindow();
	}

	private void OnDefaultBtn()
	{
		int childCount = mKeysSetGrid.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(mKeysSetGrid.transform.GetChild(i).gameObject);
		}
		StartCoroutine(ResetInterator());
	}

	private IEnumerator ResetInterator()
	{
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < 0.2f)
		{
			yield return null;
		}
		PeInput.ResetSetting();
		mKeySettingLists[0] = null;
		mKeySettingLists[1] = null;
		mKeySettingLists[2] = null;
		mKeySettingLists[3] = null;
		ResetkeySetting();
		if (UIMenuList.Instance != null)
		{
			UIMenuList.Instance.RefreshHotKeyName();
		}
	}

	private void ResetVideo(int qualityLevel, bool fromUpdate = false)
	{
		mQuality.SetIndex(qualityLevel);
		switch (qualityLevel)
		{
		case 0:
			mLightCountItem.SetIndex(0);
			mAnisotropicFilteringItem.SetIndex(0);
			mAntiAliasingItem.SetIndex(0);
			mShadowProjectionItem.SetIndex(0);
			mShadowDistanceItem.SetIndex(0);
			mShadowCascadesItem.SetIndex(0);
			mWaterReflection.SetIndex(0);
			mWaterRefraction.SetIndex(0);
			mWaterDepth.SetIndex(0);
			mTerrainLevel.SetIndex(1);
			mRandomTerrainLevel.SetIndex(0);
			mTreeLevel.SetIndex(0);
			mGrassDensity.SetIndex(0);
			mGrassDistance.SetIndex(0);
			mSSAO.SetIndex(0);
			mSyncCount.SetIndex(0);
			mDepthBlur.SetIndex(0);
			mHDR.SetIndex(0);
			mLightShadows.SetIndex((!SystemSettingData.Instance.mFastLightingMode) ? 1 : 0);
			break;
		case 1:
			mLightCountItem.SetIndex(5);
			mAnisotropicFilteringItem.SetIndex(0);
			mAntiAliasingItem.SetIndex(1);
			mShadowProjectionItem.SetIndex(0);
			mShadowDistanceItem.SetIndex(1);
			mShadowCascadesItem.SetIndex(0);
			mWaterReflection.SetIndex(0);
			mWaterRefraction.SetIndex(0);
			mWaterDepth.SetIndex(0);
			mTerrainLevel.SetIndex(1);
			mRandomTerrainLevel.SetIndex(0);
			mTreeLevel.SetIndex(1);
			mGrassDensity.SetIndex(1);
			mGrassDistance.SetIndex(0);
			mSSAO.SetIndex(0);
			mSyncCount.SetIndex(0);
			mDepthBlur.SetIndex(0);
			mHDR.SetIndex(0);
			mLightShadows.SetIndex((!SystemSettingData.Instance.mFastLightingMode) ? 1 : 0);
			break;
		case 2:
			mLightCountItem.SetIndex(10);
			mAnisotropicFilteringItem.SetIndex(0);
			mAntiAliasingItem.SetIndex(1);
			mShadowProjectionItem.SetIndex(0);
			mShadowDistanceItem.SetIndex(2);
			mShadowCascadesItem.SetIndex(1);
			mWaterReflection.SetIndex(1);
			mWaterRefraction.SetIndex(1);
			mWaterDepth.SetIndex(1);
			mTerrainLevel.SetIndex(1);
			mRandomTerrainLevel.SetIndex(1);
			mTreeLevel.SetIndex(1);
			mGrassDensity.SetIndex(2);
			mGrassDistance.SetIndex(1);
			mSSAO.SetIndex(0);
			mSyncCount.SetIndex(0);
			mDepthBlur.SetIndex(1);
			mHDR.SetIndex(0);
			mLightShadows.SetIndex((!SystemSettingData.Instance.mFastLightingMode) ? 1 : 0);
			break;
		case 3:
			mLightCountItem.SetIndex(20);
			mAnisotropicFilteringItem.SetIndex(1);
			mAntiAliasingItem.SetIndex(2);
			mShadowProjectionItem.SetIndex(1);
			mShadowDistanceItem.SetIndex(2);
			mShadowCascadesItem.SetIndex(1);
			mWaterReflection.SetIndex(1);
			mWaterRefraction.SetIndex(1);
			mWaterDepth.SetIndex(1);
			mTerrainLevel.SetIndex(1);
			mRandomTerrainLevel.SetIndex(1);
			mTreeLevel.SetIndex(2);
			mGrassDensity.SetIndex(3);
			mGrassDistance.SetIndex(1);
			mSSAO.SetIndex(0);
			mSyncCount.SetIndex(0);
			mDepthBlur.SetIndex(1);
			mHDR.SetIndex(0);
			mLightShadows.SetIndex((!SystemSettingData.Instance.mFastLightingMode) ? 1 : 0);
			break;
		case 4:
			mLightCountItem.SetIndex(50);
			mAnisotropicFilteringItem.SetIndex(2);
			mAntiAliasingItem.SetIndex(3);
			mShadowProjectionItem.SetIndex(1);
			mShadowDistanceItem.SetIndex(3);
			mShadowCascadesItem.SetIndex(2);
			mWaterReflection.SetIndex(2);
			mWaterRefraction.SetIndex(1);
			mWaterDepth.SetIndex(1);
			mTerrainLevel.SetIndex(2);
			mRandomTerrainLevel.SetIndex(2);
			mTreeLevel.SetIndex(3);
			mGrassDensity.SetIndex(4);
			mGrassDistance.SetIndex(2);
			mSSAO.SetIndex(0);
			mSyncCount.SetIndex(0);
			mDepthBlur.SetIndex(1);
			mHDR.SetIndex(0);
			mLightShadows.SetIndex((!SystemSettingData.Instance.mFastLightingMode) ? 1 : 0);
			break;
		case 5:
			mLightCountItem.SetIndex(50);
			mAnisotropicFilteringItem.SetIndex(2);
			mAntiAliasingItem.SetIndex(3);
			mShadowProjectionItem.SetIndex(1);
			mShadowDistanceItem.SetIndex(4);
			mShadowCascadesItem.SetIndex(2);
			mWaterReflection.SetIndex(2);
			mWaterRefraction.SetIndex(1);
			mWaterDepth.SetIndex(1);
			mTerrainLevel.SetIndex(3);
			mRandomTerrainLevel.SetIndex(2);
			mTreeLevel.SetIndex(4);
			mGrassDensity.SetIndex(4);
			mGrassDistance.SetIndex(2);
			mSSAO.SetIndex(0);
			mSyncCount.SetIndex(1);
			mDepthBlur.SetIndex(1);
			mHDR.SetIndex(1);
			mLightShadows.SetIndex((!SystemSettingData.Instance.mFastLightingMode) ? 1 : 0);
			break;
		case 6:
			mLightCountItem.SetIndex(SystemSettingData.Instance.mLightCount);
			mAnisotropicFilteringItem.SetIndex(SystemSettingData.Instance.mAnisotropicFiltering);
			mAntiAliasingItem.SetIndex(SystemSettingData.Instance.mAntiAliasing);
			mShadowProjectionItem.SetIndex(SystemSettingData.Instance.mShadowProjection);
			mShadowDistanceItem.SetIndex(SystemSettingData.Instance.mShadowDistance);
			mShadowCascadesItem.SetIndex(SystemSettingData.Instance.mShadowCascades);
			mWaterReflection.SetIndex(SystemSettingData.Instance.mWaterReflection);
			mWaterRefraction.SetIndex(SystemSettingData.Instance.WaterRefraction ? 1 : 0);
			mWaterDepth.SetIndex(SystemSettingData.Instance.WaterDepth ? 1 : 0);
			mTerrainLevel.SetIndex(SystemSettingData.Instance.TerrainLevel);
			mRandomTerrainLevel.SetIndex(SystemSettingData.Instance.RandomTerrainLevel);
			mTreeLevel.SetIndex(SystemSettingData.Instance.mTreeLevel - 1);
			mGrassDensity.SetIndex((int)(SystemSettingData.Instance.GrassDensity * (float)(mGrassDensity.mSelections.Count - 1)));
			mGrassDistance.SetIndex(ConvertGrassLodToIndex(SystemSettingData.Instance.mGrassLod));
			mSSAO.SetIndex(SystemSettingData.Instance.mSSAO ? 1 : 0);
			mSyncCount.SetIndex(SystemSettingData.Instance.SyncCount ? 1 : 0);
			mDepthBlur.SetIndex(SystemSettingData.Instance.mDepthBlur ? 1 : 0);
			mHDR.SetIndex(SystemSettingData.Instance.HDREffect ? 1 : 0);
			mLightShadows.SetIndex((!SystemSettingData.Instance.mFastLightingMode) ? 1 : 0);
			break;
		}
	}

	private void ResetAudio()
	{
		mSoundVolume.SetValue(SystemSettingData.Instance.SoundVolume);
		mMusicVolume.SetValue(SystemSettingData.Instance.MusicVolume);
		mDialogVolume.SetValue(SystemSettingData.Instance.DialogVolume);
		mEffectVolume.SetValue(SystemSettingData.Instance.EffectVolume);
		SoundVolume = SystemSettingData.Instance.SoundVolume;
		MusicVolume = SystemSettingData.Instance.MusicVolume;
		DialogVolume = SystemSettingData.Instance.DialogVolume;
		EffectVolume = SystemSettingData.Instance.EffectVolume;
	}

	private void ResetkeySetting()
	{
		CreatKeyList(ref mKeySettingLists[0], PeInput.SettingsAll[0].Length, KeyCategory.Common);
		CreatKeyList(ref mKeySettingLists[1], PeInput.SettingsAll[1].Length, KeyCategory.Character);
		CreatKeyList(ref mKeySettingLists[2], PeInput.SettingsAll[2].Length, KeyCategory.Construct);
		CreatKeyList(ref mKeySettingLists[3], PeInput.SettingsAll[3].Length, KeyCategory.Carrier);
		mKeysSetGrid.Reposition();
	}

	private void CreatKeyList(ref KeySettingItem[] _array, int _count, KeyCategory _keycate)
	{
		if (_array == null)
		{
			KeyCategoryItem keyCategoryItem = UnityEngine.Object.Instantiate(mCategoryPrefab);
			keyCategoryItem.transform.parent = mKeysSetGrid.transform;
			keyCategoryItem.transform.localScale = Vector3.one;
			keyCategoryItem.transform.localPosition = Vector3.zero;
			keyCategoryItem.transform.localRotation = Quaternion.identity;
			keyCategoryItem.mStringID = (int)_keycate;
			_array = new KeySettingItem[_count];
			for (int i = 0; i < _count; i++)
			{
				KeySettingItem keySettingItem = UnityEngine.Object.Instantiate(mPerfab);
				keySettingItem.transform.parent = mKeysSetGrid.transform;
				keySettingItem.transform.localScale = Vector3.one;
				keySettingItem.transform.localPosition = Vector3.zero;
				keySettingItem.transform.localRotation = Quaternion.identity;
				keySettingItem.gameObject.name = "KeySetting" + i;
				switch (_keycate)
				{
				case KeyCategory.Common:
					keySettingItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[0][i]);
					keySettingItem._keySettingName = PeInput.StrIdOfGeneral(i);
					break;
				case KeyCategory.Character:
					keySettingItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[1][i]);
					keySettingItem._keySettingName = PeInput.StrIdOfChrCtrl(i);
					break;
				case KeyCategory.Construct:
					keySettingItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[2][i]);
					keySettingItem._keySettingName = PeInput.StrIdOfBuildMd(i);
					break;
				case KeyCategory.Carrier:
					keySettingItem._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[3][i]);
					keySettingItem._keySettingName = PeInput.StrIdOfVehicle(i);
					break;
				}
				_array[i] = keySettingItem;
			}
			return;
		}
		for (int j = 0; j < _count; j++)
		{
			switch (_keycate)
			{
			case KeyCategory.Common:
				_array[j]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[0][j]);
				_array[j]._keySettingName = PeInput.StrIdOfGeneral(j);
				break;
			case KeyCategory.Character:
				_array[j]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[1][j]);
				_array[j]._keySettingName = PeInput.StrIdOfChrCtrl(j);
				break;
			case KeyCategory.Construct:
				_array[j]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[2][j]);
				_array[j]._keySettingName = PeInput.StrIdOfBuildMd(j);
				break;
			case KeyCategory.Carrier:
				_array[j]._keySetting = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[3][j]);
				_array[j]._keySettingName = PeInput.StrIdOfVehicle(j);
				break;
			}
		}
	}

	private void ResetControl()
	{
		mCameraSensitivity.SetValue((SystemSettingData.Instance.cameraSensitivity - CamSMin) / (CamSMax - CamSMin));
		mHoldGunCameraSensitivity.SetValue((SystemSettingData.Instance.holdGunCameraSensitivity - CamSMin) / (CamSMax - CamSMin));
		mCameraFOVScroll.SetValue((SystemSettingData.Instance.CameraFov - CamFOVMin) / (CamFOVMax - CamFOVMin));
		DriveCameraInertiaScroll.SetValue((SystemSettingData.Instance.DriveCamInertia - m_DriveCamInertiaMin) / (m_DriveCamInertiaMax - m_DriveCamInertiaMin));
		mCameraInertia.SetValue((SystemSettingData.Instance.CamInertia - CamInertiaMin) / (CamInertiaMax - CamInertiaMin));
		mCamHorizontal.isChecked = SystemSettingData.Instance.CameraHorizontalInverse;
		mCamVertical.isChecked = SystemSettingData.Instance.CameraVerticalInverse;
		mAttacMode.isChecked = SystemSettingData.Instance.AttackWhithMouseDir;
		mUseController.isChecked = SystemSettingData.Instance.UseController;
	}

	private void ResetMisc()
	{
		mHideHeadgear.isChecked = SystemSettingData.Instance.HideHeadgear;
		mHPNumbers.isChecked = SystemSettingData.Instance.HPNumbers;
		mLockCursor.isChecked = SystemSettingData.Instance.ClipCursor;
		mMonsterIK.isChecked = SystemSettingData.Instance.ApplyMonsterIK;
		mVoxelCache.isChecked = SystemSettingData.Instance.VoxelCacheEnabled;
		mFixFontBlurry.isChecked = SystemSettingData.Instance.FixBlurryFont;
		mAndyGuidance.isChecked = SystemSettingData.Instance.AndyGuidance;
		mMouseStateTip.isChecked = SystemSettingData.Instance.MouseStateTip;
		mHidePlayerOverHeadInfo.isChecked = SystemSettingData.Instance.HidePlayerOverHeadInfo;
	}

	protected override void OnClose()
	{
		OnCanncelBtn();
	}

	private void LateUpdate()
	{
		if (LastQualityLevel != mQuality.mIndex)
		{
			LastQualityLevel = mQuality.mIndex;
			ResetVideo(LastQualityLevel, fromUpdate: true);
		}
		int num = mTabIndex;
		if (num == 1)
		{
			SystemSettingData.Instance.SoundVolume = mSoundVolume.mScroll.scrollValue;
			SystemSettingData.Instance.MusicVolume = mMusicVolume.mScroll.scrollValue;
			SystemSettingData.Instance.DialogVolume = mDialogVolume.mScroll.scrollValue;
			SystemSettingData.Instance.EffectVolume = mEffectVolume.mScroll.scrollValue;
		}
		if (mWaterDepth.mIndex == 0)
		{
			mWaterRefraction.SetIndex(0);
		}
	}

	public void OnChange()
	{
		if (mTabIndex == 0)
		{
			mQuality.SetIndex(6);
			LastQualityLevel = mQuality.mIndex;
		}
	}

	private bool TrySetKeyInCommon(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor, int conflictMask = -1)
	{
		tmpIdx[0] = accessor.FindInArray(mKeySettingLists[0], newKey);
		if (tmpIdx[0] < 0)
		{
			return false;
		}
		KeyCode keyToSet = accessor.Get(itemToSet);
		if (tmpIdx[0] > 0)
		{
			accessor.Set(mKeySettingLists[0][tmpIdx[0] - 1], keyToSet);
		}
		else
		{
			for (int i = 1; i < 4; i++)
			{
				if (((1 << i) & conflictMask) == 0)
				{
					tmpIdx[i] = 0;
					continue;
				}
				tmpIdx[i] = accessor.FindInArray(mKeySettingLists[i], newKey);
				if (tmpIdx[i] < 0)
				{
					return false;
				}
			}
			for (int j = 1; j < 4; j++)
			{
				if (tmpIdx[j] > 0)
				{
					accessor.Set(mKeySettingLists[j][tmpIdx[j] - 1], keyToSet);
				}
			}
		}
		accessor.Set(itemToSet, newKey);
		return true;
	}

	private bool TrySetKeyInOther(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor, int cur)
	{
		tmpIdx[cur] = accessor.FindInArray(mKeySettingLists[cur], newKey);
		if (tmpIdx[cur] < 0)
		{
			return false;
		}
		KeyCode keyCode = accessor.Get(itemToSet);
		if (tmpIdx[cur] > 0)
		{
			accessor.Set(mKeySettingLists[cur][tmpIdx[cur] - 1], keyCode);
		}
		else
		{
			tmpIdx[0] = accessor.FindInArray(mKeySettingLists[0], newKey);
			if (tmpIdx[0] < 0)
			{
				return false;
			}
			if (tmpIdx[0] > 0 && !TrySetKeyInCommon(mKeySettingLists[0][tmpIdx[0] - 1], keyCode, accessor, -1 & ~(1 << cur)))
			{
				return false;
			}
		}
		accessor.Set(itemToSet, newKey);
		return true;
	}

	public bool TrySetKey(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor)
	{
		if (0 <= Array.IndexOf(mKeySettingLists[0], itemToSet))
		{
			return TrySetKeyInCommon(itemToSet, newKey, accessor);
		}
		for (int i = 1; i < 4; i++)
		{
			if (0 <= Array.IndexOf(mKeySettingLists[i], itemToSet))
			{
				return TrySetKeyInOther(itemToSet, newKey, accessor, i);
			}
		}
		return false;
	}

	public KeySettingItem TestConflict(KeySettingItem itemToSet, KeyCode newKey, IKeyJoyAccessor accessor)
	{
		int[] array = new int[6] { 255, 3, 5, 9, 17, 33 };
		int num = mKeySettingLists.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (0 > Array.IndexOf(mKeySettingLists[i], itemToSet))
			{
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				if ((array[i] & (1 << j)) != 0 && (num2 = accessor.FindInArray(mKeySettingLists[j], newKey)) != 0)
				{
					if (num2 < 0)
					{
						num2 = -num2;
					}
					return mKeySettingLists[j][num2 - 1];
				}
			}
		}
		return null;
	}

	private void Update()
	{
		mCamS.text = ((float)(int)((CamSMin + (CamSMax - CamSMin) * mCameraSensitivity.mScroll.scrollValue) * 10f) / 10f).ToString();
		mHoldGunCamS.text = ((float)(int)((CamSMin + (CamSMax - CamSMin) * mHoldGunCameraSensitivity.mScroll.scrollValue) * 10f) / 10f).ToString();
		mCameraFOV.text = ((int)(CamFOVMin + (CamFOVMax - CamFOVMin) * mCameraFOVScroll.mScroll.scrollValue)).ToString();
		mCamInertiaValue.text = ((float)(int)((CamInertiaMin + (CamInertiaMax - CamInertiaMin) * mCameraInertia.mScroll.scrollValue) * 100f) / 100f).ToString();
		DriveCamInertiaValueLabel.text = ((float)(int)((m_DriveCamInertiaMin + (m_DriveCamInertiaMax - m_DriveCamInertiaMin) * DriveCameraInertiaScroll.mScroll.scrollValue) * 100f) / 100f).ToString();
	}
}
