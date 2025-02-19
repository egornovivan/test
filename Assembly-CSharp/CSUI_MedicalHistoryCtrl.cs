using System;
using UnityEngine;

public class CSUI_MedicalHistoryCtrl : MonoBehaviour
{
	[Serializable]
	public class ToolMenu
	{
		public GameObject mWnd;

		public UICheckbox mChcekedBox;

		[HideInInspector]
		public bool mTweenState;

		[HideInInspector]
		public Collider mCollider;

		[HideInInspector]
		public UIButtonTween mBtnTween;

		public void Init()
		{
			mCollider = mChcekedBox.GetComponent<BoxCollider>();
			mBtnTween = mChcekedBox.GetComponent<UIButtonTween>();
		}

		public void UpdateBtnTweenState()
		{
			if (!(mBtnTween == null) && !(mWnd == null) && !(mChcekedBox == null) && !(mCollider == null))
			{
				if (!mChcekedBox.isChecked && mWnd.activeSelf && mTweenState)
				{
					mBtnTween.Play(forward: false);
					mTweenState = false;
					mCollider.enabled = false;
				}
				else if (!mWnd.activeSelf)
				{
					mTweenState = true;
					mCollider.enabled = true;
				}
			}
		}
	}

	[SerializeField]
	public ToolMenu mMedicalHistoryMenu;

	private void Start()
	{
		mMedicalHistoryMenu.Init();
	}

	private void Update()
	{
		mMedicalHistoryMenu.UpdateBtnTweenState();
	}
}
