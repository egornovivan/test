using System;
using System.Collections.Generic;
using Pathea;
using PeMap;
using UnityEngine;

namespace PeUIMap;

public abstract class UIMap : UIStaticWnd
{
	public const float ShowNpcRadiusM = 500f;

	[SerializeField]
	protected GameObject mMaskOpWndParent;

	[SerializeField]
	protected GameObject mMaskOpWnd;

	[SerializeField]
	protected Transform mMaskOpWndCenter;

	[SerializeField]
	protected Transform mMaskOpArray;

	[SerializeField]
	protected GameObject mWarpWnd;

	[SerializeField]
	protected Transform mWarpWndCenter;

	[SerializeField]
	protected Transform mWarpArray;

	[SerializeField]
	protected GameObject mIconSelWnd;

	[SerializeField]
	protected GameObject mMapWnd;

	[SerializeField]
	protected GameObject mMapLabelPrefab;

	[SerializeField]
	protected UISprite mPlayerSpr;

	[SerializeField]
	protected UILabel mPosLabal;

	[SerializeField]
	protected Transform mPosLabelTrans;

	[SerializeField]
	protected UICheckbox mCkNpcMask;

	[SerializeField]
	protected UICheckbox mCkUserMask;

	[SerializeField]
	protected UICheckbox mCkVehicleMask;

	[SerializeField]
	protected UISprite mMaskSpr;

	[SerializeField]
	protected UISprite mMaskSpr_2;

	[SerializeField]
	protected UILabel mMaskDes;

	[SerializeField]
	protected UILabel mItemCost;

	[SerializeField]
	protected UILabel mWarpDes;

	[SerializeField]
	protected UISprite mMoneySprite;

	[SerializeField]
	protected UISprite mMeatSprite;

	[SerializeField]
	protected MaplabelMonsterSiegeEffect m_SiegeEffectPrefab;

	protected float ShowNpcRadiusPx;

	[SerializeField]
	private GameObject mOpbtns;

	protected Vector3 mMousePos = Vector3.zero;

	protected bool mShowDes;

	protected List<UIMapLabel> m_CurrentMapLabelList = new List<UIMapLabel>();

	protected Queue<UIMapLabel> m_MapLabelPool = new Queue<UIMapLabel>();

	protected List<MapIcon> mUserIconList = new List<MapIcon>();

	protected float mScale = 1f;

	protected float mScaleMin = 1f;

	protected Vector2 mMapPos = Vector2.zero;

	protected Vector2 mMapPosMin = Vector3.zero;

	private int maskIcoIndex;

	private UIMapLabel selectMapLabel;

	private Vector2 newUserLabelPos = Vector2.zero;

	private int mMoneyCost;

	private Vector3 travelPos = Vector3.zero;

	private int campId = -1;

	private int iconId = -1;

	public Action onTravel;

	private int mOpMissionID = -1;

	protected float texSize => (!PeGameMgr.IsAdventure && !PeGameMgr.IsBuild) ? 4096 : 3140;

	protected override void InitWindow()
	{
		base.InitWindow();
		ShowNpcRadiusPx = ConvetMToPx(500f);
		if (PeSingleton<LabelMgr>.Instance != null)
		{
			GetAllMapLabel();
			PeSingleton<LabelMgr>.Instance.eventor.Subscribe(AddOrRemoveLable);
			GetUserIcon();
		}
		if (mOpbtns != null)
		{
			mOpbtns.SetActive(PeGameMgr.IsMulti);
		}
	}

	public override void Show()
	{
		base.Show();
		Reflash();
		mShowDes = false;
	}

	protected override void OnHide()
	{
		mMaskOpWndParent.SetActive(value: false);
		mWarpWnd.SetActive(value: false);
		base.OnHide();
	}

	private void GetUserIcon()
	{
		mUserIconList.Clear();
		foreach (MapIcon icon in PeSingleton<MapIcon.Mgr>.Instance.iconList)
		{
			if (icon.iconType == EMapIcon.Custom)
			{
				mUserIconList.Add(icon);
			}
		}
	}

	public virtual void Reflash()
	{
		GetAllMapLabel();
	}

	protected virtual Vector3 GetUIPos(Vector3 worldPos)
	{
		return Vector3.zero;
	}

	protected virtual Vector3 GetWorldPos(Vector2 mousePos)
	{
		return Vector3.zero;
	}

	protected virtual Vector3 GetInitPos()
	{
		return Vector3.zero;
	}

