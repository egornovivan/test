using UnityEngine;

public class OptionUIHintItem : MonoBehaviour
{
	public UILabel mLabel;

	public void SetHintInfo(string _info)
	{
		mLabel.text = _info;
	}

	public Bounds GetBounds()
	{
		Bounds result = default(Bounds);
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(mLabel.transform);
		Vector3 localScale = mLabel.transform.localScale;
		bounds.min = Vector3.Scale(bounds.min, localScale);
		bounds.max = Vector3.Scale(bounds.max, localScale);
		bounds.center = mLabel.transform.localPosition;
		result.Encapsulate(bounds);
		return result;
	}
}
