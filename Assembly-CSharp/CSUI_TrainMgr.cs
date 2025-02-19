using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_TrainMgr : MonoBehaviour
{
	public enum TypeEnu
	{
		Instructor,
		Trainee
	}

	private static CSUI_TrainMgr _instance;

	[HideInInspector]
	public bool mTrainingLock;

	[SerializeField]
	private CSUI_SkillItem m_SkillPrefab;

	[SerializeField]
	private UIGrid m_TraineeSkillRoot;

	[SerializeField]
	private UIGrid m_InstructorSkillRoot;

	[SerializeField]
	private UIPopupList uip;

	[SerializeField]
	private UILabel uipLabel;

	public UIGrid m_NpcRootUI;

	public UIGrid m_TraineeRootUI;

	public UIGrid m_InstructorRootUI;

	public CSUI_NpcGridItem m_NpcGridPrefab;

	public CSUI_TrainNpcInfCtrl m_TrainNpcInfCtrl;

	public CSUI_TrainLearnPageCtrl m_TrainLearnPageCtrl;

	[SerializeField]
	private UICheckbox mInsCheckBox;

	[SerializeField]
	private UICheckbox mTraCheckBox;

	private CSPersonnel m_LearnPageSelectNpc;

	private TypeEnu m_Type;

	private CSUI_NpcGridItem m_ActiveNpcGrid;

	private CSPersonnel m_RefNpc;

	private List<NpcAbility> _traiuneeSkillLis = new List<NpcAbility>();

	private List<CSUI_SkillItem> m_TraineeSkillGrids = new List<CSUI_SkillItem>(1);

	private List<NpcAbility> _instructorSkillLis = new List<NpcAbility>();

	private List<CSUI_SkillItem> m_InstructorSkillGrids = new List<CSUI_SkillItem>(1);

	private Dictionary<string, int> _skNameIdLis = new Dictionary<string, int>();

	public UILabel _skLabel;

	private List<NpcAbility> _skillLis = new List<NpcAbility>();

	private Dictionary<int, List<NpcAbility>> LevelDic = new Dictionary<int, List<NpcAbility>>();

	private List<NpcAbility> m_StudyList = new List<NpcAbility>();

	private int m_SkillIndex;

	private List<CSUI_SkillItem> m_StudySkillGrids = new List<CSUI_SkillItem>();

	private List<CSUI_NpcGridItem> m_TraineeGrids = new List<CSUI_NpcGridItem>();

	private List<CSUI_NpcGridItem> m_InstructorGrids = new List<CSUI_NpcGridItem>();

	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private UISprite mSprSex;

	[SerializeField]
	private UILabel mLbHealth;

	[SerializeField]
	private UISlider mSdHealth;

	[SerializeField]
	private UILabel mLbStamina;

	[SerializeField]
	private UISlider mSdStamina;

	[SerializeField]
	private UILabel mLbHunger;

	[SerializeField]
	private UISlider mSdHunger;

	[SerializeField]
	private UILabel mLbComfort;

	[SerializeField]
	private UISlider mSdComfort;

	[SerializeField]
	private UILabel mLbOxygen;

	[SerializeField]
	private UISlider mSdOxygen;

	[SerializeField]
	private UILabel mLbShield;

	[SerializeField]
	private UISlider mSdShield;

	[SerializeField]
	private UILabel mLbEnergy;

	[SerializeField]
	private UISlider mSdEnergy;

	[SerializeField]
	private UILabel mLbAttack;

	[SerializeField]
	private UILabel mLbDefense;

	[SerializeField]
	private Grid_N mGridPrefab;

	[SerializeField]
	private Transform mTsInteractionGrids;

	[SerializeField]
	private Transform mTsPrivateItemGrids;

	[SerializeField]
	private UIGrid m_StudyRoot;

	[SerializeField]
	private GameObject m_LearnSkillPage;

	[SerializeField]
	private GameObject m_UpgradePage;

	[SerializeField]
	private UITexture mInstructorFace_Stats;

	[SerializeField]
	private UITexture mInstructorFace_Skill;

	[SerializeField]
	private UITexture mTraineeFace_Stats;

	[SerializeField]
	private UITexture mTraineeFace_Skill;

	private CSPersonnel mTraineeStats = new CSPersonnel();

	private CSPersonnel mInstructorStats = new CSPersonnel();

	private CSPersonnel mTraineeSkill = new CSPersonnel();

	private CSPersonnel mInstructorSkill = new CSPersonnel();

	private int count;

	[SerializeField]
	private UILabel mLbMoney;

	[SerializeField]
	private UILabel mLbPrivatePageText;

	private SlotList mInteractionPackage;

	private SlotList mPrivatePakge;

	private NpcPackageCmpt packageCmpt;

	private NpcPackageCmpt _package;

	private int mInteractionGridCount = 25;

	private int mPrivateItemGridCount = 10;

	private List<Grid_N> mInteractionList;

	private List<Grid_N> mPrivateList;

	private NpcCmpt npcCmpt;

	private PlayerPackageCmpt playerPackageCmpt;

	private int mPageIndex = 1;

	public UILabel mTrainTimeLab;

	private int _minute;

	private int _second;

	public static CSUI_TrainMgr Instance => _instance;

	public bool TrainIsOpen => base.gameObject.activeInHierarchy;

	public bool TraineeIsChecked
	{
		get
		{
			if (m_RefNpc == null || m_RefNpc.Occupation != 0)
			{
				return false;
			}
			return true;
		}
	}

	public TypeEnu Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
			m_TrainLearnPageCtrl.UpdateSetBtnsState(m_Type);
			switch (value)
			{
			case TypeEnu.Trainee:
				m_TraineeRootUI.gameObject.SetActive(value: true);
				m_InstructorRootUI.gameObject.SetActive(value: false);
				RefreshNPCGrids();
				break;
			case TypeEnu.Instructor:
				m_TraineeRootUI.gameObject.SetActive(value: false);
				m_InstructorRootUI.gameObject.SetActive(value: true);
				RefreshNPCGrids();
				break;
			}
		}
	}

	public CSUI_NpcGridItem ActiveNpcGrid
	{
		get
		{
			return m_ActiveNpcGrid;
		}
		set
		{
			m_ActiveNpcGrid = value;
			UpdateNPCRef((!(m_ActiveNpcGrid == null)) ? m_ActiveNpcGrid.m_Npc : null);
		}
	}

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
			m_TrainNpcInfCtrl.Npc = value;
			GetNpcPackage();
			if (this.UpdateInfo != null)
			{
				this.UpdateInfo();
			}
		}
	}

	private int mMaxPageIndex => (mPrivatePakge == null) ? 1 : (mPrivatePakge.Count / mPrivateItemGridCount);

	private PeEntity servant { get; set; }

	public event Action UpdateInfo;

	public event Action<ETrainingType, List<int>, CSPersonnel, CSPersonnel> OnStartTrainingEvent;

	public event Action OnStopTrainingEvent;

	private void UpdateNPCRef(CSPersonnel npc)
	{
		RefNpc = npc;
	}

	public void UpdateTraineeSkillsShow(CSPersonnel _trainee)
	{
		if (_trainee == null)
		{
			for (int i = 0; i < m_TraineeSkillGrids.Count; i++)
			{
				m_TraineeSkillGrids[i].SetSkill("Null");
			}
			return;
		}
		for (int j = 0; j < m_TraineeSkillGrids.Count; j++)
		{
			m_TraineeSkillGrids[j].SetSkill("Null");
		}
		_traiuneeSkillLis = GetNpcSkills(_trainee);
		if (_traiuneeSkillLis.Count == 0)
		{
			return;
		}
		for (int k = 0; k < m_TraineeSkillGrids.Count; k++)
		{
			if (k < _traiuneeSkillLis.Count)
			{
				m_TraineeSkillGrids[k].SetSkill(_traiuneeSkillLis[k].icon, _traiuneeSkillLis[k]);
			}
		}
	}

	public void UpdateInstructorSkillsShow(CSPersonnel _instructor)
	{
		if (_instructor == null)
		{
			for (int i = 0; i < m_InstructorSkillGrids.Count; i++)
			{
				m_InstructorSkillGrids[i].SetSkill("Null");
			}
			return;
		}
		for (int j = 0; j < m_InstructorSkillGrids.Count; j++)
		{
			m_InstructorSkillGrids[j].SetSkill("Null");
		}
		_instructorSkillLis = GetNpcSkills(_instructor);
		if (_instructorSkillLis.Count == 0)
		{
			return;
		}
		for (int k = 0; k < m_InstructorSkillGrids.Count; k++)
		{
			if (k < _instructorSkillLis.Count)
			{
				m_InstructorSkillGrids[k].SetSkill(_instructorSkillLis[k].icon, _instructorSkillLis[k]);
			}
		}
	}

	public void ApplyDataToUI()
	{
		if (_instructorSkillLis.Count != 0)
		{
			for (int i = 0; i < _instructorSkillLis.Count; i++)
			{
				if (i < m_InstructorSkillGrids.Count)
				{
					m_InstructorSkillGrids[i].SetSkill(_instructorSkillLis[i].icon, _instructorSkillLis[i]);
				}
			}
		}
		if (_traiuneeSkillLis.Count != 0)
		{
			for (int j = 0; j < _traiuneeSkillLis.Count; j++)
			{
				if (j < m_TraineeSkillGrids.Count)
				{
					m_TraineeSkillGrids[j].SetSkill(_traiuneeSkillLis[j].icon, _traiuneeSkillLis[j]);
				}
			}
		}
		if (m_StudyList.Count == 0)
		{
			return;
		}
		for (int k = 0; k < m_StudyList.Count; k++)
		{
			if (k < m_StudySkillGrids.Count)
			{
				m_StudySkillGrids[k].SetSkill(m_StudyList[k].icon, m_StudyList[k]);
			}
		}
	}

	private List<NpcAbility> GetNpcSkills(CSPersonnel _npc)
	{
		if (_npc == null)
		{
			return null;
		}
		Ablities abilityIDs = _npc.m_Npc.NpcCmpt.AbilityIDs;
		return NpcAblitycmpt.FindAblitysById(abilityIDs);
	}

	private List<NpcAbility> GetNpcLevelList(AblityType _type, CSPersonnel _npc)
	{
		if (_npc == null)
		{
			return null;
		}
		return NpcAblitycmpt.GetAbilityByType(_type);
	}

	private void SetStudyList(NpcAbility _npcAb)
	{
		if (mTrainingLock || _npcAb == null)
		{
			return;
		}
		CSPersonnel traineeNpc = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.m_TrainLearnPageCtrl.TraineeNpc;
		if (traineeNpc == null)
		{
			return;
		}
		NpcAblitycmpt npcskillcmpt = traineeNpc.m_Npc.GetCmpt<NpcCmpt>().Npcskillcmpt;
		int canLearnId = npcskillcmpt.GetCanLearnId(_npcAb.id);
		if (canLearnId == 0)
		{
			if (CSUI_MainWndCtrl.Instance != null)
			{
				CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(82209014), 5.5f);
			}
			return;
		}
		NpcAbility npcAbility = NpcAblitycmpt.FindNpcAblityById(canLearnId);
		if (!m_StudyList.Contains(npcAbility))
		{
			m_StudyList.Add(npcAbility);
			if (m_SkillIndex < 5)
			{
				m_StudySkillGrids[m_SkillIndex].SetSkill(npcAbility.icon, npcAbility);
				m_SkillIndex++;
			}
		}
	}

	public void ClearStudyList()
	{
		m_StudyList.Clear();
		m_SkillIndex = 0;
		foreach (CSUI_SkillItem studySkillGrid in m_StudySkillGrids)
		{
			studySkillGrid.SetSkill("Null");
		}
		HideAllDeleteBtn();
	}

	private void OnSkillGridDestroySelf(CSUI_SkillItem skillGrid)
	{
		if (!mTrainingLock)
		{
			skillGrid.DeleteIcon();
			m_StudyList.RemoveAt(skillGrid.m_Index);
			m_SkillIndex--;
			RefreshStudyList();
		}
	}

	private void RefreshStudyList()
	{
		foreach (CSUI_SkillItem studySkillGrid in m_StudySkillGrids)
		{
			studySkillGrid.DeleteIcon();
		}
		for (int i = 0; i < m_StudyList.Count; i++)
		{
			m_StudySkillGrids[i].SetSkill(m_StudyList[i].icon, m_StudyList[i]);
		}
	}

	public void HideAllDeleteBtn()
	{
		if (m_StudySkillGrids.Count == 0)
		{
			return;
		}
		foreach (CSUI_SkillItem studySkillGrid in m_StudySkillGrids)
		{
			studySkillGrid.OnHideBtn();
		}
	}

	private void RefreshNPCGrids()
	{
		if (CSUI_MainWndCtrl.Instance == null)
		{
			return;
		}
		List<CSUI_NpcGridItem> list = null;
		list = ((Type != TypeEnu.Trainee) ? m_InstructorGrids : m_TraineeGrids);
		if (CSUI_MainWndCtrl.Instance.Creator == null)
		{
			return;
		}
		CSPersonnel[] npcs = CSUI_MainWndCtrl.Instance.Creator.GetNpcs();
		int num = 0;
		if (Type == TypeEnu.Trainee)
		{
			for (int i = 0; i < npcs.Length; i++)
			{
				if (npcs[i].Occupation == 7 && npcs[i].trainerType == ETrainerType.Trainee)
				{
					if (num < list.Count)
					{
						list[num].m_Npc = npcs[i];
					}
					else
					{
						CSUI_NpcGridItem item = _createNPCGird(npcs[i], m_TraineeRootUI.transform);
						list.Add(item);
					}
					num++;
				}
			}
		}
		else
		{
			for (int j = 0; j < npcs.Length; j++)
			{
				if (npcs[j].Occupation == 7 && npcs[j].trainerType == ETrainerType.Instructor)
				{
					if (num < list.Count)
					{
						list[num].m_Npc = npcs[j];
					}
					else
					{
						CSUI_NpcGridItem item2 = _createNPCGird(npcs[j], m_InstructorRootUI.transform);
						list.Add(item2);
					}
					num++;
				}
			}
		}
		if (num < list.Count)
		{
			int num2 = num;
			while (num2 < list.Count)
			{
				UnityEngine.Object.DestroyImmediate(list[num2].gameObject);
				list.RemoveAt(num2);
			}
		}
		if (list.Count != 0)
		{
			GameObject gameObject = null;
			if (m_LearnPageSelectNpc != null)
			{
				for (int k = 0; k < list.Count; k++)
				{
					if (m_LearnPageSelectNpc == list[k].m_Npc)
					{
						gameObject = list[k].gameObject;
						m_LearnPageSelectNpc = null;
						break;
					}
				}
			}
			if (null == gameObject)
			{
				gameObject = list[0].gameObject;
			}
			gameObject.GetComponent<UICheckbox>().isChecked = true;
			OnNPCGridActive(list[0].gameObject, actvie: true);
		}
		else
		{
			ActiveNpcGrid = null;
		}
		m_TraineeRootUI.repositionNow = true;
		m_InstructorRootUI.repositionNow = true;
	}

	private void OnInstructorIconClick()
	{
		if (null != m_TrainLearnPageCtrl && m_TrainLearnPageCtrl.InsNpc != null && null != ActiveNpcGrid && ActiveNpcGrid.m_Npc != m_TrainLearnPageCtrl.InsNpc)
		{
			m_LearnPageSelectNpc = m_TrainLearnPageCtrl.InsNpc;
			if (!mInsCheckBox.isChecked)
			{
				mInsCheckBox.isChecked = true;
			}
			else
			{
				Type = TypeEnu.Instructor;
			}
		}
	}

	private void OnTraineeIconClick()
	{
		if (null != m_TrainLearnPageCtrl && m_TrainLearnPageCtrl.TraineeNpc != null && null != ActiveNpcGrid && ActiveNpcGrid.m_Npc != m_TrainLearnPageCtrl.TraineeNpc)
		{
			m_LearnPageSelectNpc = m_TrainLearnPageCtrl.TraineeNpc;
			if (!mTraCheckBox.isChecked)
			{
				mTraCheckBox.isChecked = true;
			}
			else
			{
				Type = TypeEnu.Trainee;
			}
		}
	}

	private void OnNPCGridActive(GameObject go, bool actvie)
	{
		if (actvie)
		{
			ActiveNpcGrid = go.GetComponent<CSUI_NpcGridItem>();
		}
	}

	private CSUI_NpcGridItem _createNPCGird(CSPersonnel npc, Transform root)
	{
		CSUI_NpcGridItem cSUI_NpcGridItem = UnityEngine.Object.Instantiate(m_NpcGridPrefab);
		cSUI_NpcGridItem.transform.parent = root;
		CSUtils.ResetLoacalTransform(cSUI_NpcGridItem.transform);
		cSUI_NpcGridItem.m_UseDeletebutton = false;
		cSUI_NpcGridItem.m_NpcNameLabel.enabled = false;
		cSUI_NpcGridItem.m_Npc = npc;
		UICheckbox component = cSUI_NpcGridItem.gameObject.GetComponent<UICheckbox>();
		component.radioButtonRoot = root;
		UIEventListener.Get(cSUI_NpcGridItem.gameObject).onActivate = OnNPCGridActive;
		return cSUI_NpcGridItem;
	}

	public void SetServantInfo(string name, PeSex sex, int health, int healthMax, int stamina, int stamina_max, int hunger, int hunger_max, int comfort, int comfort_max, int oxygen, int oxygen_max, int shield, int shield_max, int energy, int energy_max, int attack, int defense)
	{
		mLbName.text = name;
		mSprSex.spriteName = ((sex != PeSex.Male) ? "woman" : "man");
		mLbHealth.text = Convert.ToString(health) + "/" + Convert.ToString(healthMax);
		mSdHealth.sliderValue = ((healthMax <= 0) ? 0f : (Convert.ToSingle(health) / (float)healthMax));
		mLbStamina.text = Convert.ToString(stamina) + "/" + Convert.ToString(stamina_max);
		mSdStamina.sliderValue = ((stamina_max <= 0) ? 0f : (Convert.ToSingle(stamina) / (float)stamina_max));
		mLbHunger.text = Convert.ToString(hunger) + "/" + Convert.ToString(hunger_max);
		mSdHunger.sliderValue = ((hunger_max <= 0) ? 0f : (Convert.ToSingle(hunger) / (float)hunger_max));
		mLbComfort.text = Convert.ToString(comfort) + "/" + Convert.ToString(comfort_max);
		mSdComfort.sliderValue = ((comfort_max <= 0) ? 0f : (Convert.ToSingle(comfort) / (float)comfort_max));
		mLbOxygen.text = Convert.ToString(oxygen) + "/" + Convert.ToString(oxygen_max);
		mSdOxygen.sliderValue = ((oxygen_max <= 0) ? 0f : (Convert.ToSingle(oxygen) / (float)oxygen_max));
		mLbShield.text = Convert.ToString(shield) + "/" + Convert.ToString(shield_max);
		mSdShield.sliderValue = ((shield_max <= 0) ? 0f : (Convert.ToSingle(shield) / (float)shield_max));
		mLbEnergy.text = Convert.ToString(energy) + "/" + Convert.ToString(energy_max);
		mSdEnergy.sliderValue = ((energy_max <= 0) ? 0f : (Convert.ToSingle(energy) / (float)energy_max));
		mLbAttack.text = Convert.ToString(attack);
		mLbDefense.text = Convert.ToString(defense);
	}

	public void SetSprSex(string sex)
	{
		mSprSex.spriteName = sex;
	}

	private void PageTraineeOnActive(bool active)
	{
		if (active)
		{
			Type = TypeEnu.Trainee;
		}
	}

	private void PageInstructorOnActive(bool active)
	{
		if (active)
		{
			Type = TypeEnu.Instructor;
		}
	}

	private void InitGrid()
	{
		mInteractionList = new List<Grid_N>();
		for (int i = 0; i < mInteractionGridCount; i++)
		{
			mInteractionList.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mInteractionList[i].gameObject.name = "Interaction" + i;
			mInteractionList[i].transform.parent = mTsInteractionGrids;
			mInteractionList[i].transform.localPosition = new Vector3(i % 5 * 60, -(i / 5) * 54, 0f);
			mInteractionList[i].transform.localRotation = Quaternion.identity;
			mInteractionList[i].transform.localScale = Vector3.one;
			mInteractionList[i].SetItemPlace(ItemPlaceType.IPT_ConolyServantInteractionTrain, i);
		}
		mPrivateList = new List<Grid_N>();
		for (int j = 0; j < mPrivateItemGridCount; j++)
		{
			mPrivateList.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mPrivateList[j].gameObject.name = "PrivateItem" + j;
			mPrivateList[j].transform.parent = mTsPrivateItemGrids;
			if (j < 5)
			{
				mPrivateList[j].transform.localPosition = new Vector3(j * 60, 0f, 0f);
			}
			else if (j < 10)
			{
				mPrivateList[j].transform.localPosition = new Vector3((j - 5) * 60, -55f, 0f);
			}
			else
			{
				mPrivateList[j].transform.localPosition = new Vector3((j - 10) * 60, -110f, 0f);
			}
			mPrivateList[j].transform.localRotation = Quaternion.identity;
			mPrivateList[j].transform.localScale = Vector3.one;
		}
	}

	private void Awake()
	{
		_instance = this;
		InitGrid();
		for (int i = 0; i < 5; i++)
		{
			CSUI_SkillItem cSUI_SkillItem = UnityEngine.Object.Instantiate(m_SkillPrefab);
			cSUI_SkillItem.transform.parent = m_TraineeSkillRoot.transform;
			cSUI_SkillItem.transform.localPosition = Vector3.zero;
			cSUI_SkillItem.transform.localRotation = Quaternion.identity;
			cSUI_SkillItem.transform.localScale = Vector3.one;
			cSUI_SkillItem.m_Index = i;
			cSUI_SkillItem._ableToClick = false;
			m_TraineeSkillGrids.Add(cSUI_SkillItem);
		}
		m_TraineeSkillRoot.repositionNow = true;
		for (int j = 0; j < 5; j++)
		{
			CSUI_SkillItem cSUI_SkillItem2 = UnityEngine.Object.Instantiate(m_SkillPrefab);
			cSUI_SkillItem2.transform.parent = m_InstructorSkillRoot.transform;
			cSUI_SkillItem2.transform.localPosition = Vector3.zero;
			cSUI_SkillItem2.transform.localRotation = Quaternion.identity;
			cSUI_SkillItem2.transform.localScale = Vector3.one;
			cSUI_SkillItem2.m_Index = j;
			cSUI_SkillItem2._ableToClick = false;
			cSUI_SkillItem2.onLeftMouseClicked += SetStudyList;
			m_InstructorSkillGrids.Add(cSUI_SkillItem2);
		}
		m_InstructorSkillRoot.repositionNow = true;
		for (int k = 0; k < 5; k++)
		{
			CSUI_SkillItem cSUI_SkillItem3 = UnityEngine.Object.Instantiate(m_SkillPrefab);
			cSUI_SkillItem3.transform.parent = m_StudyRoot.transform;
			cSUI_SkillItem3.transform.localPosition = Vector3.zero;
			cSUI_SkillItem3.transform.localRotation = Quaternion.identity;
			cSUI_SkillItem3.transform.localScale = Vector3.one;
			cSUI_SkillItem3.OnDestroySelf = (CSUI_SkillItem.SkillGridEvent)Delegate.Combine(cSUI_SkillItem3.OnDestroySelf, new CSUI_SkillItem.SkillGridEvent(OnSkillGridDestroySelf));
			cSUI_SkillItem3.m_Index = k;
			m_StudySkillGrids.Add(cSUI_SkillItem3);
		}
		m_StudyRoot.repositionNow = true;
	}

	private void OnEnable()
	{
		m_TrainLearnPageCtrl.InsNpc = null;
		m_TrainLearnPageCtrl.TraineeNpc = null;
		if (mInsCheckBox.isChecked)
		{
			Type = TypeEnu.Instructor;
		}
		else if (mTraCheckBox.isChecked)
		{
			Type = TypeEnu.Trainee;
		}
	}

	private void Start()
	{
		ApplyDataToUI();
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.P))
		{
		}
	}

	private void GetNpcPackage()
	{
		if (m_RefNpc == null)
		{
			mInteractionPackage = null;
			mPrivatePakge = null;
			_package = null;
		}
		if (m_RefNpc != null)
		{
			_package = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>().GetComponent<NpcPackageCmpt>();
			mInteractionPackage = _package.GetSlotList();
			mPrivatePakge = _package.GetPrivateSlotList();
		}
	}

	private void GetServentCmpt()
	{
		NpcCmpt cmpt = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>();
		if (cmpt != null && cmpt != npcCmpt)
		{
			packageCmpt = cmpt.GetComponent<NpcPackageCmpt>();
			GetNpcPakgeSlotList();
		}
		npcCmpt = cmpt;
		servant = ((!(npcCmpt != null)) ? null : npcCmpt.Entity);
		if (npcCmpt == null)
		{
			packageCmpt = null;
			mInteractionPackage = null;
			mPrivatePakge = null;
			ClearNpcPackage();
		}
	}

	private void Reflash()
	{
		playerPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		GetServentCmpt();
		if (!(npcCmpt == null) && packageCmpt != null)
		{
			Reflashpackage();
		}
	}

	private void BtnLeftOnClick()
	{
		if (mPageIndex > 1)
		{
			mPageIndex--;
		}
		ReflashPrivatePackage();
	}

	private void BtnRightOnClick()
	{
		if (mPageIndex < mMaxPageIndex)
		{
			mPageIndex++;
		}
		ReflashPrivatePackage();
	}

	private void BtnTakeAllOnClick()
	{
		if (m_RefNpc != null && mInteractionPackage != null && m_RefNpc.IsRandomNpc)
		{
			PlayerPackageCmpt cmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
			List<ItemObject> items = mInteractionPackage.ToList();
			if (cmpt.CanAddItemList(items))
			{
				cmpt.AddItemList(items);
				mInteractionPackage.Clear();
			}
		}
	}

	public bool SetItemWithIndex(ItemObject itemObj, int index = -1)
	{
		if (index == -1)
		{
			return mInteractionPackage.Add(itemObj);
		}
		if (index < 0 || index > mInteractionPackage.Count)
		{
			return false;
		}
		if (mInteractionPackage != null)
		{
			mInteractionPackage[index] = itemObj;
		}
		return true;
	}

	private void GetNpcPakgeSlotList()
	{
		if (mInteractionPackage != null)
		{
			mInteractionPackage.eventor.Unsubscribe(InteractionpackageChange);
		}
		mInteractionPackage = packageCmpt.GetSlotList();
		mInteractionPackage.eventor.Subscribe(InteractionpackageChange);
		if (mPrivatePakge != null)
		{
			mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);
		}
		mPrivatePakge = packageCmpt.GetPrivateSlotList();
		mPrivatePakge.eventor.Subscribe(PrivatepackageChange);
	}

	public void Reflashpackage()
	{
		ReflashInteractionpackage();
		ReflashPrivatePackage();
		ReflashNpcMoney();
	}

	private void ReflashNpcMoney()
	{
		if (!(_package == null))
		{
			mLbMoney.text = _package.money.current.ToString();
		}
	}

	private void ClearNpcPackage()
	{
		ClearInteractionpackage();
		ClearPrivatePackage();
	}

	private void InteractionpackageChange(object sender, SlotList.ChangeEvent arg)
	{
		ReflashInteractionpackage();
	}

	private void ReflashInteractionpackage()
	{
		ClearInteractionpackage();
		for (int i = 0; i < mInteractionGridCount; i++)
		{
			if (mInteractionPackage == null)
			{
				mInteractionList[i].SetItem(null);
			}
			else
			{
				mInteractionList[i].SetItem(mInteractionPackage[i]);
			}
		}
	}

	private void ClearInteractionpackage()
	{
		if (mInteractionList == null)
		{
			return;
		}
		foreach (Grid_N mInteraction in mInteractionList)
		{
			mInteraction.SetItem(null);
		}
	}

	private void PrivatepackageChange(object sender, SlotList.ChangeEvent arg)
	{
		ReflashPrivatePackage();
	}

	private void ReflashPrivatePackage()
	{
		ClearPrivatePackage();
		int num = (mPageIndex - 1) * mPrivateItemGridCount;
		for (int i = 0; i < mPrivateList.Count; i++)
		{
			if (mPrivatePakge == null)
			{
				mPrivateList[i].SetItem(null);
			}
			else
			{
				mPrivateList[i].SetItem(mPrivatePakge[num + i]);
			}
		}
	}

	private void ClearPrivatePackage()
	{
		if (mPrivateList == null)
		{
			return;
		}
		foreach (Grid_N mPrivate in mPrivateList)
		{
			mPrivate.SetItem(null);
		}
	}

	public void OnLeftMouseCliked_InterPackage(Grid_N grid)
	{
		if (m_RefNpc != null && m_RefNpc.Occupation == 0)
		{
			SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
		}
	}

	public void OnRightMouseCliked_InterPackage(Grid_N grid)
	{
		if (!(null == servant))
		{
			UseItemCmpt useItemCmpt = servant.GetCmpt<UseItemCmpt>();
			if (null == useItemCmpt)
			{
				useItemCmpt = servant.Add<UseItemCmpt>();
			}
			if (!useItemCmpt.Request(grid.ItemObj))
			{
			}
		}
	}

	public void OnDropItem_InterPackage(Grid_N grid)
	{
		if (m_RefNpc == null || m_RefNpc.Occupation != 0 || grid.ItemObj != null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			switch (SelectItem_N.Instance.Place)
			{
			case ItemPlaceType.IPT_Bag:
			case ItemPlaceType.IPT_ServantEqu:
				PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				break;
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Bag:
		case ItemPlaceType.IPT_ServantEqu:
		case ItemPlaceType.IPT_ServantInteraction:
		case ItemPlaceType.IPT_ConolyServantInteractionTrain:
			SetItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
			SelectItem_N.Instance.RemoveOriginItem();
			grid.SetItem(SelectItem_N.Instance.ItemObj);
			SelectItem_N.Instance.SetItem(null);
			break;
		default:
			SelectItem_N.Instance.SetItem(null);
			break;
		}
	}

	private List<int> GetSkillIds(List<NpcAbility> _lis)
	{
		List<int> list = new List<int>(1);
		if (_lis.Count == 0)
		{
			return list;
		}
		foreach (NpcAbility _li in _lis)
		{
			list.Add(_li.id);
		}
		return list;
	}

	private List<int> GetLastList(ETrainingType _trainingtype)
	{
		List<int> list = new List<int>();
		return _trainingtype switch
		{
			ETrainingType.Skill => GetSkillIds(m_StudyList), 
			ETrainingType.Attribute => list, 
			_ => list, 
		};
	}

	private void OnStartBtn()
	{
		if (this.OnStartTrainingEvent != null && m_TrainLearnPageCtrl.InsNpc != null && m_TrainLearnPageCtrl.TraineeNpc != null)
		{
			this.OnStartTrainingEvent(m_TrainLearnPageCtrl.TrainingType, GetLastList(m_TrainLearnPageCtrl.TrainingType), m_TrainLearnPageCtrl.InsNpc, m_TrainLearnPageCtrl.TraineeNpc);
		}
	}

	private void OnStopBtn()
	{
		if (this.OnStopTrainingEvent != null)
		{
			this.OnStopTrainingEvent();
		}
	}

	public List<int> GetStudyList()
	{
		List<int> list = new List<int>();
		return GetSkillIds(m_StudyList);
	}

	public void SetBtnState(bool _state)
	{
		m_TrainLearnPageCtrl.mStartBtn.gameObject.SetActive(!_state);
		m_TrainLearnPageCtrl.UpdateStatBtnState();
		m_TrainLearnPageCtrl.mStopBtn.SetActive(_state);
	}

	public void RefreshAfterTraining()
	{
		m_TrainLearnPageCtrl.InsNpc = m_TrainLearnPageCtrl.InsNpc;
		m_TrainLearnPageCtrl.TraineeNpc = m_TrainLearnPageCtrl.TraineeNpc;
	}

	public void RefreshAfterTraining(CSPersonnel csp_instructor, CSPersonnel csp_trainee)
	{
		m_TrainLearnPageCtrl.InsNpc = csp_instructor;
		m_TrainLearnPageCtrl.TraineeNpc = csp_trainee;
	}

	public void SetStudyListInterface(List<int> _lis)
	{
		if (_lis.Count == 0)
		{
			return;
		}
		m_SkillIndex = 0;
		m_StudyList.Clear();
		for (int i = 0; i < _lis.Count; i++)
		{
			NpcAbility npcAbility = NpcAblitycmpt.FindNpcAblityById(_lis[i]);
			m_StudyList.Add(npcAbility);
			if (m_SkillIndex < 5)
			{
				if (m_StudySkillGrids.Count > m_SkillIndex)
				{
					m_StudySkillGrids[m_SkillIndex].SetSkill(npcAbility.icon, npcAbility);
				}
				m_SkillIndex++;
			}
		}
	}

	public void LearnSkillTimeShow(float _time)
	{
		if (mTrainTimeLab != null)
		{
			_minute = (int)(_time / 60f);
			_second = (int)(_time - (float)(_minute * 60));
			mTrainTimeLab.text = TimeTransition(_minute).ToString() + ":" + TimeTransition(_second).ToString();
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
}
