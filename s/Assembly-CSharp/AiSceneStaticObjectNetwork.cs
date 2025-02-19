using System.Collections;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class AiSceneStaticObjectNetwork : AiObject
{
	protected Vector3 pos;

	protected Quaternion rot;

	protected double attackTime;

	protected int attackTeamId;

	protected int _ownerId;

	public int OwnerId => _ownerId;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("SceneObjectNetMgr");
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.group;
		attackTeamId = -1;
		attackTime = uLink.Network.time;
		pos = base.transform.position;
		rot = base.transform.rotation;
		Add(this);
		InitializeData();
		AddSkEntity();
		base.PEInstantiateEvent += DropItemManager.OnInitialized;
		base.PEDestroyEvent += DropItemManager.OnDestroy;
		if (!DropItemManager.HasRecord(base.Id))
		{
			SyncSave();
		}
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_SO_InitData, RPC_SO_InitData);
	}

	protected override void InitializeData()
	{
		InitCmpt();
	}

	internal override void OnDamage(int casterId, float damage)
	{
		if (damage != 0f)
		{
			base.OnDamage(casterId, damage);
			if (attackTeamId == -1)
			{
				ChannelNetwork.SyncTeamData(base.WorldId, base.TeamId, EPacketType.PT_InGame_AttackArea, base.Id, casterId);
				attackTime = (float)uLink.Network.time;
				attackTeamId = casterId;
				Invoke("ResetAttackedState", 3f);
			}
		}
	}

	internal override void OnDeath(int casterId = 0)
	{
		ItemManager.RemoveItem(base.Id);
		Delete();
		base.OnDeath(casterId);
	}

	protected override IEnumerator DestroyAiObjectCoroutine()
	{
		yield return new WaitForSeconds(2f);
		NetInterface.NetDestroy(this);
	}

	public override void SkCreater()
	{
		if (!(_skEntity == null) && _skEntity._attribs != null)
		{
			SKAttribute.InitFlagBaseAttrs(_skEntity._attribs, out _skEntity._baseAttribs);
			_skEntity.SetAllAttribute(AttribType.HpMax, 300f);
			_skEntity.SetAllAttribute(AttribType.Hp, 300f);
			_skEntity.SetAllAttribute(AttribType.DefaultPlayerID, base.TeamId);
		}
	}

	private void ResetAttackedState()
	{
		if (attackTime + 30.0 >= uLink.Network.time)
		{
			attackTeamId = -1;
		}
	}

	private void OnTeamReset()
	{
		NetInterface.NetDestroy(this);
	}

	public void Delete()
	{
		NetObjectData netObjectData = new NetObjectData();
		netObjectData.DeleteData(base.Id);
		AsyncSqlite.AddRecord(netObjectData);
	}

	public void SyncSave()
	{
		SceneStaticData sceneStaticData = new SceneStaticData();
		sceneStaticData.ExportData(this);
		AsyncSqlite.AddRecord(sceneStaticData);
	}

	private void RPC_SO_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemByID = ItemManager.GetItemByID(base.Id);
		if (itemByID != null)
		{
			RPCPeer(info.sender, EPacketType.PT_SO_InitData, itemByID, base.transform.position, base.transform.rotation);
		}
	}
}
