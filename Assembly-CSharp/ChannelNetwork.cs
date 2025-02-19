using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class ChannelNetwork : NetworkInterface
{
	public const int RoomWorldId = 100;

	public const int PlayerWorldId = 101;

	public const int MinWorldId = 200;

	private static List<ChannelNetwork> Channels = new List<ChannelNetwork>();

	private int mChannelId;

	public int ChannelId => mChannelId;

	public static ChannelNetwork CurChannel => Channels.Find((ChannelNetwork iter) => iter.ChannelId == ((!(null != PlayerNetwork.mainPlayer)) ? 101 : PlayerNetwork.mainPlayer.WorldId));

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		mChannelId = info.networkView.initialData.Read<int>(new object[0]);
		Channels.Add(this);
	}

	protected override void OnPEStart()
	{
		BindAction(EPacketType.PT_Common_BlockData, ChunkManager.RPC_S2C_BlockData);
		BindAction(EPacketType.PT_Common_VoxelData, ChunkManager.RPC_S2C_VoxelData);
		BindAction(EPacketType.PT_InGame_SkillBlockRange, ChunkManager.RPC_S2C_BlockDestroyInRange);
		BindAction(EPacketType.PT_InGame_SkillVoxelRange, ChunkManager.RPC_S2C_TerrainDestroyInRange);
		BindAction(EPacketType.PT_InGame_AttackArea, RPC_S2C_FlagAttacked);
		BindAction(EPacketType.PT_Common_NativeTowerDestroyed, VArtifactTownManager.RPC_S2C_NativeTowerDestroyed);
		BindAction(EPacketType.PT_InGame_ItemObjectList, RPC_S2C_ItemList);
		BindAction(EPacketType.PT_InGame_ItemObject, RPC_S2C_Item);
		BindAction(EPacketType.PT_InGame_BlockRedo, ChunkManager.RPC_S2C_BuildBlock);
		BindAction(EPacketType.PT_InGame_BlockUndo, ChunkManager.RPC_S2C_BuildBlock);
		BindAction(EPacketType.PT_InGame_SKDigTerrain, ChunkManager.RPC_SKDigTerrain);
		BindAction(EPacketType.PT_InGame_SKChangeTerrain, ChunkManager.RPC_SKChangeTerrain);
		BindAction(EPacketType.PT_InGame_BattleInfo, BattleManager.RPC_S2C_BattleInfo);
		BindAction(EPacketType.PT_InGame_BattleInfos, BattleManager.RPC_S2C_BattleInfos);
		BindAction(EPacketType.PT_InGame_BattleOver, BattleManager.RPC_S2C_BattleOver);
		BindAction(EPacketType.PT_InGame_Plant_UpdateInfo, RPC_S2C_Plant_UpdateInfo);
		BindAction(EPacketType.PT_InGame_Plant_UpdateInfoList, RPC_S2C_Plant_UpdateInfoList);
		BindAction(EPacketType.PT_InGame_TowerDefense, RPC_S2C_TowerDefenseComplete);
		BindAction(EPacketType.PT_InGame_WeaponDurability, RPC_S2C_WeaponDurability);
		BindAction(EPacketType.PT_CustomEvent_Death, RPC_S2C_Death);
		BindAction(EPacketType.PT_CustomEvent_Damage, RPC_S2C_Damage);
		BindAction(EPacketType.PT_CustomEvent_UseItem, RPC_S2C_UseItem);
		BindAction(EPacketType.PT_CustomEvent_PutoutItem, RPC_S2C_PutOutItem);
		BindAction(EPacketType.PT_InGame_RemoveDunEntrance, RPC_S2C_RemoveDunEntrance);
		BindAction(EPacketType.PT_InGame_RandomIsoObj, RPC_S2C_RandomIsoObj);
		BindAction(EPacketType.PT_Common_TownDestroyed, RPC_S2C_TownDestroy);
	}

	protected override void OnPEDestroy()
	{
		Channels.Remove(this);
	}

	private void RPC_S2C_ItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<ItemObject[]>(new object[0]);
		PeEntity entity = PeSingleton<MainPlayer>.Instance.entity;
		if (null != entity)
		{
			(entity.packageCmpt as PlayerPackageCmpt).NetOnItemUpdate();
		}
	}

	private void RPC_S2C_Item(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemObj = stream.Read<ItemObject>(new object[0]);
		PeEntity entity = PeSingleton<MainPlayer>.Instance.entity;
		if (null != entity)
		{
			(entity.packageCmpt as PlayerPackageCmpt).NetOnItemUpdate(itemObj);
		}
	}

	private void RPC_S2C_FlagAttacked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		stream.Read<int>(new object[0]);
		string content = PELocalization.GetString(8000855) + " [" + DateTime.Now.ToLongTimeString() + "]";
		new PeTipMsg(content, PeTipMsg.EMsgLevel.Warning);
	}

	private void RPC_S2C_Plant_UpdateInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		FarmPlantLogic farmPlantLogic = stream.Read<FarmPlantLogic>(new object[0]);
		if (farmPlantLogic != null)
		{
			farmPlantLogic.UpdateInMultiMode();
		}
	}

	private void RPC_S2C_Plant_UpdateInfoList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		FarmPlantLogic[] array = stream.Read<FarmPlantLogic[]>(new object[0]);
		FarmPlantLogic[] array2 = array;
		foreach (FarmPlantLogic farmPlantLogic in array2)
		{
			if (farmPlantLogic != null)
			{
				farmPlantLogic.UpdateInMultiMode();
			}
		}
	}

	private void RPC_S2C_TowerDefenseComplete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (UITowerInfo.Instance != null)
		{
			UITowerInfo.Instance.Hide();
		}
	}

	private void RPC_S2C_WeaponDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float current = stream.Read<float>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			Durability cmpt = itemObject.GetCmpt<Durability>();
			if (cmpt != null)
			{
				cmpt.floatValue.current = current;
			}
		}
	}

	private void RPC_S2C_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int scenarioId = stream.Read<int>(new object[0]);
		if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.OnCustomDeath(scenarioId);
		}
	}

	private void RPC_S2C_Damage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int scenarioId = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int casterScenarioId = stream.Read<int>(new object[0]);
		float damage = stream.Read<float>(new object[0]);
		if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.OnCustomDamage(scenarioId, casterScenarioId, damage);
		}
	}

	private void RPC_S2C_UseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int customId = stream.Read<int>(new object[0]);
		int itemInstanceId = stream.Read<int>(new object[0]);
		if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.OnCustomUseItem(customId, itemInstanceId);
		}
	}

	private void RPC_S2C_PutOutItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int customId = stream.Read<int>(new object[0]);
		int itemInstanceId = stream.Read<int>(new object[0]);
		if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.OnCustomPutoutItem(customId, itemInstanceId);
		}
	}

	private void RPC_S2C_RemoveDunEntrance(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] array = stream.Read<Vector3[]>(new object[0]);
		Vector3[] array2 = array;
		foreach (Vector3 entrancePos in array2)
		{
			RandomDungenMgr.Instance.DestroyEntrance(entrancePos);
		}
	}

	private void RPC_S2C_RandomIsoObj(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(vector);
		if (randomItemObj != null)
		{
			randomItemObj.AddRareInstance(id);
			if (Application.isEditor)
			{
				Debug.LogError(string.Concat("<color=yellow>A Rare RandomItem is Ready!", vector, " </color>"));
			}
		}
	}

	private void RPC_S2C_TownDestroy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		VArtifactTownManager.Instance.OnTownDestroyed(num);
		VArtifactTownManager.Instance.SetCaptured(num);
	}
}
