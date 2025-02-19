using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using SkillSystem;
using UnityEngine;

namespace WhiteCat;

public class PlayerArmorCmpt : PeCmpt, IPeMsg
{
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
		private PlayerArmorCmpt _character;

		private ArmorPartData _data;

		private ItemObject _item;

		private int _boneGroup;

		private int _boneIndex;

		private Transform _armor;

		private VCPArmorPivot _pivot;

		private Durability _durability;

		private SkBuffInst _defence;

		private static List<int> _defenceIndex;

		private static List<float> _defenceValue;

		private int[] _armorID = new int[1];

		public bool isBroken => _durability.floatValue.current == 0f;

		public ItemObject item => _item;

		public ArmorPartData data => _data;

		public ArmorPartObject(PlayerArmorCmpt character, ArmorPartData data, ItemObject item)
		{
			_character = character;
			_data = data;
			_item = item;
			_armorID[0] = item.instanceId;
			_boneGroup = -1;
			_boneIndex = -1;
			if (data.type == ArmorType.Decoration)
			{
				_character._decorationCount++;
			}
			_durability = _item.GetCmpt<Durability>();
			SyncAttachedBone();
			AddDefence();
			if (_character.hasModelLayer)
			{
				CreateModel();
			}
			_character.TriggerAddOrRemoveEvent();
		}

		static ArmorPartObject()
		{
			_defenceIndex = new List<int>(1);
			_defenceValue = new List<float>(1);
			_defenceIndex.Add(0);
			_defenceValue.Add(0f);
		}

		private void AddToCharacterModel()
		{
			if ((bool)_armor)
			{
				_character._boneCollector.AddEquipment(_armor, ArmorBones.boneNames[_boneGroup][_boneIndex]);
				if (_data.type == ArmorType.Decoration)
				{
					_character._modelCmpt.nodes(_boneGroup, _boneIndex).decoration = _armor;
				}
				else
				{
					_character._modelCmpt.nodes(_boneGroup, _boneIndex).normal = _armor;
				}
			}
		}

		private void RemoveFromCharacterModel()
		{
			if ((bool)_armor)
			{
				_character._boneCollector.RemoveEquipment(_armor);
				if (_data.type == ArmorType.Decoration)
				{
					_character._modelCmpt.nodes(_boneGroup, _boneIndex).decoration = null;
				}
				else
				{
					_character._modelCmpt.nodes(_boneGroup, _boneIndex).normal = null;
				}
			}
		}

		public void CreateModel()
		{
			if (!_armor)
			{
				_armor = item.GetCmpt<Instantiate>().CreateViewGameObj(null).transform;
				_pivot = _armor.GetComponentInChildren<VCPArmorPivot>();
				_pivot.DestroyModels();
				SyncModelPivot();
				AddToCharacterModel();
				SyncModelFirstPersonMode();
			}
		}

		public void DestroyModel()
		{
			if ((bool)_armor)
			{
				RemoveFromCharacterModel();
				UnityEngine.Object.Destroy(_armor.gameObject);
				_armor = null;
				_pivot = null;
			}
		}

