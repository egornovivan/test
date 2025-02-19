using UnityEngine;

public class HelperLabel_N : MonoBehaviour
{
	private static HelperLabel_N mInstance;

	private UILabel mLabel;

	private float mShowTime = 10f;

	private float mElapseTime;

	public static HelperLabel_N Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mLabel = GetComponent<UILabel>();
	}

	private void Start()
	{
		mLabel.text = PELocalization.GetString(8000106);
	}

	public void SetText(string text)
	{
		mLabel.text = text;
		mElapseTime = 0f;
		mLabel.color = Color.white;
	}
}
