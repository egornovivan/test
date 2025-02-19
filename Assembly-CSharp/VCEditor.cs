using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class VCEditor : MonoBehaviour
{
	public delegate void DNoParam();

	public delegate void DSceneParam(VCEScene scene);

	private static VCEditor s_Instance;

	public static bool s_Active;

	public static bool s_Ready;

	private static float s_OpenTime = -1f;

	private static float s_QuitTime = -1f;

	public static int s_SceneId;

	public static VCEScene s_Scene;

	public static VCEMirrorSetting s_Mirror;

	public static bool s_ProtectLock0;

	public static bool s_ProtectLock1;

	public static bool s_ProtectLock2;

	public static bool s_ProtectLock3;

	private static VCMaterial s_SelectedMaterial;

	private static int s_SelectedVoxelType;

	private static ulong s_SelectedDecalGUID;

	private static int s_SelectedDecalIndex;

	private static EVCETransformType s_TransformType;

	private static VCPartInfo s_SelectedPart;

	public static int s_OutsideCameraCullingMask;

	public static bool s_ConnectedToGame;

	public static bool s_MultiplayerMode;

	public GameObject m_EditorGO;

	public GameObject m_UIPrefab;

	public VCEUI m_UI;

	public GUISkin m_GUISkin;

	public Camera m_MainCamera;

	public Camera m_CaptureCamera;

	public GameObject m_GLGroup;

	public GameObject m_MirrorGroup;

	public GameObject m_BrushGroup;

	public GLNearVoxelIndicator m_NearVoxelIndicator;

	public VCEVoxelSelection m_VoxelSelection;

	public GameObject m_PartGroup;

	public GameObject m_DecalGroup;

	public GameObject m_EffectGroup;

	public Material m_TempIsoMat;

	public Material m_HolographicMat;

	public GameObject m_WaterMaskPrefab;

	public VCMeshMgr m_MeshMgr;

	public VCEUISceneGizmoCam m_SceneGizmo;

	public GameObject m_CreationGroup;

	public Transform m_MassCenterTrans;

	public bool m_CheatWhenMakeCreation;

	public static VCEditor Instance => s_Instance;

	public static int SelectedVoxelType => s_SelectedVoxelType;

	public static VCMaterial SelectedMaterial
	{
		get
		{
			return s_SelectedMaterial;
		}
		set
		{
			s_SelectedMaterial = value;
			if (value == null)
			{
				s_SelectedVoxelType = -1;
			}
			else
			{
				s_SelectedVoxelType = s_Scene.m_IsoData.QueryVoxelType(value);
			}
		}
	}

	public static int SelectedDecalIndex => s_SelectedDecalIndex;

	public static ulong SelectedDecalGUID
	{
		get
		{
			return s_SelectedDecalGUID;
		}
		set
		{
			s_SelectedDecalGUID = value;
			if (value == 0L)
			{
				s_SelectedDecalIndex = -1;
				return;
			}
			VCDecalAsset decal = VCEAssetMgr.GetDecal(value);
			s_SelectedDecalIndex = s_Scene.m_IsoData.QueryNewDecalIndex(decal);
		}
	}

	public static VCDecalAsset SelectedDecal
	{
		get
		{
			return VCEAssetMgr.GetDecal(s_SelectedDecalGUID);
		}
		set
		{
			if (value == null)
			{
				s_SelectedDecalGUID = 0uL;
				s_SelectedDecalIndex = -1;
			}
			else
			{
				s_SelectedDecalGUID = value.m_Guid;
				s_SelectedDecalIndex = s_Scene.m_IsoData.QueryNewDecalIndex(value);
			}
		}
	}

	public static VCEBrush[] SelectedBrushes
	{
		get
		{
			VCEBrush[] result = new VCEBrush[0];
			if (s_Instance != null)
			{
				result = s_Instance.m_BrushGroup.GetComponentsInChildren<VCEBrush>();
			}
			return result;
		}
	}

	public static VCESelectComponent SelectComponentBrush => s_Instance.m_BrushGroup.GetComponentInChildren<VCESelectComponent>();

	public static bool SelectedGeneralBrush
	{
		get
		{
			VCEBrush[] selectedBrushes = SelectedBrushes;
			VCEBrush[] array = selectedBrushes;
			foreach (VCEBrush vCEBrush in array)
			{
				if (vCEBrush.m_Type == EVCEBrushType.General)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static EVCETransformType TransformType
	{
		get
		{
			return s_TransformType;
		}
		set
		{
			s_TransformType = value;
		}
	}

	public static VCPartInfo SelectedPart
	{
		get
		{
			return s_SelectedPart;
		}
		set
		{
			s_SelectedPart = value;
		}
	}

	public static Color32 SelectedColor
	{
		get
		{
			if (Instance == null)
			{
				return VCIsoData.BLANK_COLOR;
			}
			if (Instance.m_UI == null)
			{
				return VCIsoData.BLANK_COLOR;
			}
			Color finalColor = Instance.m_UI.m_PaintColorPick.FinalColor;
			float a = Instance.m_UI.m_PaintBlendMethodSlider.sliderValue * 0.8f + 0.1f;
			return new Color(finalColor.r, finalColor.g, finalColor.b, a);
		}
		set
		{
			if (!(Instance == null) && !(Instance.m_UI == null))
			{
				Instance.m_UI.m_PaintColorPick.FinalColor = value;
				Instance.m_UI.m_PaintBlendMethodSlider.sliderValue = Mathf.Clamp01(((float)(int)value.a - 0.2f) / 0.65f);
			}
		}
	}

	public static Dictionary<int, byte> VoxelSelection => Instance.m_VoxelSelection.m_Selection;

	public static event DNoParam OnOpen;

	public static event DNoParam OnMakeCreation;

	public static event DNoParam OnReady;

	public static event DNoParam OnCloseComing;

	public static event DNoParam OnCloseFinally;

	public static event DSceneParam OnSceneCreate;

	public static event DSceneParam OnSceneClose;

	public static void DeselectBrushes()
	{
		VCEBrush[] selectedBrushes = SelectedBrushes;
		VCEBrush[] array = selectedBrushes;
		foreach (VCEBrush vCEBrush in array)
		{
			vCEBrush.Cancel();
		}
	}

	public static void DestroyBrushes()
	{
		VCEBrush[] selectedBrushes = SelectedBrushes;
		VCEBrush[] array = selectedBrushes;
		foreach (VCEBrush vCEBrush in array)
		{
			UnityEngine.Object.Destroy(vCEBrush.gameObject);
		}
	}

	public static void DeselectAllTools()
	{
		if (DocumentOpen())
		{
			SelectedMaterial = null;
			SelectedPart = null;
			SelectedColor = Color.white;
			DeselectBrushes();
		}
	}

	public static void ClearSelections()
	{
		VCESelect[] componentsInChildren = s_Instance.m_BrushGroup.GetComponentsInChildren<VCESelect>();
		VCESelect[] array = componentsInChildren;
		foreach (VCESelect vCESelect in array)
		{
			vCESelect.ClearSelection();
		}
		if (Instance != null && Instance.m_VoxelSelection != null)
		{
			Instance.m_VoxelSelection.ClearSelection();
		}
	}

	private static void Init()
	{
		s_Active = false;
		s_Ready = false;
		VCEAssetMgr.Init();
		VCEHistory.Init();
		GameObject gameObject = UnityEngine.Object.Instantiate(s_Instance.m_UIPrefab);
		gameObject.transform.parent = s_Instance.m_EditorGO.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.SetActive(value: true);
		s_Instance.m_UI = gameObject.GetComponent<VCEUI>();
		s_Instance.m_UI.Init();
	}

	private static void Destroy()
	{
		if (s_Active)
		{
			QuitFinally();
		}
		VCEAssetMgr.Destroy();
		VCEHistory.Destroy();
	}

	public static void Open()
	{
		if (!(s_Instance == null) && !s_Active)
		{
			Debug.Log("VCE open.");
			NewScene(VCConfig.FirstSceneSetting);
			s_Instance.m_EditorGO.SetActive(value: true);
			s_Instance.m_GLGroup.SetActive(value: true);
			s_Instance.m_UI.ShowUI();
			s_Active = true;
			s_OpenTime = 0f;
			if (Camera.main != null)
			{
				Debug.Log("VCE set main camera layer nothing.");
				VCGameMediator.CloseGameMainCamera();
			}
			if (VCEditor.OnOpen != null)
			{
				VCEditor.OnOpen();
			}
		}
	}

	public static void Quit()
	{
		if (s_Active && s_QuitTime < 0f)
		{
			s_QuitTime = 0f;
			s_Ready = false;
			s_Instance.m_UI.AllBoxTweenOut();
			Debug.Log("VCE will close.");
			if (VCEditor.OnCloseComing != null)
			{
				VCEditor.OnCloseComing();
			}
		}
	}

	public static void QuitFinally()
	{
		if (!(s_Instance == null) && s_Active)
		{
			CloseScene();
			Debug.Log("VCE hide ui.");
			s_Instance.m_UI.HideUI();
			Debug.Log("VCE disable editor go.");
			s_Instance.m_GLGroup.SetActive(value: false);
			s_Instance.m_EditorGO.SetActive(value: false);
			Debug.Log("VCE trying to revert outside camera.");
			VCGameMediator.RevertGameMainCamera();
			Debug.Log("VCE revert outside camera succeed.");
			s_Active = false;
			s_Ready = false;
			if (VCEditor.OnCloseFinally != null)
			{
				VCEditor.OnCloseFinally();
			}
			Debug.Log("VCE is closed.");
		}
	}

	public static void NewScene(VCESceneSetting setting)
	{
		if (!(s_Instance == null))
		{
			CloseScene();
			s_SceneId++;
			s_Scene = new VCEScene(setting);
			s_Scene.BuildScene();
			Debug.Log("VCE new scene. sceneid = " + s_SceneId);
			AfterSceneChanged(setting);
			if (VCEditor.OnSceneCreate != null)
			{
				VCEditor.OnSceneCreate(s_Scene);
			}
			string text = "New scene".ToLocalizationString() + " ";
			string[] array = s_Scene.m_IsoData.m_HeadInfo.ScenePaths();
			string[] array2 = array;
			foreach (string origin in array2)
			{
				text = text + "[" + origin.ToLocalizationString() + "] ";
			}
			text = text + "is ready".ToLocalizationString() + " !";
			VCEStatusBar.ShowText(text, 10f, typeeffect: true);
		}
	}

	public static void NewScene(VCESceneSetting setting, int template)
	{
		if (!(s_Instance == null))
		{
			CloseScene();
			s_SceneId++;
			s_Scene = new VCEScene(setting, template);
			s_Scene.BuildScene();
			Debug.Log("VCE new scene. sceneid = " + s_SceneId);
			AfterSceneChanged(setting);
			if (VCEditor.OnSceneCreate != null)
			{
				VCEditor.OnSceneCreate(s_Scene);
			}
			string text = "New scene".ToLocalizationString() + " ";
			string[] array = s_Scene.m_IsoData.m_HeadInfo.ScenePaths();
			string[] array2 = array;
			foreach (string origin in array2)
			{
				text = text + "[" + origin.ToLocalizationString() + "] ";
			}
			text = text + "is ready".ToLocalizationString() + " !";
			VCEStatusBar.ShowText(text, 10f, typeeffect: true);
		}
	}

	public static void LoadIso(string path)
	{
		if (!(s_Instance == null))
		{
			CloseScene();
			try
			{
				s_SceneId++;
				s_Scene = new VCEScene(path);
				s_Scene.BuildScene();
				AfterSceneChanged(s_Scene.m_Setting);
			}
			catch (Exception)
			{
				NewScene(VCConfig.FirstSceneSetting);
				VCEMsgBox.Show(VCEMsgBoxType.CORRUPT_ISO);
				return;
			}
			if (VCEditor.OnSceneCreate != null)
			{
				VCEditor.OnSceneCreate(s_Scene);
			}
			Debug.Log("VCE load iso " + path + " sceneid = " + s_SceneId);
			VCEStatusBar.ShowText("Load".ToLocalizationString() + " ISO [" + path + "] " + "Complete".ToLocalizationString() + " !", 10f, typeeffect: true);
		}
	}

	private static void AfterSceneChanged(VCESceneSetting setting)
	{
		s_Mirror = new VCEMirrorSetting();
		s_Mirror.Reset(setting.m_EditorSize.x, setting.m_EditorSize.y, setting.m_EditorSize.z);
		Instance.m_MeshMgr.m_VoxelSize = s_Scene.m_Setting.m_VoxelSize;
		Instance.m_MeshMgr.m_MeshMat = s_Scene.m_TempIsoMat.m_EditorMat;
		float num = setting.m_EditorSize.ToVector3().magnitude * setting.m_VoxelSize;
		s_Instance.m_MainCamera.nearClipPlane = setting.m_VoxelSize * 0.5f;
		s_Instance.m_MainCamera.farClipPlane = num * 5f;
		VCECamera component = s_Instance.m_MainCamera.GetComponent<VCECamera>();
		if (s_Scene.m_IsoData.m_Voxels.Count == 0)
		{
			component.BeginTarget = setting.m_EditorSize.ToVector3() * setting.m_VoxelSize * 0.5f;
			component.BeginTarget.y = 0f;
			component.BeginDistance = new Vector2(setting.m_EditorSize.x, setting.m_EditorSize.z).magnitude * setting.m_VoxelSize * 1.2f;
			component.MinDistance = setting.m_VoxelSize * 0.5f;
			component.MaxDistance = num * 3f;
			component.Reset();
		}
		else
		{
			component.BeginTarget = setting.m_EditorSize.ToVector3() * setting.m_VoxelSize * 0.5f;
			component.BeginDistance = setting.m_EditorSize.ToVector3().magnitude * setting.m_VoxelSize * 1.2f;
			component.MinDistance = setting.m_VoxelSize * 0.5f;
			component.MaxDistance = num * 3f;
			component.Reset();
		}
		GLGridPlane[] componentsInChildren = s_Instance.m_GLGroup.GetComponentsInChildren<GLGridPlane>(includeInactive: true);
		GLBound[] componentsInChildren2 = s_Instance.m_GLGroup.GetComponentsInChildren<GLBound>(includeInactive: true);
		GLGridPlane[] array = componentsInChildren;
		foreach (GLGridPlane gLGridPlane in array)
		{
			gLGridPlane.m_CellCount = new IntVector3(setting.m_EditorSize.x, setting.m_EditorSize.y, setting.m_EditorSize.z);
			gLGridPlane.m_CellSize = Vector3.one * setting.m_VoxelSize;
			gLGridPlane.m_MajorGridInterval = setting.m_MajorInterval;
			gLGridPlane.m_MinorGridInterval = setting.m_MinorInterval;
			gLGridPlane.m_Fdisk = setting.m_Category == EVCCategory.cgDbSword;
		}
		GLBound[] array2 = componentsInChildren2;
		foreach (GLBound gLBound in array2)
		{
			gLBound.m_Bound = new Bounds(Vector3.zero, Vector3.zero);
			gLBound.m_Bound.SetMinMax(Vector3.zero, setting.m_EditorSize.ToVector3() * setting.m_VoxelSize);
		}
		s_Instance.m_UI.OnSceneCreate();
		VCESelectMethod_Box.s_RecentDepth = 1;
		VCESelectMethod_Box.s_RecentFeatherLength = 0;
		VCESelectMethod_Box.s_RecentPlaneFeather = true;
		SelectedColor = Color.white;
	}

	private static void CloseScene()
	{
		if (s_Scene != null)
		{
			Debug.Log("VCE close scene. sceneid = " + s_SceneId);
			if (VCEditor.OnSceneClose != null)
			{
				VCEditor.OnSceneClose(s_Scene);
			}
		}
		DeselectAllTools();
		DestroyBrushes();
		ClearSelections();
		DestroySceneData();
		VCEAssetMgr.ClearTempMaterials();
		VCEAssetMgr.ClearTempDecals();
		s_Instance.m_UI.OnSceneClose();
		VCERefPlane.Reset();
		VCEHistory.Clear();
		VCEHistory.s_Modified = false;
		s_Mirror = null;
	}

	private static void DestroySceneData()
	{
		if (s_Scene != null)
		{
			s_Scene.Destroy();
			s_Scene = null;
		}
	}

	public static bool DocumentOpen()
	{
		if (s_Scene == null)
		{
			return false;
		}
		if (s_Scene.m_IsoData == null)
		{
			return false;
		}
		if (s_Scene.m_IsoData.m_Components == null)
		{
			return false;
		}
		if (s_Scene.m_IsoData.m_Voxels == null)
		{
			return false;
		}
		if (s_Scene.m_IsoData.m_Colors == null)
		{
			return false;
		}
		return true;
	}

	public static int MakeCreation()
	{
		if (!s_ConnectedToGame)
		{
			Debug.LogWarning("You can not make creation outside the game!");
			return -2;
		}
		if (s_MultiplayerMode)
		{
			if (!VCConfig.s_Categories.ContainsKey(s_Scene.m_IsoData.m_HeadInfo.Category))
			{
				return -1;
			}
			byte[] array = s_Scene.m_IsoData.Export();
			if (array == null || array.Length <= 0)
			{
				return -1;
			}
			ulong hashCode = CRC64.Compute(array);
			ulong fileHandle = SteamWorkShop.GetFileHandle(hashCode);
			VCGameMediator.SendIsoDataToServer(s_Scene.m_IsoData.m_HeadInfo.Name, s_Scene.m_IsoData.m_HeadInfo.SteamDesc, s_Scene.m_IsoData.m_HeadInfo.SteamPreview, array, SteamWorkShop.AddNewVersionTag(s_Scene.m_IsoData.m_HeadInfo.ScenePaths()), sendToServer: true, fileHandle);
			return 0;
		}
		CreationData creationData = new CreationData();
		creationData.m_ObjectID = CreationMgr.QueryNewId();
		creationData.m_RandomSeed = UnityEngine.Random.value;
		creationData.m_Resource = s_Scene.m_IsoData.Export();
		creationData.ReadRes();
		creationData.GenCreationAttr();
		if (creationData.m_Attribute.m_Type == ECreation.Null)
		{
			Debug.LogWarning("Creation is not a valid type !");
			creationData.Destroy();
			return -1;
		}
		if (creationData.SaveRes())
		{
			creationData.BuildPrefab();
			creationData.Register();
			CreationMgr.AddCreation(creationData);
			ItemObject item;
			int num = creationData.SendToPlayer(out item);
			Debug.Log("Make creation succeed !");
			switch (num)
			{
			case 0:
				return -1;
			case -1:
				return -4;
			default:
				if (VCEditor.OnMakeCreation != null)
				{
					VCEditor.OnMakeCreation();
				}
				return 0;
			}
		}
		Debug.LogWarning("Save creation resource file failed !");
		creationData.Destroy();
		return -3;
	}

	public static int MakeCreation(string path)
	{
		TextAsset textAsset = Resources.Load<TextAsset>(path);
		VCIsoData vCIsoData = new VCIsoData();
		vCIsoData.Import(textAsset.bytes, new VCIsoOption(editor: false));
		if (s_MultiplayerMode)
		{
			if (!VCConfig.s_Categories.ContainsKey(vCIsoData.m_HeadInfo.Category))
			{
				return -1;
			}
			byte[] array = vCIsoData.Export();
			if (array == null || array.Length <= 0)
			{
				return -1;
			}
			ulong hashCode = CRC64.Compute(array);
			ulong fileHandle = SteamWorkShop.GetFileHandle(hashCode);
			VCGameMediator.SendIsoDataToServer(vCIsoData.m_HeadInfo.Name, vCIsoData.m_HeadInfo.SteamDesc, vCIsoData.m_HeadInfo.SteamPreview, array, SteamWorkShop.AddNewVersionTag(vCIsoData.m_HeadInfo.ScenePaths()), sendToServer: true, fileHandle, free: true);
			return 0;
		}
		CreationData creationData = new CreationData();
		creationData.m_ObjectID = CreationMgr.QueryNewId();
		creationData.m_RandomSeed = UnityEngine.Random.value;
		creationData.m_Resource = vCIsoData.Export();
		creationData.ReadRes();
		creationData.GenCreationAttr();
		if (creationData.m_Attribute.m_Type == ECreation.Null)
		{
			Debug.LogWarning("Creation is not a valid type !");
			creationData.Destroy();
			return -1;
		}
		if (creationData.SaveRes())
		{
			creationData.BuildPrefab();
			creationData.Register();
			CreationMgr.AddCreation(creationData);
			ItemObject item;
			int num = creationData.SendToPlayer(out item);
			Debug.Log("Make creation succeed !");
			switch (num)
			{
			case 0:
				return -1;
			case -1:
				return -4;
			default:
				if (VCEditor.OnMakeCreation != null)
				{
					VCEditor.OnMakeCreation();
				}
				return 0;
			}
		}
		Debug.LogWarning("Save creation resource file failed !");
		creationData.Destroy();
		return -3;
	}

	public static void CopyCretion(ECreation type)
	{
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return;
		}
		List<int> creationInstanceId = cmpt.package.GetCreationInstanceId(type);
		if (creationInstanceId == null || creationInstanceId.Count == 0)
		{
			return;
		}
		CreationData creation = CreationMgr.GetCreation(creationInstanceId[0]);
		if (PeGameMgr.IsMulti)
		{
			ulong hashCode = CRC64.Compute(creation.m_Resource);
			ulong fileHandle = SteamWorkShop.GetFileHandle(hashCode);
			VCGameMediator.SendIsoDataToServer(creation.m_IsoData.m_HeadInfo.Name, creation.m_IsoData.m_HeadInfo.SteamDesc, creation.m_IsoData.m_HeadInfo.SteamPreview, creation.m_Resource, SteamWorkShop.AddNewVersionTag(creation.m_IsoData.m_HeadInfo.ScenePaths()), sendToServer: true, fileHandle, free: true);
			return;
		}
		CreationData creationData = new CreationData();
		creationData.m_ObjectID = CreationMgr.QueryNewId();
		creationData.m_RandomSeed = UnityEngine.Random.value;
		creationData.m_Resource = creation.m_Resource;
		creationData.ReadRes();
		creationData.GenCreationAttr();
		if (creationData.m_Attribute.m_Type == ECreation.Null)
		{
			Debug.LogWarning("Creation is not a valid type !");
			creationData.Destroy();
		}
		else if (creationData.SaveRes())
		{
			creationData.BuildPrefab();
			creationData.Register();
			CreationMgr.AddCreation(creationData);
			ItemObject item;
			int num = creationData.SendToPlayer(out item);
			Debug.Log("Make creation succeed !");
			switch (num)
			{
			}
		}
		else
		{
			Debug.LogWarning("Save creation resource file failed !");
			creationData.Destroy();
		}
	}

	public static void CopyCretion(int instanceID)
	{
		CreationData creationData = new CreationData();
		creationData.m_ObjectID = CreationMgr.QueryNewId();
		creationData.m_RandomSeed = UnityEngine.Random.value;
		CreationData creation = CreationMgr.GetCreation(instanceID);
		if (creation == null)
		{
			return;
		}
		creationData.m_Resource = creation.m_Resource;
		creationData.ReadRes();
		creationData.GenCreationAttr();
		if (creationData.m_Attribute.m_Type == ECreation.Null)
		{
			Debug.LogWarning("Creation is not a valid type !");
			creationData.Destroy();
		}
		else if (creationData.SaveRes())
		{
			creationData.BuildPrefab();
			creationData.Register();
			CreationMgr.AddCreation(creationData);
			ItemObject item;
			int num = creationData.SendToPlayer(out item);
			Debug.Log("Make creation succeed !");
			switch (num)
			{
			}
		}
		else
		{
			Debug.LogWarning("Save creation resource file failed !");
			creationData.Destroy();
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		s_Instance = this;
		Init();
	}

	private void Start()
	{
	}

	private void Update()
	{
		VCGameMediator.Update();
		if (s_Active)
		{
			RunOpenCloseTime();
			RenderSettings.fog = false;
			RenderSettings.ambientLight = Color.white * 0.35f;
			if (DocumentOpen())
			{
				UserInput();
				UpdateBrushLogic();
				UpdateMirror();
				s_Scene.PreventRenderTextureLost();
				s_Scene.m_MeshComputer.ReqMesh();
			}
		}
	}

	private void UpdateBrushLogic()
	{
		if (SelectedGeneralBrush)
		{
			ClearSelections();
		}
	}

	private void UpdateMirror()
	{
		if (Instance.m_UI.m_PartTab.isChecked)
		{
			if (SelectedPart != null)
			{
				s_Mirror.m_Mask = (byte)SelectedPart.m_MirrorMask;
				return;
			}
			VCESelectComponent vCESelectComponent = null;
			VCEBrush[] selectedBrushes = SelectedBrushes;
			foreach (VCEBrush vCEBrush in selectedBrushes)
			{
				if (vCEBrush is VCESelectComponent)
				{
					vCESelectComponent = vCEBrush as VCESelectComponent;
					break;
				}
			}
			if (vCESelectComponent != null)
			{
				if (vCESelectComponent.m_Selection.Count == 0)
				{
					s_Mirror.m_Mask = 7;
					return;
				}
				int num = 7;
				foreach (VCESelectComponent.SelectInfo item in vCESelectComponent.m_Selection)
				{
					if (item.m_Component.m_Data is VCPartData vCPartData)
					{
						num &= VCConfig.s_Parts[vCPartData.m_ComponentId].m_MirrorMask;
					}
				}
				s_Mirror.m_Mask = (byte)num;
			}
			else
			{
				s_Mirror.m_Mask = 7;
			}
		}
		else if (Instance.m_UI.m_MaterialTab.isChecked)
		{
			s_Mirror.m_Mask = 7;
		}
		else if (Instance.m_UI.m_PaintTab.isChecked)
		{
			s_Mirror.m_Mask = 7;
		}
		else if (Instance.m_UI.m_DecalTab.isChecked)
		{
			s_Mirror.m_Mask = 7;
		}
		else
		{
			s_Mirror.m_Mask = 0;
		}
	}

	private void UserInput()
	{
		if (s_Ready)
		{
			KeyEscape();
			KeyFocus();
			KeyResetCam();
			UIHotKeys();
		}
	}

	private void KeyEscape()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !SelectedGeneralBrush && !s_ProtectLock0)
		{
			m_UI.OnQuitClick();
		}
	}

	private void KeyFocus()
	{
		if ((Input.GetKeyDown(KeyCode.F) || VCEInput.s_RightDblClick) && !UICamera.inputHasFocus && !Input.GetMouseButton(0) && !s_ProtectLock0 && VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out var target, 128))
		{
			Vector3 target2 = (target.snapto.ToVector3() + target.cursor.ToVector3() + Vector3.one) * 0.5f * s_Scene.m_Setting.m_VoxelSize;
			VCECamera component = m_MainCamera.GetComponent<VCECamera>();
			float num = component.Distance;
			float num2 = s_Scene.m_Setting.m_VoxelSize * 10f;
			if (num > num2)
			{
				num = num2;
			}
			component.SetTarget(target2);
			component.SetDistance(num);
		}
	}

	private void KeyResetCam()
	{
		if (Input.GetKeyDown(KeyCode.R) && !UICamera.inputHasFocus && !Input.GetMouseButton(0) && !s_ProtectLock0)
		{
			VCECamera component = m_MainCamera.GetComponent<VCECamera>();
			component.SmoothReset();
		}
	}

	private void UIHotKeys()
	{
		if (!Input.GetMouseButton(0) && !s_ProtectLock0)
		{
			if (VCEInput.s_Undo && m_UI.m_UndoButton.enabled)
			{
				m_UI.OnUndoClick();
			}
			if (VCEInput.s_Redo && m_UI.m_RedoButton.enabled)
			{
				m_UI.OnRedoClick();
			}
			if (VCEInput.s_Delete && m_UI.m_DeleteButton.enabled)
			{
				m_UI.OnDeleteClick();
			}
			if (Input.GetKeyDown(KeyCode.F1))
			{
				m_UI.OnTutorialClick();
			}
		}
	}

	private void OnDestroy()
	{
		Destroy();
		s_Instance = null;
	}

	private void RunOpenCloseTime()
	{
		if (s_OpenTime >= 0f)
		{
			s_OpenTime += Time.deltaTime;
			if (s_OpenTime > 1.6f)
			{
				s_Ready = true;
				s_OpenTime = -1f;
				if (VCEditor.OnReady != null)
				{
					VCEditor.OnReady();
				}
			}
		}
		if (s_QuitTime >= 0f)
		{
			s_QuitTime += Time.deltaTime;
			if (s_QuitTime > 1f)
			{
				QuitFinally();
				s_QuitTime = -1f;
			}
		}
	}

	private static void GenerateAllMaterialIcons()
	{
		foreach (KeyValuePair<ulong, VCMaterial> s_Material in VCEAssetMgr.s_Materials)
		{
			VCMatGenerator.Instance.GenMaterialIcon(s_Material.Value);
		}
	}

	private static void FreeAllMaterialIcons()
	{
		foreach (KeyValuePair<ulong, VCMaterial> s_Material in VCEAssetMgr.s_Materials)
		{
			s_Material.Value.FreeIcon();
		}
	}
}
