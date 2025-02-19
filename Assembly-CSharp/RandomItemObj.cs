using System;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using UnityEngine;

public class RandomItemObj : ISceneObjAgent
{
	public Vector3 genPos;

	public int boxId;

	public Vector3 position;

	public Quaternion rotation;

	public int id;

	public string path;

	public List<int> rareItemInstance = new List<int>();

	public List<ItemIdCount> rareItemProto = new List<ItemIdCount>();

	public int[] items;

	public GameObject gameObj;

	public bool isNew;

	public bool needToActivate;

	public bool tstYOnActivate;

	public RandomObjType type = RandomObjType.RandomItem;

	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public int ScenarioId { get; set; }

	public GameObject Go => gameObj;

	public Vector3 Pos => position;

	public IBoundInScene Bound => null;

	public bool NeedToActivate => needToActivate;

	public bool TstYOnActivate => tstYOnActivate;

	public RandomItemObj()
	{
	}

	public RandomItemObj(int boxId, Vector3 pos, Dictionary<int, int> itemIdNum, string path = "Prefab/Item/Scene/randomitem_test", int id = 0)
	{
		genPos = pos;
		this.boxId = boxId;
		position = pos;
		this.id = id;
		items = new int[itemIdNum.Keys.Count * 2];
		int num = 0;
		foreach (KeyValuePair<int, int> item in itemIdNum)
		{
			items[num++] = item.Key;
			items[num++] = item.Value;
		}
		this.path = path;
		SceneMan.AddSceneObj(this);
	}

	public RandomItemObj(int boxId, Vector3 pos, int[] itemIdNum, string path, int id = 0)
	{
		genPos = pos;
		this.boxId = boxId;
		position = pos;
		rotation = Quaternion.Euler(0f, new System.Random().Next(360), 0f);
		this.id = id;
		items = itemIdNum;
		this.path = path;
		isNew = true;
		SceneMan.AddSceneObj(this);
	}

	public RandomItemObj(int boxId, Vector3 pos, Quaternion rot, int[] itemIdNum, string path, int id = 0)
	{
		genPos = pos;
		this.boxId = boxId;
		position = pos;
		rotation = Quaternion.Euler(0f, new System.Random().Next(360), 0f);
		this.id = id;
		items = itemIdNum;
		this.path = path;
		isNew = true;
		SceneMan.AddSceneObj(this);
	}

	public RandomItemObj(Vector3 pos, int[] itemIdNum, string path = "Prefab/RandomItems/item_drop", int id = 0)
	{
		while (RandomItemMgr.Instance.ContainsPos(pos))
		{
			pos += new Vector3(0f, 0.01f, 0f);
		}
		genPos = pos;
		position = pos;
		rotation = Quaternion.Euler(0f, new System.Random().Next(360), 0f);
		this.id = id;
		items = itemIdNum;
		this.path = path;
		isNew = true;
		TryGenObject();
	}

	public RandomItemObj(string name, Vector3 pos, int[] itemIdNum, Quaternion rot, string path = "AiPrefab/MonsterSpecialItem/Monster_feces01", int id = 0)
	{
		genPos = pos;
		needToActivate = true;
		tstYOnActivate = true;
		boxId = -1;
		position = pos;
		rotation = rot;
		this.id = id;
		items = itemIdNum;
		this.path = path;
		isNew = true;
		TryGenFeces();
		type = RandomObjType.MonsterFeces;
	}

	public RandomItemObj(Vector3 pos, int[] itemIdNum, Quaternion rot, string path = "Prefab/RandomItems/random_box01", int id = 0)
	{
		genPos = pos;
		position = pos;
		rotation = rot;
		this.id = id;
		items = itemIdNum;
		this.path = path;
		isNew = true;
		SceneMan.AddSceneObj(this);
	}

	public void AddRareProto(int id, int count)
	{
		rareItemProto.Add(new ItemIdCount(id, count));
	}

	public void AddRareInstance(int id)
	{
		rareItemInstance.Add(id);
		if (rareItemProto.Count > 0)
		{
			rareItemProto.RemoveAt(0);
		}
		RefreshToPick();
	}

	public void RefreshToPick()
	{
		if (gameObj != null)
		{
			MousePickableRandomItem component = gameObj.GetComponent<MousePickableRandomItem>();
			if (component != null)
			{
				component.genPos = genPos;
				component.RefreshItem(items, rareItemProto, rareItemInstance);
			}
		}
	}

