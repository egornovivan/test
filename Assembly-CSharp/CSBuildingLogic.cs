using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using UnityEngine;

public class CSBuildingLogic : DragItemLogic
{
	public bool InTest;

	public PESkEntity _skEntity;

	public PeTrans _peTrans;

	public PeEntity _peEntity;

	public CSEntity m_Entity;

	public CSConst.ObjectType m_Type;

	public Transform travelTrans;

	public Transform[] m_ResultTrans;

	public Transform[] m_WorkTrans;

	public Transform[] m_NPCTrans;

	public bool IsFirstConstruct = true;

	private GameObject _mainGo;

	private Dictionary<PeEntity, SkInst> monsterSkillDict = new Dictionary<PeEntity, SkInst>();

	public GameObject ModelObj => _mainGo;

	public ColonyNetwork network => mNetlayer as ColonyNetwork;

	public int TeamId => mNetlayer.TeamId;

	public int InstanceId => itemDrag.itemObj.instanceId;

	public int protoId => itemDrag.itemObj.protoId;

	public bool HasModel => _mainGo != null;

	public bool GetNpcPos(int index, out Vector3 pos)
	{
		if (!CSMain.GetAssemblyPos(out pos))
		{
			return false;
		}
		if (index < m_NPCTrans.Length)
		{
			pos = m_NPCTrans[index].position;
		}
		return true;
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
			if (_peTrans == null)
			{
				_peTrans = base.gameObject.AddComponent<PeTrans>();
			}
			if (_peTrans != null)
			{
				_peTrans.SetModel(_mainGo.transform);
			}
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
			IsFirstConstruct = false;
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

	private void Start()
	{
		if (GameConfig.IsMultiMode)
		{
			return;
		}
		m_Type = GetMTypeFromProtoId(itemDrag.itemObj.protoId);
		CSMgCreator s_MgCreator = CSMain.s_MgCreator;
		if (s_MgCreator != null)
		{
			CSEntityAttr attr = default(CSEntityAttr);
			attr.m_InstanceId = InstanceId;
			attr.m_protoId = protoId;
			attr.m_Type = (int)m_Type;
			attr.m_Pos = base.transform.position;
			attr.m_LogicObj = base.gameObject;
			int num = s_MgCreator.CreateEntity(attr, out m_Entity);
			if (num != 4)
			{
				Debug.LogError("Error with Init Entities");
				return;
			}
			_peEntity = base.gameObject.GetComponent<PeEntity>();
			_peTrans = base.gameObject.GetComponent<PeTrans>();
			_skEntity = base.gameObject.GetComponent<PESkEntity>();
			_skEntity.m_Attrs = new PESkEntity.Attr[5];
			_skEntity.m_Attrs[0] = new PESkEntity.Attr();
			_skEntity.m_Attrs[1] = new PESkEntity.Attr();
			_skEntity.m_Attrs[2] = new PESkEntity.Attr();
			_skEntity.m_Attrs[3] = new PESkEntity.Attr();
			_skEntity.m_Attrs[4] = new PESkEntity.Attr();
			_skEntity.m_Attrs[0].m_Type = AttribType.HpMax;
			_skEntity.m_Attrs[1].m_Type = AttribType.Hp;
			_skEntity.m_Attrs[2].m_Type = AttribType.CampID;
			_skEntity.m_Attrs[3].m_Type = AttribType.DefaultPlayerID;
			_skEntity.m_Attrs[4].m_Type = AttribType.DamageID;
			_skEntity.m_Attrs[0].m_Value = m_Entity.MaxDurability;
			_skEntity.m_Attrs[1].m_Value = m_Entity.CurrentDurability;
			_skEntity.m_Attrs[2].m_Value = PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.CampID);
			_skEntity.m_Attrs[3].m_Value = 1f;
			_skEntity.m_Attrs[4].m_Value = PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DamageID);
			_skEntity.onHpChange += OnHpChange;
			if (m_Type == CSConst.ObjectType.Assembly)
			{
				_skEntity.onHpChange += SendHpChangeMessage;
			}
			_skEntity.deathEvent += OnDeathEvent;
			_skEntity.InitEntity();
			m_Entity.onDuraChange = SetHp;
			OnHpChange(_skEntity, 0f);
			int entityId = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
			PeSingleton<EntityMgr>.Instance.InitEntity(entityId, _peEntity.gameObject);
			s_MgCreator.AddLogic(id, this);
		}
		StartCoroutine(SetFirstConstruct());
	}

	private IEnumerator SetFirstConstruct()
	{
		yield return new WaitForSeconds(10f);
		IsFirstConstruct = false;
	}

	public void OnHpChange(SkEntity caster, float hpChange)
	{
		m_Entity.CurrentDurability = _skEntity.GetAttribute(AttribType.Hp);
	}

	public void SendHpChangeMessage(SkEntity caster, float hpChange)
	{
		PeNpcGroup.Instance.OnAssemblyHpChange(caster, hpChange);
	}

	public void OnDeathEvent(SkEntity skSelf, SkEntity skCaster)
	{
		m_Entity.m_Creator.RemoveEntity(InstanceId);
	}

	public void SetHp(float hp)
	{
		_skEntity.SetAttribute(AttribType.Hp, hp);
	}

	public void InitInMultiMode(CSEntity m_Entity, int ownerId)
	{
		m_Type = GetMTypeFromProtoId(itemDrag.itemObj.protoId);
		this.m_Entity = m_Entity;
		m_Entity.gameLogic = base.gameObject;
		_peEntity = base.gameObject.GetComponent<PeEntity>();
		_peTrans = base.gameObject.GetComponent<PeTrans>();
		_skEntity = base.gameObject.GetComponent<PESkEntity>();
		_skEntity.m_Attrs = new PESkEntity.Attr[5];
		_skEntity.m_Attrs[0] = new PESkEntity.Attr();
		_skEntity.m_Attrs[1] = new PESkEntity.Attr();
		_skEntity.m_Attrs[2] = new PESkEntity.Attr();
		_skEntity.m_Attrs[3] = new PESkEntity.Attr();
		_skEntity.m_Attrs[4] = new PESkEntity.Attr();
		_skEntity.m_Attrs[0].m_Type = AttribType.HpMax;
		_skEntity.m_Attrs[1].m_Type = AttribType.Hp;
		_skEntity.m_Attrs[2].m_Type = AttribType.CampID;
		_skEntity.m_Attrs[3].m_Type = AttribType.DefaultPlayerID;
		_skEntity.m_Attrs[4].m_Type = AttribType.DamageID;
		_skEntity.m_Attrs[0].m_Value = m_Entity.MaxDurability;
		_skEntity.m_Attrs[1].m_Value = m_Entity.CurrentDurability;
		_skEntity.m_Attrs[2].m_Value = 1f;
		_skEntity.m_Attrs[3].m_Value = ownerId;
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(ownerId);
		if (peEntity != null)
		{
			_skEntity.m_Attrs[2].m_Value = peEntity.GetAttribute(AttribType.CampID);
			_skEntity.m_Attrs[4].m_Value = peEntity.GetAttribute(AttribType.DamageID);
		}
		_skEntity.onHpChange += OnHpChange;
		if (m_Type == CSConst.ObjectType.Assembly)
		{
			_skEntity.onHpChange += SendHpChangeMessage;
		}
		_skEntity.deathEvent += OnDeathEvent;
		_skEntity.InitEntity();
		OnHpChange(_skEntity, 0f);
		PeSingleton<EntityMgr>.Instance.InitEntity(InstanceId, _peEntity.gameObject);
		m_Entity.m_MgCreator.AddLogic(id, this);
		if (CSMain.s_MgCreator == m_Entity.m_MgCreator && m_Entity is CSAssembly)
		{
			Vector3 vector = base.gameObject.transform.position + new Vector3(0f, 2f, 0f);
			if (travelTrans != null)
			{
				vector = travelTrans.position;
			}
		}
		StartCoroutine(SetFirstConstruct());
	}

	public static CSConst.ObjectType GetMTypeFromProtoId(int protoId)
	{
		CSConst.ObjectType result = CSConst.ObjectType.None;
		switch (protoId)
		{
		case 1127:
			result = CSConst.ObjectType.Assembly;
			break;
		case 1128:
			result = CSConst.ObjectType.PowerPlant_Coal;
			break;
		case 1129:
			result = CSConst.ObjectType.Storage;
			break;
		case 1130:
			result = CSConst.ObjectType.Repair;
			break;
		case 1131:
			result = CSConst.ObjectType.Dwelling;
			break;
		case 1132:
			result = CSConst.ObjectType.Enhance;
			break;
		case 1133:
			result = CSConst.ObjectType.Recyle;
			break;
		case 1134:
			result = CSConst.ObjectType.Farm;
			break;
		case 1135:
			result = CSConst.ObjectType.Factory;
			break;
		case 1356:
			result = CSConst.ObjectType.Processing;
			break;
		case 1357:
			result = CSConst.ObjectType.Trade;
			break;
		case 1423:
			result = CSConst.ObjectType.Train;
			break;
		case 1424:
			result = CSConst.ObjectType.Check;
			break;
		case 1422:
			result = CSConst.ObjectType.Treat;
			break;
		case 1421:
			result = CSConst.ObjectType.Tent;
			break;
		case 1558:
			result = CSConst.ObjectType.PowerPlant_Fusion;
			break;
		}
		return result;
	}

	public void DestroySelf()
	{
		DragArticleAgent.Destory(id);
	}

	public void ShieldOn(PeEntity monster, int skillId)
	{
		SkInst value = _skEntity.StartSkill(monster.skEntity, skillId);
		monsterSkillDict[monster] = value;
	}

	public void ShieldOff(PeEntity monster)
	{
		if (monsterSkillDict.ContainsKey(monster))
		{
			_skEntity.CancelSkill(monsterSkillDict[monster]);
			monsterSkillDict.Remove(monster);
		}
	}
}
