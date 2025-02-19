using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using UnityEngine;

public class LootItemMgr : ArchivableSingleton<LootItemMgr>
{
	private static readonly int CURRENT_VERSION = 2;

	private static readonly float RemoveTime = 93600f;

	private static readonly float GenerateSqrDis = 2304f;

	private static readonly int UpdateNumPerFrame = 30;

	private static readonly float SqrFetchRange = 16f;

	private int m_NextID;

	private int m_UpdateGenerateIndex;

	private int m_UpdateDestroyIndex;

	private Dictionary<int, LootItemData> m_Datas = new Dictionary<int, LootItemData>();

	private List<LootItemData> m_NotGeneratedItems = new List<LootItemData>();

	private List<MousePickableLootItem> m_SceneItems = new List<MousePickableLootItem>();

	private List<MousePickableLootItem> m_FetchItems = new List<MousePickableLootItem>();

	private List<MousePickableLootItem> m_NetFetchItems = new List<MousePickableLootItem>();

	private Stack<MousePickableLootItem> m_SceneItemStack = new Stack<MousePickableLootItem>();

	private MousePickableLootItem m_Perfab;

	private Transform m_Root;

	public LootItemMgr()
	{
		m_Root = new GameObject("LootItemMgr").transform;
		m_Root.position = Vector3.zero;
		m_Root.rotation = Quaternion.identity;
		m_Root.localScale = Vector3.one;
		m_Root.gameObject.AddComponent<UIPanel>();
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/LootItem/LootItem")) as GameObject;
		m_Perfab = gameObject.GetComponent<MousePickableLootItem>();
		m_Perfab.transform.parent = m_Root;
		Recycle(m_Perfab);
	}

	protected override void WriteData(BinaryWriter w)
	{
		w.Write(CURRENT_VERSION);
		w.Write(m_Datas.Count);
		foreach (LootItemData value in m_Datas.Values)
		{
			value.Export(w);
		}
	}

