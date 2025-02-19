using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

namespace ItemAsset;

public class ItemProto
{
	public class Bundle
	{
		private class RandBundle
		{
			public int itemProtoId;

			public float probablity;
		}

		private class FixBundle
		{
			public int itemProtoId;

			public int count;
		}

		private int mCountMax;

		private int mCountMin;

		private List<RandBundle> mRandList = new List<RandBundle>();

		private List<FixBundle> mFixList = new List<FixBundle>();

		public static Bundle Load(string desc)
		{
			if (string.IsNullOrEmpty(desc) || "0" == desc)
			{
				return null;
			}
			string[] array = desc.Split(';');
			string[] array2 = array[0].Split(',');
			Bundle bundle = new Bundle();
			if (array2.Length >= 4 && array2.Length % 2 == 0)
			{
				bundle.mCountMin = Convert.ToInt32(array2[0]);
				bundle.mCountMax = Convert.ToInt32(array2[1]);
				int num = 2;
				while (num < array2.Length)
				{
					RandBundle randBundle = new RandBundle();
					randBundle.itemProtoId = Convert.ToInt32(array2[num++]);
					randBundle.probablity = Convert.ToSingle(array2[num++]);
					bundle.mRandList.Add(randBundle);
				}
			}
			if (desc.Contains(";"))
			{
				string[] array3 = array[1].Split(',');
				if (array3.Length >= 2 && array3.Length % 2 == 0)
				{
					int num2 = 0;
					while (num2 < array3.Length)
					{
						FixBundle fixBundle = new FixBundle();
						fixBundle.itemProtoId = Convert.ToInt32(array3[num2++]);
						fixBundle.count = Convert.ToInt32(array3[num2++]);
						bundle.mFixList.Add(fixBundle);
					}
				}
			}
			return bundle;
		}

