using System.IO;
using ItemAsset;
using Pathea;
using UnityEngine;

public class DragArticleAgent : DragItemAgent
{
	public DragItemLogic itemLogic;

	public ItemScript itemScript;

	private SceneObjAdditionalSaveData _additionalData = new SceneObjAdditionalSaveData();

	private static Transform sRoot;

	public override int id
	{
		get
		{
			return base.id;
		}
		protected set
		{
			base.id = value;
			if (itemLogic != null)
			{
				itemLogic.id = value;
			}
		}
	}

	public override GameObject gameObject
	{
		get
		{
			if (itemLogic != null)
			{
				return itemLogic.gameObject;
			}
			if (itemScript != null)
			{
				return itemScript.gameObject;
			}
			return null;
		}
	}

	public static Transform Root
	{
		get
		{
			if (sRoot == null)
			{
				sRoot = new GameObject("ArticleRoot").transform;
			}
			return sRoot;
		}
	}

	public DragArticleAgent()
	{
	}

	public DragArticleAgent(Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
		: base(drag, pos, scl, rot, id, net)
	{
	}

	public override void Create()
	{
		TryLoadLogicGo();
		SetDataToLogic();
	}

	protected override void Destroy()
	{
		DestoryGameObject();
	}

	private void TryLoadLogicGo()
	{
		if (itemDrag == null)
		{
			return;
		}
		GameObject gameObject = itemDrag.CreateLogicGameObject(base.InitTransform);
		if (gameObject != null)
		{
			if (SceneMan.self != null)
			{
				gameObject.transform.SetParent(Root, worldPositionStays: true);
			}
			itemLogic = gameObject.GetComponent<DragItemLogic>();
		}
	}

	private void SetDataToLogic()
	{
		if (!(itemLogic == null))
		{
			InitTransform(itemLogic.transform);
			itemLogic.SetItemDrag(itemDrag);
			itemLogic.InitNetlayer(network);
			if (MissionManager.Instance != null && MissionManager.Instance.m_PlayerMission.isRecordCreation && itemLogic.itemDrag.protoData.itemClassId == 66 && MissionManager.Instance.m_PlayerMission.recordCreationName.Count < 4)
			{
				MissionManager.Instance.m_PlayerMission.recordCreationName.Add(itemLogic.transform.gameObject.name);
			}
			itemLogic.id = id;
		}
	}

	private void DestoryGameObject()
	{
		if (itemLogic != null)
		{
			Object.Destroy(itemLogic.gameObject);
		}
		if (itemScript != null)
		{
			Object.Destroy(itemScript.gameObject);
		}
	}

	public void TryForceCreateGO()
	{
		if (itemScript == null)
		{
			TryLoadViewGo();
			if (null != itemScript)
			{
				itemScript.OnConstruct();
			}
		}
	}

	private void TryLoadViewGo()
	{
		if (itemDrag != null)
		{
			GameObject gameObject = itemDrag.CreateViewGameObject(base.InitTransform);
			if (!(gameObject == null))
			{
				InitTransform(gameObject.transform);
				gameObject.transform.SetParent(Root, worldPositionStays: true);
				itemScript = gameObject.GetComponent<ItemScript>();
			}
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (itemLogic != null)
		{
			itemLogic.OnActivate();
		}
		else if (null != itemScript)
		{
			itemScript.InitNetlayer(network);
			itemScript.SetItemObject(itemDrag.itemObj);
			itemScript.id = id;
			itemScript.OnActivate();
		}
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		if (itemLogic != null)
		{
			itemLogic.OnDeactivate();
		}
		else if (null != itemScript)
		{
			itemScript.OnDeactivate();
		}
	}

	protected override void OnConstruct()
	{
		if (itemLogic != null)
		{
			itemLogic.OnConstruct();
		}
		else if (itemScript == null)
		{
			TryLoadViewGo();
			if (null != itemScript)
			{
				itemScript.OnConstruct();
			}
		}
	}

	protected override void OnDestruct()
	{
		if (itemLogic != null)
		{
			itemLogic.OnDestruct();
			return;
		}
		if (null != itemScript)
		{
			itemScript.OnDestruct();
		}
		DestoryGameObject();
	}

	protected override void Deserialize(BinaryReader br)
	{
		base.Deserialize(br);
		_additionalData.Deserialize(br);
		_additionalData.DispatchData(gameObject);
	}

	protected override void Serialize(BinaryWriter bw)
	{
		base.Serialize(bw);
		_additionalData.CollectData(gameObject);
		_additionalData.Serialize(bw);
	}

	public static DragArticleAgent PutItemByProroId(int protoId, Vector3 pos, Quaternion rot)
	{
		return PutItemByProroId(protoId, pos, Vector3.one, rot);
	}

	public static Drag CreateItemDrag(int protoId)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(protoId);
		if (itemObject == null)
		{
			Debug.LogError("create item failed, protoId:" + protoId);
			return null;
		}
		return itemObject.GetCmpt<Drag>();
	}

	public static DragArticleAgent PutItemByProroId(int protoId, Vector3 pos, Vector3 scl, Quaternion rot, bool pickable = true, bool attackable = false)
	{
		Drag drag = CreateItemDrag(protoId);
		if (drag == null)
		{
			Debug.LogError("item has no drag, protoId:" + protoId);
			return null;
		}
		return Create(drag, pos, scl, rot);
	}

	public static bool Destory(int id)
	{
		DragItemAgent byId = DragItemAgent.GetById(id);
		return DragItemAgent.Destory(byId);
	}

	public static DragArticleAgent Create(Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id = 0, NetworkInterface net = null, bool isCreation = false)
	{
		DragArticleAgent dragArticleAgent = ((!isCreation) ? new DragArticleAgent(drag, pos, scl, rot, id, net) : new DragCreationAgent(drag, pos, scl, rot, id, net));
		dragArticleAgent.Create();
		SceneMan.AddSceneObj(dragArticleAgent);
		return dragArticleAgent;
	}

	public static DragArticleAgent Create(Drag drag, Vector3 pos, Quaternion rot)
	{
		return Create(drag, pos, Vector3.one, rot);
	}

	public static DragArticleAgent Create(Drag drag, Vector3 pos)
	{
		return Create(drag, pos, Vector3.one, Quaternion.identity);
	}
}
