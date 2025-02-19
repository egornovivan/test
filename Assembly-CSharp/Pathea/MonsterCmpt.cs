using System.Collections;
using System.Collections.Generic;
using PETools;
using SkillSystem;
using SoundAsset;
using UnityEngine;

namespace Pathea;

public class MonsterCmpt : PeCmpt, IPeMsg
{
	private AnimatorCmpt m_Animator;

	private SkAliveEntity m_SkEntity;

	private BehaveCmpt m_Behave;

	private Motion_Move_Motor m_Motor;

	private PeTrans m_Trans;

	private RequestCmpt m_Request;

	private PENative m_Native;

	private BehaveGroup m_Group;

	private Vector3 m_GroupLocal;

	private int m_InjuredLevel;

	private bool m_IsDark;

	private bool m_IsFly;

	private bool m_Injury;

	private bool m_IsAttacking;

	private bool m_IsWaterSurface;

	private bool m_SeriousInjury;

	private bool m_CanRide = true;

	public bool CanRide => m_CanRide;

	public int InjuredLevel
	{
		get
		{
			return m_InjuredLevel;
		}
		set
		{
			m_InjuredLevel = value;
		}
	}

	public bool IsFly => m_IsFly;

	public bool IsGroup => m_Group != null;

	public bool IsLeader => m_Group != null && base.Entity.Equals(m_Group.Leader);

	public bool IsMember => m_Group != null && !base.Entity.Equals(m_Group.Leader);

	public bool IsInjury => m_Injury;

	public bool IsSeriousInjury
	{
		get
		{
			return m_SeriousInjury;
		}
		set
		{
			m_SeriousInjury = value;
		}
	}

	public bool IsDark
	{
		get
		{
			return m_IsDark;
		}
		set
		{
			m_IsDark = value;
		}
	}

	public NativeProfession Profession => (m_Native != null) ? m_Native.Profession : NativeProfession.None;

	public NativeAge Age => (m_Native != null) ? m_Native.Age : NativeAge.None;

	public NativeSex Sex => (m_Native != null) ? m_Native.Sex : NativeSex.None;

	public PeEntity Leader => (!(m_Group != null)) ? null : m_Group.Leader;

	public BehaveGroup Group
	{
		get
		{
			return m_Group;
		}
		set
		{
			m_Group = value;
		}
	}

	public Vector3 GroupLocal
	{
		get
		{
			return m_GroupLocal;
		}
		set
		{
			m_GroupLocal = value;
		}
	}

	public bool IsAttacking => m_Animator != null && m_Animator.GetBool("Attacking");

	public bool WaterSurface
	{
		get
		{
			return m_IsWaterSurface;
		}
		set
		{
			if (m_IsWaterSurface != value)
			{
				m_IsWaterSurface = value;
				m_Animator.SetBool("WaterSurface", m_IsWaterSurface);
			}
		}
	}

	private void OnDamage(SkEntity entity, float damage)
	{
		m_Injury = base.Entity.HPPercent <= 0.5f;
	}

	public override void Awake()
	{
		base.Awake();
		m_Animator = GetComponent<AnimatorCmpt>();
		m_SkEntity = GetComponent<SkAliveEntity>();
		m_Behave = GetComponent<BehaveCmpt>();
		m_Motor = GetComponent<Motion_Move_Motor>();
		m_Trans = GetComponent<PeTrans>();
		m_Request = GetComponent<RequestCmpt>();
		if (m_SkEntity != null)
		{
			m_SkEntity.deathEvent += OnDeath;
		}
	}

	public override void Start()
	{
		base.Start();
		if (base.Entity.Race == ERace.Monster && base.Entity.Field != MovementField.water)
		{
			MonsterProtoDb.Item item = MonsterProtoDb.Get(base.Entity.ProtoID);
			if (item != null && item.idleSounds != null && item.idleSounds.Length > 0)
			{
				StartCoroutine(IdleAudio(item.idleSounds, item.idleSoundDis));
			}
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_Animator != null)
		{
			m_Animator.SetBool("Ground", m_Motor != null && m_Motor.grounded);
		}
	}

	private void OnDeath(SkEntity skEntity1, SkEntity skEntity2)
	{
		MonsterProtoDb.Item item = MonsterProtoDb.Get(base.Entity.ProtoID);
		if (item != null && item.deathAudioID > 0)
		{
			AudioManager.instance.Create(base.transform.position, item.deathAudioID);
		}
	}

	private IEnumerator IdleAudio(int[] sounds, float distance)
	{
		List<int> _tmpSounds = new List<int>();
		while (sounds != null && sounds.Length > 0)
		{
			yield return new WaitForSeconds(Random.Range(20f, 35f));
			_tmpSounds.Clear();
			PeEntity player = PeSingleton<PeCreature>.Instance.mainPlayer;
			if (!(player != null) || !(m_Animator != null))
			{
				continue;
			}
			float sqrDis = PEUtil.SqrMagnitude(player.position, base.Entity.position);
			if (!(sqrDis > distance * distance) || !(Random.value < 0.2f))
			{
				continue;
			}
			for (int i = 0; i < sounds.Length; i++)
			{
				SESoundBuff buff = SESoundBuff.GetSESoundData(sounds[i]);
				if (buff != null && sqrDis < buff.mMaxDistance * buff.mMaxDistance * 0.81f)
				{
					_tmpSounds.Add(sounds[i]);
				}
			}
			if (_tmpSounds.Count > 0)
			{
				int soundID = _tmpSounds[Random.Range(0, _tmpSounds.Count)];
				AudioManager.instance.Create(base.Entity.position, soundID);
			}
		}
	}

	public void ActivateGravity(bool value)
	{
		if (m_Motor != null && m_Motor.motor != null)
		{
			if (value)
			{
				m_Motor.motor.gravity = 10f;
			}
			else
			{
				m_Motor.motor.gravity = 0f;
			}
			m_IsFly = !value;
		}
	}

	public void Ride(bool value)
	{
		m_CanRide = value;
	}

	public void Fly(bool value)
	{
		if (m_Animator != null && m_Motor != null && m_Motor.Field == MovementField.Sky)
		{
			m_Animator.SetBool("Fly", value);
		}
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.Action_Whacked:
		case EMsg.Action_Repulsed:
		case EMsg.Action_Wentfly:
		case EMsg.Action_Knocked:
			m_Behave.Reset();
			m_SkEntity.CancelAllSkills();
			m_Animator.SetTrigger("Interrupt");
			break;
		case EMsg.View_Model_Build:
		{
			GameObject gameObject = args[0] as GameObject;
			BiologyViewRoot biologyViewRoot = args[1] as BiologyViewRoot;
			m_Native = biologyViewRoot.native;
			PEMotor motor = biologyViewRoot.motor;
			if (m_Trans != null && m_Animator != null && motor != null && m_Motor != null && m_Motor.Field == MovementField.Sky)
			{
				if (motor.gravity > 0f)
				{
					m_IsFly = false;
					m_Animator.SetBool("Fly", value: false);
				}
				else
				{
					m_IsFly = true;
					m_Animator.SetBool("Fly", value: true);
				}
			}
			PEMonster monster = biologyViewRoot.monster;
			if (monster != null)
			{
				m_IsDark = monster.isDark;
			}
			break;
		}
		}
	}

	public Request Req_MoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
	{
		return (!(m_Request != null)) ? null : m_Request.Register(EReqType.MoveToPoint, position, stopRadius, isForce, state);
	}
}
