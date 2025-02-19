using System.Collections.Generic;
using UnityEngine;

public class CSUI_LeftMenuItem : MonoBehaviour
{
	[HideInInspector]
	public List<CSEntity> m_EntityList = new List<CSEntity>();

	[HideInInspector]
	public int m_Type;

	private bool m_IsSelected;

	public UISprite mSpDisabled;

	public UISprite mSpDumped;

	public UISlicedSprite mBakgroundSp;

	public UISprite mForGroundSp;

	public BoxCollider mBoxCollider;

	private string bgSprNameWhite;

	private string bgSprNameRed;

	private string fgSprNameWhite;

	private string fgSprNameRed;

	private bool m_NotHaveElectricity;

	private bool m_NotHaveAssembly;

	private bool m_AssemblyLevelInsufficient;

	private bool m_InitState;

	private int m_NotHaveAssemblyCount;

	private int m_AssemblyLevelInsufficientCount;

	private int m_NotHaveElectricityCount;

	public bool IsSelected
	{
		get
		{
			return m_IsSelected;
		}
		set
		{
			m_IsSelected = value;
			Select(m_IsSelected);
		}
	}

	[HideInInspector]
	public bool NotHaveElectricity
	{
		get
		{
			if (!m_InitState)
			{
				initInfo();
			}
			return m_NotHaveElectricity;
		}
	}

	[HideInInspector]
	public bool NotHaveAssembly
	{
		get
		{
			if (!m_InitState)
			{
				initInfo();
			}
			return m_NotHaveAssembly;
		}
	}

	[HideInInspector]
	public bool AssemblyLevelInsufficient
	{
		get
		{
			if (!m_InitState)
			{
				initInfo();
			}
			return m_AssemblyLevelInsufficient;
		}
	}

	public bool IsShow => base.gameObject.activeSelf;

	private void Start()
	{
		initInfo();
	}

	private void Update()
	{
		UpdateState();
	}

	public List<string> GetNamesByNotHaveElectricity()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < m_EntityList.Count; i++)
		{
			if (m_EntityList[i] is CSCommon { Assembly: not null } && !m_EntityList[i].IsRunning)
			{
				list.Add(CSUtils.GetEntityName(m_EntityList[i].m_Type));
			}
		}
		return list;
	}

	public List<string> GetNamesByAssemblyLevelInsufficient()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < m_EntityList.Count; i++)
		{
			if (m_EntityList[i] is CSCommon { Assembly: null } && null != CSUI_MainWndCtrl.Instance.Creator && CSUI_MainWndCtrl.Instance.Creator.Assembly != null && CSUI_MainWndCtrl.Instance.Creator.Assembly.InRange(m_EntityList[i].Position))
			{
				list.Add(CSUtils.GetEntityName(m_EntityList[i].m_Type));
			}
		}
		return list;
	}

	public List<string> GetNamesByNotHaveAssembly()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < m_EntityList.Count; i++)
		{
			if (m_EntityList[i] is CSCommon { Assembly: null })
			{
				list.Add(CSUtils.GetEntityName(m_EntityList[i].m_Type));
			}
		}
		return list;
	}

	private void initInfo()
	{
		if (!m_InitState)
		{
			UIEventListener.Get(mBoxCollider.gameObject).onClick = OnClickEvent;
			mSpDisabled.enabled = false;
			mSpDumped.enabled = false;
			bgSprNameWhite = mBakgroundSp.spriteName;
			bgSprNameRed = bgSprNameWhite + "_red";
			fgSprNameWhite = mForGroundSp.spriteName;
			fgSprNameRed = fgSprNameWhite + "_red";
			UpdateState();
			m_InitState = true;
		}
	}

	private void OnClickEvent(GameObject go)
	{
		if (!IsSelected)
		{
			IsSelected = true;
		}
	}

	private void Select(bool isSelected)
	{
		SelectSprite(isSelected);
		if (isSelected)
		{
			CSUI_MainWndCtrl.Instance.ShowWndPart(this, m_Type);
		}
		else
		{
			CSUI_MainWndCtrl.Instance.HideWndByType(m_Type);
		}
	}

	public void SelectSprite(bool isSelect)
	{
		mForGroundSp.gameObject.SetActive(isSelect);
	}

	private void UpdateState()
	{
		m_NotHaveAssemblyCount = 0;
		m_AssemblyLevelInsufficientCount = 0;
		m_NotHaveElectricityCount = 0;
		for (int i = 0; i < m_EntityList.Count; i++)
		{
			CSCommon cSCommon = m_EntityList[i] as CSCommon;
			if (cSCommon != null && cSCommon.Assembly == null)
			{
				m_NotHaveAssemblyCount++;
				if (null != CSUI_MainWndCtrl.Instance.Creator && CSUI_MainWndCtrl.Instance.Creator.Assembly != null && CSUI_MainWndCtrl.Instance.Creator.Assembly.InRange(m_EntityList[i].Position))
				{
					m_AssemblyLevelInsufficientCount++;
				}
			}
			if (cSCommon != null && cSCommon.Assembly != null && !m_EntityList[i].IsRunning)
			{
				m_NotHaveElectricityCount++;
			}
		}
		m_NotHaveAssembly = m_NotHaveAssemblyCount > 0;
		m_AssemblyLevelInsufficient = m_AssemblyLevelInsufficientCount > 0;
		m_NotHaveElectricity = m_NotHaveElectricityCount > 0;
		mSpDisabled.enabled = m_NotHaveElectricity;
		mSpDumped.enabled = m_NotHaveAssembly;
		mForGroundSp.spriteName = ((!m_NotHaveElectricity) ? fgSprNameWhite : fgSprNameRed);
		mBakgroundSp.spriteName = ((!m_NotHaveElectricity) ? bgSprNameWhite : bgSprNameRed);
	}
}
