using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManyPeopleItem : MonoBehaviour
{
	private enum SortType
	{
		None,
		LongToStart_End,
		longToCenter
	}

	[SerializeField]
	private List<UILabel> m_NameLabels;

	[SerializeField]
	private SortType m_SortType = SortType.LongToStart_End;

	public int CurLabelCount => (m_NameLabels != null) ? m_NameLabels.Count : 0;

	public void UpdateNames(params string[] nameArray)
	{
		UpdateNames(nameArray.ToList());
	}

	public void UpdateNames(List<string> nameArray)
	{
		if (nameArray == null || nameArray.Count <= 0)
		{
			return;
		}
		if (m_SortType != 0)
		{
			nameArray = TrySortByNameLength(nameArray);
		}
		for (int i = 0; i < m_NameLabels.Count; i++)
		{
			if (i < nameArray.Count)
			{
				m_NameLabels[i].text = nameArray[i];
				m_NameLabels[i].MakePixelPerfect();
			}
			else
			{
				Object.Destroy(m_NameLabels[i].gameObject);
			}
		}
	}

	private List<string> TrySortByNameLength(List<string> nameArray)
	{
		switch (m_SortType)
		{
		case SortType.LongToStart_End:
			if (nameArray.Count >= 3)
			{
				nameArray = nameArray.OrderByDescending((string a) => a.Length).ToList();
				string value2 = nameArray[1];
				nameArray[1] = nameArray[nameArray.Count - 1];
				nameArray[nameArray.Count - 1] = value2;
			}
			break;
		case SortType.longToCenter:
		{
			if (nameArray.Count < 3)
			{
				break;
			}
			int index = (int)((float)nameArray.Count * 0.5f);
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < nameArray.Count; i++)
			{
				if (nameArray[i].Length > num)
				{
					num2 = i;
					num = nameArray[i].Length;
				}
			}
			if (num2 != -1)
			{
				string value = nameArray[num2];
				nameArray[num2] = nameArray[index];
				nameArray[index] = value;
			}
			break;
		}
		}
		return nameArray;
	}
}
