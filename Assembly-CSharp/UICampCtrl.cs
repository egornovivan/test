using System;
using Pathea;
using UnityEngine;

public class UICampCtrl : UIBaseWidget
{
	[Serializable]
	public class Left_Camp
	{
		public UILabel CampName;

		public UILabel Relationship;

		public UILabel Introduction;

		public UILabel Reputation;

		public UISlider mSlider;

		public ReputationSystem.ReputationLevel mLevel;

		public int CurrentReputation;

		public int MaxReputation;

		public RelationInfo mRelationInfo;

		public GameObject warState;

		public GameObject peaceState;

		public GameObject trade;

		public GameObject specialMission;

		public GameObject normalMission;

		public UIGrid mGrid;
	}

	[Serializable]
	public class Right_Camp
	{
		public UILabel CampName;

		public UILabel Relationship;

		public UILabel Introduction;

		public UILabel Reputation;

		public UISlider mSlider;

		public ReputationSystem.ReputationLevel mLevel;

		public int CurrentReputation;

		public int MaxReputation;

		public RelationInfo mRelationInfo;

		public GameObject warState;

		public GameObject peaceState;

		public GameObject trade;

		public GameObject specialMission;

		public GameObject normalMission;

		public UIGrid mGrid;
	}

	private ReputationSystem RS;

	private int MainPlayerId;

	private bool hasrs;

	private bool hasid;

	public UIGrid m_Root;

	public GameObject m_CampPrefab;

	[SerializeField]
	private Left_Camp m_Left;

	[SerializeField]
	private Right_Camp m_Right;

	private float mTimer;

	public override void Show()
	{
		base.Show();
	}

	private void TryGEtMainPlayerId()
	{
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer == null))
		{
			MainPlayerId = (int)PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID);
			hasid = true;
		}
	}

	private void TryGetReputationsys()
	{
		if (PeSingleton<ReputationSystem>.Instance != null)
		{
			RS = PeSingleton<ReputationSystem>.Instance;
			hasrs = true;
		}
	}

	private void Update()
	{
		if (!hasid)
		{
			TryGEtMainPlayerId();
		}
		if (!hasrs)
		{
			TryGetReputationsys();
		}
		if (hasid && hasrs)
		{
			UpdateRelationship();
			UpdateReputation();
			UpdateRelationInfo();
		}
	}

	private void UpdateRelationship()
	{
		UpdatePujaRelationship();
		UpdatePajaRelationship();
	}

	private void UpdatePujaRelationship()
	{
		m_Left.mLevel = RS.GetShowLevel(MainPlayerId, 5);
		m_Left.Relationship.text = "Relationship:" + m_Left.mLevel;
	}

	private void UpdatePajaRelationship()
	{
		m_Right.mLevel = RS.GetShowLevel(MainPlayerId, 6);
		m_Right.Relationship.text = "Relationship:" + m_Right.mLevel;
	}

	private void UpdateReputation()
	{
		UpdatePujaReputation();
		UpdatePajaReputation();
	}

	private void UpdatePujaReputation()
	{
		m_Left.CurrentReputation = RS.GetShowReputationValue(MainPlayerId, 5);
		m_Left.MaxReputation = RS.GetShowLevelThreshold(MainPlayerId, 5);
		m_Left.Reputation.text = m_Left.CurrentReputation + "/" + m_Left.MaxReputation;
		m_Left.mSlider.sliderValue = (float)m_Left.CurrentReputation / (float)m_Left.MaxReputation;
	}

	private void UpdatePajaReputation()
	{
		m_Right.CurrentReputation = RS.GetShowReputationValue(MainPlayerId, 6);
		m_Right.MaxReputation = RS.GetShowLevelThreshold(MainPlayerId, 6);
		m_Right.Reputation.text = m_Right.CurrentReputation + "/" + m_Right.MaxReputation;
		m_Right.mSlider.sliderValue = (float)m_Right.CurrentReputation / (float)m_Right.MaxReputation;
	}

	private void UpdateRelationInfo()
	{
		UpdatePujaRelationInfo();
		UpdatePajaRelationInfo();
	}

	private void UpdatePujaRelationInfo()
	{
		m_Left.mRelationInfo = RelationInfo.GetData(m_Left.mLevel);
	}

	private void UpdatePajaRelationInfo()
	{
		m_Right.mRelationInfo = RelationInfo.GetData(m_Right.mLevel);
	}

	private void LateUpdate()
	{
		UpdateHint();
		mTimer += Time.deltaTime;
		if (mTimer >= 0.5f)
		{
			mTimer = 0f;
			RepositionUIGrid();
		}
	}

	private void UpdateHint()
	{
		UpdatePujaHint();
		UpdatePajaHint();
	}

	private void UpdatePujaHint()
	{
		if (m_Left.mRelationInfo != null)
		{
			m_Left.warState.SetActive(m_Left.mRelationInfo.warState);
			m_Left.peaceState.SetActive(!m_Left.mRelationInfo.warState);
			m_Left.trade.SetActive(m_Left.mRelationInfo.canUseShop);
			m_Left.normalMission.SetActive(m_Left.mRelationInfo.normalMission);
			m_Left.specialMission.SetActive(m_Left.mRelationInfo.specialMission);
		}
	}

	private void UpdatePajaHint()
	{
		if (m_Right.mRelationInfo != null)
		{
			m_Right.warState.SetActive(m_Right.mRelationInfo.warState);
			m_Right.peaceState.SetActive(!m_Right.mRelationInfo.warState);
			m_Right.trade.SetActive(m_Right.mRelationInfo.canUseShop);
			m_Right.normalMission.SetActive(m_Right.mRelationInfo.normalMission);
			m_Right.specialMission.SetActive(m_Right.mRelationInfo.specialMission);
		}
	}

	private void RepositionUIGrid()
	{
		m_Left.mGrid.Reposition();
		m_Right.mGrid.Reposition();
	}

	private void FireToPuja()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000171), OKToPuja);
	}

	private void OKToPuja()
	{
		RS.TryChangeBelligerencyState(MainPlayerId, 5, state: true);
	}

	private void FireToPaja()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000171), OKToPaja);
	}

	private void OKToPaja()
	{
		RS.TryChangeBelligerencyState(MainPlayerId, 6, state: true);
	}
}
