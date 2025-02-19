using Pathea;
using UnityEngine;

public class CSUI_NpcGridItem : MonoBehaviour
{
	public delegate void NpcGridEvent(CSUI_NpcGridItem plantGrid);

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
	private UILabel m_StateLabel;

	[SerializeField]
	private UIButton m_DeleteButton;

	[SerializeField]
	private UILabel m_TimeLabel;

	public UILabel m_NpcNameLabel;

	public bool m_UseDeletebutton = true;

	public CSPersonnel m_Npc;

	private NpcCmpt m_NpcCmpt;

	private int lastState;

	public NpcGridEvent OnDestroySelf;

	private bool m_Flag;

	private int _minute;

	private int _second;

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

	private void OnActivate(bool active)
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
		if ((m_Npc == null || active) && m_Npc != null)
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
		ShowNpcName();
		if (m_Npc != null && (bool)m_Npc.m_Npc)
		{
			m_EmptyIcon.enabled = false;
			if (m_Npc.RandomNpcFace != null)
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
				switch (m_Npc.State)
				{
				case 1:
					m_PrepareStateSprite.enabled = true;
					m_StateLabel.text = "Going...";
					break;
				case 2:
					m_IdleStateSprite.enabled = true;
					break;
				case 3:
					m_RestStateSprite.enabled = true;
					break;
				case 4:
					m_WorkStateSprite.enabled = true;
					break;
				case 5:
					m_FollowStateSprite.enabled = true;
					break;
				case 6:
					m_DeadStateSprite.enabled = true;
					break;
				case 7:
					m_AttackStateSprite.enabled = true;
					break;
				case 8:
					m_PatrolStateSprite.enabled = true;
					break;
				case 9:
					m_PlantStateSprite.enabled = true;
					break;
				case 10:
					m_WateringStateSprite.enabled = true;
					break;
				case 11:
					m_CleaningStateSprite.enabled = true;
					break;
				case 12:
					m_GainStateSprite.enabled = true;
					break;
				}
				lastState = m_Npc.State;
			}
		}
		else
		{
			SetAllStateFalse();
			CloseDiseaseState();
			m_EmptyIcon.enabled = true;
			m_IconTex.enabled = false;
			m_IconSprite.enabled = false;
			m_IconTex.mainTexture = null;
			m_Flag = false;
		}
	}

	private void CloseDiseaseState()
	{
		m_NpcCmpt = null;
		m_SickSprite.enabled = false;
	}

	public void ShowNpcName()
	{
		if (m_Npc != null && m_NpcNameLabel != null && m_NpcNameLabel.enabled)
		{
			m_NpcNameLabel.text = m_Npc.FullName;
		}
	}

	public void NpcGridTimeShow(float _time)
	{
		if (m_TimeLabel != null)
		{
			_minute = (int)(_time / 60f);
			_second = (int)(_time - (float)(_minute * 60));
			m_TimeLabel.text = TimeTransition(_minute) + ":" + TimeTransition(_second);
		}
	}

	private string TimeTransition(int _number)
	{
		if (_number < 10)
		{
			return "0" + _number;
		}
		return _number.ToString();
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
		m_StateLabel.text = string.Empty;
	}

	private void OnTooltip(bool show)
	{
		if (!show)
		{
			ToolTipsMgr.ShowText(null);
		}
		else if (m_Npc != null)
		{
			ToolTipsMgr.ShowText(m_Npc.FullName);
		}
	}
}
