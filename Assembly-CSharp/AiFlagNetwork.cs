using ItemAsset;
using Pathea;
using PeMap;
using uLink;
using UnityEngine;

public class AiFlagNetwork : SkNetworkInterface
{
	protected int _ownerId;

	protected StaticPoint _flagPos;

	public int OwnerId => _ownerId;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		info.networkView.initialData.Read<int>(new object[0]);
		base.gameObject.name = "netflag_" + base.Id;
		base._pos = base.transform.position;
		PlayerNetwork.OnLimitBoundsAdd(base.Id, new Bounds(base._pos, new Vector3(128f, 128f, 128f)));
	}

	protected override void OnPEStart()
	{
		PlayerNetwork.OnTeamChangedEventHandler += OnResetFlag;
		BindSkAction();
		BindAction(EPacketType.PT_SO_InitData, RPC_SO_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		RPCServer(EPacketType.PT_SO_InitData);
	}

	protected override void OnPEDestroy()
	{
		PlayerNetwork.OnTeamChangedEventHandler -= OnResetFlag;
		PlayerNetwork.OnLimitBoundsDel(base.Id);
		base.OnPEDestroy();
		DragArticleAgent.Destory(base.Id);
		RemoveFlag();
	}

	private void RemoveFlag()
	{
		if (_flagPos != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(_flagPos);
			_flagPos = null;
		}
		if (!PeGameMgr.IsStory && !PeGameMgr.IsCustom)
		{
			int mapIndex = PeSingleton<MaskTile.Mgr>.Instance.GetMapIndex(base.transform.position);
			MaskTile maskTile = PeSingleton<MaskTile.Mgr>.Instance.Get(mapIndex);
			if (maskTile != null)
			{
				maskTile.forceGroup = -1;
				PeSingleton<MaskTile.Mgr>.Instance.Add(mapIndex, maskTile);
			}
		}
	}

	private void AddFlag()
	{
		if (_flagPos == null)
		{
			_flagPos = new StaticPoint();
			_flagPos.ID = base.Id;
			_flagPos.icon = 10;
			_flagPos.fastTravel = true;
			_flagPos.text = "Flag_" + base.Id;
			_flagPos.position = base._pos;
			PeSingleton<LabelMgr>.Instance.Add(_flagPos);
		}
		if (!PeGameMgr.IsStory && !PeGameMgr.IsCustom)
		{
			int mapIndex = PeSingleton<MaskTile.Mgr>.Instance.GetMapIndex(base._pos);
			MaskTile maskTile = PeSingleton<MaskTile.Mgr>.Instance.Get(mapIndex);
			if (maskTile != null)
			{
				maskTile.forceGroup = base.TeamId;
			}
			else
			{
				Vector2 centerPos = PeSingleton<MaskTile.Mgr>.Instance.GetCenterPos(mapIndex);
				byte type = PeSingleton<MaskTile.Mgr>.Instance.GetType((int)centerPos.x, (int)centerPos.y);
				maskTile = new MaskTile();
				maskTile.index = mapIndex;
				maskTile.forceGroup = base.TeamId;
				maskTile.type = type;
			}
			PeSingleton<MaskTile.Mgr>.Instance.Add(mapIndex, maskTile);
		}
	}

	private void OnResetFlag()
	{
		if (!(null == Singleton<ForceSetting>.Instance))
		{
			if (Singleton<ForceSetting>.Instance.Conflict(base.TeamId, PlayerNetwork.mainPlayerId))
			{
				RemoveFlag();
			}
			else
			{
				AddFlag();
			}
		}
	}

	private void RPC_SO_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemObject = stream.Read<ItemObject>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		Quaternion rotation = stream.Read<Quaternion>(new object[0]);
		base.transform.rotation = rotation;
		base.rot = rotation;
		if (itemObject == null)
		{
			return;
		}
		Drag cmpt = itemObject.GetCmpt<Drag>();
		if (cmpt == null)
		{
			return;
		}
		DragArticleAgent dragArticleAgent = DragArticleAgent.Create(cmpt, base._pos, base.transform.localScale, base.rot, base.Id, this);
		if (dragArticleAgent.itemLogic != null)
		{
			DragItemLogicFlag dragItemLogicFlag = dragArticleAgent.itemLogic as DragItemLogicFlag;
			if (dragItemLogicFlag != null)
			{
				OnSpawned(dragItemLogicFlag.gameObject);
				PeEntity component = dragItemLogicFlag.gameObject.GetComponent<PeEntity>();
				if (null != component)
				{
					NetCmpt netCmpt = component.GetCmpt<NetCmpt>();
					if (null == netCmpt)
					{
						netCmpt = component.Add<NetCmpt>();
					}
					netCmpt.network = this;
				}
			}
		}
		OnResetFlag();
	}
}
