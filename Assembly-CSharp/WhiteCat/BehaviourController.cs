using ItemAsset;
using Pathea;
using SkillSystem;
using UnityEngine;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat;

public abstract class BehaviourController : MonoBehaviour
{
	private const int _buffersCount = 17;

	private CreationController _creationController;

	private ItemObject _srcItem;

	private LifeLimit _lifeCmpt;

	private Energy _energyCmpt;

	private PeEntity _creationPeEntity;

	private CreationSkEntity _creationSkEntity;

	private AudioSource _audioSource;

	private bool _hasOwner;

	private PeEntity _ownerEntity;

	private PESkEntity _ownerSkEntity;

	private bool _physicsEnabled;

	private int _enablePhysicsRequest;

	private bool _networkEnabled = true;

	private bool _isPlayerHost;

	private VCPWeapon[] _weapons;

	private float _deltaEnergy;

	private int _weaponGroupToggle = -1;

	private int _weaponControlToggle;

	private Rigidbody _rigidbody;

	private float _standardDrag = 0.1f;

	private float _underwaterDrag = 1f;

	private float _standardAngularDrag = 0.1f;

	private float _underwaterAngularDrag = 1f;

	private float _underWaterFactor;

	private float _speedScale;

	private Vector3 _tempVelocityForLOD;

	private Vector3 _tempAngularVelocityForLOD;

	private static byte[][] _buffer;

	protected NetData<Vector3> _netPosition;

	protected NetData<Quaternion> _netRotation;

	protected NetData<Vector3> _netVelocity;

	protected NetData<Vector3> _netAngularVelocity;

	protected NetData<Vector3> _netAimPoint;

	protected NetData<ushort> _netInput;

	private byte _isFreezed = byte.MaxValue;

	public CreationController creationController => _creationController;

	public ItemObject itemObject => _srcItem;

	public Rigidbody rigidbody => _rigidbody;

	public bool isPlayerHost => _isPlayerHost;

	public float underWaterFactor => _underWaterFactor;

	public float speedScale => _speedScale;

	protected abstract float mass { get; }

	protected abstract Vector3 centerOfMass { get; }

	protected abstract Vector3 inertiaTensorScale { get; }

	public abstract bool isAttackMode { get; }

	public abstract SkEntity attackTargetEntity { get; }

	public abstract Vector3 attackTargetPoint { get; }

	public PeEntity creationPeEntity => _creationPeEntity;

	public CreationSkEntity creationSkEntity => _creationSkEntity;

	public PeEntity ownerEntity => _ownerEntity;

	public PESkEntity ownerSkEntity => _ownerSkEntity;

	public float hp => _lifeCmpt.floatValue.current;

	public float maxHp => _creationController.creationData.m_Attribute.m_Durability;

	public float hpPercentage => _lifeCmpt.floatValue.percent;

	public bool isDead => _lifeCmpt.floatValue.current <= 0f;

	public float energy => _energyCmpt.floatValue.current;

	public float maxEnergy => _creationController.creationData.m_Attribute.m_MaxFuel;

	public AudioSource audioSource
	{
		get
		{
			if (!_audioSource)
			{
				_audioSource = _creationController.centerObject.gameObject.AddComponent<AudioSource>();
				_audioSource.spatialBlend = 1f;
			}
			return _audioSource;
		}
	}

	public bool physicsEnabled
	{
		get
		{
			return _physicsEnabled;
		}
		set
		{
			if (_physicsEnabled == value)
			{
				return;
			}
			_physicsEnabled = value;
			if (value)
			{
				if (_networkEnabled)
				{
					_enablePhysicsRequest = 2;
				}
				return;
			}
			_enablePhysicsRequest = 0;
			_creationController.AddBuildFinishedListener(delegate
			{
				_tempVelocityForLOD = rigidbody.velocity;
				_tempAngularVelocityForLOD = rigidbody.angularVelocity;
				rigidbody.isKinematic = true;
			});
		}
	}

	public bool networkEnabled
	{
		get
		{
			return _networkEnabled;
		}
		set
		{
			_networkEnabled = value;
		}
	}

	static BehaviourController()
	{
		_buffer = new byte[17][];
		for (int i = 0; i < 17; i++)
		{
			_buffer[i] = new byte[(i + 1) * 4];
		}
	}

