using ItemAsset;
using Pathea;
using UnityEngine;

public class UIPowerPlantSolar : UIBaseWnd
{
	public UIGrid mChargingItemTable;

	public GameObject mChargingItemPrefab;

	private CSUI_ChargingGrid[] mChargingItemUi;

	private CSPowerPlantObject mPowerPlantSolor;

	private Vector3 mMachinePos = Vector3.zero;

	public MapObjNetwork _net;

	protected override void InitWindow()
	{
		base.InitWindow();
		mChargingItemUi = new CSUI_ChargingGrid[12];
		for (int i = 0; i < mChargingItemUi.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(mChargingItemPrefab);
			gameObject.transform.parent = mChargingItemTable.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			mChargingItemUi[i] = gameObject.GetComponent<CSUI_ChargingGrid>();
			mChargingItemUi[i].m_bCanChargeLargedItem = false;
			mChargingItemUi[i].onItemChanded = OnItemChanged;
			mChargingItemUi[i].OnDropItemMulti = OnDropItemMulti;
			mChargingItemUi[i].OnRightMouseClickedMulti = OnRightMouseClickedMulti;
			mChargingItemUi[i].m_Index = i;
		}
		mChargingItemTable.repositionNow = true;
	}

	private void OnItemChanged(int index, ItemObject item)
	{
		if (!(mPowerPlantSolor == null))
		{
			mPowerPlantSolor.m_PowerPlant.SetChargingItem(index, item);
		}
	}

	public void OpenWnd(CSPowerPlantObject powerPlantSolor)
	{
		if (null == powerPlantSolor || powerPlantSolor.m_Entity == null)
		{
			Debug.LogError("powerPlantSolor is null.");
			return;
		}
		mPowerPlantSolor = powerPlantSolor;
		PeEntity componentInParent = powerPlantSolor.GetComponentInParent<PeEntity>();
		if (null != componentInParent)
		{
			mMachinePos = componentInParent.transform.position;
		}
		Show();
		if (PeGameMgr.IsMulti)
		{
			_net = MapObjNetwork.GetNet(mPowerPlantSolor.m_Entity.ID);
			if (_net != null)
			{
				_net.RequestItemList();
			}
			else
			{
				Debug.LogError("can't find net id = " + mPowerPlantSolor.m_Entity.ID);
			}
		}
		else
		{
			if (mPowerPlantSolor.m_PowerPlant == null || mPowerPlantSolor.m_PowerPlant.GetChargingItemsCnt() <= 0)
			{
				return;
			}
			for (int i = 0; i < mPowerPlantSolor.m_PowerPlant.GetChargingItemsCnt(); i++)
			{
				ItemObject chargingItem = mPowerPlantSolor.m_PowerPlant.GetChargingItem(i);
				if (i >= mChargingItemUi.Length)
				{
					Debug.LogError("too many charing item to show on solar ui");
					break;
				}
				mChargingItemUi[i].SetItem(chargingItem);
			}
		}
	}

	public void OnMultiOpenDropCallBack(MapObjNetwork net, int[] _idarray)
	{
		if (!(_net != null) || !(_net == net) || mChargingItemUi == null)
		{
			return;
		}
		for (int i = 0; i < _idarray.Length; i++)
		{
			if (_idarray[i] != -1)
			{
				ItemObject item = PeSingleton<ItemMgr>.Instance.Get(_idarray[i]);
				mChargingItemUi[i].SetItem(item);
			}
			else
			{
				mChargingItemUi[i].SetItem(null);
			}
		}
		SelectItem_N.Instance.SetItem(null);
		GameUI.Instance.mItemPackageCtrl.ResetItem();
	}

	public void OnMultiRemoveCallBack(MapObjNetwork net, int _index, int _id)
	{
		if (_net != null && _net == net && mChargingItemUi != null)
		{
			mChargingItemUi[_index].SetItem(null);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}

	private void OnDropItemMulti(int index, Grid_N grid)
	{
		if (SelectItem_N.Instance.Place != ItemPlaceType.IPT_HotKeyBar && SelectItem_N.Instance.ItemObj.instanceId < 100000000)
		{
			_net.InsertItemList(SelectItem_N.Instance.ItemObj.instanceId, index);
		}
	}

	private void OnRightMouseClickedMulti(int index, Grid_N grid)
	{
		_net.GetItem(grid.ItemObj.instanceId);
	}

	protected override void OnClose()
	{
		_net = null;
		mMachinePos = Vector3.zero;
		Hide();
	}

	private void Update()
	{
		if (null != GameUI.Instance.mMainPlayer && mMachinePos != Vector3.zero && Vector3.Distance(GameUI.Instance.mMainPlayer.position, mMachinePos) > 8f)
		{
			OnClose();
		}
	}
}
