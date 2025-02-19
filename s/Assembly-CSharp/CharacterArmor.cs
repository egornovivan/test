using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using UnityEngine;

public class CharacterArmor : MonoBehaviour
{
	public enum ArmorType
	{
		Head = 0,
		Body = 1,
		ArmAndLeg = 2,
		HandAndFoot = 3,
		Decoration = 4,
		None = 255
	}

	private class ArmorPartData
	{
		private int _itemId;

		private ArmorType _type;

		public int boneGroup;

		public int boneIndex;

		public bool mirrored;

		public Vector3 localPosition;

		public Vector3 localEulerAngles;

		public Vector3 localScale;

		public int itemId => _itemId;

		public ArmorType type => _type;

		private ArmorPartData()
		{
		}

		public ArmorPartData(int itemId, ArmorType type)
		{
			_itemId = itemId;
			_type = type;
			boneGroup = (int)((type != ArmorType.Decoration) ? type : ArmorType.Head);
			boneIndex = 0;
			mirrored = false;
			localPosition = Vector3.zero;
			localEulerAngles = Vector3.zero;
			localScale = Vector3.one;
		}

		public void Serialize(BinaryWriter w)
		{
			w.Write(_itemId);
			w.Write((byte)_type);
			w.Write((byte)((mirrored ? 128 : 0) | (boneGroup << 4) | boneIndex));
			w.Write(localPosition.x);
			w.Write(localPosition.y);
			w.Write(localPosition.z);
			w.Write(localEulerAngles.x);
			w.Write(localEulerAngles.y);
			w.Write(localEulerAngles.z);
			w.Write(localScale.x);
			w.Write(localScale.y);
			w.Write(localScale.z);
		}

		public static ArmorPartData Deserialize(BinaryReader r)
		{
			ArmorPartData armorPartData = new ArmorPartData();
			armorPartData._itemId = r.ReadInt32();
			armorPartData._type = (ArmorType)r.ReadByte();
			byte b = r.ReadByte();
			armorPartData.mirrored = (b & 0x80) != 0;
			armorPartData.boneGroup = (b & 0x70) >> 4;
			armorPartData.boneIndex = b & 0xF;
			armorPartData.localPosition.x = r.ReadSingle();
			armorPartData.localPosition.y = r.ReadSingle();
			armorPartData.localPosition.z = r.ReadSingle();
			armorPartData.localEulerAngles.x = r.ReadSingle();
			armorPartData.localEulerAngles.y = r.ReadSingle();
			armorPartData.localEulerAngles.z = r.ReadSingle();
			armorPartData.localScale.x = r.ReadSingle();
			armorPartData.localScale.y = r.ReadSingle();
			armorPartData.localScale.z = r.ReadSingle();
			return armorPartData;
		}
	}

	private class ArmorPartObject
	{
		private CharacterArmor _character;

		private ArmorPartData _data;

		private ItemObject _item;

		private int _boneGroup;

		private int _boneIndex;

		public ItemObject item => _item;

		public ArmorPartData data => _data;

		public ArmorPartObject(CharacterArmor character, ArmorPartData data, ItemObject item)
		{
			_character = character;
			_data = data;
			_item = item;
			_boneGroup = -1;
			_boneIndex = -1;
			if (data.type == ArmorType.Decoration)
			{
				_character._decorationCount++;
			}
			SyncAttachedBone();
		}

		public void SyncAttachedBone()
		{
			if (_boneGroup == _data.boneGroup && _boneIndex == _data.boneIndex)
			{
				return;
			}
			if (_boneGroup >= 0)
			{
				if (_data.type == ArmorType.Decoration)
				{
					_character._boneNodes[_boneGroup][_boneIndex].decoration = null;
				}
				else
				{
					_character._boneNodes[_boneGroup][_boneIndex].normal = null;
				}
			}
			_boneGroup = _data.boneGroup;
			_boneIndex = _data.boneIndex;
			if (_data.type == ArmorType.Decoration)
			{
				_character._boneNodes[_boneGroup][_boneIndex].decoration = this;
			}
			else
			{
				_character._boneNodes[_boneGroup][_boneIndex].normal = this;
			}
		}

		public void RemoveArmorPart()
		{
			if (_data.type == ArmorType.Decoration)
			{
				_character._decorationCount--;
				_character._boneNodes[_boneGroup][_boneIndex].decoration = null;
			}
			else
			{
				_character._boneNodes[_boneGroup][_boneIndex].normal = null;
			}
		}
	}

	private class BoneNode
	{
		public ArmorPartObject normal;

		public ArmorPartObject decoration;
	}

	private const int _maxDecorationCount = 4;