	protected abstract void InitDrags(out float standardDrag, out float underwaterDrag, out float standardAngularDrag, out float underwaterAngularDrag);

	protected abstract void InitNetwork();

	protected abstract void InitOtherThings();

	protected abstract void OnNetworkSync();

	protected virtual void OnOwnerChange(PESkEntity oldOwner, PESkEntity newOwner)
	{
	}

	protected virtual void OnHpChange(float deltaHp, bool isDead)
	{
		if (isDead)
		{
			DragItemAgent byId = DragItemAgent.GetById(GetComponent<DragItemLogicCreation>().id);
			SceneMan.RemoveSceneObj(byId);
			PeSingleton<ItemMgr>.Instance.DestroyItem(itemObject);
		}
	}

	public void InitController(int itemInstanceId)
	{
		_creationController = GetComponent<CreationController>();
		_srcItem = PeSingleton<ItemMgr>.Instance.Get(itemInstanceId);
		_lifeCmpt = _srcItem.GetCmpt<LifeLimit>();
		_energyCmpt = _srcItem.GetCmpt<Energy>();
		int entityId = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
		_creationPeEntity = PeSingleton<EntityMgr>.Instance.InitEntity(entityId, base.gameObject);
		_creationPeEntity.carrier = this as CarrierController;
		CreationViewCmpt creationViewCmpt = base.gameObject.AddComponent<CreationViewCmpt>();
		_creationPeEntity.viewCmpt = creationViewCmpt;
		creationViewCmpt.Init(creationController);
		PeTrans peTrans = base.gameObject.AddComponent<PeTrans>();
		_creationPeEntity.peTrans = peTrans;
		peTrans.SetModel(base.transform);
		peTrans.bound = _creationController.bounds;
		_creationSkEntity = base.gameObject.AddComponent<CreationSkEntity>();
		_creationSkEntity.Init(this);
		_creationPeEntity.skEntity = _creationSkEntity;
		_creationSkEntity.m_Attrs = new PESkEntity.Attr[5];
		for (int i = 0; i < _creationSkEntity.m_Attrs.Length; i++)
		{
			_creationSkEntity.m_Attrs[i] = new PESkEntity.Attr();
		}
		_creationSkEntity.m_Attrs[0].m_Type = AttribType.Hp;
		_creationSkEntity.m_Attrs[0].m_Value = _lifeCmpt.floatValue.current;
		_creationSkEntity.m_Attrs[1].m_Type = AttribType.HpMax;
		_creationSkEntity.m_Attrs[1].m_Value = _lifeCmpt.valueMax;
		_creationSkEntity.m_Attrs[2].m_Type = AttribType.CampID;
		_creationSkEntity.m_Attrs[2].m_Value = 0f;
		_creationSkEntity.m_Attrs[3].m_Type = AttribType.DamageID;
		_creationSkEntity.m_Attrs[3].m_Value = 0f;
		_creationSkEntity.m_Attrs[4].m_Type = AttribType.DefaultPlayerID;
		_creationSkEntity.m_Attrs[4].m_Value = 99f;
		_creationSkEntity.onHpChange += delegate(SkEntity skEntity, float deltaHp)
		{
			_lifeCmpt.floatValue.current = _creationSkEntity.GetAttribute(AttribType.Hp);
			OnHpChange(deltaHp, _lifeCmpt.floatValue.current <= 0f);
		};
		_creationSkEntity.InitEntity();
		_rigidbody = base.gameObject.AddComponent<Rigidbody>();
		_rigidbody.mass = mass;
		_rigidbody.centerOfMass = centerOfMass;
		Vector3 size = creationController.bounds.size;
		_rigidbody.inertiaTensor = Vector3.Scale(inertiaTensorScale, _rigidbody.mass * 0.05f * new Vector3(size.y * size.y + size.z * size.z, size.x * size.x + size.z * size.z, size.x * size.x + size.y * size.y));
		_rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		_rigidbody.interpolation = RigidbodyInterpolation.None;
		_rigidbody.useGravity = true;
		_rigidbody.isKinematic = true;
		_rigidbody.maxAngularVelocity = PEVCConfig.instance.maxRigidbodyAngularSpeed;
		InitDrags(out _standardDrag, out _underwaterDrag, out _standardAngularDrag, out _underwaterAngularDrag);
		InitNetwork();
		LoadParts(ref _weapons);
		for (int j = 0; j < _weapons.Length; j++)
		{
			_weapons[j].Init(j);
		}
		SetOwner(null);
		InitOtherThings();
		base.gameObject.AddComponent<CreationDamageController>().Init(this);
		base.gameObject.AddComponent<DragCreationLodCmpt>();
		base.gameObject.AddComponent<DragItemLogicCreation>();
	}

