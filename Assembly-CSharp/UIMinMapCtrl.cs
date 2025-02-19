using System;
using System.Collections.Generic;
using Pathea;
using PatheaScript;
using PeMap;
using UnityEngine;
using WhiteCat;

public class UIMinMapCtrl : UIStaticWnd
{
	private enum ReceiveSubmit
	{
		nothing,
		receive,
		submit,
		receiveSubmit
	}

	private static UIMinMapCtrl mInstance;

	public UILabel mTimeLabel;

	public UISprite mWeatherSpr;

	public UIPanel mSubInfoPanel;

	public GameObject mMapLabelPrefab;

	public GameObject mArrowLablePrefab;

	public Camera mMiniMapCam;

	public UISprite mMapPlayerSpr;

	public UITexture mMiniMapTex;

	public Material mMinMapMaterial;

	public UILabel mPlayerPosText;

	public BoxCollider mMapCollider;

	public UISlicedSprite mSpCenterBg;

	public UISlicedSprite mSpLeftBg;

	public UISlicedSprite mSpRightBg;

	public UISlicedSprite mSpLeftBg_1;

	public UISlicedSprite mSpRightBg_1;

	public UITexture mTexCenterBg;

	public UITexture mTexLeftBg;

	public UITexture mTexRightBg;

	public GameObject mWnd;

	public GameObject mRTopBtns;

	public GameObject mRBottomBtns;

	public GameObject mChangeSize;

	public UISprite mTuodong;

	public Vector2 mMinSize;

	public Vector2 mMaxSize;

	[HideInInspector]
	public Vector2 mMapScale;

	public Vector2 mMapSize;

	public float mMapAlpha;

	public float mMapBright;

	[SerializeField]
	protected MaplabelMonsterSiegeEffect m_SiegeEffectPrefab;

	[SerializeField]
	private TweenInterpolator m_MinMapTweenInterpolator;

	[SerializeField]
	private WhiteCat.TweenPosition m_MinMapPosTween;

	[SerializeField]
	private UIButton m_MinMapHideBtn;

	private bool mShowBigMap;

	private Vector3 mMapCenterPos = Vector3.zero;

	private double mMapReFlashTime;

	private bool mIsOnDrapSize;

	private PeTrans mView;

	private List<UIMapLabel> m_CurrentMapLabelList = new List<UIMapLabel>();

	protected Queue<UIMapLabel> m_MapLabelPool = new Queue<UIMapLabel>();

	private List<UIMapArrow> mMapArrowList = new List<UIMapArrow>();

	private bool m_MinMapIsHide;

	private Dictionary<int, List<MissionLabel>> npc_receiveSubmit = new Dictionary<int, List<MissionLabel>>();

	[SerializeField]
	private UIWndTutorialTip_N m_MapTutorialPrefab;

	[SerializeField]
	private Transform m_MapTutorialParent;

	[SerializeField]
	private UIWndTutorialTip_N m_MissionLogTutorialPrefab;

	[SerializeField]
	private Transform m_MissionLogTutorialParent;

	[SerializeField]
	private UIWndTutorialTip_N m_ConversationsTutorialPrefab;

	[SerializeField]
	private Transform m_ConversationsTutorialParent;

	public static UIMinMapCtrl Instance => mInstance;

	public float CameraNear
	{
		get
		{
			return mMiniMapCam.nearClipPlane;
		}
		set
		{
			mMiniMapCam.nearClipPlane = value;
		}
	}

	public float CameraFar
	{
		get
		{
			return mMiniMapCam.farClipPlane;
		}
		set
		{
			mMiniMapCam.farClipPlane = value;
		}
	}

	public float CameraPosY => mMiniMapCam.transform.position.y;

	private void Awake()
	{
		mInstance = this;
		InitWindow();
	}

