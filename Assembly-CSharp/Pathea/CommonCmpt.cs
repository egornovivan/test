using System.IO;
using UnityEngine;

namespace Pathea;

public class CommonCmpt : PeCmpt
{
	private AnimatorCmpt m_Animator;

	private EIdentity m_IdentityProto;

	private ERace m_Race;

	private EIdentity m_Identity;

	private bool m_IsBoss;

	private GameObject m_TDObj;

	private Vector3 m_TDpos;

	private int m_OwnerID;

	private int m_ItemDropId;

	public EIdentity IdentityProto
	{
		get
		{
			return m_IdentityProto;
		}
		set
		{
			m_IdentityProto = value;
			Identity = value;
		}
	}

	public ERace Race
	{
		get
		{
			return m_Race;
		}
		set
		{
			m_Race = value;
		}
	}

	public EIdentity Identity
	{
		get
		{
			return m_Identity;
		}
		set
		{
			m_Identity = value;
		}
	}

	public bool IsBoss
	{
		get
		{
			return m_IsBoss;
		}
		set
		{
			m_IsBoss = value;
		}
	}

	public GameObject TDObj
	{
		get
		{
			return m_TDObj;
		}
		set
		{
			m_TDObj = value;
		}
	}

	public Vector3 TDpos
	{
		get
		{
			return m_TDpos;
		}
		set
		{
			m_TDpos = value;
		}
	}

	public int OwnerID
	{
		get
		{
			return m_OwnerID;
		}
		set
		{
			m_OwnerID = value;
		}
	}

	public int ItemDropId
	{
		get
		{
			return m_ItemDropId;
		}
		set
		{
			m_ItemDropId = value;
		}
	}

	public bool IsPlayer => m_Identity == EIdentity.Player;

	public bool IsNpc => m_Identity == EIdentity.Npc;

	public bool isPlayerOrNpc => m_Identity == EIdentity.Player || m_Identity == EIdentity.Npc;

	public bool invincible { get; set; }

	public PeSex sex { get; set; }

	public EntityProto entityProto => base.Entity.entityProto;

	public object userData { get; set; }

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		invincible = r.ReadBoolean();
		sex = (PeSex)r.ReadInt32();
		m_OwnerID = r.ReadInt32();
		m_Race = (ERace)r.ReadInt32();
		m_Identity = (EIdentity)r.ReadInt32();
		m_ItemDropId = r.ReadInt32();
		if (base.Entity.version < 2)
		{
			int num = r.ReadInt32();
			int protoId = r.ReadInt32();
			if (num != -1)
			{
				base.Entity.entityProto = new EntityProto
				{
					proto = (EEntityProto)num,
					protoId = protoId
				};
			}
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(invincible);
		w.Write((int)sex);
		w.Write(m_OwnerID);
		w.Write((int)m_Race);
		w.Write((int)m_Identity);
		w.Write(m_ItemDropId);
	}

	public override void Start()
	{
		base.Start();
		m_Animator = GetComponent<AnimatorCmpt>();
		if (m_Animator != null)
		{
			m_Animator.SetInteger("Owner", OwnerID);
			m_Animator.SetInteger("Sex", (int)sex);
		}
	}
}
