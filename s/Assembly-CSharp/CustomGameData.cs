using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using UnityEngine;

public class CustomGameData
{
	public static class Mgr
	{
		public static CustomGameData data;

		public static DialogMgr dialogMgr = new DialogMgr();

		public static bool Load(string gameName)
		{
			data = new CustomGameData();
			if (data.Load(gameName))
			{
				return true;
			}
			return false;
		}

		public static YirdData GetYirdData(string yirdName = null)
		{
			if (data == null)
			{
				return null;
			}
			return data.GetYirdData(yirdName);
		}

		public static int GetGameWorldId(string yirdName)
		{
			int worldIndex = data.mWorldNames.FindIndex((string iter) => iter.Equals(yirdName));
			return GameWorld.GetWorldId(worldIndex);
		}

		public static void LoadGameData()
		{
			foreach (YirdData m in data.mList)
			{
				int gameWorldId = GetGameWorldId(m.name);
				IEnumerable<WEDoodad> doodads = m.GetDoodads();
				foreach (WEDoodad item in doodads)
				{
					if (gameWorldId != -1)
					{
						int newItemId = IdGenerator.NewItemId;
						DoodadMgr.CreateCustomDoodad(gameWorldId, newItemId, item.Prototype, -1, item.Position, item.Scale, item.Rotation);
						SPTerrainEvent.AddCustomId(newItemId, item.ID, item.Prototype, item.PlayerIndex);
					}
				}
				IEnumerable<WEItem> items = m.GetItems();
				foreach (WEItem item2 in items)
				{
					ItemObject itemObject = ItemManager.CreateItem(item2.Prototype, 1);
					if (itemObject != null)
					{
						SceneItem sceneItem = SceneObjMgr.Create<SceneItem>();
						if (sceneItem != null && gameWorldId != -1)
						{
							sceneItem.Init(itemObject.instanceId, item2.Prototype, item2.Position, Vector3.one, item2.Rotation, gameWorldId);
							sceneItem.SetItem(itemObject);
							sceneItem.SetType(ESceneObjType.ITEM);
							sceneItem.SetScenarioId(item2.ID);
							SceneObjMgr.Save(sceneItem);
							GameWorld.AddSceneObj(sceneItem, gameWorldId);
							SceneObjMgr.AddItem(itemObject, sync: false);
							SPTerrainEvent.AddCustomId(itemObject.instanceId, item2.ID, item2.Prototype, item2.PlayerIndex);
						}
					}
				}
				IEnumerable<WEEffect> effects = m.GetEffects();
				foreach (WEEffect item3 in effects)
				{
					SceneObject sceneObject = SceneObjMgr.Create<SceneObject>();
					if (sceneObject != null && gameWorldId != -1)
					{
						int newItemId2 = IdGenerator.NewItemId;
						sceneObject.Init(newItemId2, item3.Prototype, item3.Position, Vector3.one, item3.Rotation, gameWorldId);
						sceneObject.SetType(ESceneObjType.EFFECT);
						sceneObject.SetScenarioId(item3.ID);
						SceneObjMgr.Save(sceneObject);
						GameWorld.AddSceneObj(sceneObject, gameWorldId);
						SPTerrainEvent.AddCustomId(newItemId2, item3.ID, item3.Prototype, 0);
					}
				}
				IEnumerable<WENPC> npcs = m.GetNpcs();
				foreach (WENPC item4 in npcs)
				{
					if (gameWorldId != -1)
					{
						int num = SPTerrainEvent.CreateNpcWithoutLimit(-1, gameWorldId, item4.Position, item4.Prototype, 6, 1f, -1, isStand: false, item4.Rotation.eulerAngles.y, forcedServant: false, item4.ObjectName);
						SPTerrainEvent.AddCustomId(num, item4.ID, item4.Prototype, item4.PlayerIndex);
						SPTerrainEvent.AddCustomNpc(gameWorldId, num, item4.ID, item4.Prototype, item4.PlayerIndex);
					}
				}
				InitCustomMonster(m.GetMonsters(), gameWorldId);
			}
		}

		public static void LoadMonster()
		{
			foreach (YirdData m in data.mList)
			{
				int gameWorldId = GetGameWorldId(m.name);
				InitCustomMonster(m.GetMonsters(), gameWorldId);
			}
		}

