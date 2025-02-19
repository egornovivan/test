using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class CSUI_HeroItem : MonoBehaviour
{
	[SerializeField]
	private UILabel m_NameLbUI;

	[SerializeField]
	private UIButton m_BtnUI;

	[SerializeField]
	private UILabel m_BtnLbUI;

	[SerializeField]
	private ServantShowItem_N m_ServantShowingUI;

	public int m_HeroIndex;

	public CSPersonnel HandlePerson;

	private PeEntity m_CurNpc;

	private bool m_Cancel;

	private bool m_Replace;

	private bool m_Runing;

	private bool m_Active
	{
		get
		{
			if (m_Cancel)
			{
				return m_Runing && null != m_CurNpc && !m_CurNpc.IsDead();
			}
			if (m_Replace)
			{
				return m_Runing && null != m_CurNpc && !m_CurNpc.IsDead() && HandlePerson != null && null != HandlePerson.m_Npc && !HandlePerson.m_Npc.IsDead();
			}
			return m_Runing && null == m_CurNpc && HandlePerson != null && null != HandlePerson.m_Npc && !HandlePerson.m_Npc.IsDead();
		}
	}

	public void Activate(bool running)
	{
		m_Runing = running;
	}

	private void Start()
	{
		m_NameLbUI.text = string.Empty;
		m_BtnLbUI.text = "Set";
		m_BtnUI.isEnabled = false;
	}

	private void Update()
	{
		NpcCmpt servant = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>().GetServant(m_HeroIndex);
		if (servant != null)
		{
			PeEntity entity = servant.Entity;
			if (entity != m_CurNpc)
			{
				m_NameLbUI.text = servant.Entity.enityInfoCmpt.characterName.ToString();
				m_ServantShowingUI.SetNpc(entity);
				m_CurNpc = entity;
			}
			m_Cancel = false;
			m_Replace = false;
			if (HandlePerson != null)
			{
				if (HandlePerson.m_Npc == m_CurNpc)
				{
					m_BtnLbUI.text = "Cancel";
					m_Cancel = true;
					m_Replace = false;
					m_BtnUI.isEnabled = m_Active;
				}
				else if (HandlePerson.NPC.IsFollower())
				{
					m_BtnUI.isEnabled = false;
					m_BtnLbUI.text = "Replace";
				}
				else
				{
					m_BtnLbUI.text = "Replace";
					m_Replace = true;
					m_BtnUI.isEnabled = m_Active;
				}
			}
			else
			{
				m_BtnUI.isEnabled = m_Active;
			}
			return;
		}
		m_CurNpc = null;
		m_NameLbUI.text = string.Empty;
		m_ServantShowingUI.SetNpc(null);
		m_BtnLbUI.text = "Set";
		m_Cancel = false;
		if (HandlePerson != null)
		{
			if (HandlePerson.NPC.IsFollower())
			{
				m_BtnUI.isEnabled = false;
			}
			else
			{
				m_BtnUI.isEnabled = m_Active;
			}
		}
	}

	private void OnSetBtn()
	{
		if (!m_Cancel)
		{
			if (HandlePerson == null)
			{
				return;
			}
			if (m_Replace)
			{
				if (PeGameMgr.IsMulti)
				{
					if (m_CurNpc != null)
					{
						ServantLeaderCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
						NpcCmpt component = m_CurNpc.GetComponent<NpcCmpt>();
						if (cmpt == component.Master && null != PlayerNetwork.mainPlayer)
						{
							PlayerNetwork.RequestDismissNpc(m_CurNpc.Id);
							PlayerNetwork.RequestNpcRecruit(HandlePerson.ID);
						}
					}
				}
				else
				{
					if (m_CurNpc != null)
					{
						m_CurNpc.SetFollower(bFlag: false);
					}
					HandlePerson.FollowMe(m_HeroIndex);
				}
			}
			else if (PeGameMgr.IsMulti)
			{
				if (null != PlayerNetwork.mainPlayer)
				{
					PlayerNetwork.RequestNpcRecruit(HandlePerson.ID);
				}
			}
			else
			{
				HandlePerson.FollowMe(m_HeroIndex);
			}
		}
		else
		{
			if (!(m_CurNpc != null))
			{
				return;
			}
			if (PeGameMgr.IsMulti)
			{
				if (null != PlayerNetwork.mainPlayer)
				{
					PlayerNetwork.RequestDismissNpc(m_CurNpc.Id);
				}
			}
			else
			{
				m_CurNpc.SetFollower(bFlag: false);
			}
		}
	}
}