	public IProjectileData GetProjectileData(ISkPara param)
	{
		return _weapons[(param as SkCarrierCanonPara)._idxCanon];
	}

	public void StartSkill(int skillId, ISkPara para)
	{
		_creationSkEntity.StartSkill(attackTargetEntity, skillId, para);
	}

	public VCPWeapon GetWeapon(ISkPara para)
	{
		return _weapons[(para as SkCarrierCanonPara)._idxCanon];
	}

	public VCPWeapon GetWeapon(int index = 0)
	{
		return _weapons[index];
	}

	public bool IsWeaponGroupEnabled(int index)
	{
		return _weaponGroupToggle.GetBit(index);
	}

	public bool IsWeaponControlEnabled(WeaponType type)
	{
		return _weaponControlToggle.GetBit((int)type);
	}

	public void Destroy()
	{
		PeSingleton<PeCreature>.Instance.Destory(_creationPeEntity.Id);
	}

	public bool isEnergyEnough(float expend)
	{
		return _energyCmpt.floatValue.current + _deltaEnergy - expend >= 0f;
	}

	public void ExpendEnergy(float expend)
	{
		_deltaEnergy -= expend;
	}

	public void GetAndResetDeltaEnergy(ref float delta)
	{
		delta = _deltaEnergy;
		_deltaEnergy = 0f;
	}

	public void SetEnergy(float energy)
	{
		_energyCmpt.floatValue.current = energy;
	}

	protected void ChangeOwner(PeEntity owner)
	{
		if (_ownerEntity != owner)
		{
			SetOwner(owner);
		}
	}

	public bool IsController()
	{
		if (_creationPeEntity != null && _creationPeEntity.netCmpt != null)
		{
			return _creationPeEntity.netCmpt.IsController;
		}
		return false;
	}

	protected void SetOwner(PeEntity owner)
	{
		_ownerEntity = owner;
		_hasOwner = owner;
		PESkEntity oldOwner = _ownerSkEntity;
		if (_hasOwner)
		{
			_ownerSkEntity = owner.GetComponent<PESkEntity>();
			_creationSkEntity.SetAttribute(AttribType.CampID, 5f);
			_creationSkEntity.SetAttribute(AttribType.DamageID, 5f);
			_creationSkEntity.SetAttribute(AttribType.DefaultPlayerID, owner.GetAttribute(AttribType.DefaultPlayerID));
		}
		else
		{
			_ownerSkEntity = null;
			_creationSkEntity.SetAttribute(AttribType.CampID, 0f);
			_creationSkEntity.SetAttribute(AttribType.DamageID, 0f);
			_creationSkEntity.SetAttribute(AttribType.DefaultPlayerID, 99f);
		}
		OnOwnerChange(oldOwner, _ownerSkEntity);
	}

	public void ResetHost(int controllerId)
	{
		if (_isPlayerHost != (controllerId == PeSingleton<PeCreature>.Instance.mainPlayer.Id))
		{
			_isPlayerHost = !_isPlayerHost;
			if (_isPlayerHost)
			{
				_isFreezed = 0;
			}
			else
			{
				_isFreezed = byte.MaxValue;
			}
		}
	}

	protected void ReverseWeaponGroupEnabled(int index)
	{
		_weaponGroupToggle = _weaponGroupToggle.ReverseBit(index);
	}

	protected void SetWeaponControlEnabled(WeaponType type, bool enabled)
	{
		_weaponControlToggle = _weaponControlToggle.SetBit((int)type, enabled);
	}

	protected void DisableAllWeaponControl()
	{
		_weaponControlToggle = 0;
	}