	public void TryGenObject()
	{
		if (!PeGameMgr.IsMulti)
		{
			RandomItemMgr.Instance.AddItemToManager(this);
			SceneMan.AddSceneObj(this);
		}
	}

	public void TryGenFeces()
	{
		RandomItemMgr.Instance.AddFeces(this);
		SceneMan.AddSceneObj(this);
	}

	public void OnActivate()
	{
		Rigidbody component = gameObj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.useGravity = true;
		}
	}

	public void OnDeactivate()
	{
		Rigidbody component = gameObj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.useGravity = false;
		}
	}

	public void OnConstruct()
	{
		GameObject gameObject = AssetsLoader.Instance.LoadPrefabImm(path) as GameObject;
		if (null != gameObject)
		{
			gameObj = UnityEngine.Object.Instantiate(gameObject);
			gameObj.transform.position = position;
			gameObj.transform.rotation = rotation;
			if (RandomItemMgr.Instance != null && RandomItemMgr.Instance.gameObject != null)
			{
				gameObj.transform.SetParent(RandomItemMgr.Instance.gameObject.transform);
			}
			MousePickableRandomItem mousePickableRandomItem = gameObj.AddComponent<MousePickableRandomItem>();
			mousePickableRandomItem.genPos = genPos;
			mousePickableRandomItem.RefreshItem(items, rareItemProto, rareItemInstance);
			Rigidbody component = gameObj.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.useGravity = false;
			}
		}
		else
		{
			Debug.LogError("randomItem model not found: " + path);
		}
	}

	public void OnDestruct()
	{
		if (gameObj != null)
		{
			position = gameObj.transform.position;
			rotation = gameObj.transform.rotation;
			UnityEngine.Object.Destroy(gameObj);
		}
	}

	public bool CanFetch(int index, int protoId, int count, out int removeIndex)
	{
		removeIndex = 0;
		int num = index * 2;
		if (num > items.Count() || items[num] != protoId || items[num + 1] != count)
		{
			for (int i = 0; i < items.Count(); i += 2)
			{
				if (items[i] == protoId && items[i + 1] == count)
				{
					removeIndex = i / 2;
					return true;
				}
			}
			return false;
		}
		removeIndex = index;
		return true;
	}

	public bool TryFetch(int index, int protoId, int count)
	{
		if (CanFetch(index, protoId, count, out var removeIndex))
		{
			List<ItemIdCount> list = GetItems();
			list.RemoveAt(removeIndex);
			SaveItems(list);
			if (gameObj != null)
			{
				ItemDropMousePickRandomItem component = gameObj.GetComponent<ItemDropMousePickRandomItem>();
				if (component != null)
				{
					component.Remove(index, protoId, count);
				}
			}
			CheckDestroyObj();
			return true;
		}
		CheckDestroyObj();
		return false;
	}

	public List<ItemIdCount> TryFetchAll()
	{
		List<ItemIdCount> result = GetItems();
		SaveItems(new List<ItemIdCount>());
		if (gameObj != null)
		{
			ItemDropMousePickRandomItem component = gameObj.GetComponent<ItemDropMousePickRandomItem>();
			if (component != null)
			{
				component.RemoveAll();
			}
		}
		CheckDestroyObj();
		return result;
	}

	public List<ItemIdCount> GetItems()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		for (int i = 0; i < items.Count(); i += 2)
		{
			if (items[i + 1] > 0)
			{
				list.Add(new ItemIdCount(items[i], items[i + 1]));
			}
		}
		return list;
	}

	public void SaveItems(List<ItemIdCount> itemlist)
	{
		items = new int[itemlist.Count * 2];
		int num = 0;
		foreach (ItemIdCount item in itemlist)
		{
			items[num++] = item.protoId;
			items[num++] = item.count;
		}
	}

	public void CheckDestroyObj()
	{
		if (items.Count() <= 0)
		{
			RandomItemMgr.Instance.RemoveRandomItemObj(this);
			if (gameObj != null)
			{
				UnityEngine.Object.Destroy(gameObj);
			}
			SceneMan.RemoveSceneObj(this);
		}
	}

	public void ClickedInMultiMode(int[] items)
	{
		for (int i = 0; i < items.Length; i += 2)
		{
			PeSingleton<LootItemMgr>.Instance.AddLootItem(genPos, items[i], items[i + 1]);
		}
		DestroySelf();
	}

	public void DestroySelf()
	{
		RandomItemMgr.Instance.RemoveRandomItemObj(this);
		if (gameObj != null)
		{
			UnityEngine.Object.Destroy(gameObj);
		}
		SceneMan.RemoveSceneObj(this);
	}
}
