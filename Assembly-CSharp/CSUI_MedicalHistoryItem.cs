using System.Collections.Generic;
using UnityEngine;

public class CSUI_MedicalHistoryItem : MonoBehaviour
{
	public UILabel npcNameL;

	public UILabel diseaseNameL;

	public UILabel treatNameL;

	public UILabel needTreatTimes;

	public List<ItemIdCount> medicineList;

	public UISprite medicineIcon;

	public UILabel medicineNum;

	public GameObject root;

	public void SetInfo(string _npcName, string _diseaseName, string _treatName, int _treatTime, string _icon, int _medicineCount)
	{
		npcNameL.text = "Name:" + _npcName;
		diseaseNameL.text = "Disease:" + _diseaseName;
		treatNameL.text = "TreatmentPlan:" + _treatName;
		needTreatTimes.text = "Visits:" + _treatTime;
		medicineIcon.spriteName = _icon;
		medicineNum.text = "x" + _medicineCount;
		medicineIcon.MakePixelPerfect();
		root.SetActive(value: true);
	}

	public void ClearInfo()
	{
		npcNameL.text = string.Empty;
		diseaseNameL.text = string.Empty;
		treatNameL.text = string.Empty;
		needTreatTimes.text = string.Empty;
		medicineIcon.spriteName = "Null";
		medicineNum.text = string.Empty;
		medicineIcon.MakePixelPerfect();
		root.SetActive(value: false);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
