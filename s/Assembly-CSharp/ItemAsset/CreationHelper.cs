using System;
using CustomData;

namespace ItemAsset;

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

	public static int ItemClassIdtoProtoId(int itemClassId)
	{
		int result = 0;
		switch (itemClassId)
		{
		case 66:
			result = 1330;
			break;
		case 63:
			result = 1328;
			break;
		case 68:
			result = 1322;
			break;
		case 70:
			result = 1323;
			break;
		case 71:
			result = 1324;
			break;
		case 78:
			result = 1542;
			break;
		}
		return result;
	}

	public static CreationItemClass ECreationToItemClass(ECreation type)
	{
		return type switch
		{
			ECreation.Aircraft => CreationItemClass.Aircraft, 
			ECreation.Vehicle => CreationItemClass.Vehicle, 
			ECreation.Sword => CreationItemClass.Sword, 
			ECreation.Bow => CreationItemClass.Bow, 
			ECreation.Axe => CreationItemClass.Axe, 
			ECreation.Boat => CreationItemClass.Boat, 
			ECreation.HandGun => CreationItemClass.HandGun, 
			ECreation.Rifle => CreationItemClass.Rifle, 
			ECreation.AITurret => CreationItemClass.AITurret, 
			ECreation.Robot => CreationItemClass.Robot, 
			ECreation.Shield => CreationItemClass.Shield, 
			ECreation.SimpleObject => CreationItemClass.SimpleObject, 
			_ => CreationItemClass.None, 
		};
	}
}
