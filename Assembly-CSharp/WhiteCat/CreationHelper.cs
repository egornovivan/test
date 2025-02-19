using System;
using ItemAsset;

namespace WhiteCat;

public struct CreationHelper
{
	public static CreationItemClass GetCreationItemClass(ItemObject item)
	{
		if (Enum.IsDefined(typeof(CreationItemClass), item.protoData.itemClassId))
		{
			return (CreationItemClass)item.protoData.itemClassId;
		}
		return CreationItemClass.None;
	}

	public static CreationItemClass GetCreationItemClass(int itemClassId)
	{
		if (Enum.IsDefined(typeof(CreationItemClass), itemClassId))
		{
			return (CreationItemClass)itemClassId;
		}
		return CreationItemClass.None;
	}

	public static ArmorType GetArmorType(int itemInstanceID)
	{
		CreationData creation = CreationMgr.GetCreation(itemInstanceID);
		if (creation != null)
		{
			switch (creation.m_Attribute.m_Type)
			{
			case ECreation.ArmorHead:
				return ArmorType.Head;
			case ECreation.ArmorBody:
				return ArmorType.Body;
			case ECreation.ArmorArmAndLeg:
				return ArmorType.ArmAndLeg;
			case ECreation.ArmorHandAndFoot:
				return ArmorType.HandAndFoot;
			case ECreation.ArmorDecoration:
				return ArmorType.Decoration;
			}
		}
		return ArmorType.None;
	}

	public static CreationItemClass ECreationToItemClass(ECreation type)
	{
		switch (type)
		{
		case ECreation.Aircraft:
			return CreationItemClass.Aircraft;
		case ECreation.Vehicle:
			return CreationItemClass.Vehicle;
		case ECreation.Sword:
		case ECreation.SwordLarge:
		case ECreation.SwordDouble:
			return CreationItemClass.Sword;
		case ECreation.Bow:
			return CreationItemClass.Bow;
		case ECreation.Axe:
			return CreationItemClass.Axe;
		case ECreation.Boat:
			return CreationItemClass.Boat;
		case ECreation.HandGun:
			return CreationItemClass.HandGun;
		case ECreation.Rifle:
			return CreationItemClass.Rifle;
		case ECreation.AITurret:
			return CreationItemClass.AITurret;
		case ECreation.Robot:
			return CreationItemClass.Robot;
		case ECreation.Shield:
			return CreationItemClass.Shield;
		case ECreation.SimpleObject:
			return CreationItemClass.SimpleObject;
		default:
			return CreationItemClass.None;
		}
	}
}
