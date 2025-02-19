using Pathea.Operate;
using UnityEngine;

public abstract class PersonnelBase
{
	public enum EAnimateType
	{
		WorkOnEnhanceMachine,
		WorkOnRepairMachine,
		WorkOnRecycleMachine,
		WorkOnFactory,
		WorkOnStorage,
		SitDown,
		WorkOnWatering,
		WorkOnWeedding
	}

	public delegate void AiNpcDelegate(AiNpcDelegate npc);

	public delegate void ArriveDistination(PersonnelBase npc);

	public int ID;

	private CounterScript m_CounterScript;

	protected bool m_Active = true;

	public static Vector3[] s_BoundPos = new Vector3[2]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f)
	};

	public virtual Vector3 m_Pos
	{
		get
		{
			return Vector3.zero;
		}
		set
		{
		}
	}

	public virtual Quaternion m_Rot
	{
		get
		{
			return Quaternion.identity;
		}
		set
		{
		}
	}

	public virtual string m_Name => "Personnel";

	public virtual bool ResetWorkSpace
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public virtual GameObject m_Go => null;

	public virtual float RunSpeed => 0f;

	public virtual float WalkSpeed => 0f;

	public virtual bool EqupsMeleeWeapon => true;

	public virtual bool EqupsRangeWeapon => false;

	public PersonnelBase()
	{
	}

	public abstract void MoveToImmediately(Vector3 destPos);

	public abstract bool CanBehave();

	public abstract void Sleep(bool v);

	public abstract void Stay();

	public abstract void PlayAnimation(EAnimateType type, bool v);

	public abstract PersonnelSpace[] GetWorkSpaces();

	protected virtual void OnWorkToDest(PersonnelBase npc)
	{
		Debug.Log("The NPC [" + npc.m_Name + "] is starting to Work.");
	}

	protected virtual void OnWorkMeetBlock(PersonnelBase npc)
	{
	}

	protected virtual void OnRestToDest(PersonnelBase npc)
	{
		Debug.Log("The NPC [" + npc.m_Name + "] is starting to Rest.");
	}

	protected virtual void OnRestMeetBlock(PersonnelBase npc)
	{
	}

	protected virtual void OnIdleToDest(PersonnelBase npc)
	{
		Debug.Log("The NPC [" + npc.m_Name + "] is starting to Idle.");
	}

	public abstract void UpdateWorkSpace(PersonnelSpace ps);

	public abstract void UpdateWorkMachine(PEMachine pm);

	public abstract void UpdateHospitalMachine(PEDoctor pd);

	public abstract void UpdateTrainerMachine(PETrainner pt);

	public virtual void Update()
	{
	}
}
