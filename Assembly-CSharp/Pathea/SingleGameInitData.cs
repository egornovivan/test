using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea.PeEntityExt;
using UnityEngine;

namespace Pathea;

public static class SingleGameInitData
{
	private static void AddItemToPlayer(MaterialItem[] items)
	{
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return;
		}
		foreach (MaterialItem materialItem in items)
		{
			cmpt.package.Add(materialItem.protoId, materialItem.count);
			ItemObject itemByProtoID = cmpt.package._playerPak.GetItemByProtoID(materialItem.protoId);
			if (itemByProtoID != null)
			{
				PeSingleton<PeCreature>.Instance.mainPlayer.UseItem.Use(itemByProtoID);
			}
		}
		cmpt.package._playerPak.Sort(ItemPackage.ESlotType.Item);
	}

	private static void AddMoneyToPlayer(int money)
	{
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (!(null == cmpt))
		{
			cmpt.money.current = money;
		}
	}

	private static void AddStoryPresent()
	{
		MaterialItem[] items = new MaterialItem[1]
		{
			new MaterialItem
			{
				protoId = 1358,
				count = 1
			}
		};
		AddItemToPlayer(items);
	}

	private static void AddAdventurePresent()
	{
		List<MaterialItem> list = new List<MaterialItem>();
		list.Add(new MaterialItem
		{
			protoId = 1289,
			count = 1
		});
		if (RandomMapConfig.openAllScripts)
		{
			list.Add(new MaterialItem
			{
				protoId = 1743,
				count = 1
			});
		}
		AddItemToPlayer(list.ToArray());
		AddMoneyToPlayer(500);
	}

	private static void AddBuildPresent()
	{
		AddItemToPlayer(new MaterialItem[1]
		{
			new MaterialItem
			{
				protoId = 1291,
				count = 1
			}
		});
		AddMoneyToPlayer(500);
	}

	private static void AddDefaultClothToPlayer()
	{
		int[] equipmentItemProtoIds = new int[6] { 113, 149, 210, 95, 131, 192 };
		InitEquipment(PeSingleton<PeCreature>.Instance.mainPlayer, equipmentItemProtoIds);
	}

	private static void InitEquipment(PeEntity entity, IEnumerable<int> equipmentItemProtoIds)
	{
		if (equipmentItemProtoIds == null)
		{
			return;
		}
		EquipmentCmpt cmpt = entity.GetCmpt<EquipmentCmpt>();
		if (null == cmpt)
		{
			Debug.LogError("no equipment cmpt");
			return;
		}
		PeSex require = entity.ExtGetSex();
		foreach (int equipmentItemProtoId in equipmentItemProtoIds)
		{
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(equipmentItemProtoId);
			if (itemProto != null && PeGender.IsMatch(itemProto.equipSex, require))
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(equipmentItemProtoId);
				if (itemObject != null)
				{
					cmpt.PutOnEquipment(itemObject);
				}
			}
		}
	}

	private static Vector3 GetRandomPosNearPlayer()
	{
		PeTrans peTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		return peTrans.position;
	}

	private static Vector3 GetRandomPos(Vector3 pos)
	{
		return pos + Vector3.forward * Random.Range(0f, 18f) + Vector3.left * Random.Range(0f, 18f) + Vector3.up * Random.Range(0f, 3f);
	}

	private static void SetPos(PeEntity entity, Vector3 pos)
	{
		PeTrans peTrans = entity.peTrans;
		if (null == peTrans)
		{
			Debug.LogError("entity has no PeTrans");
		}
		else
		{
			peTrans.position = pos;
		}
	}

	private static void CreateTestRandomNpc()
	{
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
		PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateRandomNpc(1, id, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == peEntity)
		{
			Debug.LogError("create random npc failed");
		}
		else
		{
			SetPos(peEntity, GetRandomPos(GetRandomPosNearPlayer()));
		}
	}

	private static void CreateTestNpc()
	{
		int protoId = 1;
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
		PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateNpc(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == peEntity)
		{
			Debug.LogError("create random npc failed");
		}
		else
		{
			SetPos(peEntity, GetRandomPosNearPlayer());
		}
	}

	public static void AddTestData()
	{
		AddStoryPresent();
	}

	public static void AddStoryInitData()
	{
		AddDefaultClothToPlayer();
		AddStoryPresent();
	}

	public static void AddBuildInitData()
	{
		AddDefaultClothToPlayer();
		AddBuildPresent();
	}

	public static void AddAdventureInitData()
	{
		AddDefaultClothToPlayer();
		AddAdventurePresent();
	}

	public static void AddCustomInitData()
	{
		AddDefaultClothToPlayer();
	}

	public static void AddTutorialInitData()
	{
		AddDefaultClothToPlayer();
	}
}
