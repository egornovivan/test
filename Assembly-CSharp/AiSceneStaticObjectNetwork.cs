using System.Collections;
using ItemAsset;
using Pathea;
using PeMap;
using uLink;
using UnityEngine;

public class AiSceneStaticObjectNetwork : AiNetwork
{
	protected int _ownerId;

	protected StaticPoint _flagPos;

	public int OwnerId => _ownerId;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		death = false;
		base.gameObject.name = "scenestatic_" + base.Id;
	}

	protected override void OnPEStart()
	{
		BindSkAction();
		BindAction(EPacketType.PT_SO_InitData, RPC_SO_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		RPCServer(EPacketType.PT_SO_InitData);
	}

	protected override void OnPEDestroy()
	{
		StopAllCoroutines();
		DragArticleAgent.Destory(base.Id);
		if (_flagPos != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(_flagPos);
			_flagPos = null;
		}
		int mapIndex = PeSingleton<MaskTile.Mgr>.Instance.GetMapIndex(base.transform.position);
		MaskTile maskTile = PeSingleton<MaskTile.Mgr>.Instance.Get(mapIndex);
		if (maskTile != null)
		{
			maskTile.forceGroup = -1;
			PeSingleton<MaskTile.Mgr>.Instance.Add(mapIndex, maskTile);
		}
		if (!(null == base.Runner))
		{
			Object.Destroy(base.Runner.gameObject);
		}
	}

	private void RPC_SO_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemObject = stream.Read<ItemObject>(new object[0]);
		base.transform.position = stream.Read<Vector3>(new object[0]);
		base.transform.rotation = stream.Read<Quaternion>(new object[0]);
		if (itemObject == null)
		{
			return;
		}
		Drag cmpt = itemObject.GetCmpt<Drag>();
		if (cmpt == null)
		{
			return;
		}
		DragArticleAgent dragArticleAgent = DragArticleAgent.Create(cmpt, base.transform.position, base.transform.localScale, base.transform.rotation, base.Id, this);
		if (dragArticleAgent.itemLogic != null)
		{
			DragItemLogicFlag dragItemLogicFlag = dragArticleAgent.itemLogic as DragItemLogicFlag;
			if (dragItemLogicFlag != null)
			{
				_entity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
				OnSpawned(dragItemLogicFlag.gameObject);
			}
		}
		if (_flagPos == null)
		{
			_flagPos = new StaticPoint();
			_flagPos.icon = 10;
			_flagPos.fastTravel = true;
			_flagPos.text = "Flag_" + base.Id;
			_flagPos.position = base.transform.position;
			PeSingleton<LabelMgr>.Instance.Add(_flagPos);
		}
		StartCoroutine(RefreshFlag());
	}

	private IEnumerator RefreshFlag()
	{
		while (null == PlayerNetwork.mainPlayer)
		{
			yield return null;
		}
		int index = PeSingleton<MaskTile.Mgr>.Instance.GetMapIndex(base.transform.position);
		MaskTile mt = PeSingleton<MaskTile.Mgr>.Instance.Get(index);
		if (mt != null)
		{
			mt.forceGroup = base.TeamId;
		}
		else
		{
			Vector2 tilePos = PeSingleton<MaskTile.Mgr>.Instance.GetCenterPos(index);
			byte type = PeSingleton<MaskTile.Mgr>.Instance.GetType((int)tilePos.x, (int)tilePos.y);
			mt = new MaskTile
			{
				index = index,
				forceGroup = base.TeamId,
				type = type
			};
		}
		PeSingleton<MaskTile.Mgr>.Instance.Add(index, mt);
	}
}
