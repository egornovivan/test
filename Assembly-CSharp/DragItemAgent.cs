using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using PETools;
using UnityEngine;

public abstract class DragItemAgent : ISerializable, ISceneObjAgent, ISceneSerializableObjAgent
{
	protected int mId;

	private Vector3 _pos = Vector3.zero;

	private Vector3 _sca = Vector3.one;

	private Quaternion _rot = Quaternion.identity;

	public Drag itemDrag;

	public NetworkInterface network;

	private PeTrans mPeTrans;

	int ISceneObjAgent.Id
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

	GameObject ISceneObjAgent.Go => gameObject;

	Vector3 ISceneObjAgent.Pos => position;

	IBoundInScene ISceneObjAgent.Bound => null;

	bool ISceneObjAgent.NeedToActivate => NeedToActivate;

	bool ISceneObjAgent.TstYOnActivate => TstYOnActivate;

	public Vector3 position
	{
		get
		{
			return (!transform) ? _pos : (_pos = transform.position);
		}
		set
		{
			_pos = value;
			if ((bool)transform)
			{
				transform.position = value;
			}
		}
	}

	public Quaternion rotation
	{
		get
		{
			return (!transform) ? _rot : (_rot = transform.rotation);
		}
		set
		{
			_rot = value;
			if ((bool)transform)
			{
				transform.rotation = value;
			}
		}
	}

	public Vector3 scale
	{
		get
		{
			return (!transform) ? _sca : (_sca = transform.localScale);
		}
		set
		{
			_sca = value;
			if ((bool)transform)
			{
				transform.localScale = value;
			}
		}
	}

	public PeTrans peTrans => mPeTrans;

	public virtual int id
	{
		get
		{
			return mId;
		}
		protected set
		{
			mId = value;
		}
	}

	public int ScenarioId { get; set; }

	protected virtual bool NeedToActivate => true;

	protected virtual bool TstYOnActivate => true;

	public abstract GameObject gameObject { get; }

	private Transform transform
	{
		get
		{
			GameObject gameObject = this.gameObject;
			return (!gameObject) ? null : gameObject.transform;
		}
	}

	public DragItemAgent()
	{
	}

	public DragItemAgent(Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
	{
		mId = id;
		_pos = pos;
		_sca = scl;
		_rot = rot;
		network = net;
		itemDrag = drag;
	}

	public DragItemAgent(Drag drag, Vector3 pos, Vector3 scl, Quaternion rot)
		: this(drag, pos, scl, rot, 0)
	{
	}

	public DragItemAgent(Drag drag, Vector3 pos, Quaternion rot)
		: this(drag, pos, Vector3.one, rot, 0)
	{
	}

	public DragItemAgent(Drag drag, Vector3 pos)
		: this(drag, pos, Vector3.one, Quaternion.identity, 0)
	{
	}

	void ISceneObjAgent.OnConstruct()
	{
		OnConstruct();
	}

	void ISceneObjAgent.OnActivate()
	{
		OnActivate();
	}

	void ISceneObjAgent.OnDeactivate()
	{
		OnDeactivate();
	}

	void ISceneObjAgent.OnDestruct()
	{
		OnDestruct();
	}

	void ISerializable.Serialize(BinaryWriter bw)
	{
		Serialize(bw);
	}

	void ISerializable.Deserialize(BinaryReader br)
	{
		Deserialize(br);
	}

	protected void InitTransform(Transform t)
	{
		t.position = _pos;
		t.rotation = _rot;
		t.localScale = _sca;
		mPeTrans = t.GetComponent<PeTrans>();
	}

	public virtual void Create()
	{
	}

	protected virtual void Destroy()
	{
	}

	public void Rotate(Vector3 v)
	{
		rotation *= Quaternion.Euler(v);
	}

	protected virtual void Serialize(BinaryWriter bw)
	{
		bw.Write(itemDrag.itemObj.instanceId);
		bw.Write(mId);
		PETools.Serialize.WriteVector3(bw, position);
		PETools.Serialize.WriteVector3(bw, scale);
		PETools.Serialize.WriteQuaternion(bw, rotation);
	}

	protected virtual void Deserialize(BinaryReader br)
	{
		int num = br.ReadInt32();
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
		if (itemObject != null)
		{
			itemDrag = itemObject.GetCmpt<Drag>();
		}
		else
		{
			Debug.LogError("[Error]Cant find item object by id:" + num);
			itemDrag = null;
		}
		mId = br.ReadInt32();
		_pos = PETools.Serialize.ReadVector3(br);
		_sca = PETools.Serialize.ReadVector3(br);
		_rot = PETools.Serialize.ReadQuaternion(br);
		Create();
	}

	protected virtual void OnActivate()
	{
		GameObject gameObject = this.gameObject;
		if (null != gameObject && null == gameObject.GetComponent<PathfindingObstacle>())
		{
			gameObject.AddComponent<PathfindingObstacle>();
		}
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnConstruct()
	{
	}

	protected virtual void OnDestruct()
	{
	}

	public static bool Destory(DragItemAgent item)
	{
		if (item == null)
		{
			return false;
		}
		SceneMan.RemoveSceneObj(item);
		item.Destroy();
		return true;
	}

	public static DragItemAgent GetById(int id)
	{
		return SceneMan.GetSceneObjById(id) as DragItemAgent;
	}

	public static void DestroyAllInDungeon()
	{
		List<ISceneObjAgent> list = SceneMan.FindAllDragItemInDungeon();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Destory(list[num] as DragItemAgent);
		}
	}
}
