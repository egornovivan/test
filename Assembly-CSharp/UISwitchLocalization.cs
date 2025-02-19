using UnityEngine;

public class UISwitchLocalization : MonoBehaviour
{
	private void Start()
	{
		UIPopupList[] componentsInChildren = GetComponentsInChildren<UIPopupList>(includeInactive: true);
		UIPopupList[] array = componentsInChildren;
		foreach (UIPopupList uIPopupList in array)
		{
			for (int j = 0; j < uIPopupList.items.Count; j++)
			{
				uIPopupList.items[j] = uIPopupList.items[j].ToLocalizationString();
				uIPopupList.selection = uIPopupList.selection.ToLocalizationString();
				uIPopupList.textLabel.MakePixelPerfect();
			}
		}
		UILabel[] componentsInChildren2 = GetComponentsInChildren<UILabel>(includeInactive: true);
		UILabel[] array2 = componentsInChildren2;
		foreach (UILabel uILabel in array2)
		{
			uILabel.text = uILabel.text.ToLocalizationString();
			uILabel.MakePixelPerfect();
		}
	}
}