	protected virtual void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			mMousePos = Input.mousePosition;
		}
		if (!(GameUI.Instance.mMainPlayer == null))
		{
			if (PeGameMgr.IsSingleAdventure && PeGameMgr.yirdName == AdventureScene.Dungen.ToString())
			{
				mPlayerSpr.transform.localPosition = GetUIPos(RandomDungenMgrData.revivePos);
			}
			else
			{
				mPlayerSpr.transform.localPosition = GetUIPos(GameUI.Instance.mMainPlayer.position);
			}
			mPlayerSpr.transform.rotation = Quaternion.Euler(0f, 0f, 0f - GameUI.Instance.mMainPlayer.rotation.eulerAngles.y);
			ChangeScale(Input.GetAxis("Mouse ScrollWheel") * 0.5f);
			UpdatePosLabel();
			UpdateMapLabelState();
		}
	}

	protected virtual void UIMapBg_OnClick()
	{
		if (!(Vector3.Distance(mMousePos, Input.mousePosition) > 5f))
		{
			if (Input.GetMouseButtonUp(1))
			{
				OpenMaskWnd(null);
			}
			if (Input.GetMouseButtonUp(0))
			{
				mMaskOpWndParent.SetActive(value: false);
			}
		}
	}

	protected abstract float ConvetMToPx(float m);

	protected void ChangeScale(float delta)
	{
		if (!(Mathf.Abs(delta) < float.Epsilon))
		{
			float num = mScale;
			mScale = Mathf.Clamp(mScale + delta, mScaleMin, 1f);
			mMapWnd.transform.localScale = new Vector3(mScale, mScale, 1f);
			mMapPosMin.x = (texSize * mScale - (float)Screen.width) / 2f;
			mMapPosMin.y = (texSize * mScale - (float)Screen.height) / 2f;
			mMapPos *= mScale / num;
			if (Mathf.Abs(mScale - num) > float.Epsilon)
			{
				mWarpWnd.SetActive(value: false);
				mMaskOpWndParent.SetActive(value: false);
			}
			OnMapDrag(Vector2.zero);
		}
	}

	public void OnMapDrag(Vector2 delta)
	{
		mMapPos.x = Mathf.Clamp(mMapPos.x + delta.x, 0f - mMapPosMin.x, mMapPosMin.x);
		mMapPos.y = Mathf.Clamp(mMapPos.y + delta.y, 0f - mMapPosMin.y, mMapPosMin.y);
		mMapWnd.transform.localPosition = new Vector3(mMapPos.x, mMapPos.y, -10f);
		if (delta.magnitude > float.Epsilon)
		{
			mWarpWnd.SetActive(value: false);
			mMaskOpWndParent.SetActive(value: false);
		}
	}

	private void AddOrRemoveLable(object sender, LabelMgr.Args arg)
	{
		if (arg != null && arg.label != null)
		{
			if (arg.add)
			{
				AddMapLabel(arg.label);
			}
			else
			{
				RemvoeMapLabel(arg.label);
			}
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
		for (int i = 0; i < PeSingleton<LabelMgr>.Instance.mList.Count; i++)
		{
			AddMapLabel(PeSingleton<LabelMgr>.Instance.mList[i]);
		}
	}

	private void AddMapLabel(ILabel label)
	{
		if (label.GetShow() != EShow.All && label.GetShow() != 0)
		{
			return;
		}
		UIMapLabel uIMapLabel;
		if (m_MapLabelPool.Count > 0)
		{
			uIMapLabel = m_MapLabelPool.Dequeue();
			uIMapLabel.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mMapLabelPrefab);
			gameObject.transform.parent = mMapWnd.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			uIMapLabel = gameObject.GetComponent<UIMapLabel>();
		}
		if (uIMapLabel != null)
		{
			uIMapLabel.transform.localScale = Vector3.one;
			uIMapLabel.SetLabel(label);
			uIMapLabel.e_OnMouseOver += UIMapLabel_OnMouseOver;
			uIMapLabel.e_OnClick += UIMapLabel_OnClick;
			RepositionMapLabel(uIMapLabel);
			uIMapLabel.gameObject.name = $"MapLabel_{uIMapLabel._ILabel.GetType().ToString()}_{uIMapLabel._ILabel.GetText()}";
			m_CurrentMapLabelList.Add(uIMapLabel);
			UpdateMapLabel(uIMapLabel);
		}
		TryAddMonsterSiegeEffect(uIMapLabel);
		if (label.GetType() == ELabelType.Mission && label.GetIcon() == 13)
		{
			MissionLabel missionLabel = label as MissionLabel;
			uIMapLabel.transform.localScale *= 0.5f;
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
		UIMapLabel uIMapLabel = m_CurrentMapLabelList.Find((UIMapLabel itr) => itr._ILabel == label);
		if (uIMapLabel != null)
		{
			TryRemoveMonsterSiegeEffect(uIMapLabel);
			uIMapLabel.gameObject.SetActive(value: false);
			m_CurrentMapLabelList.Remove(uIMapLabel);
			m_MapLabelPool.Enqueue(uIMapLabel);
		}
	}

	private void RepositionMapLabel(UIMapLabel uiLabel)
	{
		uiLabel.transform.localPosition = GetUIPos(uiLabel.worldPos);
	}

	protected virtual void UIMapLabel_OnMouseOver(UIMapLabel sender, bool isOver)
	{
		if (isOver)
		{
			if (sender.descText == string.Empty)
			{
				mPosLabal.text = (int)sender.worldPos.x + "," + (int)sender.worldPos.z;
			}
			else
			{
				mPosLabal.text = sender.descText + "\n" + (int)sender.worldPos.x + "," + (int)sender.worldPos.z;
			}
		}
		mShowDes = isOver;
	}

	protected virtual void UIMapLabel_OnClick(UIMapLabel sender)
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (sender.type == ELabelType.User)
			{
				OpenMaskWnd(sender);
			}
			else if (sender.fastTrval)
			{
				OpenWarpWnd(sender);
			}
			else
			{
				TryClickNextLable(sender, leftClick: true);
			}
		}
		else
		{
			if (!Input.GetMouseButtonUp(1))
			{
				return;
			}
			if (sender.type == ELabelType.User)
			{
				if (PeGameMgr.IsMulti)
				{
					if (sender._ILabel is UserLabel userLabel && userLabel.playerID == PlayerNetwork.mainPlayerId)
					{
						PlayerNetwork.mainPlayer.RequestRemoveMask(userLabel.index);
					}
				}
				else
				{
					PeSingleton<UserLabel.Mgr>.Instance.Remove(sender._ILabel as UserLabel);
				}
				mShowDes = false;
			}
			else
			{
				TryClickNextLable(sender, leftClick: false);
			}
		}
	}

	private void TryClickNextLable(UIMapLabel sender, bool leftClick)
	{
		Ray ray = default(Ray);
		ray.origin = sender.transform.position;
		ray.direction = ray.origin;
		RaycastHit[] array = Physics.RaycastAll(ray);
		if (array == null || array.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			UIMapLabel component = array[i].collider.GetComponent<UIMapLabel>();
			if (null == component || component == sender)
			{
				continue;
			}
			if (leftClick)
			{
				if (component.type == ELabelType.User)
				{
					OpenMaskWnd(component);
					break;
				}
				if (component.fastTrval)
				{
					OpenWarpWnd(component);
					break;
				}
			}
			else
			{
				if (component.type != ELabelType.User)
				{
					continue;
				}
				if (PeGameMgr.IsMulti)
				{
					if (component._ILabel is UserLabel userLabel && userLabel.playerID == PlayerNetwork.mainPlayerId)
					{
						PlayerNetwork.mainPlayer.RequestRemoveMask(userLabel.index);
					}
				}
				else
				{
					PeSingleton<UserLabel.Mgr>.Instance.Remove(component._ILabel as UserLabel);
				}
				break;
			}
		}
	}

	public void UpdateMapLabelState()
	{
		for (int i = 0; i < m_CurrentMapLabelList.Count; i++)
		{
			UpdateMapLabel(m_CurrentMapLabelList[i]);
		}
	}

	private void UpdateMapLabel(UIMapLabel lb)
	{
		if (lb == null)
		{
			m_CurrentMapLabelList.Remove(lb);
			return;
		}
		if (lb.type == ELabelType.Npc)
		{
			Vector2 a = new Vector2(lb.worldPos.x, lb.worldPos.z);
			if (null != GameUI.Instance && null != GameUI.Instance.mMainPlayer)
			{
				Vector2 b = new Vector2(GameUI.Instance.mMainPlayer.position.x, GameUI.Instance.mMainPlayer.position.z);
				if (PeGameMgr.IsMulti && lb._ILabel.GetIcon() == 11)
				{
					RepositionMapLabel(lb);
					lb.gameObject.SetActive(value: true);
				}
				else if (lb._ILabel.GetIcon() != 43)
				{
					if (Vector2.Distance(a, b) < ShowNpcRadiusPx && mCkNpcMask.isChecked)
					{
						RepositionMapLabel(lb);
						lb.gameObject.SetActive(value: true);
					}
					else
					{
						lb.gameObject.SetActive(value: false);
					}
				}
			}
		}
		else if (lb.type == ELabelType.User)
		{
			lb.gameObject.SetActive(mCkUserMask.isChecked);
		}
		else if (lb.type == ELabelType.Vehicle)
		{
			lb.gameObject.SetActive(mCkVehicleMask.isChecked);
			if (mCkVehicleMask.isChecked)
			{
				RepositionMapLabel(lb);
			}
		}
		if (lb.type != ELabelType.Mission)
		{
			return;
		}
		MissionLabel missionLabel = (MissionLabel)lb._ILabel;
		if (missionLabel.m_type != MissionLabelType.misLb_target && lb.NpcID != -1)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(lb.NpcID);
			if (null != peEntity && null != peEntity.NpcCmpt && peEntity.NpcCmpt.IsFollower)
			{
				if (lb.gameObject.activeSelf)
				{
					lb.gameObject.SetActive(value: false);
				}
			}
			else if (!lb.gameObject.activeSelf)
			{
				lb.gameObject.SetActive(value: true);
			}
		}
		if (missionLabel.NeedOneRefreshPos)
		{
			RepositionMapLabel(lb);
			missionLabel.NeedOneRefreshPos = false;
		}
	}

	protected virtual void OpenMaskWnd(UIMapLabel label)
	{
		if (null != label && label.type != ELabelType.User)
		{
			return;
		}
		mMaskOpWndParent.SetActive(value: true);
		mWarpWnd.SetActive(value: false);
		mIconSelWnd.SetActive(value: false);
		Vector3 zero = Vector3.zero;
		if (label == null)
		{
			if (mUserIconList.Count > 0)
			{
				mMaskSpr.spriteName = mUserIconList[maskIcoIndex].iconName;
				mMaskSpr_2.spriteName = mUserIconList[maskIcoIndex].iconName;
				mMaskSpr_2.enabled = true;
			}
			mMaskDes.text = PELocalization.GetString(8000702);
			newUserLabelPos.x = Input.mousePosition.x;
			newUserLabelPos.y = Input.mousePosition.y;
			mMaskSpr_2.transform.localPosition = new Vector3(Input.mousePosition.x - (float)(Screen.width / 2), Input.mousePosition.y - (float)(Screen.height / 2), -78f);
			zero = mMaskSpr_2.transform.position;
		}
		else
		{
			mMaskSpr.spriteName = label.iconStr;
			mMaskSpr_2.enabled = false;
			mMaskDes.text = label.descText;
			zero = label.transform.position;
		}
		mMaskOpWnd.transform.SetTransInScreenByMousePos();
		RotateArrayByWnd(mMaskOpWndCenter.position, zero, mMaskOpArray, 266f, 54f);
		selectMapLabel = label;
	}

	public void MaskYes()
	{
		if (selectMapLabel == null)
		{
			if (!PeGameMgr.IsMulti)
			{
				UserLabel userLabel = new UserLabel();
				if (maskIcoIndex < 0 || maskIcoIndex >= mUserIconList.Count)
				{
					maskIcoIndex = 0;
				}
				userLabel.icon = mUserIconList[maskIcoIndex].id;
				userLabel.pos = GetWorldPos(newUserLabelPos);
				userLabel.text = mMaskDes.text;
				PeSingleton<UserLabel.Mgr>.Instance.Add(userLabel);
			}
			else
			{
				PlayerNetwork.mainPlayer.RequestMakeMask(byte.MaxValue, GetWorldPos(newUserLabelPos), mUserIconList[maskIcoIndex].id, mMaskDes.text);
			}
		}
		else if (selectMapLabel._ILabel is UserLabel userLabel2)
		{
			if (!PeGameMgr.IsMulti)
			{
				userLabel2.icon = mUserIconList[maskIcoIndex].id;
				userLabel2.text = mMaskDes.text;
				selectMapLabel.UpdateIcon();
			}
			else
			{
				PlayerNetwork.mainPlayer.RequestMakeMask(userLabel2.index, userLabel2.pos, mUserIconList[maskIcoIndex].id, mMaskDes.text);
			}
		}
	}

	public void ChangeMaskIcon(int index)
	{
		if (index >= 0 && index < mUserIconList.Count)
		{
			maskIcoIndex = index;
			mMaskSpr.spriteName = mUserIconList[maskIcoIndex].iconName;
			mMaskSpr_2.spriteName = mUserIconList[maskIcoIndex].iconName;
		}
	}

	private void UpdatePosLabel()
	{
		if (!mMaskOpWndParent.gameObject.activeSelf && !mWarpWnd.gameObject.activeSelf)
		{
			if (!mMaskOpWndParent.gameObject.activeSelf)
			{
				mPosLabelTrans.gameObject.SetActive(value: true);
			}
			Vector3 worldPos = GetWorldPos(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			if (!mShowDes)
			{
				mPosLabal.text = (int)worldPos.x + "," + (int)worldPos.z;
			}
			mPosLabelTrans.localPosition = new Vector3(Input.mousePosition.x - (float)(Screen.width / 2) + 20f, Input.mousePosition.y - (float)(Screen.height / 2) - 10f, -20f);
		}
		else if (mMaskOpWndParent.gameObject.activeSelf)
		{
			mPosLabelTrans.gameObject.SetActive(value: false);
		}
	}

	protected virtual void OpenWarpWnd(UIMapLabel label)
	{
		if (!(label == null))
		{
			mWarpWnd.SetActive(value: true);
			mMaskOpWndParent.SetActive(value: false);
			float magnitude = (label.worldPos - GameUI.Instance.mMainPlayer.position).magnitude;
			mMoneyCost = 2 + (int)(magnitude * 0.02f);
			mItemCost.text = mMoneyCost.ToString();
			mWarpDes.text = label.descText;
			mWarpWnd.transform.SetTransInScreenByMousePos();
			RotateArrayByWnd(mWarpWndCenter.position, label.transform.position, mWarpArray, 264f, 70f);
			travelPos = label.worldPos;
			iconId = -1;
			if (!object.Equals(label._ILabel, null))
			{
				iconId = label._ILabel.GetIcon();
			}
			if (label._ILabel is StaticPoint)
			{
				StaticPoint staticPoint = (StaticPoint)label._ILabel;
				campId = staticPoint.campId;
			}
		}
	}

	public void OnWarpYes()
	{
		if (MissionManager.Instance != null)
		{
			if (MissionManager.Instance.HasTowerDifMission())
			{
				string @string = PELocalization.GetString(8000002);
				MessageBox_N.ShowOkBox(@string);
				return;
			}
			if (GameUI.Instance.playerMoney < mMoneyCost)
			{
				string @string = PELocalization.GetString(8000003);
				MessageBox_N.ShowOkBox(@string);
				return;
			}
			if (PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt.IsOnCarrier())
			{
				string @string = PELocalization.GetString(8000004);
				MessageBox_N.ShowOkBox(@string);
				return;
			}
			int num = -1;
			num = ((!PeGameMgr.IsMulti) ? MissionManager.Instance.HasFollowMission() : MissionManager.Instance.HasFollowMissionNet());
			if (num != -1)
			{
				string @string = PELocalization.GetString(8000005);
				mOpMissionID = num;
				MessageBox_N.ShowYNBox(@string, FailureMission);
				return;
			}
		}
		if (!PeGameMgr.IsMulti)
		{
			if (onTravel != null)
			{
				onTravel();
			}
			FastTravel();
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			GameUI.Instance.mUIWorldMap.Hide();
			int wrapType = ((iconId == 10) ? 1 : 0);
			PlayerNetwork.mainPlayer.RequestFastTravel(wrapType, travelPos, mMoneyCost);
			Hide();
		}
	}

	private void FailureMission()
	{
		if (GameUI.Instance.mMainPlayer != null)
		{
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, mOpMissionID);
			}
			MissionManager.Instance.FailureMission(mOpMissionID);
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RequestFastTravel(0, travelPos, 0);
				Hide();
			}
			else
			{
				FastTravel();
			}
			mOpMissionID = -1;
		}
	}

	private void FastTravel()
	{
		GameUI.Instance.mUIWorldMap.Hide();
		GameUI.Instance.playerMoney -= mMoneyCost;
		PeSingleton<FastTravelMgr>.Instance.TravelTo(travelPos);
	}

	private void RotateArrayByWnd(Vector3 wndCenterPos, Vector3 labPos, Transform array, float width, float height)
	{
		float y;
		float z;
		float x = (y = (z = 0f));
		float num = 4.5f;
		float num2 = 4.5f;
		if (wndCenterPos.x > labPos.x && wndCenterPos.y < labPos.y)
		{
			x = num;
			y = 0f - num2;
			z = 0f;
		}
		else if (wndCenterPos.x < labPos.x && wndCenterPos.y < labPos.y)
		{
			x = width - num;
			y = 0f - num2;
			z = -90f;
		}
		else if (wndCenterPos.x < labPos.x && wndCenterPos.y > labPos.y)
		{
			x = width - num;
			y = 0f - height + num2;
			z = -180f;
		}
		else if (wndCenterPos.x > labPos.x && wndCenterPos.y > labPos.y)
		{
			x = num;
			y = 0f - height + num2;
			z = -270f;
		}
		array.transform.localPosition = new Vector3(x, y, -82f);
		array.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, z));
	}
}
