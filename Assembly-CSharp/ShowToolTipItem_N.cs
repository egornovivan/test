using UnityEngine;

public class ShowToolTipItem_N : MonoBehaviour
{
	public string mTipContent;

	public int mStrID;

	private void OnTooltip(bool show)
	{
		if (mStrID != 0)
		{
			UITooltip.ShowText(PELocalization.GetString(mStrID));
		}
		else if (mTipContent != string.Empty)
		{
			UITooltip.ShowText(mTipContent);
		}
		else
		{
			UITooltip.ShowText(null);
		}
	}
}
