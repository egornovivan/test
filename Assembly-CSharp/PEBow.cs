using ItemAsset;
using Pathea;
using PETools;
using SkillSystem;
using UnityEngine;

public class PEBow : PEAimAbleEquip, IWeapon, IAimWeapon
{
	private const string m_ArrowBagBone = "Bow_box";

	private const string m_ArrowFinger = "Bip01 R Finger21";

	public AttackMode[] m_AttackMode;

	public int[] m_CostItemID;

	public int[] m_SkillID;

	public string[] m_Idles;

	private int m_CurIndex;

	public string m_ReloadAnim = "BowReload";

	public Transform m_ArrowBagTrans;

	public Transform m_LineBone;

	private Vector3 m_LineBoneDefaultPos;

	public GameObject[] m_ArrowModel;

	private Arrow m_ItemAmmoAttr;

	private Animator m_Anim;

	private GameObject m_ShowArrow;

	private Transform m_FingerBone;

	private float m_AimTime = 0.3f;

	private float m_StartTime;

	private bool m_BowOpen;

	public int curItemIndex
	{
		get
		{
			return m_CurIndex;
		}
		set
		{
			m_CurIndex = Mathf.Min(value, m_CostItemID.Length - 1);
			if (m_ItemAmmoAttr != null)
			{
				m_ItemAmmoAttr.index = m_CurIndex;
			}
		}
	}

	public int curItemID => m_CostItemID[curItemIndex];

	public int skillID => m_SkillID[curItemIndex];

	public ItemObject ItemObj => m_ItemObj;

	public bool HoldReady => m_MotionMgr.GetMaskState(m_HandChangeAttr.m_HoldActionMask);

	public bool UnHoldReady => !m_MotionMgr.IsActionRunning(m_HandChangeAttr.m_ActiveActionType);

	public bool Aimed
	{
		get
		{
			if (null != m_IKCmpt)
			{
				return m_IKCmpt.aimed;
			}
			return false;
		}
	}

	public string[] leisures => m_Idles;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Anim = GetComponentInChildren<Animator>();
		m_MotionEquip = m_Entity.GetCmpt<Motion_Equip>();
		m_ItemAmmoAttr = itemObj.GetCmpt<Arrow>();
		if (m_ItemAmmoAttr != null)
		{
			m_CurIndex = m_ItemAmmoAttr.index;
		}
		if (null != m_ArrowBagTrans)
		{
			m_ArrowBagTrans.gameObject.SetActive(value: true);
			m_View.AttachObject(m_ArrowBagTrans.gameObject, "Bow_box");
			m_ArrowBagTrans.localPosition = Vector3.zero;
			m_ArrowBagTrans.localRotation = Quaternion.identity;
			m_ArrowBagTrans.localScale = Vector3.one;
		}
		m_FingerBone = PEUtil.GetChild(m_View.modelTrans, "Bip01 R Finger21");
		if (null != m_LineBone)
		{
			m_LineBoneDefaultPos = m_LineBone.localPosition;
		}
		if (m_ArrowModel.Length != m_CostItemID.Length || m_SkillID.Length != m_CostItemID.Length)
		{
			Debug.LogError("ArrowModelNum, ItemNum, SkillNum not match");
		}
		m_StartTime = 0f;
		m_BowOpen = false;
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (null != m_ArrowBagTrans)
		{
			m_View.DetachObject(m_ArrowBagTrans.gameObject);
			Object.Destroy(m_ArrowBagTrans.gameObject);
		}
		if (null != m_ShowArrow)
		{
			m_View.DetachObject(m_ShowArrow);
			Object.Destroy(m_ShowArrow.gameObject);
		}
	}

	public override void ResetView()
	{
		base.ResetView();
		if (null != m_ShowArrow)
		{
			Object.Destroy(m_ShowArrow);
		}
	}

	public void OnShoot()
	{
		if (null != m_Anim)
		{
			m_Anim.SetTrigger("Shoot");
		}
	}

	public void SetBowOpenState(bool openBow)
	{
		if (null != m_Anim)
		{
			m_Anim.SetBool("Open", openBow);
		}
		m_BowOpen = openBow;
	}

	public void SetArrowShowState(bool show)
	{
		if (m_ArrowModel == null || m_ArrowModel.Length <= 0)
		{
			return;
		}
		if (null != m_ShowArrow)
		{
			m_View.DetachObject(m_ShowArrow);
			Object.Destroy(m_ShowArrow.gameObject);
		}
		if (show && null != m_ArrowModel[curItemIndex])
		{
			m_ShowArrow = Object.Instantiate(m_ArrowModel[curItemIndex]);
			if (null != m_ShowArrow)
			{
				m_View.AttachObject(m_ShowArrow, "Bip01 R Finger21");
				m_StartTime = Time.time;
			}
		}
	}

	private void LateUpdate()
	{
		if (null != m_ShowArrow)
		{
			m_ShowArrow.transform.rotation = Quaternion.LookRotation(Vector3.Slerp(m_AimAttr.m_AimTrans.right, (m_AimAttr.m_AimTrans.position - m_ShowArrow.transform.position).normalized, Mathf.Clamp01((Time.time - m_StartTime) / m_AimTime)));
		}
		if (null != m_LineBone)
		{
			if (m_BowOpen && null != m_FingerBone)
			{
				m_LineBone.position = m_FingerBone.position;
			}
			else
			{
				m_LineBone.localPosition = m_LineBoneDefaultPos;
			}
		}
	}

	public void HoldWeapon(bool hold)
	{
		m_MotionEquip.ActiveWeapon(this, hold);
	}

	public AttackMode[] GetAttackMode()
	{
		return m_AttackMode;
	}

	public bool CanAttack(int index = 0)
	{
		return m_MotionMgr.CanDoAction(PEActionType.BowShoot);
	}

	public void Attack(int index = 0, SkEntity targetEntity = null)
	{
		if (null != m_MotionEquip)
		{
			m_MotionEquip.SetTarget(targetEntity);
		}
		if (null != m_MotionMgr)
		{
			m_MotionMgr.DoAction(PEActionType.BowShoot);
		}
		if (m_AttackMode != null && m_AttackMode.Length > index)
		{
			m_AttackMode[index].ResetCD();
		}
	}

	public bool AttackEnd(int index = 0)
	{
		if (null != m_MotionMgr)
		{
			return !m_MotionMgr.IsActionRunning(PEActionType.BowShoot) && !m_MotionMgr.IsActionRunning(PEActionType.BowReload);
		}
		return true;
	}

	public virtual bool IsInCD(int index = 0)
	{
		if (m_AttackMode != null && m_AttackMode.Length > index)
		{
			return m_AttackMode[index].IsInCD();
		}
		return false;
	}

	public virtual void SetAimState(bool aimState)
	{
		if (aimState)
		{
			m_MotionMgr.ContinueAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.BowShoot);
		}
		else
		{
			m_MotionMgr.PauseAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.BowShoot);
		}
	}

	public void SetTarget(Vector3 aimPos)
	{
		if (null != m_IKCmpt)
		{
			m_IKCmpt.aimTargetPos = aimPos;
		}
	}

	public void SetTarget(Transform trans)
	{
		if (null != m_IKCmpt)
		{
			m_IKCmpt.aimTargetTrans = trans;
		}
	}
}
