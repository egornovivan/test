using Pathea;
using UnityEngine;

public class CSUI_TrainLearnPageCtrl : MonoBehaviour
{
	[SerializeField]
	private GameObject mLearnSkillGrid;

	[SerializeField]
	private GameObject mUpgradeGrid;

	[SerializeField]
	private UICheckbox m_LearnSkillCk;

	[SerializeField]
	private UICheckbox m_UpgradeCk;

	[SerializeField]
	private UITexture mInstructorFace;

	[SerializeField]
	private UITexture mTraineeFace;

	[SerializeField]
	private ShowToolTipItem_N mTipTrainee;

	[SerializeField]
	private ShowToolTipItem_N mTipInstructor;

	[SerializeField]
	private UILabel mAttributeItemNameLabel;

	[SerializeField]
	private UILabel mAttributeItemPlusLabel;

	[SerializeField]
	private UILabel mAttributeItemContentLabel;

	[SerializeField]
	private UILabel mUpgradeTimesLabel;

	[SerializeField]
	private UILabel mCannotUpgradeLabel;

	[SerializeField]
	private N_ImageButton m_InstructorSetBtn;

	[SerializeField]
	private UILabel m_InstructorBtnLbl;

	[SerializeField]
	private N_ImageButton m_TraineeSetBtn;

	[SerializeField]
	private UILabel m_TraineeSetBtnLbl;

	public N_ImageButton mStartBtn;

	public GameObject mStopBtn;

	private ETrainingType mTrainingType;

	private CSPersonnel m_InsNpc;

	private CSPersonnel m_TraineeNpc;

	public ETrainingType TrainingType
	{
		get
		{
			return mTrainingType;
		}
		set
		{
			mTrainingType = value;
		}
	}

	public CSPersonnel InsNpc
	{
		get
		{
			return m_InsNpc;
		}
		set
		{
			if (GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.mTrainingLock)
			{
				return;
			}
			m_InsNpc = value;
			UpdateStatBtnState();
			if (value != null)
			{
				mInstructorFace.mainTexture = value.RandomNpcFace;
				mInstructorFace.enabled = true;
				mTipInstructor.mTipContent = value.FullName;
				if (TraineeNpc != null)
				{
					ShowAttributeItem(_show: true);
				}
				m_InstructorBtnLbl.text = PELocalization.GetString(8000897);
			}
			else
			{
				mInstructorFace.mainTexture = null;
				mInstructorFace.enabled = false;
				mTipInstructor.mTipContent = string.Empty;
				ShowAttributeItem(_show: false);
				m_InstructorBtnLbl.text = PELocalization.GetString(82230010);
			}
			GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.UpdateInstructorSkillsShow(m_InsNpc);
			GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.ClearStudyList();
		}
	}

	public CSPersonnel TraineeNpc
	{
		get
		{
			return m_TraineeNpc;
		}
		set
		{
			if (GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.mTrainingLock)
			{
				return;
			}
			m_TraineeNpc = value;
			UpdateStatBtnState();
			if (value != null)
			{
				mTraineeFace.mainTexture = value.RandomNpcFace;
				mTraineeFace.enabled = true;
				mTipTrainee.mTipContent = value.FullName;
				if (InsNpc != null)
				{
					ShowAttributeItem(_show: true);
				}
				m_TraineeSetBtnLbl.text = PELocalization.GetString(8000897);
			}
			else
			{
				mTraineeFace.mainTexture = null;
				mTraineeFace.enabled = false;
				mTipTrainee.mTipContent = string.Empty;
				ShowAttributeItem(_show: false);
				m_TraineeSetBtnLbl.text = PELocalization.GetString(82230010);
			}
			GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.ClearStudyList();
		}
	}

	private void OnInstructorSetBtn()
	{
		if (null != CSUI_TrainMgr.Instance)
		{
			InsNpc = CSUI_TrainMgr.Instance.RefNpc;
		}
	}

	private void OnTraineeSetBtn()
	{
		if (null != CSUI_TrainMgr.Instance)
		{
			TraineeNpc = CSUI_TrainMgr.Instance.RefNpc;
		}
	}

	public void UpdateSetBtnsState(CSUI_TrainMgr.TypeEnu type)
	{
		switch (type)
		{
		case CSUI_TrainMgr.TypeEnu.Instructor:
			m_InstructorSetBtn.isEnabled = mStartBtn.gameObject.activeSelf;
			m_TraineeSetBtn.isEnabled = false;
			break;
		case CSUI_TrainMgr.TypeEnu.Trainee:
			m_InstructorSetBtn.isEnabled = false;
			m_TraineeSetBtn.isEnabled = mStartBtn.gameObject.activeSelf;
			break;
		}
	}

	public void UpdateStatBtnState()
	{
		if (mStartBtn.gameObject.activeSelf)
		{
			mStartBtn.isEnabled = m_InsNpc != null && null != m_TraineeNpc;
		}
		if (null != CSUI_TrainMgr.Instance)
		{
			UpdateSetBtnsState(CSUI_TrainMgr.Instance.Type);
		}
	}