		public void OnBeAttacked(float delta, SkEntity caster)
		{
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RequestArmorDurability(_character.Entity.Id, _armorID, delta, caster);
				return;
			}
			_durability.Expend(delta);
			if (_durability.floatValue.current == 0f)
			{
				RemoveDefence();
			}
		}

		private void AddDefence()
		{
			if (_defence == null && _durability.floatValue.current != 0f)
			{
				_defenceValue[0] = VCUtility.GetArmorDefence(_durability.valueMax);
				_defence = SkEntity.MountBuff(_character.Entity.aliveEntity, 30200129, _defenceIndex, _defenceValue);
			}
		}

		public void RemoveDefence()
		{
			if (_defence != null)
			{
				SkEntity.UnmountBuff(_character.Entity.aliveEntity, _defence);
				_defence = null;
			}
		}

		public void SyncAttachedBone()
		{
			if (_boneGroup == _data.boneGroup && _boneIndex == _data.boneIndex)
			{
				return;
			}
			if (_boneGroup >= 0)
			{
				RemoveFromCharacterModel();
				if (_data.type == ArmorType.Decoration)
				{
					_character._boneNodes[_boneGroup][_boneIndex].decoration = null;
				}
				else
				{
					_character._boneNodes[_boneGroup][_boneIndex].normal = null;
				}
			}
			SyncModelPivot();
			_boneGroup = _data.boneGroup;
			_boneIndex = _data.boneIndex;
			AddToCharacterModel();
			if (_data.type == ArmorType.Decoration)
			{
				_character._boneNodes[_boneGroup][_boneIndex].decoration = this;
			}
			else
			{
				_character._boneNodes[_boneGroup][_boneIndex].normal = this;
			}
		}

		public void SyncModelPivot()
		{
			if ((bool)_armor)
			{
				_armor.SetParent(null, worldPositionStays: false);
				_armor.localPosition = Vector3.zero;
				_armor.localRotation = Quaternion.identity;
				_armor.localScale = Vector3.one;
				int index = 0;
				if (_data.type != ArmorType.Decoration)
				{
					index = _data.boneIndex;
				}
				Transform child = _armor.GetChild(0);
				child.SetParent(null, worldPositionStays: true);
				Transform pivot = _pivot.GetPivot(index);
				_armor.position = pivot.position;
				_armor.rotation = pivot.rotation;
				child.SetParent(_armor, worldPositionStays: true);
				SyncModelPosition();
				SyncModelEulerAngles();
				SyncModelScale();
			}
		}

		public void SyncModelPosition()
		{
			if ((bool)_armor)
			{
				_armor.localPosition = _data.localPosition;
			}
		}

		public void SyncModelEulerAngles()
		{
			if ((bool)_armor)
			{
				_armor.localEulerAngles = _data.localEulerAngles;
			}
		}

		public void SyncModelScale()
		{
			if ((bool)_armor)
			{
				_armor.localScale = _data.localScale;
			}
		}

		public void SyncModelFirstPersonMode()
		{
			if ((bool)_armor)
			{
				_armor.GetComponent<CreationController>().visible = !_character._isFirstPersonMode;
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
			DestroyModel();
			RemoveDefence();
			_character.TriggerAddOrRemoveEvent();
		}
	}

	private class BoneNode
	{
		public ArmorPartObject normal;

		public ArmorPartObject decoration;
	}

	private const int _maxDecorationCount = 4;

	private int _requestCount;

	private List<List<ArmorPartData>> _data;

	private int _currentSuitIndex;

	private List<ArmorPartObject> _armorObjects = new List<ArmorPartObject>(20);

	private SlotList _slotList;

	private BoneNode[][] _boneNodes;

	private int _decorationCount;

	private BoneCollector _boneCollector;

	private ArmorBones _modelCmpt;

	private bool _isFirstPersonMode;

	private Action<bool> _switchArmorSuitCallback;

	private Action<bool> _equipArmorPartFromPackageCallback;

	private Action<bool> _removeArmorPartCallback;

	private Action<bool> _switchArmorPartMirrorCallback;

	private List<ItemObject> _armorItems = new List<ItemObject>(20);

	private int[] _armorIndexes = new int[20];

	public int currentSuitIndex => _currentSuitIndex;

	public bool hasModelLayer => _boneCollector;

	public bool hasRequest => _requestCount > 0;

	public event Action onAddOrRemove;

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
		{
			BiologyViewRoot biologyViewRoot = (BiologyViewRoot)args[1];
			_boneCollector = (args[0] as BiologyViewCmpt).monoBoneCollector;
			_modelCmpt = biologyViewRoot.armorBones;
			if (_armorObjects != null)
			{
				for (int j = 0; j < _armorObjects.Count; j++)
				{
					_armorObjects[j].CreateModel();
				}
			}
			break;
		}
		case EMsg.Battle_BeAttacked:
		{
			int num = SelectArmorPartToAttack();
			if (num >= 0)
			{
				_armorObjects[num].OnBeAttacked((float)args[0] * PEVCConfig.instance.armorDamageRatio, (SkEntity)args[1]);
			}
			break;
		}
		case EMsg.View_FirstPerson:
			if ((bool)args[0] == _isFirstPersonMode)
			{
				break;
			}
			_isFirstPersonMode = !_isFirstPersonMode;
			if (_armorObjects != null)
			{
				for (int i = 0; i < _armorObjects.Count; i++)
				{
					_armorObjects[i].SyncModelFirstPersonMode();
				}
			}
			break;
		}
	}

	private void TriggerAddOrRemoveEvent()
	{
		if (this.onAddOrRemove != null)
		{
			this.onAddOrRemove();
		}
	}

	public void C2S_SwitchArmorSuit(int newSuitIndex, Action<bool> callback)
	{
		_requestCount++;
		_switchArmorSuitCallback = callback;
		PlayerNetwork.RequestSwitchArmorSuit(newSuitIndex);
	}

	public void S2C_SwitchArmorSuit(int newSuitIndex, bool success)
	{
		if (success && newSuitIndex != _currentSuitIndex)
		{
			for (int num = _armorObjects.Count - 1; num >= 0; num--)
			{
				_armorObjects[num].RemoveArmorPart();
				_armorObjects.RemoveAt(num);
			}
			_currentSuitIndex = newSuitIndex;
			List<ArmorPartData> list = _data[newSuitIndex];
			for (int i = 0; i < list.Count; i++)
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(list[i].itemId);
				if (itemObject != null)
				{
					_armorObjects.Add(new ArmorPartObject(this, list[i], itemObject));
				}
				else
				{
					list.RemoveAt(i--);
				}
			}
		}
		_requestCount--;
		if (_switchArmorSuitCallback != null)
		{
			_switchArmorSuitCallback(success);
		}
	}

	public void C2S_EquipArmorPartFromPackage(int itemID, int typeValue, int boneGroup, int boneIndex, Action<bool> callback)
	{
		_requestCount++;
		_equipArmorPartFromPackageCallback = callback;
		PlayerNetwork.RequestEquipArmorPart(itemID, typeValue, boneGroup, boneIndex);
	}

	public void S2C_EquipArmorPartFromPackage(int itemID, int typeValue, int boneGroup, int boneIndex, bool success)
	{
		if (success)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemID);
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
				oldArmor.RemoveArmorPart();
				int num = _armorObjects.FindIndex((ArmorPartObject armor) => armor == oldArmor);
				if (num >= 0)
				{
					_armorObjects.RemoveAt(num);
					_data[_currentSuitIndex].RemoveAt(num);
				}
			}
			ArmorPartData armorPartData = new ArmorPartData(itemObject.instanceId, (ArmorType)typeValue);
			armorPartData.boneGroup = boneGroup;
			armorPartData.boneIndex = boneIndex;
			_data[_currentSuitIndex].Add(armorPartData);
			_armorObjects.Add(new ArmorPartObject(this, armorPartData, itemObject));
		}
		_requestCount--;
		if (_equipArmorPartFromPackageCallback != null)
		{
			_equipArmorPartFromPackageCallback(success);
		}
	}

	public void C2S_RemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration, Action<bool> callback)
	{
		_requestCount++;
		_removeArmorPartCallback = callback;
		PlayerNetwork.RequestRemoveArmorPart(boneGroup, boneIndex, isDecoration);
	}

	public void S2C_RemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration, bool success)
	{
		if (success)
		{
			ArmorPartObject part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
			part.RemoveArmorPart();
			int num = _armorObjects.FindIndex((ArmorPartObject armor) => armor == part);
			if (num >= 0)
			{
				_armorObjects.RemoveAt(num);
				_data[_currentSuitIndex].RemoveAt(num);
			}
		}
		_requestCount--;
		if (_removeArmorPartCallback != null)
		{
			_removeArmorPartCallback(success);
		}
	}

	public void C2S_SwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration, Action<bool> callback)
	{
		_requestCount++;
		_switchArmorPartMirrorCallback = callback;
		PlayerNetwork.RequestSwitchArmorPartMirror(boneGroup, boneIndex, isDecoration);
	}

	public void S2C_SwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration, bool success)
	{
		if (success)
		{
			SwitchArmorPartMirror(boneGroup, boneIndex, isDecoration);
		}
		_requestCount--;
		if (_switchArmorPartMirrorCallback != null)
		{
			_switchArmorPartMirrorCallback(success);
		}
	}

	public void C2S_SyncArmorPartPosition(int boneGroup, int boneIndex, bool isDecoration, Vector3 position)
	{
		PlayerNetwork.SyncArmorPartPos(boneGroup, boneIndex, isDecoration, position);
	}

	public void S2C_SyncArmorPartPosition(int boneGroup, int boneIndex, bool isDecoration, Vector3 position)
	{
		SetArmorPartPosition(boneGroup, boneIndex, isDecoration, position);
	}

	public void C2S_SyncArmorPartRotation(int boneGroup, int boneIndex, bool isDecoration, Quaternion rotation)
	{
		PlayerNetwork.SyncArmorPartRot(boneGroup, boneIndex, isDecoration, rotation);
	}

	public void S2C_SyncArmorPartRotation(int boneGroup, int boneIndex, bool isDecoration, Quaternion rotation)
	{
		SetArmorPartRotation(boneGroup, boneIndex, isDecoration, rotation);
	}

	public void C2S_SyncArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 scale)
	{
		PlayerNetwork.SyncArmorPartScale(boneGroup, boneIndex, isDecoration, scale);
	}

	public void S2C_SyncArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 scale)
	{
		SetArmorPartScale(boneGroup, boneIndex, isDecoration, scale);
	}

	public override void Serialize(BinaryWriter w)
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

	public override void Deserialize(BinaryReader r)
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

	public void ForEachBone(Action<int, int, bool, bool> action)
	{
		for (int i = 0; i < _boneNodes.Length; i++)
		{
			BoneNode[] array = _boneNodes[i];
			for (int j = 0; j < array.Length; j++)
			{
				action(i, j, array[j].normal != null, array[j].decoration != null);
			}
		}
	}

	public void RemoveBufferWhenBroken(ItemObject item)
	{
		ArmorPartObject armorPartObject = _armorObjects.Find((ArmorPartObject a) => a.item == item);
		if (armorPartObject != null && armorPartObject.isBroken)
		{
			armorPartObject.RemoveDefence();
		}
	}

	public override void Start()
	{
		base.Start();
		if (!PeGameMgr.IsMulti)
		{
			Init();
		}
	}

	public void Init(PlayerNetwork net = null)
	{
		_slotList = GetComponent<PlayerPackageCmpt>().package.GetSlotList(ItemPackage.ESlotType.Armor);
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
		if (_data == null)
		{
			_data = new List<List<ArmorPartData>>(5);
			for (int k = 0; k < 5; k++)
			{
				_data.Add(new List<ArmorPartData>(20));
			}
			_currentSuitIndex = 0;
		}
		if (_currentSuitIndex >= _data.Count)
		{
			return;
		}
		List<ArmorPartData> list = _data[_currentSuitIndex];
		for (int l = 0; l < list.Count; l++)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(list[l].itemId);
			if (itemObject != null)
			{
				_armorObjects.Add(new ArmorPartObject(this, list[l], itemObject));
			}
			else
			{
				list.RemoveAt(l--);
			}
		}
	}

	public int SelectArmorPartToAttack()
	{
		int count = _armorObjects.Count;
		int num = UnityEngine.Random.Range(0, count);
		for (int i = 0; i < count; i++)
		{
			if (_armorObjects[num].isBroken)
			{
				num = (num + 1) % count;
				continue;
			}
			return num;
		}
		return -1;
	}

	private int FindArmorItemsInSlotListAndCurrentSuit(int suitIndex)
	{
		List<ArmorPartData> suit = _data[suitIndex];
		_armorItems.Clear();
		int num = 0;
		for (int i = 0; i < suit.Count; i++)
		{
			int num2 = _slotList.FindItemIndexById(suit[i].itemId);
			if (num2 >= 0)
			{
				_armorItems.Add(_slotList[num2]);
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
			if (_slotList.vacancyCount + FindArmorItemsInSlotListAndCurrentSuit(newSuitIndex) < _armorObjects.Count)
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
						_slotList[num] = null;
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
					_slotList.Add(_armorObjects[num2].item);
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

	public void QuickEquipArmorPartFromPackage(ItemObject item)
	{
		ArmorType armorType = CreationHelper.GetArmorType(item.instanceId);
		if (armorType == ArmorType.None)
		{
			return;
		}
		int i = 0;
		int num = 0;
		if (armorType != ArmorType.Decoration)
		{
			i = (int)armorType;
			num = CreationMgr.GetCreation(item.instanceId).creationController.armorBoneIndex;
			if (_boneNodes[i][num].normal != null)
			{
				num = Array.FindIndex(_boneNodes[i], (BoneNode node) => node.normal == null);
				if (num < 0)
				{
					num = CreationMgr.GetCreation(item.instanceId).creationController.armorBoneIndex;
				}
			}
		}
		else if (_decorationCount == 4)
		{
			int num2 = UnityEngine.Random.Range(0, 4);
			for (int j = 0; j < _armorObjects.Count; j++)
			{
				if (_armorObjects[j].data.type == ArmorType.Decoration)
				{
					if (num2 == 0)
					{
						num = _armorObjects[j].data.boneIndex;
						i = _armorObjects[j].data.boneGroup;
						break;
					}
					num2--;
				}
			}
		}
		else
		{
			for (i = 0; i < 4; i++)
			{
				num = Array.FindIndex(_boneNodes[i], (BoneNode node) => node.decoration == null);
				if (num >= 0)
				{
					break;
				}
			}
		}
		if (PeGameMgr.IsMulti)
		{
			if (!hasRequest)
			{
				C2S_EquipArmorPartFromPackage(item.instanceId, (int)armorType, i, num, null);
			}
		}
		else
		{
			EquipArmorPartFromPackage(item, armorType, i, num);
		}
	}

	public bool EquipArmorPartFromPackage(ItemObject item, ArmorType type, int boneGroup, int boneIndex)
	{
		if (item == null || type == ArmorType.None)
		{
			return false;
		}
		if (type == ArmorType.Decoration)
		{
			if (_decorationCount == 4 && _boneNodes[boneGroup][boneIndex].decoration == null)
			{
				return false;
			}
		}
		else if (type != (ArmorType)boneGroup)
		{
			return false;
		}
		int num = _slotList.FindItemIndexById(item.instanceId);
		if (num < 0)
		{
			return false;
		}
		_slotList[num] = null;
		ArmorPartObject oldArmor = null;
		if (type == ArmorType.Decoration)
		{
			oldArmor = _boneNodes[boneGroup][boneIndex].decoration;
		}
		else
		{
			oldArmor = _boneNodes[boneGroup][boneIndex].normal;
		}
		if (oldArmor != null)
		{
			_slotList.Add(oldArmor.item);
			oldArmor.RemoveArmorPart();
			num = _armorObjects.FindIndex((ArmorPartObject armor) => armor == oldArmor);
			if (num >= 0)
			{
				_armorObjects.RemoveAt(num);
				_data[_currentSuitIndex].RemoveAt(num);
			}
		}
		ArmorPartData armorPartData = new ArmorPartData(item.instanceId, type);
		armorPartData.boneGroup = boneGroup;
		armorPartData.boneIndex = boneIndex;
		_data[_currentSuitIndex].Add(armorPartData);
		_armorObjects.Add(new ArmorPartObject(this, armorPartData, item));
		return true;
	}

	private ArmorPartObject GetArmorPartObject(int boneGroup, int boneIndex, bool isDecoration)
	{
		BoneNode boneNode = _boneNodes[boneGroup][boneIndex];
		return (!isDecoration) ? boneNode.normal : boneNode.decoration;
	}

	public bool RemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration)
	{
		if (_slotList.vacancyCount <= 0)
		{
			return false;
		}
		ArmorPartObject part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
		if (part == null)
		{
			return false;
		}
		_slotList.Add(part.item);
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
		armorPartObject.SyncModelScale();
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
		armorPartObject.SyncModelPosition();
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
		armorPartObject.SyncModelEulerAngles();
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
		armorPartObject.SyncModelScale();
		return true;
	}
}