	protected override void SetData(byte[] data)
	{
		MemoryStream memoryStream = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		if (num != CURRENT_VERSION)
		{
			return;
		}
		int num2 = binaryReader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			LootItemData lootItemData = new LootItemData();
			lootItemData.Import(binaryReader);
			m_Datas[lootItemData.id] = lootItemData;
			if (lootItemData.itemObj != null)
			{
				m_NotGeneratedItems.Add(lootItemData);
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public void RequestCreateLootItem(PeEntity entity)
	{
		if (null == entity || PeGameMgr.IsMulti)
		{
			return;
		}
		CommonCmpt commonCmpt = entity.commonCmpt;
		if (!(commonCmpt != null))
		{
			return;
		}
		List<ItemSample> list = ItemDropData.GetDropItems(commonCmpt.ItemDropId);
		if (commonCmpt.entityProto.proto == EEntityProto.Monster)
		{
			if (list == null)
			{
				list = GetSpecialItem.MonsterItemAdd(commonCmpt.entityProto.protoId);
			}
			else
			{
				list.AddRange(GetSpecialItem.MonsterItemAdd(commonCmpt.entityProto.protoId));
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AddLootItem(entity.position, list[i].protoId, list[i].stackCount);
			}
		}
	}

	public void AddLootItem(Vector3 pos, int instanceID)
	{
		LootItemData lootItemData = new LootItemData();
		lootItemData.id = instanceID;
		lootItemData.position = pos;
		lootItemData.dropTime = GameTime.Timer.Second;
		AddLootItem(lootItemData, generateImmediately: true);
	}

	public void AddLootItem(Vector3 pos, int itemProtoID, int num)
	{
		if (num < 1)
		{
			return;
		}
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(itemProtoID);
		int num2 = (num - 1) / itemProto.maxStackNum + 1;
		int num3 = 0;
		while (num3 < num2)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(itemProtoID);
			if (num >= itemProto.maxStackNum)
			{
				itemObject.SetStackCount(itemProto.maxStackNum);
			}
			else
			{
				itemObject.SetStackCount(num);
			}
			LootItemData lootItemData = new LootItemData();
			lootItemData.id = itemObject.instanceId;
			lootItemData.position = pos;
			lootItemData.dropTime = GameTime.Timer.Second;
			AddLootItem(lootItemData, generateImmediately: true);
			num3++;
			num -= itemProto.maxStackNum;
		}
	}

	private void AddLootItem(LootItemData data, bool generateImmediately = false)
	{
		m_Datas[data.id] = data;
		if (generateImmediately)
		{
			CreatSceneItem(data, move: true);
		}
		else
		{
			m_NotGeneratedItems.Add(data);
		}
	}

	public void NetAddLootItem(Vector3 pos, int instanceId)
	{
		LootItemData lootItemData = new LootItemData();
		lootItemData.id = instanceId;
		lootItemData.position = pos;
		lootItemData.dropTime = GameTime.Timer.Second;
		AddLootItem(lootItemData, generateImmediately: true);
	}

	public void NetRemoveLootItem(int id)
	{
		RemoveLootItem(id);
	}

	public void RequestFetch(int id)
	{
		if (PeGameMgr.IsMulti)
		{
			NetFetchRequest(id);
		}
		else
		{
			Fetch(id);
		}
	}

	private void NetFetchRequest(int id)
	{
		PlayerNetwork.mainPlayer.RequestGetLootItemBack(id, bTimeout: false);
		for (int i = 0; i < m_SceneItems.Count; i++)
		{
			if (id == m_SceneItems[i].data.id)
			{
				m_NetFetchItems.Add(m_SceneItems[i]);
				m_SceneItems.RemoveAt(i);
			}
		}
	}

	public void NetFetch(int lootItemid, int entityID)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityID);
		if (null != peEntity && null != peEntity.centerBone)
		{
			LootToTarget(lootItemid, peEntity.centerBone);
		}
		else
		{
			RemoveLootItem(lootItemid);
		}
	}

	private void Fetch(int lootItemid)
	{
		if (AddItemToPlayer(lootItemid))
		{
			LootToTarget(lootItemid, PeSingleton<MainPlayer>.Instance.entity.centerBone);
		}
	}

	private void LootToTarget(int id, Transform targetTrans)
	{
		if (null == targetTrans)
		{
			Debug.LogError("LootItem can't find centerBone.");
			return;
		}
		if (m_Datas.ContainsKey(id))
		{
			m_Datas.Remove(id);
		}
		for (int i = 0; i < m_NotGeneratedItems.Count; i++)
		{
			if (m_NotGeneratedItems[i].id == id)
			{
				m_NotGeneratedItems.RemoveAt(i);
				return;
			}
		}
		for (int j = 0; j < m_SceneItems.Count; j++)
		{
			if (id == m_SceneItems[j].data.id)
			{
				m_FetchItems.Add(m_SceneItems[j]);
				m_SceneItems[j].SetMoveState(MousePickableLootItem.MoveState.Loot, OnFetchEnd, targetTrans);
				m_SceneItems.RemoveAt(j);
			}
		}
		for (int k = 0; k < m_NetFetchItems.Count; k++)
		{
			if (m_NetFetchItems[k].data.id == id)
			{
				m_FetchItems.Add(m_NetFetchItems[k]);
				m_NetFetchItems[k].SetMoveState(MousePickableLootItem.MoveState.Loot, OnFetchEnd, targetTrans);
				m_NetFetchItems.RemoveAt(k);
				break;
			}
		}
	}

	private bool AddItemToPlayer(int id)
	{
		if (m_Datas.ContainsKey(id) && null != PeSingleton<MainPlayer>.Instance.entity && null != PeSingleton<MainPlayer>.Instance.entity.packageCmpt)
		{
			PlayerPackageCmpt playerPackageCmpt = PeSingleton<MainPlayer>.Instance.entity.packageCmpt as PlayerPackageCmpt;
			if (null != playerPackageCmpt && m_Datas[id].itemObj != null)
			{
				if (m_Datas[id].itemObj.protoData.maxStackNum > 1)
				{
					if (playerPackageCmpt.Add(m_Datas[id].itemObj.protoId, m_Datas[id].itemObj.stackCount))
					{
						PeSingleton<ItemMgr>.Instance.DestroyItem(m_Datas[id].itemObj.instanceId);
						return true;
					}
				}
				else if (playerPackageCmpt.Add(m_Datas[id].itemObj, isNew: true))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveLootItem(int id)
	{
		if (m_Datas.ContainsKey(id))
		{
			m_Datas.Remove(id);
		}
		for (int i = 0; i < m_NotGeneratedItems.Count; i++)
		{
			if (m_NotGeneratedItems[i].id == id)
			{
				m_NotGeneratedItems.RemoveAt(i);
				break;
			}
		}
		for (int j = 0; j < m_SceneItems.Count; j++)
		{
			if (m_SceneItems[j].data.id == id)
			{
				Recycle(m_SceneItems[j]);
				m_SceneItems.RemoveAt(j);
				break;
			}
		}
		for (int k = 0; k < m_NetFetchItems.Count; k++)
		{
			if (m_NetFetchItems[k].data.id == id)
			{
				Recycle(m_NetFetchItems[k]);
				m_NetFetchItems.RemoveAt(k);
				break;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (null == PeSingleton<MainPlayer>.Instance.entity)
		{
			return;
		}
		Vector3 position = PeSingleton<MainPlayer>.Instance.entity.position;
		int num = Mathf.Min(m_UpdateGenerateIndex, m_NotGeneratedItems.Count - 1);
		int num2 = Mathf.Max(m_UpdateGenerateIndex - UpdateNumPerFrame, 0);
		m_UpdateGenerateIndex = ((num2 != 0) ? num2 : (m_NotGeneratedItems.Count - 1));
		for (int num3 = num; num3 >= num2; num3--)
		{
			if (CheckItemRemove(m_NotGeneratedItems[num3]))
			{
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RequestGetLootItemBack(m_NotGeneratedItems[num3].id, bTimeout: true);
				}
				else
				{
					PeSingleton<ItemMgr>.Instance.DestroyItem(m_NotGeneratedItems[num3].id);
					m_Datas.Remove(m_NotGeneratedItems[num3].id);
					m_NotGeneratedItems.RemoveAt(num3);
				}
			}
			else if (Vector3.SqrMagnitude(position - m_NotGeneratedItems[num3].position) < GenerateSqrDis)
			{
				CreatSceneItem(m_NotGeneratedItems[num3], move: true);
				m_NotGeneratedItems.RemoveAt(num3);
			}
		}
		num = Mathf.Min(m_UpdateDestroyIndex, m_SceneItems.Count - 1);
		num2 = Mathf.Max(m_UpdateDestroyIndex - UpdateNumPerFrame, 0);
		m_UpdateDestroyIndex = ((num2 != 0) ? num2 : (m_SceneItems.Count - 1));
		for (int num4 = num; num4 >= num2; num4--)
		{
			if (num4 >= 0 && num4 < m_SceneItems.Count)
			{
				if (null == m_SceneItems[num4])
				{
					m_SceneItems.RemoveAt(num4);
				}
				else if (CheckItemRemove(m_SceneItems[num4].data))
				{
					if (PeGameMgr.IsMulti)
					{
						PlayerNetwork.mainPlayer.RequestGetLootItemBack(m_SceneItems[num4].data.id, bTimeout: true);
					}
					else
					{
						PeSingleton<ItemMgr>.Instance.DestroyItem(m_SceneItems[num4].data.id);
						m_Datas.Remove(m_SceneItems[num4].data.id);
						Recycle(m_SceneItems[num4]);
						m_SceneItems.RemoveAt(num4);
					}
				}
				else
				{
					float num5 = Vector3.SqrMagnitude(position - m_SceneItems[num4].transform.position);
					if (num5 < SqrFetchRange && !PeSingleton<MainPlayer>.Instance.entity.IsDeath())
					{
						RequestFetch(m_SceneItems[num4].data.id);
					}
					else if (num5 > GenerateSqrDis)
					{
						Recycle(m_SceneItems[num4]);
						m_NotGeneratedItems.Add(m_SceneItems[num4].data);
						m_SceneItems.RemoveAt(num4);
					}
				}
			}
		}
	}

	private void ClearData()
	{
		m_Datas.Clear();
		m_NotGeneratedItems.Clear();
		m_SceneItems.Clear();
	}

	private Vector3 GetLootPos(Vector3 pos)
	{
		return pos + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f));
	}

	private MousePickableLootItem GetSceneLootItem()
	{
		MousePickableLootItem mousePickableLootItem;
		if (m_SceneItemStack.Count > 0)
		{
			mousePickableLootItem = m_SceneItemStack.Pop();
			if (null != mousePickableLootItem)
			{
				return mousePickableLootItem;
			}
		}
		mousePickableLootItem = Object.Instantiate(m_Perfab);
		mousePickableLootItem.transform.parent = m_Root;
		return mousePickableLootItem;
	}

	private void Recycle(MousePickableLootItem item)
	{
		item.gameObject.SetActive(value: false);
		m_SceneItemStack.Push(item);
	}

	private void CreatSceneItem(LootItemData data, bool move = false)
	{
		MousePickableLootItem sceneLootItem = GetSceneLootItem();
		sceneLootItem.gameObject.SetActive(value: true);
		sceneLootItem.SetData(data);
		sceneLootItem.SetMoveState((!move) ? MousePickableLootItem.MoveState.Stay : MousePickableLootItem.MoveState.Drop);
		m_SceneItems.Add(sceneLootItem);
	}

	private void OnFetchEnd(int id)
	{
		RemoveLootItem(id);
		for (int i = 0; i < m_FetchItems.Count; i++)
		{
			if (m_FetchItems[i].data.id == id)
			{
				Recycle(m_FetchItems[i]);
				m_FetchItems.RemoveAt(i);
				break;
			}
		}
	}

	private bool CheckItemRemove(LootItemData data)
	{
		ItemObject itemObj = data.itemObj;
		if (Time.time > data.checkItemExistTime)
		{
			if (PeSingleton<ItemMgr>.Instance.Get(data.id) == null)
			{
				return true;
			}
			data.checkItemExistTime = Time.time + Random.Range(5f, 10f);
		}
		if (data.itemObj != null && itemObj.protoData.category != "Quest Item")
		{
			return GameTime.Timer.Second - data.dropTime > (double)RemoveTime;
		}
		return false;
	}
}
