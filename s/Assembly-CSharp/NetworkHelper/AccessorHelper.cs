using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;

namespace NetworkHelper;

public static class AccessorHelper
{
	public static void GenItemData(this ItemProto item, int objID, byte[] buffer)
	{
		using MemoryStream input = new MemoryStream(buffer);
		using BinaryReader binaryReader = new BinaryReader(input);
		item.id = binaryReader.ReadInt32();
		item.itemLabel = binaryReader.ReadByte();
		item.setUp = binaryReader.ReadByte();
		item.equipPos = binaryReader.ReadInt32();
		item.buffId = binaryReader.ReadInt32();
		item.durabilityMax = binaryReader.ReadInt32();
		item.currencyValue = binaryReader.ReadInt32();
		item.currencyValue2 = binaryReader.ReadInt32();
		item.maxStackNum = binaryReader.ReadInt32();
		item.equipSex = (PeSex)binaryReader.ReadInt32();
		item.tabIndex = binaryReader.ReadInt32();
		item.equipType = (EquipType)binaryReader.ReadInt32();
		item.itemClassId = binaryReader.ReadInt32();
		item.sortLabel = binaryReader.ReadInt32();
		item.engergyMax = binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32();
		if (num != 0)
		{
			ItemProto.PropertyList.PropertyValue[] array = new ItemProto.PropertyList.PropertyValue[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new ItemProto.PropertyList.PropertyValue();
				array[i].type = (AttribType)binaryReader.ReadInt32();
				array[i].value = binaryReader.ReadSingle();
			}
			item.propertyList = new ItemProto.PropertyList(array);
		}
		num = binaryReader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			int protoId = binaryReader.ReadInt32();
			int count = binaryReader.ReadInt32();
			MaterialItem materialItem = new MaterialItem();
			materialItem.protoId = protoId;
			materialItem.count = count;
			if (item.repairMaterialList == null)
			{
				item.repairMaterialList = new List<MaterialItem>();
			}
			item.repairMaterialList.Add(materialItem);
		}
		num = binaryReader.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			int protoId2 = binaryReader.ReadInt32();
			int count2 = binaryReader.ReadInt32();
			MaterialItem materialItem2 = new MaterialItem();
			materialItem2.protoId = protoId2;
			materialItem2.count = count2;
			if (item.strengthenMaterialList == null)
			{
				item.strengthenMaterialList = new List<MaterialItem>();
			}
			item.strengthenMaterialList.Add(materialItem2);
		}
		if (ServerConfig.RecordVersion >= 273)
		{
			item.durabilityFactor = binaryReader.ReadSingle();
		}
		else
		{
			item.durabilityFactor = 1f;
		}
		ItemProto.Mgr.Instance.Add(item);
	}
}
