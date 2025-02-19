using UnityEngine;

public class CSUI_NPCFollower : MonoBehaviour
{
	[SerializeField]
	private UIGrid m_HeroItemRootUI;

	[SerializeField]
	private CSUI_HeroItem HeroItemUIPrefab;

	private CSUI_HeroItem[] m_HeroItems;

	private CSPersonnel m_RefNpc;

	private bool m_Active = true;

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
			if (m_HeroItems != null)
			{
				for (int i = 0; i < m_HeroItems.Length; i++)
				{
					m_HeroItems[i].HandlePerson = m_RefNpc;
				}
			}
		}
	}

	public void Activate(bool active)
	{
		if (m_Active != active)
		{
			m_Active = active;
			_activate();
		}
		else
		{
			m_Active = active;
		}
	}

	private void _activate()
	{
		if (m_HeroItems != null)
		{
			for (int i = 0; i < m_HeroItems.Length; i++)
			{
				m_HeroItems[i].Activate(m_Active);
			}
		}
	}

	public void Init()
	{
	}

	private void Awake()
	{
		m_HeroItems = new CSUI_HeroItem[2];
		for (int i = 0; i < m_HeroItems.Length; i++)
		{
			m_HeroItems[i] = Object.Instantiate(HeroItemUIPrefab);
			m_HeroItems[i].transform.parent = m_HeroItemRootUI.transform;
			CSUtils.ResetLoacalTransform(m_HeroItems[i].transform);
			m_HeroItems[i].m_HeroIndex = i;
			m_HeroItems[i].HandlePerson = m_RefNpc;
		}
		m_HeroItemRootUI.repositionNow = true;
	}

	private void Start()
	{
		_activate();
	}

	private void Update()
	{
	}

	private void OnPopupListClick()
	{
		if (!m_Active)
		{
			CSUI_StatusBar.ShowText(UIMsgBoxInfo.mCantHandlePersonnel.GetString(), Color.red, 5.5f);
		}
	}
}
