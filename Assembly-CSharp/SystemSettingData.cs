using System;
using System.IO;
using System.Xml;
using InControl;
using Pathea;
using RedGrass;
using UnityEngine;

public class SystemSettingData
{
	private static SystemSettingData mInstance;

	public bool dataDirty;

	public bool mHasData;

	public string mVersion = "0.795";

	public string mLanguage = "english";

	private int _bLangChinese = -1;

	public byte TerrainLevel = 3;

	public byte RandomTerrainLevel;

	public int mTreeLevel = 3;

	public int mQualityLevel = 6;

	public bool VoxelCacheEnabled = true;

	public int mLightCount = 20;

	public int mAnisotropicFiltering = 2;

	public int mAntiAliasing = 1;

	public int mShadowProjection = 1;

	public int mShadowDistance = 2;

	public int mShadowCascades = 2;

	private int mWaterReflection_ = 2;

	private bool mWaterRefraction = true;

	private bool mWaterDepth = true;

	public float GrassDensity = 0.6f;

	public ELodType mGrassLod = ELodType.LOD_3_TYPE_1;

	public bool mDepthBlur = true;

	public bool mSSAO;

	public float SoundVolume = 1f;

	public float MusicVolume = 1f;

	public float DialogVolume = 1f;

	public float EffectVolume = 1f;

	public float CameraSensitivity = 1.5f;

	public float CameraFov = 60f;

	public bool CameraHorizontalInverse;

	public bool CameraVerticalInverse;

	public bool mMMOControlType = true;

	public bool UseController = true;

	public bool FirstPersonCtrl;

	public bool AttackWhithMouseDir;

	public bool Tutorialed;

	public bool HideHeadgear;

	public bool HPNumbers;

	public bool ClipCursor;

	public bool ApplyMonsterIK;

	public bool SyncCount = true;

	public bool HDREffect = true;

	private bool _fastLightingMode = true;

	public float CamInertia = 7.5f;

	public float DriveCamInertia = 50f;

	public string GLSetting = "test2";

	private bool LoadedData;

	private string mFilepath = string.Empty;

	public bool FixBlurryFont;

	public bool AndyGuidance = true;

	public bool MouseStateTip = true;

	public bool HidePlayerOverHeadInfo;

