using UnityEngine;

public class BuildMenuItem : MonoBehaviour
{
	[SerializeField]
	private UICheckbox mCheckBox;

	private int mItemPos_x;

	private void Update()
	{
		if (mCheckBox != null)
		{
			if (mCheckBox.isChecked)
			{
				mItemPos_x = -15;
			}
			else
			{
				mItemPos_x = 10;
			}
		}
		Vector3 localPosition = base.gameObject.transform.localPosition;
		base.gameObject.transform.localPosition = Vector3.Lerp(localPosition, new Vector3(mItemPos_x, localPosition.y, localPosition.z), 0.1f);
		if (Mathf.Abs(localPosition.x - (float)mItemPos_x) < 1f)
		{
			base.gameObject.transform.localPosition = new Vector3(mItemPos_x, localPosition.y, localPosition.z);
		}
	}
}