	protected virtual void FixedUpdate()
	{
		CheckPhysicsEnabled();
		if (_hasOwner && _ownerEntity == null)
		{
			SetOwner(null);
		}
		if (!GameConfig.IsMultiMode)
		{
			if (_energyCmpt != null)
			{
				_energyCmpt.floatValue.current += _deltaEnergy;
			}
			_deltaEnergy = 0f;
		}
		float sqrMagnitude = rigidbody.velocity.sqrMagnitude;
		if (sqrMagnitude > PEVCConfig.instance.maxSqrRigidbodySpeed)
		{
			rigidbody.velocity *= PEVCConfig.instance.maxRigidbodySpeed / Mathf.Sqrt(sqrMagnitude);
		}
		Vector3 extents = _creationController.bounds.extents;
		Vector3 center = _creationController.bounds.center;
		int num = 0;
		Vector3 position = center + extents;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		position.x = center.x - extents.x;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		position.y = center.y - extents.y;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		position.x = center.x + extents.x;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		position = center - extents;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		position.x = center.x + extents.x;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		position.y = center.y + extents.y;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		position.x = center.x - extents.x;
		if (VFVoxelWater.self.IsInWater(base.transform.TransformPoint(position)))
		{
			num++;
		}
		_underWaterFactor = (float)num / 8f;
		rigidbody.drag = underWaterFactor * (_underwaterDrag - _standardDrag) + _standardDrag;
		rigidbody.angularDrag = underWaterFactor * (_underwaterAngularDrag - _standardAngularDrag) + _standardAngularDrag;
		_speedScale = PEVCConfig.instance.speedScaleCurve.Evaluate(Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up).magnitude);
		if (_networkEnabled)
		{
			UpdateNetwork();
		}
	}

	private void CheckPhysicsEnabled()
	{
		PeEntity peEntity = ((PeSingleton<MainPlayer>.Instance != null) ? PeSingleton<MainPlayer>.Instance.entity : null);
		if (_enablePhysicsRequest == 0 || !(peEntity != null) || !(peEntity.peTrans != null) || !((peEntity.position - base.transform.position).sqrMagnitude < 10000f))
		{
			return;
		}
		_enablePhysicsRequest--;
		if (_enablePhysicsRequest == 0)
		{
			_creationController.AddBuildFinishedListener(delegate
			{
				rigidbody.isKinematic = false;
				rigidbody.velocity = _tempVelocityForLOD;
				rigidbody.angularVelocity = _tempAngularVelocityForLOD;
			});
		}
	}

	public void LoadPart<T>(ref T member) where T : VCPart
	{
		member = _creationController.partRoot.GetComponentInChildren<T>();
	}

	public void LoadParts<T>(ref T[] lists) where T : VCPart
	{
		lists = _creationController.partRoot.GetComponentsInChildren<T>(includeInactive: true);
	}

	private static byte[] GetShortestBuffer(int byteCount)
	{
		int num = byteCount / 4;
		if (byteCount % 4 == 0)
		{
			num--;
		}
		return _buffer[num];
	}

	private void UpdateNetwork()
	{
		if (PeGameMgr.IsMulti && !_isPlayerHost)
		{
			byte b = 1;
			if (_rigidbody.isKinematic)
			{
				_rigidbody.position = Vector3.Lerp(_rigidbody.position, _netPosition.lastData, PEVCConfig.instance.netDataApplyDamping * Time.deltaTime);
			}
			else if ((_isFreezed & b) != 0)
			{
				_rigidbody.position = _netPosition.lastData;
				_rigidbody.velocity = Vector3.zero;
				_rigidbody.drag = float.MaxValue;
			}
			b <<= 1;
			if (_rigidbody.isKinematic)
			{
				_rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation, _netRotation.lastData, PEVCConfig.instance.netDataApplyDamping * Time.deltaTime);
			}
			else if ((_isFreezed & b) != 0)
			{
				_rigidbody.rotation = _netRotation.lastData;
				rigidbody.angularVelocity = Vector3.zero;
				rigidbody.angularDrag = float.MaxValue;
			}
		}
	}

	public byte[] C2S_GetData()
	{
		byte b = 0;
		byte b2 = 0;
		int num = 1;
		byte b3 = 1;
		SyncAction syncAction = _netPosition.GetSyncAction();
		if (syncAction != 0)
		{
			b |= b3;
			if (syncAction == SyncAction.freeze)
			{
				b2 |= b3;
			}
			num += 12;
		}
		b3 <<= 1;
		syncAction = _netRotation.GetSyncAction();
		if (syncAction != 0)
		{
			b |= b3;
			if (syncAction == SyncAction.freeze)
			{
				b2 |= b3;
			}
			num += 16;
		}
		b3 <<= 1;
		syncAction = _netVelocity.GetSyncAction();
		if (syncAction != 0)
		{
			b |= b3;
			if (syncAction == SyncAction.freeze)
			{
				b2 |= b3;
			}
			num += 12;
		}
		b3 <<= 1;
		syncAction = _netAngularVelocity.GetSyncAction();
		if (syncAction != 0)
		{
			b |= b3;
			if (syncAction == SyncAction.freeze)
			{
				b2 |= b3;
			}
			num += 12;
		}
		b3 <<= 1;
		syncAction = _netAimPoint.GetSyncAction();
		if (syncAction != 0)
		{
			b |= b3;
			if (syncAction == SyncAction.freeze)
			{
				b2 |= b3;
			}
			num += 12;
		}
		b3 <<= 1;
		syncAction = _netInput.GetSyncAction();
		if (syncAction != 0)
		{
			b |= b3;
			if (syncAction == SyncAction.freeze)
			{
				b2 |= b3;
			}
			num += 2;
		}
		b3 <<= 1;
		if (b2 != 0)
		{
			b |= b3;
			num++;
		}
		byte[] array = null;
		if (b != 0)
		{
			array = GetShortestBuffer(num);
			int offset = 0;
			UnionValue unionValue = new UnionValue(b);
			unionValue.WriteByteTo(array, ref offset);
			b3 = 1;
			if ((b & b3) != 0)
			{
				Kit.WriteToBuffer(array, ref offset, _netPosition.GetData());
			}
			b3 <<= 1;
			if ((b & b3) != 0)
			{
				Kit.WriteToBuffer(array, ref offset, _netRotation.GetData());
			}
			b3 <<= 1;
			if ((b & b3) != 0)
			{
				Kit.WriteToBuffer(array, ref offset, _netVelocity.GetData());
			}
			b3 <<= 1;
			if ((b & b3) != 0)
			{
				Kit.WriteToBuffer(array, ref offset, _netAngularVelocity.GetData());
			}
			b3 <<= 1;
			if ((b & b3) != 0)
			{
				Kit.WriteToBuffer(array, ref offset, _netAimPoint.GetData());
			}
			b3 <<= 1;
			if ((b & b3) != 0)
			{
				unionValue.ushortValue = _netInput.GetData();
				unionValue.WriteUShortTo(array, ref offset);
			}
			b3 <<= 1;
			if ((b & b3) != 0)
			{
				unionValue.byteValue = b2;
				unionValue.WriteByteTo(array, ref offset);
			}
		}
		return array;
	}

	public void S2C_SetData(byte[] data)
	{
		int offset = 0;
		UnionValue unionValue = default(UnionValue);
		unionValue.ReadByteFrom(data, ref offset);
		byte byteValue = unionValue.byteValue;
		byte b = 1;
		if ((byteValue & b) != 0)
		{
			_netPosition.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
			_isFreezed = (byte)(_isFreezed & ~b);
		}
		b <<= 1;
		if ((byteValue & b) != 0)
		{
			_netRotation.SetData(Kit.ReadQuaternionFromBuffer(data, ref offset));
			_isFreezed = (byte)(_isFreezed & ~b);
		}
		b <<= 1;
		if ((byteValue & b) != 0)
		{
			_netVelocity.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
			_isFreezed = (byte)(_isFreezed & ~b);
		}
		b <<= 1;
		if ((byteValue & b) != 0)
		{
			_netAngularVelocity.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
			_isFreezed = (byte)(_isFreezed & ~b);
		}
		b <<= 1;
		if ((byteValue & b) != 0)
		{
			_netAimPoint.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
			_isFreezed = (byte)(_isFreezed & ~b);
		}
		b <<= 1;
		if ((byteValue & b) != 0)
		{
			unionValue.ReadUShortFrom(data, ref offset);
			_netInput.SetData(unionValue.ushortValue);
			_isFreezed = (byte)(_isFreezed & ~b);
		}
		b <<= 1;
		if ((byteValue & b) != 0)
		{
			unionValue.ReadByteFrom(data, ref offset);
			byte byteValue2 = unionValue.byteValue;
			b = 1;
			for (int i = 0; i < 8; i++)
			{
				if ((byteValue2 & b) != 0)
				{
					_isFreezed |= b;
				}
				b <<= 1;
			}
		}
		OnNetworkSync();
	}
}
