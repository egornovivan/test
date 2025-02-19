using System;
using UnityEngine;

public class UIInputControl : MonoBehaviour
{
	public UIInput mInput;

	public UILabel mlabel;

	public UILabel mDefaultlabel;

	public BoxCollider mBoxCollider;

	public GameObject mPartentBg;

	public float OtherObjectWdith;

	private float mLabelWidth;

	private void Start()
	{
		if (null != mDefaultlabel)
		{
			mDefaultlabel.color = new Color32(180, 180, 180, byte.MaxValue);
			UIEventListener uIEventListener = UIEventListener.Get(mInput.gameObject);
			uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isSelect)
			{
				if (null != mDefaultlabel)
				{
					if (isSelect)
					{
						mDefaultlabel.gameObject.SetActive(value: false);
						mlabel.gameObject.SetActive(value: true);
					}
					else if (string.IsNullOrEmpty(mlabel.text))
					{
						mDefaultlabel.gameObject.SetActive(value: true);
						mlabel.gameObject.SetActive(value: false);
					}
				}
			});
		}
		else if (mInput != null && mlabel != null)
		{
			if (mInput.selected)
			{
				mlabel.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			}
			else
			{
				mlabel.color = new Color32(111, 111, 111, byte.MaxValue);
			}
		}
		if (mPartentBg != null)
		{
			mLabelWidth = mPartentBg.transform.localScale.x - OtherObjectWdith;
		}
	}

	private void Update()
	{
		if (!mInput.selected && null != mDefaultlabel && !mDefaultlabel.gameObject.activeSelf && string.IsNullOrEmpty(mlabel.text))
		{
			mDefaultlabel.gameObject.SetActive(value: true);
			mlabel.gameObject.SetActive(value: false);
		}
		if (!(mPartentBg != null))
		{
			return;
		}
		mLabelWidth = mPartentBg.transform.localScale.x - OtherObjectWdith;
		if (Convert.ToInt32(mLabelWidth) != mlabel.lineWidth - 10)
		{
			mlabel.lineWidth = Convert.ToInt32(mLabelWidth) - 10;
			if (mBoxCollider != null)
			{
				Vector3 size = mBoxCollider.size;
				mBoxCollider.size = new Vector3(mLabelWidth, size.y, size.z);
				mBoxCollider.center = new Vector3(mLabelWidth / 2f, 0f, -1.5f);
			}
		}
	}
}
