using System;
using Pathea;
using PeMap;
using UnityEngine;

public class AllianceItem_N : MonoBehaviour
{
	[SerializeField]
	private UISprite m_IconSpr;

	[SerializeField]
	private UILabel m_AllianceNameLbl;

	[SerializeField]
	private UISprite m_SurplusSpr;

	[SerializeField]
	private UILabel m_SurplusCountLbl;

	[SerializeField]
	private UISprite m_DestorySpr;

	[SerializeField]
	private UILabel m_DestoryCountLbl;

	[SerializeField]
	private UISlider m_ReputationSlider;

	[SerializeField]
	private UILabel m_ReputationNumLbl;

	[SerializeField]
	private UILabel m_ReputationLvLbl;

	[SerializeField]
	private UIButton m_FightBtn;

	private int m_AllianceID;

	private int m_PlayerID;

	private int m_MainPlayerID;

	private UIPanel m_CurPanel;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(m_FightBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			PeSingleton<ReputationSystem>.Instance.TryChangeBelligerencyState(m_MainPlayerID, m_PlayerID, state: true);
		});
	}

	private void UpdateAllyIcon()
	{
		string spriteName = "Null";
		AllyType allyType = VATownGenerator.Instance.GetAllyType(m_AllianceID);
		int allyNum = VATownGenerator.Instance.GetAllyNum(m_AllianceID);
		if (allyNum >= 0)
		{
			switch (allyType)
			{
			case AllyType.Player:
			case AllyType.Npc:
				if (allyNum < AllyIcon.HummanIcon.Length)
				{
					spriteName = AllyIcon.HummanIcon[allyNum];
				}
				break;
			case AllyType.Puja:
				if (allyNum < AllyIcon.PujaIcon.Length)
				{
					spriteName = AllyIcon.PujaIcon[allyNum];
				}
				break;
			case AllyType.Paja:
				if (allyNum < AllyIcon.PajaIcon.Length)
				{
					spriteName = AllyIcon.PajaIcon[allyNum];
				}
				break;
			}
		}
		m_IconSpr.spriteName = spriteName;
		m_IconSpr.MakePixelPerfect();
	}

	private void UpdateSurplusBulidIcon()
	{
		AllyType allyType = VATownGenerator.Instance.GetAllyType(m_AllianceID);
		int iconID = -1;
		switch (allyType)
		{
		case AllyType.Player:
		case AllyType.Npc:
			iconID = 27;
			break;
		case AllyType.Puja:
			iconID = 19;
			break;
		case AllyType.Paja:
			iconID = 22;
			break;
		}
		if (iconID != -1)
		{
			MapIcon mapIcon = PeSingleton<MapIcon.Mgr>.Instance.iconList.Find((MapIcon itr) => itr.id == iconID);
			m_SurplusSpr.spriteName = ((mapIcon == null) ? "Null" : mapIcon.iconName);
			m_SurplusSpr.MakePixelPerfect();
		}
	}

	private void UpdateDestoryBulidIcon()
	{
		AllyType allyType = VATownGenerator.Instance.GetAllyType(m_AllianceID);
		int iconID = -1;
		switch (allyType)
		{
		case AllyType.Player:
		case AllyType.Npc:
			iconID = 46;
			break;
		case AllyType.Puja:
			iconID = 48;
			break;
		case AllyType.Paja:
			iconID = 47;
			break;
		}
		if (iconID != -1)
		{
			MapIcon mapIcon = PeSingleton<MapIcon.Mgr>.Instance.iconList.Find((MapIcon itr) => itr.id == iconID);
			m_DestorySpr.spriteName = ((mapIcon == null) ? "Null" : mapIcon.iconName);
			m_DestorySpr.MakePixelPerfect();
		}
	}

	private void UpdateIconCol()
	{
		int allyColor = VATownGenerator.Instance.GetAllyColor(m_AllianceID);
		if (allyColor >= 0 && allyColor < AllyColor.AllianceCols.Length)
		{
			Color32 color = AllyColor.AllianceCols[allyColor];
			Color color2 = new Color((float)(int)color.r / 255f, (float)(int)color.g / 255f, (float)(int)color.b / 255f, (float)(int)color.a / 255f);
			m_IconSpr.color = color2;
			m_SurplusSpr.color = color2;
			m_DestorySpr.color = color2;
		}
	}

	private void UpdateName()
	{
		int allyName = VATownGenerator.Instance.GetAllyName(m_AllianceID);
		m_AllianceNameLbl.text = PELocalization.GetString(allyName);
		m_AllianceNameLbl.MakePixelPerfect();
	}

	private void UpdateReputationProgress(float progress)
	{
		m_ReputationSlider.sliderValue = Mathf.Clamp01(progress);
	}

	private void UpdateReputationNum(float number)
	{
		m_ReputationNumLbl.text = number.ToString();
		m_ReputationNumLbl.MakePixelPerfect();
	}

	private void UpdateReputationLv(ReputationSystem.ReputationLevel level)
	{
		int num = -1;
		switch (level)
		{
		case ReputationSystem.ReputationLevel.Fear:
			num = 8000704;
			break;
		case ReputationSystem.ReputationLevel.Hatred:
			num = 8000705;
			break;
		case ReputationSystem.ReputationLevel.Animosity:
			num = 8000706;
			break;
		case ReputationSystem.ReputationLevel.Cold:
			num = 8000707;
			break;
		case ReputationSystem.ReputationLevel.Neutral:
			num = 8000708;
			break;
		case ReputationSystem.ReputationLevel.Cordial:
			num = 8000709;
			break;
		case ReputationSystem.ReputationLevel.Amity:
			num = 8000710;
			break;
		case ReputationSystem.ReputationLevel.Respectful:
			num = 8000711;
			break;
		case ReputationSystem.ReputationLevel.Reverence:
			num = 8000712;
			break;
		case ReputationSystem.ReputationLevel.MAX:
			return;
		}
		if (num != -1)
		{
			m_ReputationLvLbl.text = PELocalization.GetString(num);
			m_ReputationLvLbl.MakePixelPerfect();
		}
	}

	private void UpdateFightBtnState()
	{
		m_FightBtn.isEnabled = !PeSingleton<ReputationSystem>.Instance.GetBelligerency(m_MainPlayerID, m_PlayerID);
	}

	private void GetPanelCmpt()
	{
		UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		if (null != uIDraggablePanel)
		{
			m_CurPanel = uIDraggablePanel.GetComponent<UIPanel>();
			uIDraggablePanel.onDragFinished = (UIDraggablePanel.OnDragFinished)Delegate.Combine(uIDraggablePanel.onDragFinished, new UIDraggablePanel.OnDragFinished(OnDragFinishEvent));
		}
	}

	private void OnDragFinishEvent()
	{
		if (null != m_CurPanel)
		{
			bool flag = m_CurPanel.IsVisible(base.transform.position);
			BoxCollider[] componentsInChildren = base.transform.GetComponentsInChildren<BoxCollider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = flag;
			}
		}
	}

	public void UpdateInfo(int allyId, int playerID, int mainPlayerID)
	{
		m_AllianceID = allyId;
		m_PlayerID = playerID;
		m_MainPlayerID = mainPlayerID;
		UpdateName();
		UpdateBuildCount();
		UpdateReputation();
		UpdateIconCol();
		UpdateIconCol();
		UpdateAllyIcon();
		UpdateSurplusBulidIcon();
		UpdateDestoryBulidIcon();
		GetPanelCmpt();
	}

	public void UpdateBuildCount()
	{
		int allyTownCount = VATownGenerator.Instance.GetAllyTownCount(m_AllianceID);
		int allyTownDestroyedCount = VATownGenerator.Instance.GetAllyTownDestroyedCount(m_AllianceID);
		m_SurplusCountLbl.text = (allyTownCount - allyTownDestroyedCount).ToString();
		m_SurplusCountLbl.MakePixelPerfect();
		m_DestoryCountLbl.text = allyTownDestroyedCount.ToString();
		m_DestoryCountLbl.MakePixelPerfect();
	}

	public void UpdateReputation()
	{
		int showReputationValue = PeSingleton<ReputationSystem>.Instance.GetShowReputationValue(m_MainPlayerID, m_PlayerID);
		int showLevelThreshold = PeSingleton<ReputationSystem>.Instance.GetShowLevelThreshold(m_MainPlayerID, m_PlayerID);
		ReputationSystem.ReputationLevel showLevel = PeSingleton<ReputationSystem>.Instance.GetShowLevel(m_MainPlayerID, m_PlayerID);
		UpdateReputationProgress((float)showReputationValue / (float)showLevelThreshold);
		UpdateReputationNum(showReputationValue);
		UpdateReputationLv(showLevel);
		UpdateFightBtnState();
	}

	public void Reset()
	{
		m_AllianceID = -1;
		m_PlayerID = -1;
		m_IconSpr.spriteName = "Null";
		m_SurplusSpr.spriteName = "Null";
		m_DestorySpr.spriteName = "Null";
		m_AllianceNameLbl.text = string.Empty;
		m_SurplusCountLbl.text = string.Empty;
		m_DestoryCountLbl.text = string.Empty;
		m_ReputationSlider.sliderValue = 0f;
		m_ReputationNumLbl.text = string.Empty;
		m_ReputationLvLbl.text = string.Empty;
	}
}