	private void Start()
	{
		if (UIRecentDataMgr.Instance != null)
		{
			int intValue = UIRecentDataMgr.Instance.GetIntValue("MinMapSize_x", (int)mMapSize.x);
			int intValue2 = UIRecentDataMgr.Instance.GetIntValue("MinMapSize_y", (int)mMapSize.y);
			ReSetMapSize(new Vector2(intValue, intValue2));
			mMapAlpha = UIRecentDataMgr.Instance.GetFloatValue("MinMapAlpha", mMapAlpha);
			mMapBright = UIRecentDataMgr.Instance.GetFloatValue("MinMapBright", mMapBright);
		}
		if (PeSingleton<LabelMgr>.Instance != null)
		{
			GetAllMapLabel();
			PeSingleton<LabelMgr>.Instance.eventor.Subscribe(AddOrRemoveLable);
		}
	}

	private void Update()
	{
		if (mIsOnDrapSize)
		{
			OnDrapSize();
		}
		if (!(mMiniMapCam == null))
		{
			if (!mMiniMapCam.gameObject.activeSelf)
			{
				mMiniMapCam.gameObject.SetActive(value: true);
			}
			UpdateCurAllLabelShow();
			UpdateMiniMap();
			UpdateTime();
			UpdateSubInfo();
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(mMiniMapTex.material);
		UIEventListener uIEventListener = UIEventListener.Get(m_MinMapHideBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(HideMapBtnOnClick));
		m_MinMapTweenInterpolator.onArriveAtBeginning.RemoveListener(OnMinMapTweenFinish);
		m_MinMapTweenInterpolator.onArriveAtEnding.RemoveListener(OnMinMapTweenFinish);
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		UIEventListener uIEventListener = UIEventListener.Get(mMapCollider.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OnOpenWorldMap();
		});
		if (mMiniMapCam == null)
		{
			GameObject gameObject = GameObject.Find("MinmapCamera");
			if (gameObject == null)
			{
				return;
			}
			mMiniMapCam = gameObject.GetComponent<Camera>();
			mMiniMapCam.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
		}
		RenderTexture renderTexture = new RenderTexture(Mathf.CeilToInt(256f * mMiniMapCam.aspect), 256, 1);
		renderTexture.isCubemap = false;
		mMiniMapCam.cullingMask |= 2048;
		mMiniMapCam.targetTexture = renderTexture;
		mMiniMapCam.orthographicSize = 128f;
		mMiniMapCam.gameObject.SetActive(value: false);
		mMiniMapCam.aspect = 1f;
		mMiniMapTex.material = UnityEngine.Object.Instantiate(mMinMapMaterial);
		mMiniMapTex.material.SetTexture("_MainTex", renderTexture);
		mMapScale = Vector2.one;
		UIEventListener uIEventListener2 = UIEventListener.Get(m_MinMapHideBtn.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(HideMapBtnOnClick));
		m_MinMapTweenInterpolator.onArriveAtBeginning.AddListener(OnMinMapTweenFinish);
		m_MinMapTweenInterpolator.onArriveAtEnding.AddListener(OnMinMapTweenFinish);
		m_MinMapIsHide = false;
	}

	private void HideMapBtnOnClick(GameObject go)
	{
		m_MinMapIsHide = !m_MinMapIsHide;
		m_MinMapHideBtn.isEnabled = false;
		m_MinMapTweenInterpolator.speed = (m_MinMapIsHide ? 1 : (-1));
		m_MinMapTweenInterpolator.isPlaying = true;
	}

	private void OnMinMapTweenFinish()
	{
		m_MinMapHideBtn.transform.localRotation = Quaternion.Euler((!m_MinMapIsHide) ? Vector3.zero : new Vector3(0f, 0f, 180f));
		m_MinMapHideBtn.isEnabled = true;
	}

