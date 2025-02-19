using System.Collections.Generic;
using UnityEngine;

public class ProfessionItem : MonoBehaviour
{
	[SerializeField]
	private UILabel m_TitleLabel;

	[SerializeField]
	private UIGrid m_Grid;

	[SerializeField]
	private GameObject m_2NameItemPrefab;

	[SerializeField]
	private GameObject m_4NameItemPrefab;

	[SerializeField]
	public int CellHeight;

	public bool TitleIsNullOrEmpty { get; private set; }

	private void UpdateTitleName(string titleName)
	{
		if (string.IsNullOrEmpty(titleName))
		{
			TitleIsNullOrEmpty = true;
			m_TitleLabel.gameObject.SetActive(value: false);
			Object.Destroy(m_TitleLabel.gameObject);
			m_Grid.transform.localPosition = Vector3.zero;
		}
		else
		{
			TitleIsNullOrEmpty = false;
			m_TitleLabel.text = titleName;
		}
	}

	private void UpdatePeoples(List<ManyPeopleName> manyPeopleList, bool isShow2Name)
	{
		if (manyPeopleList != null && manyPeopleList.Count > 0)
		{
			for (int i = 0; i < manyPeopleList.Count; i++)
			{
				ManyPeopleName manyPeopleName = manyPeopleList[i];
				ManyPeopleItem newManyPeopleItem = GetNewManyPeopleItem(manyPeopleName.NameList.Count, isShow2Name);
				newManyPeopleItem.name = "ManyPeopleName_" + i.ToString("D4");
				newManyPeopleItem.UpdateNames(manyPeopleName.NameList.ToArray());
			}
			m_Grid.cellHeight = CellHeight;
			m_Grid.Reposition();
		}
	}

	private ManyPeopleItem GetNewManyPeopleItem(int nameCount, bool isShow2Name)
	{
		GameObject gameObject = Object.Instantiate((!isShow2Name) ? m_4NameItemPrefab : m_2NameItemPrefab);
		gameObject.transform.parent = m_Grid.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		return gameObject.GetComponent<ManyPeopleItem>();
	}

	public void UpdateInfo(ProfessionInfo professionInfo, bool isShow2Name = true)
	{
		UpdateTitleName(professionInfo.ProfessionName);
		if (professionInfo.ManyPeopleList != null && professionInfo.ManyPeopleList.Count > 0)
		{
			UpdatePeoples(professionInfo.ManyPeopleList, isShow2Name);
		}
	}
}
