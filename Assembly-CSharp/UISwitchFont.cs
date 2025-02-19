using System.Collections;
using UnityEngine;

public class UISwitchFont : MonoBehaviour
{
	private bool mInit;

	private IEnumerator Start()
	{
		while (mInit || !(UIFontMgr.Instance != null))
		{
			yield return 0;
		}
		mInit = true;
		UILabel[] findLabels = GetComponentsInChildren<UILabel>(includeInactive: true);
		UILabel[] array = findLabels;
		foreach (UILabel label in array)
		{
			label.font = UIFontMgr.Instance.GetFontForLanguage(label.font);
			label.MakePixelPerfect();
		}
		UIPopupList[] findPopList = GetComponentsInChildren<UIPopupList>(includeInactive: true);
		UIPopupList[] array2 = findPopList;
		foreach (UIPopupList pop in array2)
		{
			pop.font = UIFontMgr.Instance.GetFontForLanguage(pop.font);
			pop.textLabel.MakePixelPerfect();
		}
	}
}
