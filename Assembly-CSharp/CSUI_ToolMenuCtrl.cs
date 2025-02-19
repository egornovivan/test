using System;
using UnityEngine;

public class CSUI_ToolMenuCtrl : MonoBehaviour
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
	public ToolMenu mOptionMenu;

	[SerializeField]
	public ToolMenu mWorkerMenu;

	[SerializeField]
	private UISprite mMenuBg;

	[SerializeField]
	private UIButton mHelpBtn;

	private void Start()
	{
		mOptionMenu.Init();
		mWorkerMenu.Init();
	}

	private void Update()
	{
		mOptionMenu.UpdateBtnTweenState();
		mWorkerMenu.UpdateBtnTweenState();
		UpdateWorkMenuState();
	}

	private void UpdateWorkMenuState()
	{
		bool flag = IsShowOptionMenu();
		bool flag2 = IsShowWorkMenu();
		if (!flag && mOptionMenu.mChcekedBox.isChecked)
		{
			mOptionMenu.mChcekedBox.isChecked = false;
		}
		if (!flag2 && mWorkerMenu.mChcekedBox.isChecked)
		{
			mWorkerMenu.mChcekedBox.isChecked = false;
		}
		mOptionMenu.mChcekedBox.gameObject.SetActive(flag);
		mWorkerMenu.mChcekedBox.gameObject.SetActive(flag2);
		if (flag && flag2)
		{
			mMenuBg.transform.localScale = new Vector3(110f, 30f, 1f);
		}
		else if (flag)
		{
			mMenuBg.transform.localScale = new Vector3(75f, 30f, 1f);
		}
		else
		{
			mMenuBg.transform.localScale = new Vector3(40f, 30f, 1f);
		}
	}

	private bool IsShowWorkMenu()
	{
		return CSUI_MainWndCtrl.Instance.mWndPartTag switch
		{
			1 => false, 
			2 => true, 
			33 => false, 
			21 => false, 
			3 => true, 
			7 => true, 
			8 => true, 
			50 => false, 
			_ => false, 
		};
	}

	private bool IsShowOptionMenu()
	{
		int mWndPartTag = CSUI_MainWndCtrl.Instance.mWndPartTag;
		if (mWndPartTag == 50 || mWndPartTag == 21 || mWndPartTag == -1)
		{
			return false;
		}
		return true;
	}

	private void CloseOptionWnd()
	{
		mOptionMenu.mChcekedBox.isChecked = false;
	}

	private void CloseWorkWnd()
	{
		mWorkerMenu.mChcekedBox.isChecked = false;
	}
}