	private List<List<ArmorPartData>> _data;

	private int _currentSuitIndex;

	private List<ArmorPartObject> _armorObjects;

	private ItemPackage _package;

	private BoneNode[][] _boneNodes;

	private int _decorationCount;

	private List<ItemObject> _armorItems = new List<ItemObject>(20);

	private int[] _armorIndexes = new int[20];

	public int suitsItemsCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < _data.Count; i++)
			{
				num += _data[i].Count;
			}
			return num;
		}
	}

	public void ForEachArmorItem(Action<ItemObject> action)
	{
		for (int i = 0; i < _armorObjects.Count; i++)
		{
		}
	}

	public void ForEachSuitsItemIDs(Action<int> action)
	{
		for (int i = 0; i < _data.Count; i++)
		{
			for (int j = 0; j < _data[i].Count; j++)
			{
				action(_data[i][j].itemId);
			}
		}
	}

	public void Serialize(BinaryWriter w)
	{
		w.Write((byte)1);
		w.Write((byte)_currentSuitIndex);
		int count = _data.Count;
		w.Write((byte)count);
		for (int i = 0; i < count; i++)
		{
			int count2 = _data[i].Count;
			w.Write((byte)count2);
			for (int j = 0; j < count2; j++)
			{
				_data[i][j].Serialize(w);
			}
		}
	}

	public void Deserialize(BinaryReader r)
	{
		r.ReadByte();
		_currentSuitIndex = r.ReadByte();
		int num = r.ReadByte();
		_data = new List<List<ArmorPartData>>((num >= 8) ? num : 8);
		for (int i = 0; i < num; i++)
		{
			int num2 = r.ReadByte();
			_data.Add(new List<ArmorPartData>(20));
			for (int j = 0; j < num2; j++)
			{
				_data[i].Add(ArmorPartData.Deserialize(r));
			}
		}
	}

	public void ValidateData()
	{
		if (_data == null)
		{
			_data = new List<List<ArmorPartData>>(5);
			for (int i = 0; i < 5; i++)
			{
				_data.Add(new List<ArmorPartData>(20));
			}
			_currentSuitIndex = 0;
		}
	}

	public void Init(ItemPackage package)
	{
		_package = package;
		_armorObjects = new List<ArmorPartObject>(20);
		_boneNodes = new BoneNode[4][]
		{
			new BoneNode[1],
			new BoneNode[3],
			new BoneNode[8],
			new BoneNode[4]
		};
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < _boneNodes[i].Length; j++)
			{
				_boneNodes[i][j] = new BoneNode();
			}
		}
		List<ArmorPartData> list = _data[_currentSuitIndex];
		for (int k = 0; k < list.Count; k++)
		{
			ItemObject itemByID = ItemManager.GetItemByID(list[k].itemId);
			if (itemByID != null)
			{
				_armorObjects.Add(new ArmorPartObject(this, list[k], itemByID));
			}
			else
			{
				list.RemoveAt(k--);
			}
		}
	}

	private int FindArmorItemsInSlotListAndCurrentSuit(int suitIndex)
	{
		List<ArmorPartData> suit = _data[suitIndex];
		_armorItems.Clear();
		int num = 0;
		for (int i = 0; i < suit.Count; i++)
		{
			int num2 = _package.GetItemIndex(ItemPackage.ESlotType.Armor, suit[i].itemId);
			if (num2 >= 0)
			{
				_armorItems.Add(_package.GetItemByIndex(ItemPackage.ESlotType.Armor, num2));
				num++;
			}
			else
			{
				num2 = _armorObjects.FindIndex((ArmorPartObject part) => part.item.instanceId == suit[i].itemId);
				if (num2 >= 0)
				{
					_armorItems.Add(_armorObjects[num2].item);
					num2 = -(num2 + 1);
					num++;
				}
				else
				{
					_armorItems.Add(null);
				}
			}
			_armorIndexes[i] = num2;
		}
		return num;
	}

	public bool SwitchArmorSuit(int newSuitIndex)
	{
		if (newSuitIndex < 0 || newSuitIndex >= _data.Count)
		{
			return false;
		}
		if (_currentSuitIndex != newSuitIndex)
		{
			if (_package.GetEmptyGridCount(3) + FindArmorItemsInSlotListAndCurrentSuit(newSuitIndex) < _armorObjects.Count)
			{
				_armorItems.Clear();
				return false;
			}
			for (int i = 0; i < _armorItems.Count; i++)
			{
				if (_armorItems[i] != null)
				{
					int num = _armorIndexes[i];
					if (num >= 0)
					{
						_package.RemoveItemByIndex(ItemPackage.ESlotType.Armor, num);
						continue;
					}
					num = -num - 1;
					_armorObjects[num].RemoveArmorPart();
					_armorObjects[num] = null;
				}
			}
			for (int num2 = _armorObjects.Count - 1; num2 >= 0; num2--)
			{
				if (_armorObjects[num2] != null)
				{
					_package.AddItem(_armorObjects[num2].item);
					_armorObjects[num2].RemoveArmorPart();
				}
				_armorObjects.RemoveAt(num2);
			}
			_currentSuitIndex = newSuitIndex;
			List<ArmorPartData> list = _data[newSuitIndex];
			for (int j = 0; j < _armorItems.Count; j++)
			{
				if (_armorItems[j] != null)
				{
					_armorObjects.Add(new ArmorPartObject(this, list[j], _armorItems[j]));
					continue;
				}
				list.RemoveAt(j);
				_armorItems.RemoveAt(j);
				j--;
			}
		}
		return true;
	}

	public bool EquipArmorPartFromPackage(int itemID, int typeValue, int boneGroup, int boneIndex)
	{
		ItemObject itemByID = ItemManager.GetItemByID(itemID);
		if (itemByID == null || typeValue == 255)
		{
			return false;
		}
		if (typeValue == 4)
		{
			if (_decorationCount == 4 && _boneNodes[boneGroup][boneIndex].decoration == null)
			{
				return false;
			}
		}
		else if (typeValue != boneGroup)
		{
			return false;
		}
		if (_package.RemoveItem(itemByID) == -1)
		{
			return false;
		}
		ArmorPartObject oldArmor = null;
		if (typeValue == 4)
		{
			oldArmor = _boneNodes[boneGroup][boneIndex].decoration;
		}
		else
		{
			oldArmor = _boneNodes[boneGroup][boneIndex].normal;
		}
		if (oldArmor != null)
		{
			_package.AddItem(oldArmor.item);
			oldArmor.RemoveArmorPart();
			int num = _armorObjects.FindIndex((ArmorPartObject armor) => armor == oldArmor);
			if (num >= 0)
			{
				_armorObjects.RemoveAt(num);
				_data[_currentSuitIndex].RemoveAt(num);
			}
		}
		ArmorPartData armorPartData = new ArmorPartData(itemByID.instanceId, (ArmorType)typeValue);
		armorPartData.boneGroup = boneGroup;
		armorPartData.boneIndex = boneIndex;
		_data[_currentSuitIndex].Add(armorPartData);
		_armorObjects.Add(new ArmorPartObject(this, armorPartData, itemByID));
		return true;
	}

	private ArmorPartObject GetArmorPartObject(int boneGroup, int boneIndex, bool isDecoration)
	{
		BoneNode boneNode = _boneNodes[boneGroup][boneIndex];
		return (!isDecoration) ? boneNode.normal : boneNode.decoration;
	}

	public bool RemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration)
	{
		if (_package.GetEmptyGridCount(3) <= 0)
		{
			return false;
		}
		ArmorPartObject part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
		if (part == null)
		{
			return false;
		}
		_package.AddItem(part.item);
		part.RemoveArmorPart();
		int num = _armorObjects.FindIndex((ArmorPartObject armor) => armor == part);
		if (num >= 0)
		{
			_armorObjects.RemoveAt(num);
			_data[_currentSuitIndex].RemoveAt(num);
		}
		return true;
	}

	public bool SwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration)
	{
		ArmorPartObject armorPartObject = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
		if (armorPartObject == null)
		{
			return false;
		}
		armorPartObject.data.mirrored = !armorPartObject.data.mirrored;
		float num = Mathf.Abs(armorPartObject.data.localScale.z);
		armorPartObject.data.localScale.z = ((!armorPartObject.data.mirrored) ? num : (0f - num));
		return true;
	}

	public bool SetArmorPartPosition(int boneGroup, int boneIndex, bool isDecoration, Vector3 value)
	{
		ArmorPartObject armorPartObject = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
		if (armorPartObject == null)
		{
			return false;
		}
		armorPartObject.data.localPosition = value;
		return true;
	}

	public bool SetArmorPartRotation(int boneGroup, int boneIndex, bool isDecoration, Quaternion value)
	{
		ArmorPartObject armorPartObject = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
		if (armorPartObject == null)
		{
			return false;
		}
		armorPartObject.data.localEulerAngles = value.eulerAngles;
		return true;
	}

	public bool SetArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 value)
	{
		ArmorPartObject armorPartObject = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
		if (armorPartObject == null)
		{
			return false;
		}
		armorPartObject.data.localScale = value;
		return true;
	}
}