		private static void InitCustomMonster(IEnumerable<WEMonster> monsters, int worldId)
		{
			List<MonsterSpawnPoint> list = new List<MonsterSpawnPoint>();
			foreach (WEMonster monster in monsters)
			{
				if (monster.AreaSpwan)
				{
					if (monster.IsSocial)
					{
						MonsterSpawnArea monsterSpawnArea = new MonsterSpawnArea(monster);
						List<MonsterSpawnArea.SocialSpawns> list2 = monsterSpawnArea.RandomPointsForSocials();
						foreach (MonsterSpawnArea.SocialSpawns item2 in list2)
						{
							list.AddRange(item2.spawnPoints);
						}
					}
					else
					{
						MonsterSpawnArea monsterSpawnArea2 = new MonsterSpawnArea(monster);
						List<MonsterSpawnPoint> collection = monsterSpawnArea2.RandomPoints();
						list.AddRange(collection);
					}
				}
				else
				{
					MonsterSpawnPoint item = new MonsterSpawnPoint(monster);
					list.Add(item);
				}
			}
			foreach (MonsterSpawnPoint item3 in list)
			{
				int num = SPTerrainEvent.CreateMonsterWithoutLimit(-1, worldId, item3.Position, item3.Prototype);
				if (num != -1)
				{
					SPTerrainEvent.AddCustomId(num, item3.ID, item3.Prototype, item3.PlayerIndex);
				}
			}
		}

		public static void LoadCustomDialogs()
		{
			PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
			if (pEDbOp != null)
			{
				pEDbOp.SetCmdText("SELECT * FROM customdialogs;");
				pEDbOp.BindReaderHandler(LoadCustomDialogsDone);
				pEDbOp.Exec();
				pEDbOp = null;
			}
		}

		private static void LoadCustomDialogsDone(SqliteDataReader reader)
		{
			if (reader.Read())
			{
				reader.GetInt32(reader.GetOrdinal("ver"));
				byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("data"));
				BufferHelper.Import(buffer, delegate(BinaryReader r)
				{
					dialogMgr.Import(r);
				});
			}
		}

		public static void SaveCustomDialogs()
		{
			byte[] array = dialogMgr.Export();
			if (array != null)
			{
				CustomDialogsData customDialogsData = new CustomDialogsData();
				customDialogsData.data = array;
				AsyncSqlite.AddRecord(customDialogsData);
			}
		}
	}

	private static readonly string s_WorldsDir = "Worlds";

	private static readonly string s_ScenarioDir = "Scenario";

	protected List<YirdData> mList = new List<YirdData>(2);

	protected List<string> mWorldNames = new List<string>(2);

	protected ScenarioMapDesc mScenarioDesc;

	protected string mDir;

	public string name => Path.GetFileNameWithoutExtension(mDir);

	public string[] WorldNames => mWorldNames.ToArray();

	private YirdData defaultYird
	{
		get
		{
			if (mList.Count > 0)
			{
				return mList[0];
			}
			return null;
		}
	}

	public Vector3 size => defaultYird?.size ?? Vector3.zero;

	public YirdData GetYirdData(string yirdName = null)
	{
		if (string.IsNullOrEmpty(yirdName))
		{
			return defaultYird;
		}
		return mList.Find((YirdData item) => (item.name == yirdName) ? true : false);
	}

	public bool Load(string dir)
	{
		string text = Path.Combine(dir, s_ScenarioDir);
		if (!Directory.Exists(text))
		{
			return false;
		}
		string path = Path.Combine(text, "WorldSettings.xml");
		if (!File.Exists(path))
		{
			return false;
		}
		string s = File.ReadAllText(path, Encoding.UTF8);
		XmlDocument xmlDocument = new XmlDocument();
		StringReader txtReader = new StringReader(s);
		xmlDocument.Load(txtReader);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("WORLDSETTINGS");
		XmlNodeList elementsByTagName = ((XmlElement)xmlNode).GetElementsByTagName("WORLD");
		foreach (XmlNode item2 in elementsByTagName)
		{
			XmlElement e = item2 as XmlElement;
			string attributeString = XmlUtil.GetAttributeString(e, "path");
			mWorldNames.Add(attributeString);
		}
		string path2 = Path.Combine(text, "ForceSettings.xml");
		if (!File.Exists(path2))
		{
			return false;
		}
		s = File.ReadAllText(path2, Encoding.UTF8);
		ForceSetting.Load(s);
		string path3 = Path.Combine(dir, "MAP.uid");
		if (!File.Exists(path3))
		{
			return false;
		}
		string text2 = File.ReadAllText(path3, Encoding.UTF8);
		text2 = text2.Trim();
		if (text2.Length != 32)
		{
			return false;
		}
		ServerConfig.UID = text2;
		mScenarioDesc = new ScenarioMapDesc(text2, dir);
		if (mWorldNames.Count == 0)
		{
			return false;
		}
		string path4 = Path.Combine(dir, s_WorldsDir);
		foreach (string mWorldName in mWorldNames)
		{
			string dir2 = Path.Combine(path4, mWorldName);
			YirdData item = new YirdData(dir2);
			mList.Add(item);
		}
		mDir = dir;
		return true;
	}
}