	private void UpdateTweenInfo()
	{
		Vector3 localPosition = m_MinMapHideBtn.transform.localPosition;
		localPosition.y = 0f - mSpRightBg.transform.localScale.y - 15f;
		m_MinMapHideBtn.transform.localPosition = localPosition;
		m_MinMapPosTween.from = Vector3.one;
		float x = GetMinMapWidth() + 5f;
		m_MinMapPosTween.to = new Vector3(x, 0f, 0f);
	}

	private ReceiveSubmit CheckNpcMissionLabel(List<MissionLabel> tmp)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (MissionLabel item in tmp)
		{
			if (item.m_type == MissionLabelType.misLb_unActive)
			{
				flag = true;
			}
			else if (item.m_type == MissionLabelType.misLb_end)
			{
				flag2 = true;
			}
		}
		if (flag && flag2)
		{
			return ReceiveSubmit.receiveSubmit;
		}
		if (flag)
		{
			return ReceiveSubmit.receive;
		}
		if (flag2)
		{
			return ReceiveSubmit.submit;
		}
		return ReceiveSubmit.nothing;
	}

	private void MinMapMissionLabelRealation(MissionLabel tmp, bool add)
	{
		if (add)
		{
			int attachOnID = tmp.m_attachOnID;
			if (tmp.m_type == MissionLabelType.misLb_target)
			{
				AddMapLabel(tmp);
				AddArrowLabel(tmp);
			}
			else if (tmp.m_type == MissionLabelType.misLb_unActive)
			{
				if (!npc_receiveSubmit.ContainsKey(attachOnID))
				{
					npc_receiveSubmit.Add(attachOnID, new List<MissionLabel>());
					npc_receiveSubmit[attachOnID].Add(tmp);
					AddMapLabel(tmp);
					AddArrowLabel(tmp);
					return;
				}
				npc_receiveSubmit[attachOnID].Add(tmp);
				if (CheckNpcMissionLabel(npc_receiveSubmit[attachOnID]) == ReceiveSubmit.receive)
				{
					AddMapLabel(tmp);
					AddArrowLabel(tmp);
				}
			}
			else
			{
				if (tmp.m_type != MissionLabelType.misLb_end)
				{
					return;
				}
				if (!npc_receiveSubmit.ContainsKey(attachOnID))
				{
					npc_receiveSubmit.Add(attachOnID, new List<MissionLabel>());
					npc_receiveSubmit[attachOnID].Add(tmp);
					AddMapLabel(tmp);
					AddArrowLabel(tmp);
					return;
				}
				if (CheckNpcMissionLabel(npc_receiveSubmit[attachOnID]) == ReceiveSubmit.receive)
				{
					foreach (MissionLabel item in npc_receiveSubmit[attachOnID])
					{
						if (item.m_type == MissionLabelType.misLb_unActive)
						{
							RemvoeMapLabel(item);
							RemvoArrowLabel(item);
						}
					}
				}
				npc_receiveSubmit[attachOnID].Add(tmp);
				AddMapLabel(tmp);
				AddArrowLabel(tmp);
			}
			return;
		}
		int attachOnID2 = tmp.m_attachOnID;
		if (tmp.m_type == MissionLabelType.misLb_target)
		{
			RemvoeMapLabel(tmp);
			RemvoArrowLabel(tmp);
		}
		else if (tmp.m_type == MissionLabelType.misLb_unActive)
		{
			if (npc_receiveSubmit.ContainsKey(attachOnID2))
			{
				if (CheckNpcMissionLabel(npc_receiveSubmit[attachOnID2]) == ReceiveSubmit.receive)
				{
					RemvoeMapLabel(tmp);
					RemvoArrowLabel(tmp);
				}
				npc_receiveSubmit[attachOnID2].Remove(tmp);
			}
			else
			{
				RemvoeMapLabel(tmp);
				RemvoArrowLabel(tmp);
			}
		}
		else
		{
			if (tmp.m_type != MissionLabelType.misLb_end)
			{
				return;
			}
			RemvoeMapLabel(tmp);
			RemvoArrowLabel(tmp);
			if (!npc_receiveSubmit.ContainsKey(attachOnID2))
			{
				return;
			}
			npc_receiveSubmit[attachOnID2].Remove(tmp);
			if (CheckNpcMissionLabel(npc_receiveSubmit[attachOnID2]) != ReceiveSubmit.receive)
			{
				return;
			}
			foreach (MissionLabel item2 in npc_receiveSubmit[attachOnID2])
			{
				AddMapLabel(item2);
				AddArrowLabel(item2);
			}
		}
	}

	private void AddOrRemoveLable(object sender, LabelMgr.Args arg)
	{
		if (arg == null || arg.label == null || (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial && arg.label.GetType() == ELabelType.Npc && ((MapCmpt)arg.label).Common.entityProto.proto == EEntityProto.Monster))
		{
			return;
		}
		if (arg.add)
		{
			if (arg.label.GetType() == ELabelType.Mission)
			{
				MissionLabel tmp = arg.label as MissionLabel;
				MinMapMissionLabelRealation(tmp, arg.add);
			}
			else
			{
				AddMapLabel(arg.label);
				AddArrowLabel(arg.label);
			}
		}
		else if (arg.label.GetType() == ELabelType.Mission)
		{
			MissionLabel tmp2 = arg.label as MissionLabel;
			MinMapMissionLabelRealation(tmp2, arg.add);
		}
		else
		{
			RemvoeMapLabel(arg.label);
			RemvoArrowLabel(arg.label);
		}
	}

	private void GetAllMapLabel()
	{
		foreach (UIMapLabel currentMapLabel in m_CurrentMapLabelList)
		{
			if (currentMapLabel != null)
			{
				TryRemoveMonsterSiegeEffect(currentMapLabel);
				currentMapLabel.gameObject.SetActive(value: false);
				m_MapLabelPool.Enqueue(currentMapLabel);
			}
		}
		m_CurrentMapLabelList.Clear();
		PeSingleton<LabelMgr>.Instance.ForEach(delegate(ILabel label)
		{
			AddMapLabel(label);
		});
	}

	private void AddMapLabel(ILabel label)
	{
		if (label.GetShow() != EShow.All && label.GetShow() != EShow.MinMap)
		{
			return;
		}
		UIMapLabel uIMapLabel = null;
		if (m_MapLabelPool.Count > 0)
		{
			uIMapLabel = m_MapLabelPool.Dequeue();
			uIMapLabel.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mMapLabelPrefab);
			gameObject.transform.parent = mSubInfoPanel.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			uIMapLabel = gameObject.GetComponent<UIMapLabel>();
		}
		if (uIMapLabel != null)
		{
			uIMapLabel.transform.localScale = Vector3.one;
			uIMapLabel.SetLabel(label, _inMinMap: true);
			uIMapLabel.gameObject.name = "MinMapLabel: " + uIMapLabel._ILabel.GetText();
			m_CurrentMapLabelList.Add(uIMapLabel);
		}
		TryAddMonsterSiegeEffect(uIMapLabel);
		if (label.GetType() == ELabelType.Mission)
		{
			MissionLabel missionLabel = label as MissionLabel;
			if (missionLabel.m_attachOnID != 0)
			{
				uIMapLabel.SetLabelPosByNPC(missionLabel.m_attachOnID);
			}
		}
	}

	private void TryAddMonsterSiegeEffect(UIMapLabel uiLabel)
	{
		ILabel iLabel = uiLabel._ILabel;
		if (iLabel != null && iLabel is MonsterBeaconMark)
		{
			MonsterBeaconMark monsterBeaconMark = iLabel as MonsterBeaconMark;
			if (monsterBeaconMark.IsMonsterSiege)
			{
				TryRemoveMonsterSiegeEffect(uiLabel);
				GameObject gameObject = UnityEngine.Object.Instantiate(m_SiegeEffectPrefab.gameObject);
				gameObject.transform.parent = uiLabel.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.GetComponent<MaplabelMonsterSiegeEffect>().Run = true;
			}
		}
	}

	private void TryRemoveMonsterSiegeEffect(UIMapLabel uiLabel)
	{
		ILabel iLabel = uiLabel._ILabel;
		if (iLabel == null || !(iLabel is MonsterBeaconMark))
		{
			return;
		}
		MonsterBeaconMark monsterBeaconMark = iLabel as MonsterBeaconMark;
		if (!monsterBeaconMark.IsMonsterSiege)
		{
			return;
		}
		MaplabelMonsterSiegeEffect[] componentsInChildren = uiLabel.GetComponentsInChildren<MaplabelMonsterSiegeEffect>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Run = false;
				UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
	}

	private void RemvoeMapLabel(ILabel label)
	{
		UIMapLabel uIMapLabel = m_CurrentMapLabelList.Find((UIMapLabel itr) => itr._ILabel.CompareTo(label));
		if (uIMapLabel != null)
		{
			TryRemoveMonsterSiegeEffect(uIMapLabel);
			uIMapLabel.gameObject.SetActive(value: false);
			m_CurrentMapLabelList.Remove(uIMapLabel);
			m_MapLabelPool.Enqueue(uIMapLabel);
		}
	}

	private void AddArrowLabel(ILabel label)
	{
		if ((label.GetShow() == EShow.All || label.GetShow() == EShow.MinMap) && label is MissionLabel { m_target: not null })
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mArrowLablePrefab);
			gameObject.transform.parent = mSubInfoPanel.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			UIMapArrow component = gameObject.GetComponent<UIMapArrow>();
			if (component != null)
			{
				component.SetLabel(label, UIMapArrow.EArrowType.Main);
				mMapArrowList.Add(component);
				component.visualWidth = mMapSize.x - 10f;
				component.visualHeight = mMapSize.y - 10f;
			}
		}
	}

	private void RemvoArrowLabel(ILabel label)
	{
		UIMapArrow uIMapArrow = mMapArrowList.Find((UIMapArrow itr) => itr.trackLabel.CompareTo(label));
		if (uIMapArrow != null)
		{
			UnityEngine.Object.Destroy(uIMapArrow.gameObject);
			uIMapArrow.gameObject.transform.parent = null;
			mMapArrowList.Remove(uIMapArrow);
		}
	}

	private void UpdateMapPos()
	{
		mWnd.transform.localPosition = new Vector3(-Convert.ToInt32(mMapSize.x / 2f + 1f), -Convert.ToInt32(mMapSize.y / 2f + 1f), 0f);
		mSpCenterBg.transform.localScale = new Vector3(mMapSize.x - 30f, mMapSize.y, 1f);
		mMiniMapTex.transform.localScale = new Vector3(mMapSize.x, mMapSize.y, 1f);
		mSpLeftBg.transform.localScale = new Vector3(20f, mMapSize.y, 1f);
		mSpRightBg.transform.localScale = new Vector3(33f, mMapSize.y, 1f);
		mSpLeftBg.transform.parent.localPosition = new Vector3(-Convert.ToInt32(mMapSize.x / 2f + 10f), 0f, 0f);
		mSpRightBg.transform.parent.localPosition = new Vector3(Convert.ToInt32(mMapSize.x / 2f - 15f), 0f, 0f);
		mPlayerPosText.transform.localPosition = new Vector3(15f, -Convert.ToInt32(mMapSize.y / 2f - 10f), 0f);
		mPlayerPosText.MakePixelPerfect();
		mTimeLabel.transform.localPosition = new Vector3(Convert.ToInt32(mMapSize.x / 2f + 5f), Convert.ToInt32(mMapSize.y / 2f - 12f), 0f);
		mTimeLabel.MakePixelPerfect();
		mRTopBtns.transform.localPosition = new Vector3(0f, Convert.ToInt32(mMapSize.y / 2f), 0f);
		mRBottomBtns.transform.localPosition = new Vector3(0f, -Convert.ToInt32(mMapSize.y / 2f), 0f);
		mWeatherSpr.transform.localPosition = new Vector3(0f, Convert.ToInt32(mMapSize.y / 2f - 12f), 0f);
		mChangeSize.transform.localPosition = new Vector3(0f, -Convert.ToInt32(mMapSize.y / 2f), 0f);
		mSubInfoPanel.clipRange = new Vector4(-15f, 0f, mMapSize.x - 25f, mMapSize.y);
		mSpLeftBg_1.transform.localPosition = mSpLeftBg.transform.localPosition;
		mSpLeftBg_1.transform.localScale = mSpLeftBg.transform.localScale;
		mTexLeftBg.transform.localPosition = new Vector3(mSpLeftBg.transform.localPosition.x, mSpLeftBg.transform.localPosition.y, -2f);
		mTexLeftBg.transform.localScale = mSpLeftBg.transform.localScale;
		mSpRightBg_1.transform.localPosition = mSpRightBg.transform.localPosition;
		mSpRightBg_1.transform.localScale = mSpRightBg.transform.localScale;
		mTexRightBg.transform.localPosition = new Vector3(mSpRightBg.transform.localPosition.x, mSpRightBg.transform.localPosition.y, -2f);
		mTexRightBg.transform.localScale = mSpRightBg.transform.localScale;
		mTexCenterBg.transform.localPosition = new Vector3(mSpCenterBg.transform.localPosition.x, mSpCenterBg.transform.localPosition.y, -2f);
		mTexCenterBg.transform.localScale = mSpCenterBg.transform.localScale;
		for (int i = 0; i < mMapArrowList.Count; i++)
		{
			mMapArrowList[i].visualWidth = mMapSize.x - 20f;
			mMapArrowList[i].visualHeight = mMapSize.y - 10f;
		}
	}

	private void ReSetMapSize(Vector2 _MapSize)
	{
		if (_MapSize.x < mMinSize.x)
		{
			_MapSize.x = mMinSize.x;
		}
		if (_MapSize.y < mMinSize.y)
		{
			_MapSize.y = mMinSize.y;
		}
		if (_MapSize.x > mMaxSize.x)
		{
			_MapSize.x = mMaxSize.x;
		}
		if (_MapSize.y > mMaxSize.y)
		{
			_MapSize.y = mMaxSize.y;
		}
		mMapSize = new Vector2(Convert.ToInt32(_MapSize.x), Convert.ToInt32(_MapSize.y));
		mMapScale = new Vector2(mMapSize.x / 120f, mMapSize.y / 120f);
		UIRecentDataMgr.Instance.SetIntValue("MinMapSize_x", (int)mMapSize.x);
		UIRecentDataMgr.Instance.SetIntValue("MinMapSize_y", (int)mMapSize.y);
		UpdateMapPos();
		UpdateTweenInfo();
	}

	private void OnDrapSize()
	{
		if (Input.GetMouseButton(0))
		{
			int num = Screen.width - Convert.ToInt32(Input.mousePosition.x) - 16;
			int num2 = Screen.height - Convert.ToInt32(Input.mousePosition.y) + 2;
			ReSetMapSize(new Vector2(num, num2));
		}
	}

	private void OnDrapSizeMouseMoveIn()
	{
		if (!mIsOnDrapSize)
		{
			Cursor.visible = false;
			mTuodong.enabled = true;
			mIsOnDrapSize = true;
		}
	}

	private void OnDrapSizeMouseMoveOut()
	{
		if (mIsOnDrapSize)
		{
			Cursor.visible = true;
			mTuodong.enabled = false;
			mIsOnDrapSize = false;
		}
	}

	private void UpdateMiniMap()
	{
		if (null == mView)
		{
			PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
			if (null != mainPlayer)
			{
				mView = mainPlayer.GetCmpt<PeTrans>();
			}
		}
		if (null != mView && GameUI.Instance.bVoxelComplete && !GameConfig.IsInVCE)
		{
			if (!mMiniMapCam.targetTexture.IsCreated())
			{
				mMiniMapCam.targetTexture.Create();
				ReFlashMap();
			}
			Vector3 a = mView.position + 1000f * Vector3.up;
			a.y = 0f;
			if (Vector3.Distance(a, mMapCenterPos) > 62f || GameTime.Timer.Second - mMapReFlashTime > 600.0)
			{
				mMiniMapCam.enabled = true;
				mMapCenterPos = a;
				mMiniMapCam.transform.position = mView.position + 300f * Vector3.up;
				mMapReFlashTime = GameTime.Timer.Second;
			}
			else if (mMiniMapCam.enabled)
			{
				mMiniMapCam.enabled = false;
			}
			mMiniMapTex.uvRect = new Rect((a.x - mMapCenterPos.x) / 248f / mMiniMapCam.aspect + 0.25f, (a.z - mMapCenterPos.z) / 248f + 0.25f, 0.5f / mMiniMapCam.aspect, 0.5f);
			float value = Convert.ToSingle(0.5 + (double)((a.x - mMapCenterPos.x) / 248f));
			float value2 = Convert.ToSingle(0.5 + (double)((a.z - mMapCenterPos.z) / 248f));
			mMiniMapTex.material.SetFloat("_Center_x", value);
			mMiniMapTex.material.SetFloat("_Center_y", value2);
			mMiniMapTex.material.SetFloat("_Alpha", mMapAlpha);
			mMiniMapTex.material.SetFloat("_Bright", mMapBright);
		}
	}

	private void UpdateTime()
	{
		mTimeLabel.text = GameTime.Timer.GetStrHhMm();
	}

	private void UpdateSubInfo()
	{
		if (mView != null)
		{
			mPlayerPosText.text = mView.GetStrPXZ();
			mMapPlayerSpr.transform.rotation = Quaternion.Euler(0f, 0f, 0f - mView.rotation.eulerAngles.y);
		}
	}

	private void UpdateCurAllLabelShow()
	{
		if (m_CurrentMapLabelList == null || m_CurrentMapLabelList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_CurrentMapLabelList.Count; i++)
		{
			UIMapLabel uIMapLabel = m_CurrentMapLabelList[i];
			if (!(null != uIMapLabel) || uIMapLabel._ILabel == null)
			{
				continue;
			}
			if (CheckInViewYRange(uIMapLabel._ILabel.GetPos().y))
			{
				if (!uIMapLabel.gameObject.activeSelf)
				{
					uIMapLabel.gameObject.SetActive(value: true);
				}
			}
			else if (uIMapLabel.gameObject.activeSelf)
			{
				uIMapLabel.gameObject.SetActive(value: false);
			}
			if (uIMapLabel.type == ELabelType.Mission)
			{
				MissionLabel missionLabel = (MissionLabel)uIMapLabel._ILabel;
				if (missionLabel.m_type != MissionLabelType.misLb_target && uIMapLabel.NpcID != -1)
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(uIMapLabel.NpcID);
					if (null != peEntity && null != peEntity.NpcCmpt && peEntity.NpcCmpt.IsFollower)
					{
						if (uIMapLabel.gameObject.activeSelf)
						{
							uIMapLabel.gameObject.SetActive(value: false);
						}
					}
					else if (!uIMapLabel.gameObject.activeSelf)
					{
						uIMapLabel.gameObject.SetActive(value: true);
					}
				}
			}
			if (PeGameMgr.IsMulti && uIMapLabel._ILabel.GetIcon() == 11)
			{
				MapCmpt mapCmpt = uIMapLabel._ILabel as MapCmpt;
				if ((bool)mapCmpt && (bool)mapCmpt.Entity && (bool)mapCmpt.Entity.peTrans)
				{
					uIMapLabel.transform.rotation = Quaternion.Euler(0f, 0f, 0f - mapCmpt.Entity.peTrans.rotation.eulerAngles.y);
				}
			}
		}
	}

	private void BtnAlphaOnClick()
	{
		mMapAlpha += 0.15f;
		if ((double)mMapAlpha > 0.9)
		{
			mMapAlpha = 0.2f;
		}
		UIRecentDataMgr.Instance.SetFloatValue("MinMapAlpha", mMapAlpha);
	}

	private void BtnBrightOnClick()
	{
		mMapBright += 0.5f;
		if (mMapBright > 3f)
		{
			mMapBright = 1f;
		}
		UIRecentDataMgr.Instance.SetFloatValue("MinMapBright", mMapBright);
	}

	private void BtnMissionOnClick()
	{
		if (PeGameMgr.IsCustom)
		{
			if (GameUI.Instance.mCustomMissionTrack.isShow)
			{
				GameUI.Instance.mCustomMissionTrack.Hide();
				return;
			}
			GameUI.Instance.mCustomMissionTrack.Show();
			GameUI.Instance.mCustomMissionTrack.Show();
		}
		else if (GameUI.Instance.mMissionTrackWnd.isShow)
		{
			GameUI.Instance.mMissionTrackWnd.Hide();
		}
		else
		{
			GameUI.Instance.mMissionTrackWnd.Show();
		}
	}

	private void OnOpenWorldMap()
	{
		if (Input.GetMouseButtonUp(0) && GameUI.Instance != null && GameUI.Instance.mUIWorldMap != null && PeGameMgr.playerType != PeGameMgr.EPlayerType.Tutorial)
		{
			GameUI.Instance.mUIWorldMap.Show();
		}
	}

	private void OnNpcTalkHistoryClick()
	{
		if (null != GameUI.Instance)
		{
			GameUI.Instance.mNpcTalkHistoryWnd.ChangeWindowShowState();
		}
	}

	public void ReFlashMap()
	{
		mMapCenterPos = -100000f * Vector3.one;
	}

	public void UpdateCameraPos()
	{
		mMiniMapCam.transform.position = mView.position + 300f * Vector3.up;
	}

	public bool CheckInViewYRange(float curY)
	{
		if (null != mMiniMapCam)
		{
			float num = mMiniMapCam.transform.position.y - CameraNear;
			float num2 = mMiniMapCam.transform.position.y - CameraFar;
			return curY <= num && curY >= num2;
		}
		return false;
	}

	public float GetMinMapWidth()
	{
		return mSpLeftBg.transform.localScale.x + mSpCenterBg.transform.localScale.x + mSpRightBg.transform.localScale.x;
	}

	public void ShowMapTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			if (m_MinMapIsHide)
			{
				HideMapBtnOnClick(m_MinMapHideBtn.gameObject);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(m_MapTutorialPrefab.gameObject);
			gameObject.transform.parent = m_MapTutorialParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
		}
	}

	public void ShowMissionTrackTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			if (m_MinMapIsHide)
			{
				HideMapBtnOnClick(m_MinMapHideBtn.gameObject);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(m_MissionLogTutorialPrefab.gameObject);
			gameObject.transform.parent = m_MissionLogTutorialParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.GetComponent<UIWndTutorialTip_N>().DeleteEvent = ShowConversationsBtnTutorial;
		}
	}

	private void ShowConversationsBtnTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			if (m_MinMapIsHide)
			{
				HideMapBtnOnClick(m_MinMapHideBtn.gameObject);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ConversationsTutorialPrefab.gameObject);
			gameObject.transform.parent = m_ConversationsTutorialParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
		}
	}
}
