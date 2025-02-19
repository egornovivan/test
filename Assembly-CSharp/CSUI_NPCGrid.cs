using Pathea;
using UnityEngine;

public class CSUI_NPCGrid : MonoBehaviour
{
	public delegate void NpcGridEvent(CSUI_NPCGrid plantGrid);

	public delegate void ClickDel(CSPersonnel csp);

	[SerializeField]
	private UISlicedSprite m_CheckMark;

	[SerializeField]
	private UITexture m_IconTex;

	[SerializeField]
	private UISlicedSprite m_IconSprite;

	[SerializeField]
	private UISlicedSprite m_EmptyIcon;

	[SerializeField]
	private UISlicedSprite m_WorkStateSprite;

	[SerializeField]
	private UISlicedSprite m_RestStateSprite;

	[SerializeField]
	private UISlicedSprite m_IdleStateSprite;

	[SerializeField]
	private UISlicedSprite m_FollowStateSprite;

	[SerializeField]
	private UISlicedSprite m_PrepareStateSprite;

	[SerializeField]
	private UISlicedSprite m_DeadStateSprite;

	[SerializeField]
	private UISlicedSprite m_AttackStateSprite;

	[SerializeField]
	private UISlicedSprite m_PatrolStateSprite;

	[SerializeField]
	private UISlicedSprite m_PlantStateSprite;

	[SerializeField]
	private UISlicedSprite m_WateringStateSprite;

	[SerializeField]
	private UISlicedSprite m_CleaningStateSprite;

	[SerializeField]
	private UISlicedSprite m_GainStateSprite;

	[SerializeField]
	private UISlicedSprite m_SickSprite;

	[SerializeField]
	private ShowToolTipItem_N m_ShowToolTipItem;

	[SerializeField]
	private UIButton m_DeleteButton;

	public bool m_UseDeletebutton = true;

	public CSPersonnel m_Npc;

	private NpcCmpt m_NpcCmpt;

	private int lastState;

	public NpcGridEvent OnDestroySelf;

	public Transform NpcIconRadio
	{
		get
		{
			return base.gameObject.GetComponent<UICheckbox>().radioButtonRoot;
		}
		set
		{
			base.gameObject.GetComponent<UICheckbox>().radioButtonRoot = value;
		}
	}

	public event ClickDel GetInstructorSkill;

	public event ClickDel ShowFace;

	public event ClickDel ShowInstructorSkill;

	public void OnActivate(bool active)
	{
		if (m_UseDeletebutton && m_Npc != null)
		{
			m_DeleteButton.gameObject.SetActive(active);
		}
		else
		{
			m_DeleteButton.gameObject.SetActive(value: false);
		}
		if (active)
		{
			m_CheckMark.color = new Color(1f, 1f, 1f, 0f);
		}
		else
		{
			m_CheckMark.color = new Color(1f, 1f, 1f, 1f);
		}
		if (active && m_Npc != null)
		{
			if (!m_Npc.IsRandomNpc || GameUI.Instance != null)
			{
			}
			if (m_Npc != null && this.ShowFace != null)
			{
				this.ShowFace(m_Npc);
			}
			if (m_Npc != null && this.GetInstructorSkill != null)
			{
				this.GetInstructorSkill(m_Npc);
			}
			if (m_Npc != null && this.ShowInstructorSkill != null)
			{
				this.ShowInstructorSkill(m_Npc);
			}
		}
	}

	private void OnDeleteBtn()
	{
		if (OnDestroySelf != null)
		{
			OnDestroySelf(this);
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
		SetAllStateFalse();
	}

	private void Update()
	{
		if (m_Npc != null && m_Npc.m_Npc != null)
		{
			m_EmptyIcon.enabled = false;
			if (m_Npc.IsRandomNpc)
			{
				m_IconTex.mainTexture = m_Npc.RandomNpcFace;
				m_IconSprite.enabled = false;
				m_IconTex.enabled = true;
			}
			else
			{
				m_IconSprite.enabled = true;
				m_IconTex.enabled = false;
				m_IconSprite.spriteName = m_Npc.MainNpcFace;
			}
			if (m_NpcCmpt == null)
			{
				m_NpcCmpt = m_Npc.m_Npc.GetCmpt<NpcCmpt>();
			}
			else
			{
				m_SickSprite.enabled = m_NpcCmpt.IsNeedMedicine;
			}
			if (lastState != m_Npc.State)
			{
				SetAllStateFalse();
				int num = -1;
				switch (m_Npc.State)
				{
				case 1:
					m_PrepareStateSprite.enabled = true;
					num = 8000575;
					break;
				case 2:
					m_IdleStateSprite.enabled = true;
					num = 8000576;
					break;
				case 3:
					m_RestStateSprite.enabled = true;
					num = 8000577;
					break;
				case 4:
					m_WorkStateSprite.enabled = true;
					num = 8000578;
					break;
				case 5:
					m_FollowStateSprite.enabled = true;
					num = 8000579;
					break;
				case 6:
					m_DeadStateSprite.enabled = true;
					num = 8000580;
					break;
				case 7:
					m_AttackStateSprite.enabled = true;
					num = 8000581;
					break;
				case 8:
					m_PatrolStateSprite.enabled = true;
					num = 8000582;
					break;
				case 9:
					m_PlantStateSprite.enabled = true;
					num = 8000583;
					break;
				case 10:
					m_WateringStateSprite.enabled = true;
					num = 8000584;
					break;
				case 11:
					m_CleaningStateSprite.enabled = true;
					num = 8000585;
					break;
				case 12:
					m_GainStateSprite.enabled = true;
					num = 8000586;
					break;
				}
				lastState = m_Npc.State;
				if (null != m_ShowToolTipItem && num != -1)
				{
					m_ShowToolTipItem.mTipContent = PELocalization.GetString(num);
				}
			}
		}
		else
		{
			SetAllStateFalse();
			m_EmptyIcon.enabled = true;
			m_IconTex.enabled = false;
			m_IconSprite.enabled = false;
			m_IconTex.mainTexture = null;
		}
	}

	private void SetAllStateFalse()
	{
		m_RestStateSprite.enabled = false;
		m_IdleStateSprite.enabled = false;
		m_WorkStateSprite.enabled = false;
		m_FollowStateSprite.enabled = false;
		m_PrepareStateSprite.enabled = false;
		m_DeadStateSprite.enabled = false;
		m_AttackStateSprite.enabled = false;
		m_PatrolStateSprite.enabled = false;
		m_PlantStateSprite.enabled = false;
		m_WateringStateSprite.enabled = false;
		m_CleaningStateSprite.enabled = false;
		m_GainStateSprite.enabled = false;
		if (null != m_ShowToolTipItem)
		{
			m_ShowToolTipItem.mTipContent = string.Empty;
		}
	}
}
