using System.IO;
using ItemAsset;
using Pathea;
using PeMap;
using PETools;
using UnityEngine;

public class DragTowerAgent : DragItemAgent
{
	private PeEntity mTowerEntity;

	public override int id
	{
		get
		{
			return base.id;
		}
		protected set
		{
			base.id = value;
			if (!(null == mTowerEntity))
			{
				ItemScript component = mTowerEntity.GetGameObject().GetComponent<ItemScript>();
				if (component != null)
				{
					component.id = id;
				}
			}
		}
	}

	public override GameObject gameObject
	{
		get
		{
			if (mTowerEntity == null)
			{
				return null;
			}
			return mTowerEntity.GetGameObject();
		}
	}

	public DragTowerAgent()
	{
	}

	public DragTowerAgent(Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
		: base(drag, pos, scl, rot, id, net)
	{
	}

	public DragTowerAgent(Drag drag, Vector3 pos, Vector3 scl, Quaternion rot)
		: this(drag, pos, scl, rot, 0)
	{
	}

	public DragTowerAgent(Drag drag, Vector3 pos, Quaternion rot)
		: this(drag, pos, Vector3.one, rot, 0)
	{
	}

	public DragTowerAgent(Drag drag, Vector3 pos)
		: this(drag, pos, Vector3.one, Quaternion.identity, 0)
	{
	}

	private void CreateEntity()
	{
		int num = id;
		if (!GameConfig.IsMultiMode)
		{
			num = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
		}
		if (itemDrag != null && itemDrag.itemObj != null)
		{
			Tower cmpt = itemDrag.itemObj.GetCmpt<Tower>();
			if (cmpt != null)
			{
				mTowerEntity = PeSingleton<PeEntityCreator>.Instance.CreateTower(num, cmpt.id, base.position, base.rotation, base.scale);
			}
		}
	}

	private void DestoryEntity()
	{
		if (!(null == mTowerEntity))
		{
			PeSingleton<EntityMgr>.Instance.Destroy(mTowerEntity.Id);
		}
	}

	private void SetData()
	{
		if (!(mTowerEntity == null))
		{
			ItemScript component = mTowerEntity.GetGameObject().GetComponent<ItemScript>();
			if (component != null)
			{
				component.InitNetlayer(network);
				component.SetItemObject(itemDrag.itemObj);
				component.id = id;
			}
		}
	}

	public override void Create()
	{
		CreateEntity();
		SetData();
	}

	protected override void Destroy()
	{
		if (itemDrag != null && itemDrag.itemObj != null)
		{
			TowerMark towerMark = PeSingleton<TowerMark.Mgr>.Instance.Find((TowerMark tower) => itemDrag.itemObj.instanceId == tower.ID);
			if (towerMark != null)
			{
				PeSingleton<LabelMgr>.Instance.Remove(towerMark);
				PeSingleton<TowerMark.Mgr>.Instance.Remove(towerMark);
			}
		}
		DestoryEntity();
	}

	protected override void Serialize(BinaryWriter bw)
	{
		base.Serialize(bw);
		if (mTowerEntity != null)
		{
			PETools.Serialize.WriteData(mTowerEntity.Export, bw);
		}
		else
		{
			PETools.Serialize.WriteData(null, bw);
		}
	}

	protected override void Deserialize(BinaryReader br)
	{
		base.Deserialize(br);
		byte[] array = PETools.Serialize.ReadBytes(br);
		if (mTowerEntity != null && array != null)
		{
			mTowerEntity.Import(array);
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (!(null == mTowerEntity))
		{
			DragEntityLodCmpt component = mTowerEntity.GetComponent<DragEntityLodCmpt>();
			if (component != null)
			{
				component.Activate();
			}
		}
	}

	protected override void OnDestruct()
	{
		if (!(null == mTowerEntity))
		{
			DragEntityLodCmpt component = mTowerEntity.GetComponent<DragEntityLodCmpt>();
			if (component != null)
			{
				component.Deactivate();
			}
		}
	}
}