		public List<MaterialItem> GetItems()
		{
			List<MaterialItem> list = new List<MaterialItem>(10);
			Dictionary<int, int> dictionary = new Dictionary<int, int>(10);
			int num = UnityEngine.Random.Range(mCountMin, mCountMax);
			for (int i = 0; i <= num; i++)
			{
				float value = UnityEngine.Random.value;
				for (int j = 0; j < mRandList.Count; j++)
				{
					if (j == 0)
					{
						if (value <= mRandList[j].probablity)
						{
							if (dictionary.ContainsKey(mRandList[j].itemProtoId))
							{
								Dictionary<int, int> dictionary2;
								Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
								int itemProtoId;
								int key = (itemProtoId = mRandList[j].itemProtoId);
								itemProtoId = dictionary2[itemProtoId];
								dictionary3[key] = itemProtoId + 1;
							}
							else
							{
								dictionary.Add(mRandList[j].itemProtoId, 1);
							}
							break;
						}
					}
					else if (j == mRandList.Count - 1)
					{
						if (value <= mRandList[j].probablity)
						{
							if (dictionary.ContainsKey(mRandList[j].itemProtoId))
							{
								Dictionary<int, int> dictionary4;
								Dictionary<int, int> dictionary5 = (dictionary4 = dictionary);
								int itemProtoId;
								int key2 = (itemProtoId = mRandList[j].itemProtoId);
								itemProtoId = dictionary4[itemProtoId];
								dictionary5[key2] = itemProtoId + 1;
							}
							else
							{
								dictionary.Add(mRandList[j].itemProtoId, 1);
							}
							break;
						}
					}
					else if (value > mRandList[j - 1].probablity && value <= mRandList[j].probablity)
					{
						if (dictionary.ContainsKey(mRandList[j].itemProtoId))
						{
							Dictionary<int, int> dictionary6;
							Dictionary<int, int> dictionary7 = (dictionary6 = dictionary);
							int itemProtoId;
							int key3 = (itemProtoId = mRandList[j].itemProtoId);
							itemProtoId = dictionary6[itemProtoId];
							dictionary7[key3] = itemProtoId + 1;
						}
						else
						{
							dictionary.Add(mRandList[j].itemProtoId, 1);
						}
						break;
					}
				}
			}
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				list.Add(new MaterialItem
				{
					protoId = item.Key,
					count = item.Value
				});
			}
			foreach (FixBundle mFix in mFixList)
			{
				list.Add(new MaterialItem
				{
					protoId = mFix.itemProtoId,
					count = mFix.count
				});
			}
			return list;
		}
	}

	public class PropertyList : IEnumerable, IEnumerable<PropertyList.PropertyValue>
	{
		public class PropertyValue
		{
			public AttribType type;

			public float value;
		}

		private PropertyValue[] mPropertys;

		public PropertyList(PropertyValue[] propertys)
		{
			mPropertys = propertys;
		}

		public PropertyList()
		{
		}

		IEnumerator<PropertyValue> IEnumerable<PropertyValue>.GetEnumerator()
		{
			if (mPropertys == null)
			{
				return null;
			}
			return mPropertys.AsEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (mPropertys == null)
			{
				return null;
			}
			return mPropertys.GetEnumerator();
		}

		public int GetCount()
		{
			return mPropertys.Length;
		}

		public float GetProperty(AttribType property)
		{
			if (mPropertys == null)
			{
				return 0f;
			}
			PropertyValue[] array = mPropertys;
			foreach (PropertyValue propertyValue in array)
			{
				if (propertyValue.type == property)
				{
					return propertyValue.value;
				}
			}
			return 0f;
		}

		public bool HasProperty(AttribType property)
		{
			if (mPropertys == null)
			{
				return false;
			}
			PropertyValue[] array = mPropertys;
			foreach (PropertyValue propertyValue in array)
			{
				if (propertyValue.type == property)
				{
					return true;
				}
			}
			return false;
		}

		public void AddProperty(AttribType property, float v)
		{
			if (HasProperty(property))
			{
				SetProperty(property, v);
			}
			PropertyValue[] destinationArray;
			if (mPropertys == null)
			{
				destinationArray = new PropertyValue[1];
			}
			else
			{
				destinationArray = new PropertyValue[mPropertys.Length + 1];
				Array.Copy(mPropertys, destinationArray, mPropertys.Length);
			}
			mPropertys = destinationArray;
			mPropertys[mPropertys.Length - 1] = new PropertyValue
			{
				type = property,
				value = v
			};
		}

		public void SetProperty(AttribType property, float value)
		{
			if (!HasProperty(property))
			{
				AddProperty(property, value);
			}
			if (mPropertys == null)
			{
				mPropertys = new PropertyValue[1];
			}
			PropertyValue[] array = mPropertys;
			foreach (PropertyValue propertyValue in array)
			{
				if (propertyValue.type == property)
				{
					propertyValue.value = value;
				}
			}
		}

		public static PropertyList LoadFromDb(SqliteDataReader reader)
		{
			float[] array = ReadFromDb(reader);
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (Mathf.Abs(array[i]) > float.Epsilon)
				{
					num++;
				}
			}
			if (num == 0)
			{
				return null;
			}
			PropertyValue[] array2 = new PropertyValue[num];
			num = 0;
			for (int j = 0; j < array.Length; j++)
			{
				if (Mathf.Abs(array[j]) > float.Epsilon)
				{
					array2[num++] = new PropertyValue
					{
						type = (AttribType)j,
						value = array[j]
					};
				}
			}
			return new PropertyList(array2);
		}

		private static float[] ReadFromDb(SqliteDataReader reader)
		{
			float[] array = new float[97];
			for (int i = 0; i < 97; i++)
			{
				AttribType attribType = (AttribType)i;
				string name = attribType.ToString();
				array[i] = Convert.ToSingle(reader.GetString(reader.GetOrdinal(name)));
			}
			return array;
		}
	}

	public class WeaponInfo
	{
		public AttackMode[] attackModes;

		public bool useEnergry;

		public int costItem;

		public int costPerShoot;
	}

	public class Mgr : MonoLikeSingleton<Mgr>
	{
		public class ItemEditorType
		{
			public int id;

			public Color color;

			public int parentID;
		}

		private List<ItemProto> mList;

		private Dictionary<int, ItemEditorType> m_ItemEditorTypes = new Dictionary<int, ItemEditorType>();

		protected override void OnInit()
		{
			base.OnInit();
			LoadData();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			Clear();
		}

		private void Clear()
		{
			mList.Clear();
			ItemLabel.Clear();
		}

		private void LoadData()
		{
			ItemLabel.LoadData();
			LoadProto();
		}

		public static int GetIdFromPin(int pin)
		{
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.SelectWhereSingle("PrototypeItem", "id", "pin", " = ", "'" + pin + "'");
			if (sqliteDataReader.Read())
			{
				return Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			}
			return -1;
		}

		private void LoadProto()
		{
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("item_editor_type");
			while (sqliteDataReader.Read())
			{
				ItemEditorType itemEditorType = new ItemEditorType();
				itemEditorType.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
				string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Colour"));
				string[] array = @string.Split(',');
				itemEditorType.color = new Color(Convert.ToSingle(array[0]) / 255f, Convert.ToSingle(array[1]) / 255f, Convert.ToSingle(array[2]) / 255f, Convert.ToSingle(array[3]) / 255f);
				itemEditorType.parentID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("parentid")));
				m_ItemEditorTypes[itemEditorType.id] = itemEditorType;
			}
			sqliteDataReader = LocalDatabase.Instance.ReadFullTable("WeaponInfo");
			Dictionary<int, WeaponInfo> dictionary = new Dictionary<int, WeaponInfo>();
			while (sqliteDataReader.Read())
			{
				WeaponInfo weaponInfo = new WeaponInfo();
				string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AttackMode"));
				weaponInfo.attackModes = ConvertToAttackModes(string2);
				weaponInfo.useEnergry = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UseEnergry"))) > 0;
				weaponInfo.costItem = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CostItem")));
				weaponInfo.costPerShoot = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CostPerShoot")));
				dictionary[Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")))] = weaponInfo;
			}
			sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeItem");
			sqliteDataReader.Read();
			mList = new List<ItemProto>();
			while (sqliteDataReader.Read())
			{
				ItemProto itemProto = new ItemProto();
				itemProto.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
				itemProto.level = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemlv")));
				itemProto.name = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_nameID"))));
				if (string.Empty == itemProto.name)
				{
					itemProto.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_engName"));
				}
				itemProto.dragName = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_dragnameID"))));
				itemProto.itemLabel = Convert.ToByte(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_typeId")));
				itemProto.setUp = Convert.ToByte(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_setUp")));
				itemProto.resourcePath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_modelPath"));
				itemProto.resourcePath1 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_logicPath"));
				itemProto.shopIcon = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("servericon"));
				string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_iconId"));
				if (!string.IsNullOrEmpty(string3))
				{
					itemProto.icon = string3.Split(',');
				}
				itemProto.placeSoundID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_placeSound")));
				itemProto.englishDescription = string.Empty;
				itemProto.descriptionStringId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_engExplain")));
				itemProto.equipReplacePos = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_replacePos")));
				itemProto.durabilityFactor = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("durDec")));
				itemProto.equipPos = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_position")));
				itemProto.buffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("buffId")));
				itemProto.skillId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skillId")));
				itemProto.towerEntityId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("turret_id")));
				itemProto.durabilityMax = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_durability")));
				string string4 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_property"));
				if (!string.IsNullOrEmpty(string4) && string4 != "0")
				{
					string[] array2 = string4.Split(',');
					if (array2.Length > 0)
					{
						string[] array3 = array2;
						foreach (string text in array3)
						{
							string[] array4 = text.Split(':');
							if (array4.Length == 2 && array4[0] == "EnergyMax")
							{
								itemProto.engergyMax = Convert.ToInt32(array4[1]);
							}
						}
					}
				}
				itemProto.currencyValue = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("currency_value")));
				itemProto.currencyValue2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("currency_value2")));
				itemProto.maxStackNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("stacking_num")));
				itemProto.equipSex = PeGender.Convert(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sex"))));
				itemProto.tabIndex = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("tab")));
				itemProto.itemClassId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_function")));
				itemProto.equipType = (EquipType)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_WeaponType")));
				itemProto.repairLevel = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_repair")));
				itemProto.sortLabel = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sort")));
				string string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("learn"));
				if (string5 != "0")
				{
					string[] array5 = string5.Split(',');
					itemProto.replicatorFormulaIds = new int[array5.Length];
					for (int j = 0; j < array5.Length; j++)
					{
						itemProto.replicatorFormulaIds[j] = Convert.ToInt32(array5[j]);
					}
				}
				itemProto.propertyList = PropertyList.LoadFromDb(sqliteDataReader);
				string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_repairMax"));
				itemProto.repairMaterialList = ConvertToMaterialItems(string5);
				string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_strengthen"));
				itemProto.strengthenMaterialList = ConvertToMaterialItems(string5);
				string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bundle"));
				itemProto.bundle = Bundle.Load(string5);
				itemProto.category = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("category"));
				itemProto.isFormula = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("is_formula"))) != 0;
				itemProto.editorTypeId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("item_editor_type"));
				itemProto.color = m_ItemEditorTypes[itemProto.editorTypeId].color;
				if (dictionary.ContainsKey(itemProto.id))
				{
					itemProto.weaponInfo = dictionary[itemProto.id];
				}
				mList.Add(itemProto);
			}
		}

		private static List<MaterialItem> ConvertToMaterialItems(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			if (text == "0")
			{
				return null;
			}
			string[] array = text.Split(';');
			List<MaterialItem> list = new List<MaterialItem>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				int protoId = Convert.ToInt32(array2[0]);
				int count = Convert.ToInt32(array2[1]);
				list.Add(new MaterialItem
				{
					protoId = protoId,
					count = count
				});
			}
			return list;
		}

		private static AttackMode[] ConvertToAttackModes(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			if (text == "0")
			{
				return null;
			}
			string[] array = text.Split(';');
			AttackMode[] array2 = new AttackMode[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string[] array3 = array[i].Split(',');
				AttackMode attackMode = new AttackMode();
				attackMode.type = (AttackType)Convert.ToInt32(array3[0]);
				attackMode.minRange = Convert.ToSingle(array3[1]);
				attackMode.maxRange = Convert.ToSingle(array3[2]);
				attackMode.minSwitchRange = Convert.ToSingle(array3[3]);
				attackMode.maxSwitchRange = Convert.ToSingle(array3[4]);
				attackMode.minAngle = Convert.ToSingle(array3[5]);
				attackMode.maxAngle = Convert.ToSingle(array3[6]);
				attackMode.frequency = Convert.ToSingle(array3[7]);
				attackMode.damage = Convert.ToSingle(array3[8]);
				attackMode.ignoreTerrain = Convert.ToInt32(array3[9]) > 0;
				array2[i] = attackMode;
			}
			return array2;
		}

		public ItemProto Get(int protoId)
		{
			return mList.Find((ItemProto item) => (item.id == protoId) ? true : false);
		}

		public ItemEditorType GetEditorType(int editorID)
		{
			if (m_ItemEditorTypes.ContainsKey(editorID))
			{
				return m_ItemEditorTypes[editorID];
			}
			return null;
		}

		public ItemProto GetByEditorType(int editorType)
		{
			return mList.Find((ItemProto item) => item.editorTypeId == editorType || true);
		}

		public void Add(ItemProto data)
		{
			mList.Add(data);
		}

		public bool Remove(ItemProto data)
		{
			return mList.Remove(data);
		}

		public bool Remove(int protoId)
		{
			return mList.RemoveAll((ItemProto item) => (item.id == protoId) ? true : false) > 0;
		}

		public void Foreach(Action<ItemProto> action)
		{
			mList.ForEach(action);
		}

		public void ClearCreation()
		{
			for (int num = mList.Count - 1; num >= 0; num--)
			{
				if (mList[num].id >= 100000000)
				{
					mList.RemoveAt(num);
				}
			}
		}
	}

	private const string InvalidStr = "0";

	public int id;

	public string name;

	public string dragName;

	public string englishDescription;

	public int descriptionStringId;

	public int placeSoundID;

	public string shopIcon;

	public string[] icon;

	public Texture2D iconTex;

	public byte itemLabel;

	public int level;

	public int sortLabel;

	public int tabIndex;

	public int itemClassId;

	public int currencyValue;

	public int currencyValue2;

	public int maxStackNum;

	public byte setUp;

	public string resourcePath;

	public string resourcePath1;

	public int equipReplacePos;

	public int equipPos;

	public PeSex equipSex;

	public EquipType equipType;

	public float durabilityFactor;

	public int durabilityMax;

	public int engergyMax;

	public bool unchargeable;

	public int[] replicatorFormulaIds;

	public Bundle bundle;

	public string category;

	public int editorTypeId;

	public bool isFormula;

	public Color color = Color.white;

	public int towerEntityId;

	public int buffId;

	public int skillId;

	public PropertyList propertyList;

	public int repairLevel;

	public List<MaterialItem> repairMaterialList;

	public List<MaterialItem> strengthenMaterialList;

	public WeaponInfo weaponInfo;

	public ItemLabel.Root rootItemLabel => ItemLabel.GetRootParent(itemLabel);

	public int currency
	{
		get
		{
			if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story)
			{
				return currencyValue;
			}
			return currencyValue2;
		}
	}

	public ItemProto()
	{
		name = string.Empty;
		resourcePath = "0";
		resourcePath1 = "0";
		icon = null;
		englishDescription = string.Empty;
	}

	public static byte[] GetBuffer(ItemProto data)
	{
		if (data == null)
		{
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(data.id);
		binaryWriter.Write(data.itemLabel);
		binaryWriter.Write(data.setUp);
		binaryWriter.Write(data.equipPos);
		binaryWriter.Write(data.buffId);
		binaryWriter.Write(data.durabilityMax);
		binaryWriter.Write(data.currencyValue);
		binaryWriter.Write(data.currencyValue2);
		binaryWriter.Write(data.maxStackNum);
		binaryWriter.Write((int)data.equipSex);
		binaryWriter.Write(data.tabIndex);
		binaryWriter.Write((int)data.equipType);
		binaryWriter.Write(data.itemClassId);
		binaryWriter.Write(data.sortLabel);
		binaryWriter.Write(data.engergyMax);
		if (data.propertyList == null)
		{
			binaryWriter.Write(0);
		}
		else
		{
			binaryWriter.Write(data.propertyList.GetCount());
			foreach (PropertyList.PropertyValue item in (IEnumerable<PropertyList.PropertyValue>)data.propertyList)
			{
				binaryWriter.Write((int)item.type);
				binaryWriter.Write(item.value);
			}
		}
		if (data.repairMaterialList == null)
		{
			binaryWriter.Write(0);
		}
		else
		{
			binaryWriter.Write(data.repairMaterialList.Count);
			foreach (MaterialItem repairMaterial in data.repairMaterialList)
			{
				binaryWriter.Write(repairMaterial.protoId);
				binaryWriter.Write(repairMaterial.count);
			}
		}
		if (data.strengthenMaterialList == null)
		{
			binaryWriter.Write(0);
		}
		else
		{
			binaryWriter.Write(data.strengthenMaterialList.Count);
			foreach (MaterialItem strengthenMaterial in data.strengthenMaterialList)
			{
				binaryWriter.Write(strengthenMaterial.protoId);
				binaryWriter.Write(strengthenMaterial.count);
			}
		}
		binaryWriter.Write(data.durabilityFactor);
		binaryWriter.Flush();
		return memoryStream.ToArray();
	}

	public static string GetName(int id)
	{
		ItemProto itemProto = PeSingleton<Mgr>.Instance.Get(id);
		return (itemProto == null) ? string.Empty : itemProto.name;
	}

	public string GetName()
	{
		return name;
	}

	public static string[] GetIconName(int id)
	{
		return PeSingleton<Mgr>.Instance.Get(id)?.icon;
	}

	public static ItemProto GetItemData(int id)
	{
		return PeSingleton<Mgr>.Instance.Get(id);
	}

	public static ItemProto GetItemDataByEditorType(int editorType)
	{
		return PeSingleton<Mgr>.Instance.GetByEditorType(editorType);
	}

	public static int GetPrice(int protoId)
	{
		return PeSingleton<Mgr>.Instance.Get(protoId).currencyValue;
	}

	public static List<MaterialItem> GetRepairMaterialList(int protoId)
	{
		if (PeSingleton<Mgr>.Instance.Get(protoId) == null)
		{
			return null;
		}
		return PeSingleton<Mgr>.Instance.Get(protoId).repairMaterialList;
	}

	public static int GetStackMax(int protoId)
	{
		return PeSingleton<Mgr>.Instance.Get(protoId).maxStackNum;
	}

	public static byte GetSetUp(int id)
	{
		return PeSingleton<Mgr>.Instance.Get(id)?.setUp ?? 0;
	}

	public bool IsBlock()
	{
		return itemClassId == 13;
	}
}