	public static SystemSettingData Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new SystemSettingData();
			}
			return mInstance;
		}
	}

	public bool IsChinese
	{
		get
		{
			if (_bLangChinese < 0)
			{
				_bLangChinese = ((mLanguage.Contains("chinese") || mLanguage.Contains("中文")) ? 1 : 0);
			}
			return _bLangChinese != 0;
		}
	}

	public int treeLevel
	{
		get
		{
			if ((TerrainLevel <= 1 && PeGameMgr.IsStory) || (RandomTerrainLevel <= 1 && PeGameMgr.randomMap))
			{
				return 1;
			}
			return mTreeLevel;
		}
	}

	public int mWaterReflection
	{
		get
		{
			return mWaterReflection_;
		}
		set
		{
			mWaterReflection_ = (WaterReflection.ReflectionSetting = value);
		}
	}

	public bool WaterRefraction
	{
		get
		{
			return mWaterRefraction;
		}
		set
		{
			mWaterRefraction = value;
			if (VFVoxelWater.self != null)
			{
				VFVoxelWater.self.ApplyQuality(WaterRefraction, WaterDepth);
			}
		}
	}

	public bool WaterDepth
	{
		get
		{
			return mWaterDepth;
		}
		set
		{
			mWaterDepth = value;
			if (!mWaterDepth)
			{
				WaterRefraction = false;
			}
			if (VFVoxelWater.self != null)
			{
				VFVoxelWater.self.ApplyQuality(WaterRefraction, WaterDepth);
			}
		}
	}

	public ELodType GrassLod
	{
		get
		{
			if ((!PeGameMgr.randomMap) ? (TerrainLevel <= 1) : (RandomTerrainLevel <= 1))
			{
				return ELodType.LOD_1_TYPE_1;
			}
			return mGrassLod;
		}
		set
		{
			mGrassLod = value;
		}
	}

	public float AbsEffectVolume => SoundVolume * EffectVolume;

	public float AbsMusicVolume => SoundVolume * MusicVolume;

	public float AbsDialogVolume => SoundVolume * DialogVolume;

	public bool mFastLightingMode
	{
		get
		{
			return _fastLightingMode;
		}
		set
		{
			_fastLightingMode = value;
			LightMgr.Instance.SetLightMode(value);
		}
	}

	public SystemSettingData()
	{
		dataDirty = false;
	}

	public static void Save()
	{
		if (Instance != null && Instance.dataDirty)
		{
			Instance.ApplySettings();
		}
	}

	public void ResetVSync()
	{
		if (PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.GameScene)
		{
			QualitySettings.vSyncCount = (SyncCount ? 1 : 0);
		}
		else
		{
			QualitySettings.vSyncCount = 1;
		}
	}

	public void ApplyAudio()
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			using FileStream inStream = new FileStream(mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read);
			xmlDocument.Load(inStream);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex, GameLog.EIOFileType.Settings);
			xmlDocument = new XmlDocument();
		}
		XmlElement xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("Sound");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("Sound");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Volume", SoundVolume.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("Music");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("Music");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Volume", MusicVolume.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("Dialog");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("Dialog");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Volume", DialogVolume.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("Effect");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("Effect");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Volume", EffectVolume.ToString());
		try
		{
			using FileStream outStream = new FileStream(mFilepath, FileMode.Create, FileAccess.Write, FileShare.None);
			xmlDocument.Save(outStream);
		}
		catch (Exception ex2)
		{
			GameLog.HandleIOException(ex2, GameLog.EIOFileType.Settings);
		}
	}

	public void ApplyVideo()
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			using FileStream inStream = new FileStream(mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read);
			xmlDocument.Load(inStream);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex, GameLog.EIOFileType.Settings);
			xmlDocument = new XmlDocument();
		}
		XmlElement xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("QualityLevel");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("QualityLevel");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mQualityLevel.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("GLSetting");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("GLSetting");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", GLSetting);
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("LightCount");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("LightCount");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mLightCount.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("AnisotropicFiltering");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("AnisotropicFiltering");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mAnisotropicFiltering.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("AntiAliasing");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("AntiAliasing");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mAntiAliasing.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("ShadowProjection");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("ShadowProjection");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mShadowProjection.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("ShadowDistance");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("ShadowDistance");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mShadowDistance.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("ShadowCascades");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("ShadowCascades");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mShadowCascades.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("mWaterReflection");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("mWaterReflection");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mWaterReflection.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("WaterRefraction");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("WaterRefraction");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", WaterRefraction.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("WaterDepth");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("WaterDepth");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", WaterDepth.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("GrassDensity");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("GrassDensity");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", GrassDensity.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("GrassLod");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("GrassLod");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		XmlElement xmlElement2 = xmlElement;
		int num = (int)mGrassLod;
		xmlElement2.SetAttribute("Index", num.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("Terrain");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("Terrain");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", TerrainLevel.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("RandomTerrain");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("RandomTerrain");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", RandomTerrainLevel.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("DepthBlur");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("DepthBlur");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", mDepthBlur.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("SSAO");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("SSAO");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", mSSAO.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("SyncCount");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("SyncCount");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", SyncCount.ToString());
		ResetVSync();
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("Tree");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("Tree");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mTreeLevel.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("HDREffect");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("HDREffect");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", HDREffect.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("FastLightingMode");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("FastLightingMode");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("Index", mFastLightingMode.ToString());
		switch (mQualityLevel)
		{
		case 0:
			QualitySettings.SetQualityLevel(0);
			TerrainLevel = 1;
			RandomTerrainLevel = 0;
			mTreeLevel = 1;
			HDREffect = false;
			break;
		case 1:
			QualitySettings.SetQualityLevel(1);
			TerrainLevel = 1;
			RandomTerrainLevel = 0;
			mTreeLevel = 1;
			HDREffect = false;
			break;
		case 2:
			QualitySettings.SetQualityLevel(2);
			TerrainLevel = 1;
			RandomTerrainLevel = 1;
			mTreeLevel = 1;
			HDREffect = false;
			break;
		case 3:
			QualitySettings.SetQualityLevel(3);
			TerrainLevel = 2;
			RandomTerrainLevel = 1;
			mTreeLevel = 3;
			HDREffect = true;
			break;
		case 4:
			QualitySettings.SetQualityLevel(4);
			TerrainLevel = 3;
			RandomTerrainLevel = 2;
			mTreeLevel = 4;
			HDREffect = true;
			break;
		case 5:
			QualitySettings.SetQualityLevel(5);
			TerrainLevel = 3;
			RandomTerrainLevel = 2;
			mTreeLevel = 5;
			HDREffect = true;
			break;
		case 6:
			QualitySettings.SetQualityLevel(3);
			QualitySettings.pixelLightCount = mLightCount;
			switch (mAnisotropicFiltering)
			{
			case 0:
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
				break;
			case 1:
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
				break;
			case 2:
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				break;
			}
			QualitySettings.antiAliasing = ((mAntiAliasing > 0) ? 4 : 0);
			QualitySettings.shadowProjection = ((mShadowProjection == 1) ? ShadowProjection.StableFit : ShadowProjection.CloseFit);
			switch (mShadowDistance)
			{
			case 0:
				QualitySettings.shadowDistance = 1f;
				break;
			case 1:
				QualitySettings.shadowDistance = 50f;
				break;
			case 2:
				QualitySettings.shadowDistance = 100f;
				break;
			case 3:
				QualitySettings.shadowDistance = 200f;
				break;
			case 4:
				QualitySettings.shadowDistance = 400f;
				break;
			}
			switch (mShadowCascades)
			{
			case 0:
				QualitySettings.shadowCascades = 0;
				break;
			case 1:
				QualitySettings.shadowCascades = 2;
				break;
			case 2:
				QualitySettings.shadowCascades = 4;
				break;
			}
			break;
		}
		PeGrassSystem.Refresh(GrassDensity, (int)GrassLod);
		PECameraMan.ApplySysSetting();
		try
		{
			using FileStream outStream = new FileStream(mFilepath, FileMode.Create, FileAccess.Write, FileShare.None);
			xmlDocument.Save(outStream);
		}
		catch (Exception ex2)
		{
			GameLog.HandleIOException(ex2, GameLog.EIOFileType.Settings);
		}
	}

	public void ApplyKeySetting()
	{
		PeInput.SaveInputConfig(mFilepath);
	}

	public void ApplyControl()
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			using FileStream inStream = new FileStream(mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read);
			xmlDocument.Load(inStream);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex, GameLog.EIOFileType.Settings);
			xmlDocument = new XmlDocument();
		}
		XmlElement xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("mMMOControlType");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("mMMOControlType");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", mMMOControlType.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("UseController");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("UseController");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", UseController.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("FirstPersonCtrl");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("FirstPersonCtrl");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", FirstPersonCtrl.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("CameraSensitivity");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("CameraSensitivity");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", CameraSensitivity.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("CameraFov");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("CameraFov");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", CameraFov.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("CameraHorizontalInverse");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("CameraHorizontalInverse");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", CameraHorizontalInverse.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("CameraVerticalInverse");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("CameraVerticalInverse");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", CameraVerticalInverse.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("CamInertia");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("CamInertia");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", CamInertia.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("DriveCamInertia");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("DriveCamInertia");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", DriveCamInertia.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("AttackWhithMouseDir");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("AttackWhithMouseDir");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", AttackWhithMouseDir.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("Tutorialed");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("Tutorialed");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", Tutorialed.ToString());
		PECameraMan.ApplySysSetting();
		try
		{
			using FileStream outStream = new FileStream(mFilepath, FileMode.Create, FileAccess.Write, FileShare.None);
			xmlDocument.Save(outStream);
		}
		catch (Exception ex2)
		{
			GameLog.HandleIOException(ex2, GameLog.EIOFileType.Settings);
		}
	}

	public void ApplyMisc()
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			using FileStream inStream = new FileStream(mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read);
			xmlDocument.Load(inStream);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex, GameLog.EIOFileType.Settings);
			xmlDocument = new XmlDocument();
		}
		XmlElement xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("HideHeadgear");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("HideHeadgear");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", HideHeadgear.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("HPNumbers");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("HPNumbers");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", HPNumbers.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("ClipCursor");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("ClipCursor");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", ClipCursor.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("FixBlurryFont");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("FixBlurryFont");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", FixBlurryFont.ToString());
		if (GameUI.Instance != null)
		{
			UILabel[] componentsInChildren = GameUI.Instance.gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].MakePixelPerfect();
			}
		}
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("AndyGuidance");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("AndyGuidance");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", AndyGuidance.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("MouseStateTip");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("MouseStateTip");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", MouseStateTip.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("HidePlayerOverHeadInfo");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("HidePlayerOverHeadInfo");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", HidePlayerOverHeadInfo.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("ApplyMonsterIK");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("ApplyMonsterIK");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", ApplyMonsterIK.ToString());
		xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("VoxelCache");
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement("VoxelCache");
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlElement.SetAttribute("value", VoxelCacheEnabled.ToString());
		PECameraMan.ApplySysSetting();
		try
		{
			using FileStream outStream = new FileStream(mFilepath, FileMode.Create, FileAccess.Write, FileShare.None);
			xmlDocument.Save(outStream);
		}
		catch (Exception ex2)
		{
			GameLog.HandleIOException(ex2, GameLog.EIOFileType.Settings);
		}
	}

	public void ApplySettings()
	{
		ApplyAudio();
		ApplyVideo();
		ApplyKeySetting();
		ApplyControl();
		ApplyMisc();
		dataDirty = false;
	}

	private void SetSystemData(XmlDocument xmlDoc)
	{
		XmlElement xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("QualityLevel");
		if (xmlElement != null)
		{
			mQualityLevel = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("LightCount");
		if (xmlElement != null)
		{
			mLightCount = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AnisotropicFiltering");
		if (xmlElement != null)
		{
			mAnisotropicFiltering = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AntiAliasing");
		if (xmlElement != null)
		{
			mAntiAliasing = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowProjection");
		if (xmlElement != null)
		{
			mShadowProjection = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowDistance");
		if (xmlElement != null)
		{
			mShadowDistance = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ShadowCascades");
		if (xmlElement != null)
		{
			mShadowCascades = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("mWaterReflection");
		if (xmlElement != null)
		{
			mWaterReflection = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("WaterRefraction");
		if (xmlElement != null)
		{
			WaterRefraction = Convert.ToBoolean(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("WaterDepth");
		if (xmlElement != null)
		{
			WaterDepth = Convert.ToBoolean(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GrassDensity");
		if (xmlElement != null)
		{
			GrassDensity = Convert.ToSingle(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GrassLod");
		if (xmlElement != null)
		{
			mGrassLod = (ELodType)Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Terrain");
		if (xmlElement != null)
		{
			TerrainLevel = Convert.ToByte(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("RandomTerrain");
		if (xmlElement != null)
		{
			RandomTerrainLevel = Convert.ToByte(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Tree");
		if (xmlElement != null)
		{
			mTreeLevel = Convert.ToInt32(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HDREffect");
		if (xmlElement != null)
		{
			HDREffect = Convert.ToBoolean(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FastLightingMode");
		if (xmlElement != null)
		{
			mFastLightingMode = Convert.ToBoolean(xmlElement.GetAttribute("Index"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Sound");
		if (xmlElement != null)
		{
			SoundVolume = Convert.ToSingle(xmlElement.GetAttribute("Volume"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Music");
		if (xmlElement != null)
		{
			MusicVolume = Convert.ToSingle(xmlElement.GetAttribute("Volume"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Dialog");
		if (xmlElement != null)
		{
			DialogVolume = Convert.ToSingle(xmlElement.GetAttribute("Volume"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Effect");
		if (xmlElement != null)
		{
			EffectVolume = Convert.ToSingle(xmlElement.GetAttribute("Volume"));
		}
		if (xmlDoc.DocumentElement.HasAttribute("Version") && xmlDoc.DocumentElement.GetAttribute("Version") == mVersion)
		{
			xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("mMMOControlType");
			if (xmlElement != null)
			{
				mMMOControlType = Convert.ToBoolean(xmlElement.GetAttribute("value"));
			}
		}
		else
		{
			mMMOControlType = false;
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("UseController");
		if (xmlElement != null)
		{
			UseController = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FirstPersonCtrl");
		if (xmlElement != null)
		{
			FirstPersonCtrl = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraSensitivity");
		if (xmlElement != null)
		{
			CameraSensitivity = Convert.ToSingle(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraFov");
		if (xmlElement != null)
		{
			CameraFov = Convert.ToSingle(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraHorizontalInverse");
		if (xmlElement != null)
		{
			CameraHorizontalInverse = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CameraVerticalInverse");
		if (xmlElement != null)
		{
			CameraVerticalInverse = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HideHeadgear");
		if (xmlElement != null)
		{
			HideHeadgear = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HPNumbers");
		if (xmlElement != null)
		{
			HPNumbers = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ClipCursor");
		if (xmlElement != null)
		{
			ClipCursor = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("ApplyMonsterIK");
		if (xmlElement != null)
		{
			ApplyMonsterIK = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("SyncCount");
		if (xmlElement != null)
		{
			SyncCount = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("VoxelCache");
		if (xmlElement != null)
		{
			VoxelCacheEnabled = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("DepthBlur");
		if (xmlElement != null)
		{
			mDepthBlur = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("CamInertia");
		if (xmlElement != null)
		{
			CamInertia = Convert.ToSingle(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("DriveCamInertia");
		if (xmlElement != null)
		{
			DriveCamInertia = Convert.ToSingle(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("SSAO");
		if (xmlElement != null)
		{
			mSSAO = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AttackWhithMouseDir");
		if (xmlElement != null)
		{
			AttackWhithMouseDir = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("Tutorialed");
		if (xmlElement != null)
		{
			Tutorialed = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("GLSetting");
		if (xmlElement != null)
		{
			GLSetting = xmlElement.GetAttribute("value");
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("FixBlurryFont");
		if (xmlElement != null)
		{
			FixBlurryFont = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("AndyGuidance");
		if (xmlElement != null)
		{
			AndyGuidance = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("MouseStateTip");
		if (xmlElement != null)
		{
			MouseStateTip = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		xmlElement = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode("HidePlayerOverHeadInfo");
		if (xmlElement != null)
		{
			HidePlayerOverHeadInfo = Convert.ToBoolean(xmlElement.GetAttribute("value"));
		}
		PeInput.LoadInputConfig(mFilepath);
		ApplySettings();
	}

	public void LoadSystemData()
	{
		if (LoadedData)
		{
			return;
		}
		if (null == Singleton<InControlManager>.Instance)
		{
			Debug.LogError("InControlManager isn't init.");
		}
		mFilepath = GameConfig.GetUserDataPath() + "/PlanetExplorers/Config";
		if (!Directory.Exists(mFilepath))
		{
			Directory.CreateDirectory(mFilepath);
		}
		mFilepath += "/SystemSaveData.xml";
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			using (FileStream inStream = new FileStream(mFilepath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				xmlDocument.Load(inStream);
			}
			SetSystemData(xmlDocument);
			mHasData = true;
		}
		catch
		{
			XmlDocument xmlDocument2 = new XmlDocument();
			xmlDocument2.LoadXml("<SystemData/>");
			xmlDocument2.DocumentElement.SetAttribute("Version", mVersion);
			try
			{
				using FileStream outStream = new FileStream(mFilepath, FileMode.Create, FileAccess.Write, FileShare.None);
				xmlDocument2.Save(outStream);
			}
			catch (Exception ex)
			{
				GameLog.HandleIOException(ex, GameLog.EIOFileType.Settings);
			}
			ApplySettings();
			mHasData = false;
		}
		LoadedData = true;
	}
}
