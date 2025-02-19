using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class CSUI_Personnel : MonoBehaviour
{
	public enum TypeEnu
	{
		MainLine,
		Other
	}

	private enum ENPCGridType
	{
		Dweller = 0,
		Worker = 1,
		Farmer = 3,
		Soldier = 2,
		Follower = 4,
		Processor = 5,
		Doctor = 6,
		Instructor = 7,
		All = 8
	}

	private const int NPC_GRID_COUNT = 11;

	private TypeEnu m_Type;

	private CSUI_NPCGrid m_ActiveNpcGrid;

	public CSUI_NPCInfo m_NPCInfoUI;

	public CSUI_NPCEquip m_NPCEquipUI;

	public CSUI_NPCOccupation m_NPCOccupaUI;

	public CSUI_NPCWorker m_NPCWorkerUI;

	public CSUI_NPCFarmer m_NPCFarmerUI;

	public CSUI_NPCSoldier m_NPCSoldierUI;

	public CSUI_NPCFollower m_NPCFollowerUI;

	public CSUI_Processor m_NPCProcessorUI;

	public CSUI_NpcDoctor m_NpcDoctorUI;

	public CSUI_NpcInstructor m_NpcInstructor;

	public UIGrid m_MainLineRootUI;

	public UIGrid m_OtherRootUI;

	[SerializeField]
	private CSUI_NPCGrid m_NpcGridPrefab;

	private List<CSUI_NPCGrid> m_MainLineGrids = new List<CSUI_NPCGrid>();

	private List<CSUI_NPCGrid> m_OtherGrids = new List<CSUI_NPCGrid>();

	private ENPCGridType m_NPCType = ENPCGridType.All;

	private int mGridPageIndex;

	public bool Ishow => base.gameObject.activeInHierarchy;

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
			case TypeEnu.MainLine:
			{
				m_MainLineRootUI.gameObject.SetActive(value: true);
				m_OtherRootUI.gameObject.SetActive(value: false);
				_refreshNPCGrids();
				CSUI_NPCGrid activeNpcGrid2 = null;
				for (int j = 0; j < m_MainLineRootUI.transform.childCount; j++)
				{
					UICheckbox component2 = m_MainLineRootUI.transform.GetChild(j).gameObject.GetComponent<UICheckbox>();
					if (component2.isChecked)
					{
						activeNpcGrid2 = component2.gameObject.GetComponent<CSUI_NPCGrid>();
						break;
					}
				}
				ActiveNpcGrid = activeNpcGrid2;
				break;
			}
			case TypeEnu.Other:
			{
				m_MainLineRootUI.gameObject.SetActive(value: false);
				m_OtherRootUI.gameObject.SetActive(value: true);
				_refreshNPCGrids();
				CSUI_NPCGrid activeNpcGrid = null;
				for (int i = 0; i < m_OtherRootUI.transform.childCount; i++)
				{
					UICheckbox component = m_OtherRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
					if (component.isChecked)
					{
						activeNpcGrid = component.gameObject.GetComponent<CSUI_NPCGrid>();
						break;
					}
				}
				ActiveNpcGrid = activeNpcGrid;
				break;
			}
			}
		}
	}

	public CSUI_NPCGrid ActiveNpcGrid
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

	public CSPersonnel ActiveNPC
	{
		get
		{
			if (m_ActiveNpcGrid == null)
			{
				return null;
			}
			return m_ActiveNpcGrid.m_Npc;
		}
	}

	private void UpdateNPCRef(CSPersonnel npc)
	{
		m_NPCInfoUI.RefNpc = npc;
		m_NPCEquipUI.RefNpc = npc?.NPC;
		m_NPCOccupaUI.RefNpc = npc;
		m_NPCWorkerUI.RefNpc = npc;
		m_NPCFarmerUI.RefNpc = npc;
		m_NPCSoldierUI.RefNpc = npc;
		m_NPCFollowerUI.RefNpc = npc;
		m_NPCProcessorUI.RefNpc = npc;
		m_NpcDoctorUI.RefNpc = npc;
		m_NpcInstructor.RefNpc = npc;
		if (npc != null)
		{
			if (npc.m_Occupation == 1)
			{
				m_NPCWorkerUI.gameObject.SetActive(value: true);
				m_NPCFarmerUI.gameObject.SetActive(value: false);
				m_NPCSoldierUI.gameObject.SetActive(value: false);
				m_NPCFollowerUI.gameObject.SetActive(value: false);
				m_NPCProcessorUI.gameObject.SetActive(value: false);
				m_NpcDoctorUI.gameObject.SetActive(value: false);
				m_NpcInstructor.gameObject.SetActive(value: false);
			}
			else if (npc.m_Occupation == 3)
			{
				m_NPCWorkerUI.gameObject.SetActive(value: false);
				m_NPCFarmerUI.gameObject.SetActive(value: true);
				m_NPCSoldierUI.gameObject.SetActive(value: false);
				m_NPCFollowerUI.gameObject.SetActive(value: false);
				m_NPCProcessorUI.gameObject.SetActive(value: false);
				m_NpcDoctorUI.gameObject.SetActive(value: false);
				m_NpcInstructor.gameObject.SetActive(value: false);
			}
			else if (npc.m_Occupation == 2)
			{
				m_NPCWorkerUI.gameObject.SetActive(value: false);
				m_NPCFarmerUI.gameObject.SetActive(value: false);
				m_NPCSoldierUI.gameObject.SetActive(value: true);
				m_NPCFollowerUI.gameObject.SetActive(value: false);
				m_NPCProcessorUI.gameObject.SetActive(value: false);
				m_NpcDoctorUI.gameObject.SetActive(value: false);
				m_NpcInstructor.gameObject.SetActive(value: false);
			}
			else if (npc.m_Occupation == 4)
			{
				m_NPCWorkerUI.gameObject.SetActive(value: false);
				m_NPCFarmerUI.gameObject.SetActive(value: false);
				m_NPCSoldierUI.gameObject.SetActive(value: false);
				m_NPCFollowerUI.gameObject.SetActive(value: true);
				m_NPCProcessorUI.gameObject.SetActive(value: false);
				m_NpcDoctorUI.gameObject.SetActive(value: false);
				m_NpcInstructor.gameObject.SetActive(value: false);
			}
			else if (npc.m_Occupation == 5)
			{
				m_NPCWorkerUI.gameObject.SetActive(value: false);
				m_NPCFarmerUI.gameObject.SetActive(value: false);
				m_NPCSoldierUI.gameObject.SetActive(value: false);
				m_NPCFollowerUI.gameObject.SetActive(value: false);
				m_NPCProcessorUI.gameObject.SetActive(value: true);
				m_NpcDoctorUI.gameObject.SetActive(value: false);
				m_NpcInstructor.gameObject.SetActive(value: false);
			}
			else if (npc.m_Occupation == 6)
			{
				m_NPCWorkerUI.gameObject.SetActive(value: false);
				m_NPCFarmerUI.gameObject.SetActive(value: false);
				m_NPCSoldierUI.gameObject.SetActive(value: false);
				m_NPCFollowerUI.gameObject.SetActive(value: false);
				m_NPCProcessorUI.gameObject.SetActive(value: false);
				m_NpcDoctorUI.gameObject.SetActive(value: true);
				m_NpcInstructor.gameObject.SetActive(value: false);
			}
			else if (npc.m_Occupation == 7)
			{
				m_NPCWorkerUI.gameObject.SetActive(value: false);
				m_NPCFarmerUI.gameObject.SetActive(value: false);
				m_NPCSoldierUI.gameObject.SetActive(value: false);
				m_NPCFollowerUI.gameObject.SetActive(value: false);
				m_NPCProcessorUI.gameObject.SetActive(value: false);
				m_NpcDoctorUI.gameObject.SetActive(value: false);
				m_NpcInstructor.gameObject.SetActive(value: true);
			}
			else
			{
				m_NPCWorkerUI.gameObject.SetActive(value: false);
				m_NPCFarmerUI.gameObject.SetActive(value: false);
				m_NPCSoldierUI.gameObject.SetActive(value: false);
				m_NPCFollowerUI.gameObject.SetActive(value: false);
				m_NPCProcessorUI.gameObject.SetActive(value: false);
				m_NpcDoctorUI.gameObject.SetActive(value: false);
				m_NpcInstructor.gameObject.SetActive(value: false);
			}
			if (npc.m_Occupation != 0 && !NpcTypeDb.CanRun(npc.NPC.NpcCmpt.NpcControlCmdId, ENpcControlType.Work))
			{
				CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000703));
			}
		}
		else
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
	}

	public void AddPersonnel(CSPersonnel npc)
	{
		if (npc == null)
		{
			Debug.LogWarning("The giving npc is null.");
		}
		if (npc.IsRandomNpc)
		{
			CSUI_NPCGrid item = _createNPCGird(npc, m_OtherRootUI.transform);
			m_OtherRootUI.repositionNow = true;
			m_OtherGrids.Add(item);
		}
		else
		{
			CSUI_NPCGrid item2 = _createNPCGird(npc, m_MainLineRootUI.transform);
			m_MainLineRootUI.repositionNow = true;
			m_MainLineGrids.Add(item2);
		}
		GridRange();
	}

	private CSUI_NPCGrid _createNPCGird(CSPersonnel npc, Transform root)
	{
		CSUI_NPCGrid cSUI_NPCGrid = Object.Instantiate(m_NpcGridPrefab);
		cSUI_NPCGrid.transform.parent = root;
		CSUtils.ResetLoacalTransform(cSUI_NPCGrid.transform);
		cSUI_NPCGrid.m_UseDeletebutton = true;
		cSUI_NPCGrid.OnDestroySelf = OnNPCGridDestroySelf;
		cSUI_NPCGrid.m_Npc = npc;
		UICheckbox component = cSUI_NPCGrid.gameObject.GetComponent<UICheckbox>();
		component.radioButtonRoot = root;
		UIEventListener.Get(cSUI_NPCGrid.gameObject).onActivate = OnNPCGridActive;
		return cSUI_NPCGrid;
	}

	public void RemovePersonnel(CSPersonnel npc)
	{
		if (npc == null)
		{
			Debug.LogWarning("The giving npc is null");
		}
		List<CSUI_NPCGrid> list = null;
		list = ((!npc.IsRandomNpc) ? m_MainLineGrids : m_OtherGrids);
		int num = list.FindIndex((CSUI_NPCGrid item0) => item0.m_Npc == npc);
		if (num != -1)
		{
			bool isChecked = list[num].gameObject.GetComponent<UICheckbox>().isChecked;
			Object.DestroyImmediate(list[num].gameObject);
			list.RemoveAt(num);
			GridRange();
			if (isChecked)
			{
				if (list.Count > 0)
				{
					int num2 = mGridPageIndex * 11;
					int num3 = Mathf.Min(num2 + 11 - 1, list.Count - 1);
					int num4 = 0;
					num4 = ((num2 < num3) ? Mathf.Clamp(num, num2, num3) : num3);
					list[num4].gameObject.GetComponent<UICheckbox>().isChecked = true;
				}
				else
				{
					ActiveNpcGrid = null;
				}
			}
		}
		else
		{
			Debug.LogWarning("The giving npc is not a Settler");
		}
	}

	public void ResetUI()
	{
		foreach (CSUI_NPCGrid mainLineGrid in m_MainLineGrids)
		{
			Object.Destroy(mainLineGrid.gameObject);
		}
		foreach (CSUI_NPCGrid otherGrid in m_OtherGrids)
		{
			Object.Destroy(otherGrid.gameObject);
		}
		ActiveNpcGrid = null;
	}

	private void OnEnable()
	{
		UpdateNPCRef((!(m_ActiveNpcGrid == null)) ? m_ActiveNpcGrid.m_Npc : null);
	}

	private void OnDisable()
	{
	}

	private void Awake()
	{
		m_NPCWorkerUI.Init();
		m_NPCFarmerUI.Init();
		m_NPCSoldierUI.Init();
		m_NPCFollowerUI.Init();
		m_NPCProcessorUI.Init();
		m_NpcDoctorUI.Init();
		m_NpcInstructor.Init();
	}

	private void Start()
	{
		m_NPCOccupaUI.onSelectChange = OnOccupationSelectChange;
	}

	private void Update()
	{
		if (ActiveNPC != null)
		{
			m_NPCOccupaUI.Activate(ActiveNPC.Running);
			m_NPCWorkerUI.Activate(ActiveNPC.Running);
			m_NPCFarmerUI.Activate(ActiveNPC.Running);
			m_NPCSoldierUI.Activate(ActiveNPC.Running);
			m_NPCFollowerUI.Activate(ActiveNPC.Running);
			m_NPCProcessorUI.Activate(ActiveNPC.Running);
			m_NpcDoctorUI.Activate(ActiveNPC.Running);
			m_NpcInstructor.Activate(ActiveNPC.Running);
		}
	}

	private void OnNPCGridActive(GameObject go, bool actvie)
	{
		if (actvie)
		{
			ActiveNpcGrid = go.GetComponent<CSUI_NPCGrid>();
		}
	}

	private void OnOccupationSelectChange(string item)
	{
		if (item == CSUtils.GetOccupaName(1))
		{
			m_NPCWorkerUI.gameObject.SetActive(value: true);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
		else if (item == CSUtils.GetOccupaName(3))
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: true);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
		else if (item == CSUtils.GetOccupaName(2))
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: true);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
		else if (item == CSUtils.GetOccupaName(4))
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: true);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
		else if (item == CSUtils.GetOccupaName(5))
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: true);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
		else if (item == CSUtils.GetOccupaName(6))
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: true);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
		else if (item == CSUtils.GetOccupaName(7))
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: true);
		}
		else
		{
			m_NPCWorkerUI.gameObject.SetActive(value: false);
			m_NPCFarmerUI.gameObject.SetActive(value: false);
			m_NPCSoldierUI.gameObject.SetActive(value: false);
			m_NPCFollowerUI.gameObject.SetActive(value: false);
			m_NPCProcessorUI.gameObject.SetActive(value: false);
			m_NpcDoctorUI.gameObject.SetActive(value: false);
			m_NpcInstructor.gameObject.SetActive(value: false);
		}
	}

	private void OnNPCGridDestroySelf(CSUI_NPCGrid grid)
	{
		if (grid != null && grid.m_Npc != null)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000101), grid.m_Npc.KickOut);
		}
	}

	private void OnAllActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.All;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void OnFarmerActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.Farmer;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void OnFollowerActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.Follower;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void OnSoldierActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.Soldier;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void OnWorkerActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.Worker;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void OnProcessorActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.Processor;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void OnDoctorActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.Doctor;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void OnInstructorActivate(bool active)
	{
		if (active)
		{
			m_NPCType = ENPCGridType.Instructor;
			_refreshNPCGrids();
			mGridPageIndex = 0;
			GridRange();
		}
	}

	private void BtnGridLeftOnClick()
	{
		if (mGridPageIndex > 0)
		{
			mGridPageIndex--;
			GridRange();
		}
	}

	private void BtnGridRightOnClick()
	{
		List<CSUI_NPCGrid> list = null;
		list = ((Type != TypeEnu.Other) ? m_MainLineGrids : m_OtherGrids);
		int count = list.Count;
		if (count > (mGridPageIndex + 1) * 11)
		{
			mGridPageIndex++;
			GridRange();
		}
	}

	private void UpdateGridPos()
	{
		float num = -mGridPageIndex * 60;
		Transform transform = null;
		transform = ((Type != TypeEnu.Other) ? m_MainLineRootUI.transform : m_OtherRootUI.transform);
		transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(num, 0f, 0f), 0.2f);
		if (Mathf.Abs(transform.localPosition.x - num) < 1f)
		{
			transform.localPosition = new Vector3(num, 0f, 0f);
		}
	}

	private void GridRange()
	{
		List<CSUI_NPCGrid> list = null;
		list = ((Type != TypeEnu.Other) ? m_MainLineGrids : m_OtherGrids);
		foreach (CSUI_NPCGrid item in list)
		{
			item.gameObject.SetActive(value: false);
			item.OnActivate(active: false);
		}
		for (int i = mGridPageIndex * 11; i < list.Count && i < 11 * (mGridPageIndex + 1); i++)
		{
			list[i].gameObject.SetActive(value: true);
			if (list[i] == m_ActiveNpcGrid)
			{
				list[i].OnActivate(active: true);
			}
		}
		if (Type == TypeEnu.Other)
		{
			m_OtherRootUI.repositionNow = true;
		}
		else
		{
			m_MainLineRootUI.repositionNow = true;
		}
	}

	private void MainNpcOnActivate(bool active)
	{
		if (active)
		{
			Type = TypeEnu.MainLine;
		}
		GridRange();
	}

	private void OtherNpcOnActivate(bool active)
	{
		if (active)
		{
			Type = TypeEnu.Other;
		}
		GridRange();
	}

	public void OnCreatorAddPersennel(CSPersonnel personnel)
	{
		ENPCGridType nPCType = m_NPCType;
		if (nPCType == ENPCGridType.All)
		{
			AddPersonnel(personnel);
		}
		else if (m_NPCType == (ENPCGridType)personnel.Occupation)
		{
			AddPersonnel(personnel);
		}
	}

	public void OnCreatorRemovePersennel(CSPersonnel personnel)
	{
		ENPCGridType nPCType = m_NPCType;
		if (nPCType == ENPCGridType.All)
		{
			RemovePersonnel(personnel);
		}
		else if (m_NPCType == (ENPCGridType)personnel.Occupation)
		{
			RemovePersonnel(personnel);
		}
	}

	private void _refreshNPCGrids()
	{
		if (CSUI_MainWndCtrl.Instance == null)
		{
			return;
		}
		mGridPageIndex = 0;
		List<CSUI_NPCGrid> list = null;
		if (Type == TypeEnu.Other)
		{
			list = m_OtherGrids;
			m_OtherRootUI.gameObject.transform.localPosition = Vector3.zero;
			m_OtherRootUI.repositionNow = true;
		}
		else
		{
			list = m_MainLineGrids;
			m_MainLineRootUI.gameObject.transform.localPosition = Vector3.zero;
			m_MainLineRootUI.repositionNow = true;
		}
		CSPersonnel[] npcs = CSUI_MainWndCtrl.Instance.Creator.GetNpcs();
		int num = 0;
		if (m_NPCType == ENPCGridType.All)
		{
			for (int i = 0; i < npcs.Length; i++)
			{
				if (Type == TypeEnu.Other && npcs[i].IsRandomNpc)
				{
					if (num < list.Count)
					{
						list[num].m_Npc = npcs[i];
					}
					else
					{
						CSUI_NPCGrid item = _createNPCGird(npcs[i], m_OtherRootUI.transform);
						list.Add(item);
					}
					num++;
				}
				else if (Type == TypeEnu.MainLine && !npcs[i].IsRandomNpc)
				{
					if (num < list.Count)
					{
						list[num].m_Npc = npcs[i];
					}
					else
					{
						CSUI_NPCGrid item2 = _createNPCGird(npcs[i], m_MainLineRootUI.transform);
						list.Add(item2);
					}
					num++;
				}
			}
		}
		else
		{
			for (int j = 0; j < npcs.Length; j++)
			{
				if (m_NPCType != (ENPCGridType)npcs[j].Occupation)
				{
					continue;
				}
				if (Type == TypeEnu.Other && npcs[j].IsRandomNpc)
				{
					if (num < list.Count)
					{
						list[num].m_Npc = npcs[j];
					}
					else
					{
						CSUI_NPCGrid item3 = _createNPCGird(npcs[j], m_OtherRootUI.transform);
						list.Add(item3);
					}
					num++;
				}
				else if (Type == TypeEnu.MainLine && !npcs[j].IsRandomNpc)
				{
					if (num < list.Count)
					{
						list[num].m_Npc = npcs[j];
					}
					else
					{
						CSUI_NPCGrid item4 = _createNPCGird(npcs[j], m_MainLineRootUI.transform);
						list.Add(item4);
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
				Object.DestroyImmediate(list[num2].gameObject);
				list.RemoveAt(num2);
			}
		}
		if (list.Count != 0)
		{
			list[0].gameObject.GetComponent<UICheckbox>().isChecked = true;
			OnNPCGridActive(list[0].gameObject, actvie: true);
		}
		else
		{
			ActiveNpcGrid = null;
		}
	}
}
