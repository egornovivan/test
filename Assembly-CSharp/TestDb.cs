using System;
using Pathea;
using PETools;
using UnityEngine;

public class TestDb : MonoBehaviour
{
	[Serializable]
	public class Item
	{
		[DbReader.DbField("id", false)]
		public int id;

		[DbReader.DbField("monster_icon", false)]
		public string icon;

		[DbReader.DbField("identity", false)]
		public EIdentity eId;

		[DbReader.DbField("isBoss", false)]
		public bool isBoss;

		[DbReader.DbField("EquipID", false)]
		public int[] initEquip;

		[DbReader.DbField("prefab_path", false)]
		public string modelPath
		{
			set
			{
				Debug.Log(value);
			}
		}

		private string EquipIDSet
		{
			set
			{
				Debug.Log("<color = yellow> private set" + value + "</color>");
			}
		}

		private string EquipIDGet
		{
			get
			{
				Debug.Log("<color = yellow> private get</color>");
				return "asdf";
			}
		}

		private string EquipIDGetSet
		{
			get
			{
				Debug.Log("<color = yellow> private get</color>");
				return "asdf";
			}
			set
			{
				Debug.Log("<color = yellow> private get set" + value + "</color>");
			}
		}

		private void privateMethod()
		{
		}

		public void publicMethod()
		{
		}
	}

	[Serializable]
	public class NpcItem
	{
		[DbReader.DbField("id", false)]
		public int id;

		[DbReader.DbField("npc_icon", false)]
		public string icon;

		[DbReader.DbField("gender", true)]
		public PeSex sex;
	}

	[SerializeField]
	private bool read;

	[SerializeField]
	private Item[] list;

	private void Update()
	{
		if (read)
		{
			read = false;
			Test();
		}
	}

	private void Test()
	{
		list = DbReader.Read<Item>("PrototypeMonster").ToArray();
	}
}