	private void ShowAttributeItem(bool _show)
	{
		if (_show)
		{
			if (!m_TraineeNpc.m_Npc.GetCmpt<NpcCmpt>().CanAttributeUp())
			{
				mCannotUpgradeLabel.enabled = true;
				mUpgradeTimesLabel.text = string.Empty;
				mUpgradeTimesLabel.transform.parent.gameObject.SetActive(value: false);
				mAttributeItemNameLabel.text = string.Empty;
				mAttributeItemPlusLabel.text = string.Empty;
				mAttributeItemContentLabel.text = string.Empty;
				return;
			}
			mCannotUpgradeLabel.enabled = false;
			mUpgradeTimesLabel.text = "[00bbff]" + m_TraineeNpc.m_Npc.GetCmpt<NpcCmpt>().curAttributeUpTimes + "/" + AttPlusNPCData.GetPlusCount(m_TraineeNpc.m_Npc.entityProto.protoId) + "[-]";
			mUpgradeTimesLabel.transform.parent.gameObject.SetActive(value: true);
			AttribType randMaxAttribute = AttPlusNPCData.GetRandMaxAttribute(m_InsNpc.m_Npc.entityProto.protoId, m_InsNpc.m_Npc.GetCmpt<SkAliveEntity>());
			if (randMaxAttribute == AttribType.Max)
			{
				Debug.Log(m_InsNpc.m_Npc.entityProto.protoId);
				mAttributeItemNameLabel.text = string.Empty;
				mAttributeItemPlusLabel.text = string.Empty;
				mAttributeItemContentLabel.text = string.Empty;
				mUpgradeTimesLabel.text = string.Empty;
				mUpgradeTimesLabel.transform.parent.gameObject.SetActive(value: false);
				mCannotUpgradeLabel.enabled = false;
				return;
			}
			float attribute = m_TraineeNpc.m_Npc.GetAttribute(randMaxAttribute);
			AttPlusNPCData.AttrPlus.RandomInt Rand = default(AttPlusNPCData.AttrPlus.RandomInt);
			if (AttPlusNPCData.GetRandom(m_InsNpc.m_Npc.entityProto.protoId, randMaxAttribute, out Rand))
			{
				mAttributeItemNameLabel.text = AtToString(randMaxAttribute) + ":";
				mAttributeItemPlusLabel.text = attribute + "+";
				mAttributeItemContentLabel.text = "[00ff00]" + Rand.m_Min + "~" + Rand.m_Max + "[-]";
			}
			else
			{
				Debug.Log("没有获取到属性");
			}
		}
		else
		{
			mAttributeItemNameLabel.text = string.Empty;
			mAttributeItemPlusLabel.text = string.Empty;
			mAttributeItemContentLabel.text = string.Empty;
			mUpgradeTimesLabel.text = string.Empty;
			mUpgradeTimesLabel.transform.parent.gameObject.SetActive(value: false);
			mCannotUpgradeLabel.enabled = false;
		}
	}

	private string AtToString(AttribType _type)
	{
		string result = string.Empty;
		switch (_type)
		{
		case AttribType.HpMax:
			result = PELocalization.GetString(10066);
			break;
		case AttribType.StaminaMax:
			result = PELocalization.GetString(10067);
			break;
		case AttribType.ComfortMax:
			result = PELocalization.GetString(8000202);
			break;
		case AttribType.OxygenMax:
			result = PELocalization.GetString(10068);
			break;
		case AttribType.HungerMax:
			result = PELocalization.GetString(10071);
			break;
		case AttribType.EnergyMax:
			result = PELocalization.GetString(10070);
			break;
		case AttribType.ShieldMax:
			result = PELocalization.GetString(2000014);
			break;
		case AttribType.Atk:
			result = PELocalization.GetString(10077);
			break;
		case AttribType.Def:
			result = PELocalization.GetString(10078);
			break;
		}
		return result;
	}

	private void OnSkillPage(bool active)
	{
		mLearnSkillGrid.SetActive(active);
		if (active)
		{
			TrainingType = ETrainingType.Skill;
		}
	}

	private void OnAttributePade(bool active)
	{
		mUpgradeGrid.SetActive(active);
		if (active)
		{
			TrainingType = ETrainingType.Attribute;
		}
	}

	private void Start()
	{
		if (m_InsNpc != null && mInstructorFace.mainTexture == null)
		{
			mInstructorFace.mainTexture = m_InsNpc.RandomNpcFace;
			mInstructorFace.enabled = true;
			mTipInstructor.mTipContent = m_InsNpc.FullName;
		}
		if (m_TraineeNpc != null && mTraineeFace.mainTexture == null)
		{
			mTraineeFace.mainTexture = m_TraineeNpc.RandomNpcFace;
			mTraineeFace.enabled = true;
			mTipTrainee.mTipContent = m_TraineeNpc.FullName;
		}
	}

	private void Update()
	{
		if (TraineeNpc != null)
		{
			mTraineeFace.mainTexture = TraineeNpc.RandomNpcFace;
		}
		if (InsNpc != null)
		{
			mInstructorFace.mainTexture = InsNpc.RandomNpcFace;
		}
	}
}
