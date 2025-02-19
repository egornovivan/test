using Pathea;
using SkillSystem;
using UnityEngine;

public class DragItemLogicFlag : DragItemLogic
{
	public PESkEntity FlagSkEntity;

	public PeTrans FlagTrans;

	public PeEntity FlagEntity;

	private GameObject _mainGo;

	public int TeamId => mNetlayer.TeamId;

	public int InstanceId => itemDrag.itemObj.instanceId;

	public int protoId => itemDrag.itemObj.protoId;

	private void Awake()
	{
		FlagEntity = base.gameObject.AddComponent<PeEntity>();
		FlagTrans = base.gameObject.AddComponent<PeTrans>();
		FlagSkEntity = base.gameObject.AddComponent<PESkEntity>();
		FlagSkEntity.onHpChange += OnHpChange;
		FlagSkEntity.deathEvent += OnDeathEvent;
		FlagSkEntity.InitEntity();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		ItemScript componentInChildren = GetComponentInChildren<ItemScript>();
		if (null != componentInChildren)
		{
			componentInChildren.OnActivate();
		}
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		ItemScript componentInChildren = GetComponentInChildren<ItemScript>();
		if (null != componentInChildren)
		{
			componentInChildren.OnDeactivate();
		}
	}

	public override void OnConstruct()
	{
		base.OnConstruct();
		_mainGo = itemDrag.CreateViewGameObject(null);
		if (!(_mainGo == null))
		{
			FlagTrans.SetModel(_mainGo.transform);
			_mainGo.transform.parent = base.transform;
			_mainGo.transform.position = base.transform.position;
			_mainGo.transform.rotation = base.transform.rotation;
			_mainGo.transform.localScale = base.transform.localScale;
			ItemScript componentInChildren = _mainGo.GetComponentInChildren<ItemScript>();
			if (null != componentInChildren)
			{
				componentInChildren.SetItemObject(itemDrag.itemObj);
				componentInChildren.InitNetlayer(mNetlayer);
				componentInChildren.id = id;
				componentInChildren.OnConstruct();
			}
		}
	}

	public override void OnDestruct()
	{
		ItemScript componentInChildren = GetComponentInChildren<ItemScript>();
		if (null != componentInChildren)
		{
			componentInChildren.OnDestruct();
		}
		if (_mainGo != null)
		{
			Object.Destroy(_mainGo);
		}
		base.OnDestruct();
	}

	public void OnHpChange(SkEntity caster, float hpChange)
	{
	}

	public void OnDeathEvent(SkEntity skSelf, SkEntity skCaster)
	{
	}
}
