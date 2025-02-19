using System;
using System.Collections.Generic;
using System.IO;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class PeEntity : MonoBehaviour
{
	public const int VERSION_0000 = 0;

	public const int VERSION_0001 = 1;

	public const int VERSION_0002 = 2;

	public const int CURRENT_VERSION = 2;

	private static List<MonoBehaviour> s_tmpLstCmps = new List<MonoBehaviour>(20);

	[NonSerialized]
	public int version;

	private string mPrefabPath;

	[SerializeField]
	private int m_Id;

	private List<IPeMsg> mMsgListener = new List<IPeMsg>();

	public int scenarioId;

	private EntityInfoCmpt mInfoCmpt;

	[HideInInspector]
	[SerializeField]
	private bool m_Inited;

	[HideInInspector]
	[SerializeField]
	private PeTrans mPeTrans;

	[HideInInspector]
	[SerializeField]
	private SkEntity m_SkEntity;

	[SerializeField]
	[HideInInspector]
	private NetCmpt mNetCmpt;

	[HideInInspector]
	[SerializeField]
	private MonsterCmpt mMonsterCmpt;

	[HideInInspector]
	[SerializeField]
	private ViewCmpt mViewCmpt;

	[HideInInspector]
	[SerializeField]
	private LodCmpt mLodCmpt;

	[SerializeField]
	[HideInInspector]
	private Motion_Move mMotionMove;

	[HideInInspector]
	[SerializeField]
	private MotionMgrCmpt mMotionMgr;

	[SerializeField]
	[HideInInspector]
	private PESkEntity m_PeSkEntity;

	[HideInInspector]
	[SerializeField]
	private SkAliveEntity m_SkAliveEntity;

	[HideInInspector]
	[SerializeField]
	private CarrierController m_Carrier;

	[SerializeField]
	[HideInInspector]
	private TargetCmpt m_Target;

	[HideInInspector]
	[SerializeField]
	private CommonCmpt mCommonCmpt;

	[HideInInspector]
	[SerializeField]
	private PassengerCmpt mPassengerCmpt;

	[HideInInspector]
	[SerializeField]
	private EquipmentCmpt mEquipmentCmpt;

	[HideInInspector]
	[SerializeField]
	private Motion_Equip mMotionEquipment;

	[HideInInspector]
	[SerializeField]
	private EntityInfoCmpt mEnityInfoCmpt;

	[SerializeField]
	[HideInInspector]
	private NpcCmpt mNpcCmpt;

	[SerializeField]
	[HideInInspector]
	private AnimatorCmpt mAnim;

	[HideInInspector]
	[SerializeField]
	private IKCmpt mIKCmpt;

	[SerializeField]
	[HideInInspector]
	private TowerCmpt mTower;

	[HideInInspector]
	[SerializeField]
	private AbnormalConditionCmpt mAlnormal;

	[HideInInspector]
	[SerializeField]
	private PackageCmpt mPackage;

	[HideInInspector]
	[SerializeField]
	private UseItemCmpt mUseItem;

	[HideInInspector]
	[SerializeField]
	private CSBuildingLogic m_Building;

	[SerializeField]
	[HideInInspector]
	private OperateCmpt mOperateCmpt;

	[SerializeField]
	[HideInInspector]
	private Motion_Beat mMotionBeat;

	[HideInInspector]
	[SerializeField]
	private RequestCmpt mRequestCmpt;

	[HideInInspector]
	[SerializeField]
	private BehaveCmpt mBehaveCmpt;

	[HideInInspector]
	[SerializeField]
	private ReplicatorCmpt mReplicatorCmpt;

	[HideInInspector]
	[SerializeField]
	private SkillTreeUnitMgr mSkillTreeUnitMgr;

	[SerializeField]
	[HideInInspector]
	private MonstermountCtrl m_MonstermountCtrl;

	[SerializeField]
	[HideInInspector]
	private MountCmpt m_MountCmpt;

	private RobotCmpt mRobotCmpt;

	private PEBuilding mDoodaBuid;

	private Dictionary<int, Transform> m_TransDic = new Dictionary<int, Transform>();

	private MonsterProtoDb.Item mMonsterProtoDb;

	public string prefabPath => mPrefabPath;

	public EntityProto entityProto { get; set; }

	public int Id => m_Id;

	public PeTrans peTrans
	{
		get
		{
			return mPeTrans;
		}
		set
		{
			mPeTrans = value;
		}
	}

	public SkEntity skEntity
	{
		get
		{
			return m_SkEntity;
		}
		set
		{
			m_SkEntity = value;
			if (m_SkEntity != null)
			{
				m_PeSkEntity = m_SkEntity as PESkEntity;
				m_SkAliveEntity = m_SkEntity as SkAliveEntity;
			}
		}
	}

	public NetCmpt netCmpt
	{
		get
		{
			return mNetCmpt;
		}
		set
		{
			mNetCmpt = value;
		}
	}

	public MonsterCmpt monster => mMonsterCmpt;

	public ViewCmpt viewCmpt
	{
		get
		{
			return mViewCmpt;
		}
		set
		{
			mViewCmpt = value;
		}
	}

	public BiologyViewCmpt biologyViewCmpt => mViewCmpt as BiologyViewCmpt;

	public LodCmpt lodCmpt => mLodCmpt;

	public Motion_Move motionMove => mMotionMove;

	public MotionMgrCmpt motionMgr => mMotionMgr;

	public PESkEntity peSkEntity => m_PeSkEntity;

	public SkAliveEntity aliveEntity => m_SkAliveEntity;

	public CarrierController carrier
	{
		get
		{
			return m_Carrier;
		}
		set
		{
			m_Carrier = value;
		}
	}

	public TargetCmpt target
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
		}
	}

	public CommonCmpt commonCmpt => mCommonCmpt;

	public PassengerCmpt passengerCmpt => mPassengerCmpt;

	public EquipmentCmpt equipmentCmpt => mEquipmentCmpt;

	public Motion_Equip motionEquipment => mMotionEquipment;

	public EntityInfoCmpt enityInfoCmpt => mEnityInfoCmpt;

	public NpcCmpt NpcCmpt => mNpcCmpt;

	public AnimatorCmpt animCmpt => mAnim;

	public IKCmpt IKCmpt => mIKCmpt;

	public TowerCmpt Tower
	{
		get
		{
			return mTower;
		}
		set
		{
			mTower = value;
		}
	}

	public AbnormalConditionCmpt Alnormal => mAlnormal;

	public PackageCmpt packageCmpt => mPackage;

	public UseItemCmpt UseItem => mUseItem;

	public CSBuildingLogic Building => m_Building;

	public OperateCmpt operateCmpt => mOperateCmpt;

	public Motion_Beat motionBeat => mMotionBeat;

	public RequestCmpt requestCmpt => mRequestCmpt;

	public BehaveCmpt BehaveCmpt
	{
		get
		{
			return mBehaveCmpt;
		}
		set
		{
			mBehaveCmpt = value;
		}
	}

	public ReplicatorCmpt replicatorCmpt
	{
		get
		{
			return mReplicatorCmpt;
		}
		set
		{
			mReplicatorCmpt = value;
		}
	}

	public SkillTreeUnitMgr skillTreeCmpt
	{
		get
		{
			return mSkillTreeUnitMgr;
		}
		set
		{
			mSkillTreeUnitMgr = value;
		}
	}

	public MonstermountCtrl monstermountCtrl => m_MonstermountCtrl;

	public MountCmpt mountCmpt => m_MountCmpt;

	public RobotCmpt robotCmpt
	{
		get
		{
			if (mRobotCmpt == null)
			{
				mRobotCmpt = GetCmpt<RobotCmpt>();
			}
			return mRobotCmpt;
		}
	}

	public PEBuilding Doodabuid
	{
		get
		{
			if (mDoodaBuid == null)
			{
				mDoodaBuid = GetComponentInChildren<PEBuilding>();
			}
			return mDoodaBuid;
		}
	}

	public bool canInjured => !biologyViewCmpt || biologyViewCmpt.canInjured;

	public Vector3 position
	{
		get
		{
			return (!(mPeTrans != null)) ? Vector3.zero : mPeTrans.position;
		}
		set
		{
			if (mPeTrans != null)
			{
				mPeTrans.position = value;
			}
		}
	}

	public Quaternion rotation
	{
		get
		{
			return (!(mPeTrans != null)) ? Quaternion.identity : mPeTrans.rotation;
		}
		set
		{
			if (mPeTrans != null)
			{
				mPeTrans.rotation = value;
			}
		}
	}

	public Transform tr => (!(mPeTrans != null)) ? null : mPeTrans.existent;

	public float maxRadius => (!(mPeTrans != null)) ? 0f : mPeTrans.radius;

	public float maxHeight => (!(mPeTrans != null)) ? 0f : mPeTrans.bound.size.y;

	public Bounds bounds => (!(mPeTrans != null)) ? default(Bounds) : mPeTrans.bound;

	public Vector3 forward => (!(mPeTrans != null)) ? Vector3.forward : mPeTrans.forward;

	public Vector3 centerPos => (!(mPeTrans != null)) ? Vector3.zero : mPeTrans.center;

	public Vector3 centerTop => (!(mPeTrans != null)) ? Vector3.zero : mPeTrans.centerUp;

	public Vector3 spawnPos => (!(mPeTrans != null)) ? Vector3.zero : mPeTrans.spawnPosition;

	public bool isRagdoll => biologyViewCmpt != null && biologyViewCmpt.IsRagdoll;

	public bool IsFly => mMonsterCmpt != null && mMonsterCmpt.IsFly;

	public bool IsGroup => mMonsterCmpt != null && mMonsterCmpt.IsGroup;

	public bool IsLeader => mMonsterCmpt != null && mMonsterCmpt.IsLeader;

	public bool IsMember => mMonsterCmpt != null && mMonsterCmpt.IsMember;

	public bool IsDark => mMonsterCmpt != null && mMonsterCmpt.IsDark;

	public bool IsDarkInDaytime => mMonsterCmpt != null && mMonsterCmpt.IsDark && !GameConfig.IsNight && !AiUtil.CheckPositionInCave(position, 128f, 71680);

	public bool IsInjury => mMonsterCmpt != null && mMonsterCmpt.IsInjury;

	public bool IsSeriousInjury
	{
		get
		{
			return mMonsterCmpt != null && mMonsterCmpt.IsSeriousInjury;
		}
		set
		{
			if (mMonsterCmpt != null)
			{
				mMonsterCmpt.IsSeriousInjury = value;
			}
		}
	}

	public bool IsAttacking => mMonsterCmpt != null && mMonsterCmpt.IsAttacking;

	public bool IsBoss => mCommonCmpt != null && mCommonCmpt.IsBoss;

	public PeEntity Leader => (!(mMonsterCmpt != null)) ? null : mMonsterCmpt.Leader;

	public BehaveGroup Group
	{
		get
		{
			return (!(mMonsterCmpt != null)) ? null : mMonsterCmpt.Group;
		}
		set
		{
			if (mMonsterCmpt != null)
			{
				mMonsterCmpt.Group = value;
			}
		}
	}

	public Vector3 GroupLocal
	{
		get
		{
			return (!(mMonsterCmpt != null)) ? Vector3.zero : mMonsterCmpt.GroupLocal;
		}
		set
		{
			if (mMonsterCmpt != null)
			{
				mMonsterCmpt.GroupLocal = value;
			}
		}
	}

	public EEntityProto proto => (!(mCommonCmpt != null)) ? EEntityProto.Max : mCommonCmpt.entityProto.proto;

	public ERace Race => (mCommonCmpt != null) ? mCommonCmpt.Race : ERace.None;

	public int ProtoID => (!(mCommonCmpt != null)) ? (-1) : mCommonCmpt.entityProto.protoId;

	public int ItemDropId => (mCommonCmpt != null) ? mCommonCmpt.ItemDropId : 0;

	public Vector3 velocity => (!(mMotionMove != null)) ? Vector3.zero : mMotionMove.velocity;

	public Vector3 movement => (!(mMotionMove != null)) ? Vector3.zero : mMotionMove.movement;

	public float gravity
	{
		get
		{
			return (!(mMotionMove != null)) ? (-1f) : mMotionMove.gravity;
		}
		set
		{
			if (mMotionMove != null)
			{
				mMotionMove.gravity = value;
			}
		}
	}

	public MovementField Field => (mMotionMove != null && mMotionMove is Motion_Move_Motor) ? (mMotionMove as Motion_Move_Motor).Field : MovementField.None;

	public MovementState MoveState => (mMotionMove != null) ? mMotionMove.state : MovementState.None;

	public CarrierController vehicle => (!(mPassengerCmpt != null)) ? null : mPassengerCmpt.carrier;

	public Enemy attackEnemy => (!(m_Target != null)) ? null : m_Target.GetAttackEnemy();

	public PeEntity Chat
	{
		get
		{
			return (!(m_Target != null)) ? null : m_Target.Chat;
		}
		set
		{
			if (m_Target != null)
			{
				m_Target.Chat = value;
			}
		}
	}

	public PeEntity Food
	{
		get
		{
			return (!(m_Target != null)) ? null : m_Target.Food;
		}
		set
		{
			if (m_Target != null)
			{
				m_Target.Food = value;
			}
		}
	}

	public PeEntity Treat
	{
		get
		{
			return (!(m_Target != null)) ? null : m_Target.Treat;
		}
		set
		{
			if (m_Target != null)
			{
				m_Target.Treat = value;
			}
		}
	}

	public Rigidbody Rigid => (!(biologyViewCmpt != null)) ? null : biologyViewCmpt.GetModelRigidbody();

	public bool IsNpcHunger => mNpcCmpt != null && mNpcCmpt.IsHunger;

	public bool IsNpcLowHp => mNpcCmpt != null && mNpcCmpt.IsLowHp;

	public bool IsNpcUncomfortable => mNpcCmpt != null && mNpcCmpt.IsUncomfortable;

	public bool IsNpcInDinnerTime => mNpcCmpt != null && mNpcCmpt.IsInDinnerTime;

	public bool IsNpcInSleepTime => mNpcCmpt != null && mNpcCmpt.IsInSleepTime;

	public bool NpcHasAnyRequest => mNpcCmpt != null && mNpcCmpt.hasAnyRequest;

	public bool IsMotorNpc => mNpcCmpt != null && motionMove is Motion_Move_Motor;

	public bool IsMainPlayer => this == PeSingleton<MainPlayer>.Instance.entity;

	public bool IsSnake => mAnim != null && !mAnim.Equals(null) && mAnim.GetBool("Snake");

	public bool IsMount { get; private set; }

	public bool HasMount => !(null == m_MountCmpt) && null != m_MountCmpt.Mount;

	public BHPatrolMode PatrolMode
	{
		get
		{
			return (mBehaveCmpt != null) ? mBehaveCmpt.PatrolMode : BHPatrolMode.None;
		}
		set
		{
			if (mBehaveCmpt != null)
			{
				mBehaveCmpt.PatrolMode = value;
			}
		}
	}

	[Obsolete("Use hasView instead.")]
	public bool HasModel => (bool)mViewCmpt && mViewCmpt.hasView;

	public bool hasView => (bool)mViewCmpt && mViewCmpt.hasView;

	public float HPPercent
	{
		get
		{
			return Mathf.Clamp01(GetAttribute(AttribType.Hp) / GetAttribute(AttribType.HpMax));
		}
		set
		{
			SetAttribute(AttribType.Hp, GetAttribute(AttribType.HpMax) * Mathf.Clamp01(value), offEvent: false);
		}
	}

	public float Atk => GetAttribute(AttribType.Atk);

	public PeEntity Afraid
	{
		get
		{
			return (!(m_Target != null)) ? null : m_Target.Afraid;
		}
		set
		{
			if (m_Target != null)
			{
				m_Target.Afraid = value;
			}
		}
	}

	public PeEntity Doubt
	{
		get
		{
			return (!(m_Target != null)) ? null : m_Target.Doubt;
		}
		set
		{
			if (m_Target != null)
			{
				m_Target.Doubt = value;
			}
		}
	}

	public Transform centerBone => mViewCmpt.centerTransform;

	public MonsterProtoDb.Item monsterProtoDb
	{
		get
		{
			if (mMonsterCmpt != null && mMonsterProtoDb == null)
			{
				mMonsterProtoDb = MonsterProtoDb.Get(entityProto.protoId);
			}
			return mMonsterProtoDb;
		}
	}

	public void SetId(int id)
	{
		m_Id = id;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public T Add<T>() where T : MonoBehaviour, IPeCmpt
	{
		return base.gameObject.AddComponent<T>();
	}

	public void AddMsgListener(IPeMsg peMsg)
	{
		mMsgListener.Add(peMsg);
	}

	public void RemoveMsgListener(IPeMsg peMsg)
	{
		mMsgListener.Remove(peMsg);
	}

	public bool Remove(IPeCmpt cmpt)
	{
		if (cmpt is MonoBehaviour)
		{
			UnityEngine.Object.Destroy(cmpt as MonoBehaviour);
		}
		return true;
	}

	private IPeCmpt GetCmpt(string cmptName)
	{
		return base.gameObject.GetComponent(cmptName) as IPeCmpt;
	}

	private void GetCmpts(List<MonoBehaviour> lst)
	{
		lst.Clear();
		base.gameObject.GetComponents(lst);
		lst.RemoveAll((MonoBehaviour item) => !(item is IPeCmpt));
	}

	public T GetCmpt<T>() where T : MonoBehaviour, IPeCmpt
	{
		return base.gameObject.GetComponent<T>();
	}

	public void SendMsg(EMsg msg, params object[] args)
	{
		for (int i = 0; i < mMsgListener.Count; i++)
		{
			mMsgListener[i].OnMsg(msg, args);
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(2);
		if (entityProto != null)
		{
			w.Write((int)entityProto.proto);
			w.Write(entityProto.protoId);
		}
		else
		{
			w.Write(-1);
			w.Write(-1);
		}
		GetCmpts(s_tmpLstCmps);
		w.Write(s_tmpLstCmps.Count);
		for (int i = 0; i < s_tmpLstCmps.Count; i++)
		{
			if (s_tmpLstCmps[i] is IPeCmpt peCmpt)
			{
				w.Write(peCmpt.GetTypeName());
				Serialize.WriteData(peCmpt.Serialize, w);
			}
		}
	}

	public byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter w = new BinaryWriter(memoryStream))
		{
			Export(w);
		}
		return memoryStream.ToArray();
	}

	public void Import(byte[] buffer)
	{
		Serialize.Import(buffer, delegate(BinaryReader r)
		{
			version = r.ReadInt32();
			if (version > 2)
			{
				Debug.LogError("error version:" + version);
			}
			else
			{
				if (version >= 2)
				{
					int num = r.ReadInt32();
					int protoId = r.ReadInt32();
					if (num != -1)
					{
						entityProto = new EntityProto
						{
							proto = (EEntityProto)num,
							protoId = protoId
						};
					}
				}
				int num2 = r.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					string cmptName = r.ReadString();
					byte[] array = Serialize.ReadBytes(r);
					if (array != null && array.Length > 0)
					{
						IPeCmpt c = GetCmpt(cmptName);
						if (c != null)
						{
							Serialize.Import(array, delegate(BinaryReader r1)
							{
								c.Deserialize(r1);
							});
						}
					}
				}
			}
		});
	}

	public static bool Destroy(PeEntity entity)
	{
		if (entity == null)
		{
			Debug.LogError("entity is null");
			return false;
		}
		UnityEngine.Object.Destroy(entity.GetGameObject());
		return true;
	}

	public static PeEntity Create(string path, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		try
		{
			GameObject gameObject = AssetsLoader.Instance.InstantiateAssetImm(path, pos, rot, Vector3.one);
			if (null == gameObject)
			{
				Debug.LogError("cant load entity object:" + path);
				return null;
			}
			PeEntity peEntity = gameObject.GetComponent<PeEntity>();
			if (null == peEntity)
			{
				peEntity = gameObject.AddComponent<PeEntity>();
			}
			PeTrans peTrans = peEntity.peTrans;
			if (null != peTrans)
			{
				peTrans.position = pos;
				peTrans.rotation = rot;
				peTrans.scale = scl;
			}
			peEntity.mPrefabPath = path;
			return peEntity;
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Concat(ex, " : ", path));
			return null;
		}
	}

	public override string ToString()
	{
		if (null == mInfoCmpt)
		{
			mInfoCmpt = GetCmpt<EntityInfoCmpt>();
		}
		if (null != mInfoCmpt)
		{
			return mInfoCmpt.characterName.fullName;
		}
		return "NoNameEntity";
	}

	public void Reset()
	{
		ColletComponents();
	}

	private void Awake()
	{
		if (!m_Inited)
		{
			ColletComponents();
		}
	}

	private void ColletComponents()
	{
		m_Inited = true;
		mPeTrans = GetComponent<PeTrans>();
		mLodCmpt = GetComponent<LodCmpt>();
		mViewCmpt = GetComponent<ViewCmpt>();
		mCommonCmpt = GetComponent<CommonCmpt>();
		mEnityInfoCmpt = GetComponent<EntityInfoCmpt>();
		m_SkEntity = GetComponent<SkEntity>();
		mMotionMove = GetComponent<Motion_Move>();
		mMotionMgr = GetComponent<MotionMgrCmpt>();
		mNpcCmpt = GetComponent<NpcCmpt>();
		mAnim = GetComponent<AnimatorCmpt>();
		mMonsterCmpt = GetComponent<MonsterCmpt>();
		m_Carrier = GetComponent<CarrierController>();
		mPassengerCmpt = GetComponent<PassengerCmpt>();
		m_Target = GetComponent<TargetCmpt>();
		mEquipmentCmpt = GetComponent<EquipmentCmpt>();
		mMotionEquipment = GetComponent<Motion_Equip>();
		mIKCmpt = GetComponent<IKCmpt>();
		mAlnormal = GetComponent<AbnormalConditionCmpt>();
		mPackage = GetComponent<PackageCmpt>();
		mUseItem = GetComponent<UseItemCmpt>();
		mOperateCmpt = GetComponent<OperateCmpt>();
		mMotionBeat = GetComponent<Motion_Beat>();
		mRequestCmpt = GetComponent<RequestCmpt>();
		mTower = GetComponent<TowerCmpt>();
		mBehaveCmpt = GetComponent<BehaveCmpt>();
		mRobotCmpt = GetComponent<RobotCmpt>();
		mReplicatorCmpt = GetComponent<ReplicatorCmpt>();
		mSkillTreeUnitMgr = GetComponent<SkillTreeUnitMgr>();
		m_MonstermountCtrl = GetComponent<MonstermountCtrl>();
		m_MountCmpt = GetComponent<MountCmpt>();
		if (m_SkEntity != null)
		{
			m_PeSkEntity = m_SkEntity as PESkEntity;
			m_SkAliveEntity = m_SkEntity as SkAliveEntity;
		}
	}

	public MovementField GetLimiter()
	{
		if (mMotionMove is Motion_Move_Human)
		{
			return MovementField.Land;
		}
		if (mMotionMove is Motion_Move_Motor)
		{
			return (mMotionMove as Motion_Move_Motor).Field;
		}
		return MovementField.None;
	}

	public float GetAttribute(AttribType type, bool bSum = true)
	{
		return (!(m_SkEntity != null)) ? 0f : m_SkEntity.GetAttribute((int)type, bSum);
	}

	public void SetAttribute(AttribType type, float attrValue, bool offEvent = true)
	{
		if (null != m_SkEntity)
		{
			m_SkEntity.SetAttribute((int)type, attrValue, offEvent);
		}
	}

	public bool IsDeath()
	{
		return m_PeSkEntity != null && m_PeSkEntity.isDead;
	}

	public bool Stucking(float time = 5f)
	{
		return mMotionMove != null && mMotionMove.Stucking(time);
	}

	public bool IntersectRayExtend(Ray ray)
	{
		if (mPeTrans != null)
		{
			Vector3 origin = mPeTrans.trans.InverseTransformPoint(ray.origin);
			Vector3 direction = mPeTrans.trans.InverseTransformDirection(ray.direction);
			Ray ray2 = new Ray(origin, direction);
			return mPeTrans.boundExtend.IntersectRay(ray2);
		}
		return false;
	}

	public bool ContainsPointExtend(Vector3 point)
	{
		if (mPeTrans != null)
		{
			Vector3 point2 = mPeTrans.trans.InverseTransformPoint(point);
			return mPeTrans.boundExtend.Contains(point2);
		}
		return false;
	}

	public bool IntersectsExtend(Bounds bounds)
	{
		if (mPeTrans != null)
		{
			return mPeTrans.boundExtend.Intersects(bounds);
		}
		return false;
	}

	public PeEntity GetReputation(ReputationSystem.ReputationLevel minType, ReputationSystem.ReputationLevel maxType)
	{
		if (m_Target != null)
		{
			return m_Target.GetReputation(minType, maxType);
		}
		return null;
	}

	public List<IWeapon> GetWeaponlist()
	{
		if (mMotionEquipment != null)
		{
			return mMotionEquipment.GetWeaponList();
		}
		return null;
	}

	public Transform GetChild(string boneName)
	{
		if (string.IsNullOrEmpty(boneName) || "0" == boneName)
		{
			return null;
		}
		int hashCode = boneName.GetHashCode();
		Transform transform;
		if (m_TransDic.ContainsKey(hashCode))
		{
			transform = m_TransDic[hashCode];
			if (null != transform)
			{
				return transform;
			}
		}
		transform = ((!(null != biologyViewCmpt) || !(null != biologyViewCmpt.modelTrans)) ? PEUtil.GetChild(base.transform, boneName) : PEUtil.GetChild(biologyViewCmpt.modelTrans, boneName));
		if (null != transform)
		{
			m_TransDic[hashCode] = transform;
		}
		return transform;
	}

	public void OnDamageMember(PeEntity caster, float value)
	{
		if (m_Target != null && caster != null)
		{
			m_Target.OnDamageMember(caster, value);
		}
	}

	public void OnTargetDiscover(PeEntity target)
	{
		if (m_Target != null && target != null)
		{
			m_Target.OnTargetDiscover(target);
		}
	}

	public void MoveToPosition(Vector3 targetPosition)
	{
		if (!(mMotionMove == null))
		{
			mMotionMove.MoveTo(targetPosition);
		}
	}

	public void StartSkill(SkEntity target, int id, ISkPara para = null, bool bStartImm = true)
	{
		if (m_SkEntity != null)
		{
			if (m_PeSkEntity != null)
			{
				m_PeSkEntity.DispatchTargetSkill(m_SkEntity);
			}
			m_SkEntity.StartSkill(target, id, para, bStartImm);
		}
	}

	public void WeaponAtttck(IWeapon weapon, SkEntity caster)
	{
		if (m_SkEntity != null && m_PeSkEntity != null)
		{
			m_PeSkEntity.DispatchWeaponAttack(m_SkEntity);
		}
	}

	public void StopSkill(int id)
	{
		if (m_SkEntity != null)
		{
			m_SkEntity.CancelSkillById(id);
		}
	}

	public void SetNpcAlert(bool value)
	{
		if (mNpcCmpt != null)
		{
			mNpcCmpt.NpcInAlert = value;
		}
	}

	public bool IsSkillRunning(int id, bool cdInclude = true)
	{
		if (m_SkEntity != null)
		{
			return m_SkEntity.IsSkillRunning(id, cdInclude);
		}
		return false;
	}

	public bool IsSkillRunable(int id)
	{
		if (m_SkEntity != null)
		{
			return m_SkEntity.IsSkillRunnable(id);
		}
		return false;
	}

	public void DispatchTargetSkill(SkEntity caster)
	{
		if (m_PeSkEntity != null)
		{
			m_PeSkEntity.DispatchTargetSkill(caster);
		}
	}

	public void DispatchWeaponAttack(SkEntity caster)
	{
		if (m_PeSkEntity != null)
		{
			m_PeSkEntity.DispatchWeaponAttack(caster);
		}
	}

	public void DispatchOnTranslate(Vector3 pos)
	{
		if (m_PeSkEntity != null)
		{
			m_PeSkEntity.DispatchOnTranslate(pos);
		}
	}

	public void SetMount(bool isMount)
	{
		IsMount = isMount;
	}
}
