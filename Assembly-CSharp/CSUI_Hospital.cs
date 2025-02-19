using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_Hospital : MonoBehaviour
{
	public delegate void MedicineDragDel(ItemObject _io, bool _inorout);

	public delegate void MedicineRealOp(ItemPackage _ip, bool _isMis, int _tabIndex, int _index, int _instanceId, bool _inorout);

	private static List<int> MedicineLis = new List<int> { 27, 81 };

	private static CSUI_Hospital m_Instance;

	public UISprite m_CheckSpr;

	public UISprite m_TreatSpr;

	public UISprite m_TentSpr;

	public CSUI_NpcGridItem m_ExamineDoc;

	public CSUI_NpcGridItem m_TreatDoc;

	public CSUI_NpcGridItem m_Nurse;

	public CSUI_NpcGridItem m_ExaminedPatient;

	public UILabel ExamineResultLabel;

	public UILabel m_CheckTimeLabel;

	private int _minute;

	private int _second;

	public CSUI_MedicineGrid m_MedicineAboutCheck1;

	public CSUI_MedicineGrid m_MedicineAboutCheck2;

	public UILabel TreatmentLabel;

	public CSUI_NpcGridItem m_TreatmentPatient;

	public UILabel m_TreatTimeLabel;

	private int _minute1;

	private int _second1;

	public CSUI_MedicineGrid m_MedicineAboutTreat_Show;

	public CSUI_MedicineGrid m_MedicineAboutTreat_Use;

	public MedicineRealOp mMedicineRealOp;

	private bool _mission;

	public UIGrid m_Grid;

	private List<CSUI_NpcGridItem> m_PatientGrids = new List<CSUI_NpcGridItem>();

	public CSUI_NpcGridItem m_NpcGridPrefab;

	public UICheckbox m_Checkbox;

	public UICheckbox m_Treatbox;

	public UICheckbox m_Tentbox;

	private List<CSEntity> m_hospitalEnties;

	public int m_CheckedPartType = 12;

	private List<CSTreatment> MedicalHistoryList = new List<CSTreatment>();

	private int m_CurrentPageIndex = 1;

	private int m_PageCount = 2;

	public CSUI_MedicalHistoryItem[] m_MedicalHistoryItem;

	public static CSUI_Hospital Instance => m_Instance;

	public CSPersonnel ExamineDoc
	{
		set
		{
			if (m_ExamineDoc != null)
			{
				m_ExamineDoc.m_Npc = value;
			}
		}
	}

	public CSPersonnel TreatDoc
	{
		set
		{
			if (m_TreatDoc != null)
			{
				m_TreatDoc.m_Npc = value;
			}
		}
	}

	public CSPersonnel Nurse
	{
		set
		{
			if (m_Nurse != null)
			{
				m_Nurse.m_Npc = value;
			}
		}
	}

	public CSPersonnel ExaminedPatient
	{
		set
		{
			m_ExaminedPatient.m_Npc = value;
		}
	}

	public CSPersonnel TreatmentPatient
	{
		set
		{
			m_TreatmentPatient.m_Npc = value;
		}
	}

	public event MedicineDragDel MedicineDragEvent;

	public static bool ItemCheck(int _id)
	{
		if (MedicineLis.Contains(_id))
		{
			return true;
		}
		return false;
	}

	public void SetCheckIcon()
	{
		if (m_CheckSpr != null)
		{
			m_CheckSpr.spriteName = "element_building_medicalcheck";
			m_CheckSpr.MakePixelPerfect();
		}
	}

	public void SetTreatIcon()
	{
		if (m_TreatSpr != null)
		{
			m_TreatSpr.spriteName = "task_medical_lab";
			m_TreatSpr.MakePixelPerfect();
		}
	}

	public void SetTentIcon()
	{
		if (m_TentSpr != null)
		{
			m_TentSpr.spriteName = "task_quarantine_tent";
			m_TentSpr.MakePixelPerfect();
		}
	}

	public void ClearCheckIcon()
	{
		if (m_CheckSpr != null)
		{
			m_CheckSpr.spriteName = "Null";
			m_CheckSpr.MakePixelPerfect();
		}
		ExamineDoc = null;
		ExaminedPatient = null;
		CheckTimeShow(0f);
	}

	public void ClearTreatIcon()
	{
		if (m_TreatSpr != null)
		{
			m_TreatSpr.spriteName = "Null";
			m_TreatSpr.MakePixelPerfect();
		}
		TreatDoc = null;
		TreatmentPatient = null;
		TreatTimeShow(0f);
	}

	public void ClearTentIcon()
	{
		if (m_TentSpr != null)
		{
			m_TentSpr.spriteName = "Null";
			m_TentSpr.MakePixelPerfect();
		}
		Nurse = null;
	}

	private void ClearMachineIcon()
	{
		ClearCheckIcon();
		ClearTreatIcon();
		ClearTentIcon();
	}

	public void CheckTimeShow(float _time)
	{
		if (m_CheckTimeLabel != null)
		{
			_minute = (int)(_time / 60f);
			_second = (int)(_time - (float)(_minute * 60));
			m_CheckTimeLabel.text = TimeTransition(_minute).ToString() + ":" + TimeTransition(_second).ToString();
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

	public void TreatTimeShow(float _time)
	{
		if (m_TreatTimeLabel != null)
		{
			_minute1 = (int)(_time / 60f);
			_second1 = (int)(_time - (float)(_minute1 * 60));
			m_TreatTimeLabel.text = TimeTransition(_minute1).ToString() + ":" + TimeTransition(_second1).ToString();
		}
	}

	public void TreatMedicineShow(ItemIdCount _ic)
	{
		if (!(m_MedicineAboutTreat_Show == null))
		{
			if (!m_MedicineAboutTreat_Show.transform.parent.gameObject.activeSelf)
			{
				m_MedicineAboutTreat_Show.transform.parent.gameObject.SetActive(value: true);
			}
			ItemSample itemSample = new ItemSample();
			itemSample.protoId = _ic.protoId;
			m_MedicineAboutTreat_Show.m_Grid.SetItem(itemSample);
			m_MedicineAboutTreat_Show.NeedCnt = _ic.count;
		}
	}

	public void ClearTreatMedicine()
	{
		if (!(m_MedicineAboutTreat_Show == null))
		{
			m_MedicineAboutTreat_Show.m_Grid.SetItem(null);
		}
	}

	private void OnPutMedicineIn(Grid_N grid)
	{
		if (this.MedicineDragEvent != null)
		{
			this.MedicineDragEvent(grid.ItemObj, _inorout: true);
			m_MedicineAboutTreat_Show.m_Grid.mItemspr.enabled = false;
		}
	}

	private void OnPutMedicineOut(Grid_N grid)
	{
		if (this.MedicineDragEvent != null)
		{
			this.MedicineDragEvent(grid.ItemObj, _inorout: false);
			m_MedicineAboutTreat_Show.m_Grid.mItemspr.enabled = true;
		}
	}

	private void MedRealOp(Grid_N grid)
	{
		if (grid.ItemObj != null || !ItemCheck(SelectItem_N.Instance.ItemObj.protoData.itemClassId))
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		UIItemPackageCtrl mItemPackageCtrl = GameUI.Instance.mItemPackageCtrl;
		ItemPackage itemPackage = mItemPackageCtrl.ItemPackage;
		if (itemPackage != null)
		{
			_mission = mItemPackageCtrl.isMission;
			int num = 0;
			num = ((!_mission) ? mItemPackageCtrl.CurrentPickTab : 0);
			int index = SelectItem_N.Instance.Index;
			if (mMedicineRealOp != null)
			{
				mMedicineRealOp(itemPackage, _mission, num, index, SelectItem_N.Instance.ItemObj.instanceId, _inorout: true);
			}
		}
	}

	public void SetLocalGrid(ItemObject obj, bool inorout = true)
	{
		if (inorout)
		{
			m_MedicineAboutTreat_Use.m_Grid.SetItem(obj);
			SelectItem_N.Instance.SetItem(null);
			OnPutMedicineIn(m_MedicineAboutTreat_Use.m_Grid);
		}
		else
		{
			OnPutMedicineOut(m_MedicineAboutTreat_Use.m_Grid);
			m_MedicineAboutTreat_Use.m_Grid.SetItem(obj);
			SelectItem_N.Instance.SetItem(null);
		}
	}

	public void RefreshPatientGrids(List<CSPersonnel> _patients)
	{
		if (CSUI_MainWndCtrl.Instance == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < _patients.Count; i++)
		{
			if (num < m_PatientGrids.Count)
			{
				m_PatientGrids[num].m_Npc = _patients[i];
			}
			else
			{
				CSUI_NpcGridItem item = _createNPCGird(_patients[i], m_Grid.transform);
				m_PatientGrids.Add(item);
			}
			num++;
		}
		if (num < m_PatientGrids.Count)
		{
			int num2 = num;
			while (num2 < m_PatientGrids.Count)
			{
				Object.DestroyImmediate(m_PatientGrids[num2].gameObject);
				m_PatientGrids.RemoveAt(num2);
			}
		}
		m_Grid.repositionNow = true;
	}

	private CSUI_NpcGridItem _createNPCGird(CSPersonnel npc, Transform root)
	{
		CSUI_NpcGridItem cSUI_NpcGridItem = Object.Instantiate(m_NpcGridPrefab);
		cSUI_NpcGridItem.transform.parent = root;
		CSUtils.ResetLoacalTransform(cSUI_NpcGridItem.transform);
		cSUI_NpcGridItem.m_UseDeletebutton = false;
		cSUI_NpcGridItem.m_Npc = npc;
		UICheckbox component = cSUI_NpcGridItem.gameObject.GetComponent<UICheckbox>();
		component.radioButtonRoot = root;
		return cSUI_NpcGridItem;
	}

	public void UpdateNpcGridTime(CSPersonnel _npc, float _seconds)
	{
		if (m_PatientGrids.Count <= 0)
		{
			return;
		}
		foreach (CSUI_NpcGridItem patientGrid in m_PatientGrids)
		{
			if (patientGrid.m_Npc == _npc)
			{
				patientGrid.NpcGridTimeShow(_seconds);
			}
		}
	}

	private void Awake()
	{
		m_Instance = this;
	}

	private void Start()
	{
		m_MedicineAboutTreat_Use.m_Grid.transform.FindChild("Bg").gameObject.SetActive(value: false);
		m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(forbiden: false);
		m_MedicineAboutTreat_Use.mRealOp += MedRealOp;
	}

	private void Update()
	{
		if (m_MedicineAboutTreat_Use.m_Grid != null && m_MedicineAboutTreat_Use.m_Grid.Item != null && m_MedicineAboutTreat_Use.m_Grid.Item.GetCount() <= 0)
		{
			m_MedicineAboutTreat_Use.m_Grid.SetItem(null);
		}
		if (m_MedicineAboutTreat_Show.m_Grid.Item == null && m_MedicineAboutTreat_Use.m_Grid.Item == null)
		{
			m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(forbiden: false);
		}
		else if (m_MedicineAboutTreat_Show.m_Grid.Item != null && m_MedicineAboutTreat_Use.m_Grid.Item != null)
		{
			m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(forbiden: false);
		}
		else if (m_MedicineAboutTreat_Show.m_Grid.Item == null && m_MedicineAboutTreat_Use.m_Grid.Item != null)
		{
			m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(forbiden: false);
		}
		else if (m_MedicineAboutTreat_Show.m_Grid.Item != null && m_MedicineAboutTreat_Use.m_Grid.Item == null)
		{
			m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(forbiden: true);
		}
	}

	private void OnActivate(bool active)
	{
		if (active)
		{
			GetMedicalHistoryList();
			Test();
		}
	}

	private void OnEnable()
	{
	}

	public void RefleshMechine(int csConstType, List<CSEntity> csEntities, CSEntity selectEnity)
	{
		m_hospitalEnties = csEntities;
		if (selectEnity != null)
		{
			m_CheckedPartType = selectEnity.m_Type;
			CSUI_MainWndCtrl.Instance.mSelectedEnntity = selectEnity;
			return;
		}
		int mechineIndex = GetMechineIndex(csConstType);
		if (mechineIndex >= 0)
		{
			m_CheckedPartType = csConstType;
			CSUI_MainWndCtrl.Instance.mSelectedEnntity = m_hospitalEnties[mechineIndex];
		}
	}

	public int GetMechineIndex(int csConstType)
	{
		if (m_hospitalEnties == null || m_hospitalEnties.Count <= 0)
		{
			return -1;
		}
		for (int i = 0; i < m_hospitalEnties.Count; i++)
		{
			if (csConstType == m_hospitalEnties[i].m_Type)
			{
				return i;
			}
		}
		return -1;
	}

	private void OnCheckActive(bool active)
	{
		int mechineIndex = GetMechineIndex(12);
		if (mechineIndex >= 0)
		{
			m_Checkbox.isChecked = active;
			RefleshMechine(12, m_hospitalEnties, m_hospitalEnties[mechineIndex]);
		}
	}

	private void OnTreatActive(bool active)
	{
		int mechineIndex = GetMechineIndex(13);
		if (mechineIndex >= 0)
		{
			m_Treatbox.isChecked = active;
			RefleshMechine(13, m_hospitalEnties, m_hospitalEnties[mechineIndex]);
		}
	}

	private void OnTentActive(bool active)
	{
		int mechineIndex = GetMechineIndex(14);
		if (mechineIndex >= 0)
		{
			m_Tentbox.isChecked = active;
			RefleshMechine(14, m_hospitalEnties, m_hospitalEnties[mechineIndex]);
		}
	}

	private void Test()
	{
		MedicalHistoryList.Clear();
		for (int i = 0; i < 7; i++)
		{
			CSTreatment cSTreatment = new CSTreatment();
			cSTreatment.npcName = "LongLongName" + i;
			cSTreatment.diseaseName = "dis" + i;
			cSTreatment.treatName = "fangan" + i;
			MedicalHistoryList.Add(cSTreatment);
		}
	}

	private void GetMedicalHistoryList()
	{
		if (CSMain.GetTreatmentList() != null)
		{
			MedicalHistoryList = CSMain.GetTreatmentList();
		}
	}

	private void OnFirstPageClick(bool active)
	{
		if (active)
		{
			m_CurrentPageIndex = 1;
			GetMedicalHistoryList();
			RefreshMedicalHistory();
		}
	}

	private void OnSecondPageClick(bool active)
	{
		if (active)
		{
			m_CurrentPageIndex = 2;
			GetMedicalHistoryList();
			RefreshMedicalHistory();
		}
	}

	private void OnThirdPageClick(bool active)
	{
		if (active)
		{
			m_CurrentPageIndex = 3;
			GetMedicalHistoryList();
			RefreshMedicalHistory();
		}
	}

	private void OnForthPageClick(bool active)
	{
		if (active)
		{
			m_CurrentPageIndex = 4;
			GetMedicalHistoryList();
			RefreshMedicalHistory();
		}
	}

	private void RefreshMedicalHistory()
	{
		if (m_MedicalHistoryItem == null || m_MedicalHistoryItem.Length <= 0)
		{
			return;
		}
		ClearMedicalHistory();
		if (MedicalHistoryList == null || MedicalHistoryList.Count <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = (m_CurrentPageIndex - 1) * m_PageCount; i < MedicalHistoryList.Count && i < m_PageCount * m_CurrentPageIndex; i++)
		{
			if (num >= m_MedicalHistoryItem.Length)
			{
				break;
			}
			if (MedicalHistoryList[i] != null && MedicalHistoryList[i].medicineList.Count > 0)
			{
				if (PeSingleton<ItemProto.Mgr>.Instance == null)
				{
					break;
				}
				ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(MedicalHistoryList[i].medicineList[0].protoId);
				if (itemProto != null && itemProto.icon.Length > 0)
				{
					m_MedicalHistoryItem[num].SetInfo(MedicalHistoryList[i].npcName, MedicalHistoryList[i].diseaseName, MedicalHistoryList[i].treatName, MedicalHistoryList[i].needTreatTimes, itemProto.icon[0], MedicalHistoryList[i].medicineList[0].count);
					num++;
				}
			}
		}
	}

	private void ClearMedicalHistory()
	{
		CSUI_MedicalHistoryItem[] medicalHistoryItem = m_MedicalHistoryItem;
		foreach (CSUI_MedicalHistoryItem cSUI_MedicalHistoryItem in medicalHistoryItem)
		{
			cSUI_MedicalHistoryItem.ClearInfo();
		}
	}

	public void RefreshGrid()
	{
		GetMedicalHistoryList();
		RefreshMedicalHistory();
	}
}
