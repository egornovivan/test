using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class CSUI_Train : MonoBehaviour
{
	public enum TypeEnu
	{
		Trainee,
		Instructor
	}

	public delegate void GetAllNpcsDel();

	public delegate void StartStudyDel(CSUIMyNpc trainee, List<CSUIMySkill> studyList);

	public delegate void StartUpgradeDel(CSUIMyNpc trainee, CSUIMyNpc instructor);

	[SerializeField]
	private UIGrid m_TraineeRootUI;

	[SerializeField]
	private UIGrid m_InstructorRootUI;

	[SerializeField]
	private GameObject m_InfoPage;

	[SerializeField]
	private GameObject m_InventoryPage;

	[SerializeField]
	private GameObject m_LearnSkillPage;

	[SerializeField]
	private GameObject m_UpgradePage;

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
	private UIGrid m_ItemRoot;

	[SerializeField]
	private UIGrid m_SkillRoot;

	[SerializeField]
	private UIGrid m_StudyRoot;

	[SerializeField]
	private UITexture mTraineeFace_Skill;

	[SerializeField]
	private UITexture mInstructorFace_Skill;

	[SerializeField]
	private UITexture mTraineeFace_Stats;

	[SerializeField]
	private UITexture mInstructorFace_Stats;

	[SerializeField]
	private UIPopupList uip;

	[SerializeField]
	private UILabel mMaxHealth;

	[SerializeField]
	private UILabel mMaxStrength;

	[SerializeField]
	private UILabel mMaxHunger;

	[SerializeField]
	private UILabel mMaxStamina;

	[SerializeField]
	private UILabel mMaxOxygen;

	[SerializeField]
	private UILabel mTrainingTime;

	[SerializeField]
	private UILabel mAddHealth;

	[SerializeField]
	private UILabel mAddStrength;

	[SerializeField]
	private UILabel mAddHunger;

	[SerializeField]
	private UILabel mAddStamina;

	[SerializeField]
	private UILabel mAddOxygen;

	[SerializeField]
	private CSUI_MyNpcItem m_NpcGridPrefab;

	[SerializeField]
	private CSUI_Grid m_GridPrefab;

	[SerializeField]
	private CSUI_SkillItem m_StudySkillPrefab;

	private TypeEnu m_Type;

	private CSUI_MyNpcItem m_ActiveNpcGrid;

	private static CSUI_Train _instance;

	private List<CSUI_MyNpcItem> m_TraineeGrids = new List<CSUI_MyNpcItem>();

	private List<CSUI_MyNpcItem> m_InstructorGrids = new List<CSUI_MyNpcItem>();

	private List<CSUIMyNpc> allRandomNpcs = new List<CSUIMyNpc>();

	private List<CSUI_Grid> m_ItemGrids = new List<CSUI_Grid>();

	private List<CSUI_SkillItem> m_SkillGrids = new List<CSUI_SkillItem>();

	private List<CSUI_SkillItem> m_StudySkillGrids = new List<CSUI_SkillItem>();

	private List<CSUIMySkill> m_StudyList = new List<CSUIMySkill>();

	private List<CSUIMySkill> mSkills = new List<CSUIMySkill>();

	private CSUIMyNpc mTraineeSkill = new CSUIMyNpc();

	private CSUIMyNpc mTraineeStats = new CSUIMyNpc();

	private CSUIMyNpc mInstructorStats = new CSUIMyNpc();

	private int m_SkillIndex;

	private CSUIMyNpc m_RefNpc;

	private int kk;

	private List<Grid_N> mInteractionList;

	private List<Grid_N> mPrivateList;

	private int mInteractionGridCount = 10;

	private int mPrivateItemGridCount = 10;

	[SerializeField]
	private Grid_N mGridPrefab;

	[SerializeField]
	private Transform mTsInteractionGrids;

	[SerializeField]
	private Transform mTsPrivateItemGrids;

	private List<CSUIMySkill> skilllistTest = new List<CSUIMySkill>();

	public TypeEnu Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
			switch (value)
			{
			case TypeEnu.Trainee:
			{
				m_TraineeRootUI.gameObject.SetActive(value: true);
				m_InstructorRootUI.gameObject.SetActive(value: false);
				RefreshNPCGrids();
				CSUI_MyNpcItem activeNpcGrid2 = null;
				for (int j = 0; j < m_TraineeRootUI.transform.childCount; j++)
				{
					UICheckbox component2 = m_TraineeRootUI.transform.GetChild(j).gameObject.GetComponent<UICheckbox>();
					if (component2.isChecked)
					{
						activeNpcGrid2 = component2.gameObject.GetComponent<CSUI_MyNpcItem>();
						break;
					}
				}
				ActiveNpcGrid = activeNpcGrid2;
				break;
			}
			case TypeEnu.Instructor:
			{
				m_TraineeRootUI.gameObject.SetActive(value: false);
				m_InstructorRootUI.gameObject.SetActive(value: true);
				RefreshNPCGrids();
				CSUI_MyNpcItem activeNpcGrid = null;
				for (int i = 0; i < m_InstructorRootUI.transform.childCount; i++)
				{
					UICheckbox component = m_InstructorRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
					if (component.isChecked)
					{
						activeNpcGrid = component.gameObject.GetComponent<CSUI_MyNpcItem>();
						break;
					}
				}
				ActiveNpcGrid = activeNpcGrid;
				break;
			}
			}
		}
	}

	public CSUI_MyNpcItem ActiveNpcGrid
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

	public static CSUI_Train Instance => _instance;

	public event GetAllNpcsDel GetAllRandomNpcsEvent;

	public event StartStudyDel StartStudyEvent;

	public event StartUpgradeDel StartUpgradeEvent;

	private void RefreshNPCGrids()
	{
		if (CSUI_MainWndCtrl.Instance == null)
		{
			return;
		}
		if (this.GetAllRandomNpcsEvent != null)
		{
			this.GetAllRandomNpcsEvent();
		}
		if (Type == TypeEnu.Trainee)
		{
			m_TraineeGrids.Clear();
			NpcGridDestroy(m_TraineeRootUI.transform);
			for (int i = 0; i < allRandomNpcs.Count; i++)
			{
				if (!allRandomNpcs[i].HasOccupation)
				{
					m_TraineeGrids.Add(CreateNPCGird(allRandomNpcs[i], m_TraineeRootUI.transform));
				}
			}
			m_TraineeRootUI.repositionNow = true;
		}
		if (Type != TypeEnu.Instructor)
		{
			return;
		}
		m_InstructorGrids.Clear();
		NpcGridDestroy(m_InstructorRootUI.transform);
		for (int j = 0; j < allRandomNpcs.Count; j++)
		{
			if (allRandomNpcs[j].HasOccupation)
			{
				m_InstructorGrids.Add(CreateNPCGird(allRandomNpcs[j], m_InstructorRootUI.transform));
			}
		}
		m_InstructorRootUI.repositionNow = true;
	}

	private void UpdateNPCRef(CSUIMyNpc npc)
	{
		if (npc != null)
		{
			m_RefNpc = npc;
		}
		if (m_RefNpc != null && !m_RefNpc.HasOccupation)
		{
			UpdateSkills();
		}
		if (m_RefNpc != null && m_RefNpc.HasOccupation)
		{
			GetInstructorSkill(m_RefNpc.OwnSkills);
		}
	}

	private void GetInstructorSkill(List<CSUIMySkill> skills)
	{
		if (skills.Count < 0)
		{
			return;
		}
		mSkills.Clear();
		foreach (CSUIMySkill skill in skills)
		{
			mSkills.Add(skill);
		}
		uip.items.Clear();
		foreach (CSUIMySkill skill2 in skills)
		{
			uip.items.Add(skill2.name);
		}
		Debug.Log("uip.items的长度：" + uip.items.Count);
	}

	private CSUI_MyNpcItem CreateNPCGird(CSUIMyNpc npc, Transform root)
	{
		CSUI_MyNpcItem cSUI_MyNpcItem = UnityEngine.Object.Instantiate(m_NpcGridPrefab);
		cSUI_MyNpcItem.transform.parent = root;
		CSUtils.ResetLoacalTransform(cSUI_MyNpcItem.transform);
		cSUI_MyNpcItem.m_UseDeletebutton = true;
		cSUI_MyNpcItem.m_Npc = npc;
		UICheckbox component = cSUI_MyNpcItem.gameObject.GetComponent<UICheckbox>();
		component.radioButtonRoot = root;
		UIEventListener.Get(cSUI_MyNpcItem.gameObject).onActivate = OnNPCGridActive;
		return cSUI_MyNpcItem;
	}

	private void NpcGridDestroy(Transform gridTr)
	{
		for (int i = 0; i < gridTr.childCount; i++)
		{
			UnityEngine.Object.Destroy(gridTr.GetChild(i).gameObject);
		}
	}

	private void UpdateSkills()
	{
		for (int i = 0; i < m_SkillGrids.Count; i++)
		{
		}
	}

	private void SetServantInfo(string name, PeSex sex, int health, int healthMax, int stamina, int stamina_max, int hunger, int hunger_max, int comfort, int comfort_max, int oxygen, int oxygen_max, int shield, int shield_max, int energy, int energy_max, int attack, int defense)
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

	private void OnSelectionChange(string skillName)
	{
	}

	private CSUIMySkill FindSkillByName(string skillName)
	{
		CSUIMySkill result = new CSUIMySkill();
		if (mSkills.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < mSkills.Count; i++)
		{
			if (mSkills[i].name == skillName)
			{
				result = mSkills[i];
			}
		}
		return result;
	}

	private void RefreshStudyList()
	{
		foreach (CSUI_SkillItem studySkillGrid in m_StudySkillGrids)
		{
			studySkillGrid.DeleteIcon();
		}
		for (int i = 0; i < m_StudyList.Count; i++)
		{
			m_StudySkillGrids[i].SetIcon(m_StudyList[i].iconImg);
		}
		uip.selection = string.Empty;
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

	private void SetTraineeInfo(int health_max, int strength_max, int hunger_max, int stamina_max, int oxygen_max)
	{
		mMaxHealth.text = Convert.ToString(health_max);
		mMaxStrength.text = Convert.ToString(strength_max);
		mMaxHunger.text = Convert.ToString(hunger_max);
		mMaxStamina.text = Convert.ToString(stamina_max);
		mMaxOxygen.text = Convert.ToString(oxygen_max);
	}

	private void SetInstructorInfo(string health_add, string strength_add, string hunger_add, string stamina_add, string oxygen_add)
	{
		mAddHealth.text = health_add;
		mAddStrength.text = strength_add;
		mAddHunger.text = hunger_add;
		mAddStamina.text = stamina_add;
		mAddOxygen.text = oxygen_add;
	}

	private void PageTraineeOnActive(bool active)
	{
	}

	private void PageInstructorOnActive(bool active)
	{
	}

	private void PageInfoOnActive(bool active)
	{
		m_InfoPage.SetActive(active);
	}

	private void PageInvetoryOnActive(bool active)
	{
		m_InventoryPage.SetActive(active);
	}

	private void PageLearnSkillOnActive(bool active)
	{
		m_LearnSkillPage.SetActive(active);
	}

	private void PageUpgradeStatsOnActive(bool active)
	{
		m_UpgradePage.SetActive(active);
	}

	private void OnBtnStartLearnSkill()
	{
		Debug.Log("************开始学习技能************");
		if (this.StartStudyEvent != null)
		{
			this.StartStudyEvent(mTraineeSkill, m_StudyList);
			Debug.Log("************开始学习技能************");
		}
	}

	private void OnBtnStartUpgradeStats()
	{
		Debug.Log("************开始提升属性************");
		if (this.StartUpgradeEvent != null)
		{
			this.StartUpgradeEvent(mTraineeStats, mInstructorStats);
			Debug.Log("************开始提升属性************");
		}
	}

	private void OnNPCGridActive(GameObject go, bool actvie)
	{
		if (actvie)
		{
			ActiveNpcGrid = go.GetComponent<CSUI_MyNpcItem>();
		}
	}

	private void OnSkillGridDestroySelf(CSUI_SkillItem skillGrid)
	{
		Debug.Log("关闭");
		skillGrid.DeleteIcon();
		m_StudyList.RemoveAt(skillGrid.m_Index);
		m_SkillIndex--;
		RefreshStudyList();
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		InitGrid();
		m_ItemRoot.repositionNow = true;
		for (int i = 0; i < 5; i++)
		{
			CSUI_SkillItem cSUI_SkillItem = UnityEngine.Object.Instantiate(m_StudySkillPrefab);
			cSUI_SkillItem.transform.parent = m_SkillRoot.transform;
			cSUI_SkillItem.transform.localPosition = Vector3.zero;
			cSUI_SkillItem.transform.localRotation = Quaternion.identity;
			cSUI_SkillItem.transform.localScale = Vector3.one;
			cSUI_SkillItem.m_Index = i;
			m_SkillGrids.Add(cSUI_SkillItem);
		}
		m_SkillRoot.repositionNow = true;
		for (int j = 0; j < 5; j++)
		{
			CSUI_SkillItem cSUI_SkillItem2 = UnityEngine.Object.Instantiate(m_StudySkillPrefab);
			cSUI_SkillItem2.transform.parent = m_StudyRoot.transform;
			cSUI_SkillItem2.transform.localPosition = Vector3.zero;
			cSUI_SkillItem2.transform.localRotation = Quaternion.identity;
			cSUI_SkillItem2.transform.localScale = Vector3.one;
			cSUI_SkillItem2.OnDestroySelf = (CSUI_SkillItem.SkillGridEvent)Delegate.Combine(cSUI_SkillItem2.OnDestroySelf, new CSUI_SkillItem.SkillGridEvent(OnSkillGridDestroySelf));
			cSUI_SkillItem2.m_Index = j;
			m_StudySkillGrids.Add(cSUI_SkillItem2);
		}
		m_StudyRoot.repositionNow = true;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			uip.items.Clear();
			uip.items = new List<string>
			{
				"White" + kk,
				"Red" + kk,
				"Green" + kk,
				"Grey" + kk
			};
			kk++;
		}
		if (Input.GetKeyDown(KeyCode.H))
		{
			CreatSkill1();
			GetInstructorSkill(skilllistTest);
		}
		if (Input.GetKeyDown(KeyCode.J))
		{
			CreatSkill2();
			GetInstructorSkill(skilllistTest);
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			uip.selection = string.Empty;
		}
		if (Input.GetKeyDown(KeyCode.V))
		{
			uip.items.Add("1234");
			Debug.Log(uip.items.Count.ToString());
		}
		if (m_RefNpc == null)
		{
			SetServantInfo("--", PeSex.Male, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
			mInstructorFace_Stats.mainTexture = null;
			mInstructorFace_Stats.enabled = false;
			mInstructorFace_Skill.mainTexture = null;
			mInstructorFace_Skill.enabled = false;
			mTraineeFace_Stats.mainTexture = null;
			mTraineeFace_Stats.enabled = false;
			mTraineeFace_Skill.mainTexture = null;
			mTraineeFace_Skill.enabled = false;
			SetInstructorInfo(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			SetTraineeInfo(0, 0, 0, 0, 0);
		}
		else if (!m_RefNpc.HasOccupation)
		{
			SetServantInfo(m_RefNpc.Name, m_RefNpc.Sex, m_RefNpc.Health, m_RefNpc.HealthMax, m_RefNpc.Stamina, m_RefNpc.Stamina_max, m_RefNpc.Hunger, m_RefNpc.Hunger_max, m_RefNpc.Comfort, m_RefNpc.Comfort_max, m_RefNpc.Oxygen, m_RefNpc.Oxygen_max, m_RefNpc.Shield, m_RefNpc.Shield_max, m_RefNpc.Energy, m_RefNpc.Energy_max, m_RefNpc.Attack, m_RefNpc.Defense);
		}
		if (m_RefNpc == null)
		{
			return;
		}
		if (m_RefNpc.HasOccupation)
		{
			if (m_UpgradePage.activeSelf)
			{
				if (m_RefNpc.State == 10)
				{
					mInstructorStats = null;
					mInstructorFace_Stats.mainTexture = null;
					mInstructorFace_Stats.enabled = false;
					SetInstructorInfo(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
				}
				else if (m_RefNpc.State != 10)
				{
					if (mInstructorStats != m_RefNpc)
					{
						mInstructorStats = m_RefNpc;
					}
					mInstructorFace_Stats.mainTexture = m_RefNpc.RandomNpcFace;
					mInstructorFace_Stats.enabled = true;
					SetInstructorInfo(m_RefNpc.AddHealth, m_RefNpc.AddStrength, m_RefNpc.AddHunger, m_RefNpc.AddStamina, m_RefNpc.AddOxygen);
				}
			}
			else if (m_LearnSkillPage.activeSelf)
			{
				mInstructorFace_Skill.mainTexture = m_RefNpc.RandomNpcFace;
				mInstructorFace_Skill.enabled = true;
			}
		}
		else
		{
			if (m_RefNpc.HasOccupation)
			{
				return;
			}
			if (m_UpgradePage.activeSelf)
			{
				if (m_RefNpc.State == 10)
				{
					mTraineeStats = null;
					mTraineeFace_Stats.mainTexture = null;
					mTraineeFace_Stats.enabled = false;
					SetTraineeInfo(0, 0, 0, 0, 0);
				}
				else if (m_RefNpc.State != 10)
				{
					if (mTraineeStats != m_RefNpc)
					{
						mTraineeStats = m_RefNpc;
					}
					mTraineeFace_Stats.mainTexture = m_RefNpc.RandomNpcFace;
					mTraineeFace_Stats.enabled = true;
					SetTraineeInfo(m_RefNpc.HealthMax, m_RefNpc.Strength_max, m_RefNpc.Hunger_max, m_RefNpc.Stamina_max, m_RefNpc.Oxygen_max);
				}
			}
			else if (m_LearnSkillPage.activeSelf && m_RefNpc.OwnSkills.Count != 5)
			{
				if (mTraineeSkill != m_RefNpc)
				{
					mTraineeSkill = m_RefNpc;
				}
				Debug.Log("学员技能学习的头像该有");
				mTraineeFace_Skill.mainTexture = m_RefNpc.RandomNpcFace;
				mTraineeFace_Skill.enabled = true;
				Debug.Log(mTraineeFace_Skill.mainTexture);
			}
		}
	}

	public void OnDropItem_InterPackage(Grid_N grid)
	{
		Debug.Log("丢掉物品");
	}

	public void OnLeftMouseCliked_InterPackage(Grid_N grid)
	{
		Debug.Log("点击左键");
	}

	public void OnRightMouseCliked_InterPackage(Grid_N grid)
	{
		Debug.Log("点击右键");
	}

	private void InitGrid()
	{
		mInteractionList = new List<Grid_N>();
		for (int i = 0; i < mInteractionGridCount; i++)
		{
			mInteractionList.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mInteractionList[i].gameObject.name = "Interaction" + i;
			mInteractionList[i].transform.parent = mTsInteractionGrids;
			if (i < 5)
			{
				mInteractionList[i].transform.localPosition = new Vector3(i * 60, 0f, 0f);
			}
			else
			{
				mInteractionList[i].transform.localPosition = new Vector3((i - 5) * 60, -55f, 0f);
			}
			mInteractionList[i].transform.localRotation = Quaternion.identity;
			mInteractionList[i].transform.localScale = Vector3.one;
			mInteractionList[i].SetItemPlace(ItemPlaceType.IPT_ServantInteraction, i);
			Grid_N grid_N = mInteractionList[i];
			grid_N.onDropItem = (Grid_N.GridDelegate)Delegate.Combine(grid_N.onDropItem, new Grid_N.GridDelegate(OnDropItem_InterPackage));
			Grid_N grid_N2 = mInteractionList[i];
			grid_N2.onLeftMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N2.onLeftMouseClicked, new Grid_N.GridDelegate(OnLeftMouseCliked_InterPackage));
			Grid_N grid_N3 = mInteractionList[i];
			grid_N3.onRightMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N3.onRightMouseClicked, new Grid_N.GridDelegate(OnRightMouseCliked_InterPackage));
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

	private void CreatSkill1()
	{
		skilllistTest.Clear();
		for (int i = 0; i < 5; i++)
		{
			CSUIMySkill cSUIMySkill = new CSUIMySkill();
			cSUIMySkill.name = "skill" + (i + 1);
			cSUIMySkill.iconImg = "npc_big_GerdyHooke";
			skilllistTest.Add(cSUIMySkill);
			Debug.Log(cSUIMySkill.name);
		}
		Debug.Log("skilllistTest" + skilllistTest.Count);
		Debug.Log("技能生成完成");
	}

	private void CreatSkill2()
	{
		skilllistTest.Clear();
		for (int i = 0; i < 5; i++)
		{
			CSUIMySkill cSUIMySkill = new CSUIMySkill();
			cSUIMySkill.name = "0skill" + (i + 1);
			cSUIMySkill.iconImg = "npc_big_GerdyHooke";
			skilllistTest.Add(cSUIMySkill);
			Debug.Log(cSUIMySkill.name);
		}
		Debug.Log("skilllistTest" + skilllistTest.Count);
		Debug.Log("技能生成完成");
	}

	public void GetAllRandomNpcsMethod(List<CSUIMyNpc> _allNpcs)
	{
		if (_allNpcs.Count > 0)
		{
			allRandomNpcs = _allNpcs;
		}
		Debug.Log("allRandomNpcs1:" + allRandomNpcs.Count);
	}

	public void AddPersonnel(CSUIMyNpc npc)
	{
		if (npc == null)
		{
			Debug.LogWarning("The giving npc is null.");
		}
		if (npc.IsRandom)
		{
			if (!npc.HasOccupation)
			{
				CSUI_MyNpcItem item = CreateNPCGird(npc, m_TraineeRootUI.transform);
				m_TraineeRootUI.repositionNow = true;
				m_TraineeGrids.Add(item);
			}
			else
			{
				CSUI_MyNpcItem item2 = CreateNPCGird(npc, m_InstructorRootUI.transform);
				m_InstructorRootUI.repositionNow = true;
				m_InstructorGrids.Add(item2);
			}
		}
	}

	public void RemovePersonnel(CSUIMyNpc npc)
	{
		if (npc == null)
		{
			Debug.LogWarning("The giving npc is null");
		}
		if (!npc.IsRandom)
		{
			return;
		}
		if (!npc.HasOccupation)
		{
			int num = m_TraineeGrids.FindIndex((CSUI_MyNpcItem item0) => item0.m_Npc == npc);
			if (num != -1)
			{
				if (m_TraineeGrids[num].gameObject.GetComponent<UICheckbox>().isChecked)
				{
					if (m_TraineeGrids.Count == 1)
					{
						ActiveNpcGrid = null;
					}
					else
					{
						int index = ((num == 0) ? 1 : (num - 1));
						m_TraineeGrids[index].gameObject.GetComponent<UICheckbox>().isChecked = true;
					}
				}
				UnityEngine.Object.DestroyImmediate(m_TraineeGrids[num].gameObject);
				m_TraineeGrids.RemoveAt(num);
				m_TraineeRootUI.repositionNow = true;
			}
			else
			{
				Debug.LogWarning("The giving npc is not a Settler");
			}
			return;
		}
		int num2 = m_InstructorGrids.FindIndex((CSUI_MyNpcItem item0) => item0.m_Npc == npc);
		if (num2 != -1)
		{
			if (m_InstructorGrids[num2].gameObject.GetComponent<UICheckbox>().isChecked)
			{
				if (m_InstructorGrids.Count == 1)
				{
					ActiveNpcGrid = null;
				}
				else
				{
					int index2 = ((num2 == 0) ? 1 : (num2 - 1));
					m_InstructorGrids[index2].gameObject.GetComponent<UICheckbox>().isChecked = true;
				}
			}
			UnityEngine.Object.DestroyImmediate(m_InstructorGrids[num2].gameObject);
			m_InstructorGrids.RemoveAt(num2);
			m_InstructorRootUI.repositionNow = true;
		}
		else
		{
			Debug.LogWarning("The giving npc is not a Settler");
		}
	}
}
