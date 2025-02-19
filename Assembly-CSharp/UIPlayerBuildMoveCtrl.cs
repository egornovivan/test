using System;
using UnityEngine;

public class UIPlayerBuildMoveCtrl : MonoBehaviour
{
	[Serializable]
	public class MoveMenu
	{
		public GameObject mWnd;

		public UICheckbox mChcekedBox;

		[HideInInspector]
		public Collider mCollider;

		[HideInInspector]
		public UIButtonTween mBtnTween;

		private Action<GameObject, bool> m_CheckBoxEvent;

		public void Init(Action<GameObject, bool> checkBoxEvent)
		{
			m_CheckBoxEvent = checkBoxEvent;
			mCollider = mChcekedBox.GetComponent<BoxCollider>();
			mBtnTween = mChcekedBox.GetComponent<UIButtonTween>();
			UIEventListener.Get(mCollider.gameObject).onActivate = CheckBoxEvent;
			mBtnTween.onFinished = TweenFinish;
		}

		private void TweenFinish(UITweener tween)
		{
			if (!mCollider.enabled)
			{
				mCollider.enabled = true;
			}
		}

		private void CheckBoxEvent(GameObject go, bool isActive)
		{
			if (!isActive)
			{
				mBtnTween.Play(forward: false);
			}
			mCollider.enabled = false;
			if (m_CheckBoxEvent != null)
			{
				m_CheckBoxEvent(go, isActive);
			}
		}
	}

	[HideInInspector]
	public int mCameraState;

	[SerializeField]
	public MoveMenu mMenu_Root;

	[SerializeField]
	public MoveMenu mMenu_Head;

	[SerializeField]
	public MoveMenu mMenu_Face;

	[SerializeField]
	public MoveMenu mMenu_Body;

	[SerializeField]
	public MoveMenu mMenu_Save;

	private UICheckbox m_CheckboxBack;

	private void Start()
	{
		mMenu_Root.Init(CheckBoxEvent);
		mMenu_Head.Init(CheckBoxEvent);
		mMenu_Face.Init(CheckBoxEvent);
		mMenu_Body.Init(CheckBoxEvent);
		mMenu_Save.Init(CheckBoxEvent);
	}

	private void CheckBoxEvent(GameObject go, bool isActive)
	{
		UICheckbox component = go.GetComponent<UICheckbox>();
		if (null != m_CheckboxBack && m_CheckboxBack != component)
		{
			m_CheckboxBack.isChecked = false;
		}
		if (isActive)
		{
			m_CheckboxBack = component;
		}
		if (isActive && (mMenu_Root.mChcekedBox == component || mMenu_Head.mChcekedBox == component || mMenu_Face.mChcekedBox == component))
		{
			mCameraState = 1;
		}
		else
		{
			mCameraState = 0;
		}
	}
}
