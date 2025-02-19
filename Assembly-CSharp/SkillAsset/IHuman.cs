using System.Collections.Generic;
using ItemAsset;
using UnityEngine;
using WhiteCat;

namespace SkillAsset;

public interface IHuman
{
	List<ItemObject> Equipments { get; }

	bool CheckAmmoCost(EArmType type, int cost);

	void ApplyAmmoCost(EArmType type, int cost);

	EquipType GetEquipType();

	Equipment MainHandEquip();

	bool PutOnEquip(ItemObject item);

	bool TakeOffEquip(ItemObject item, bool directlyRemove = false);

	void ApplyDurabilityReduce(int Type);

	void GetOnCarrier(Transform tran);

	void GetOffCarrier();
}
