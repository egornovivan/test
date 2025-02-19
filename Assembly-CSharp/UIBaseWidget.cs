using AnimationOrTween;
using UnityEngine;

public abstract class UIBaseWidget : MonoBehaviour
{
	public delegate void WndEvent(UIBaseWidget widget = null);

	[HideInInspector]
	public bool mInit;

	[HideInInspector]
	public float mDepth;

	[HideInInspector]
	public bool bMouseMoveIn;

	public float mAlpha = 1f;

	public WndEvent e_OnInit;

	public WndEvent e_OnShow;

	public WndEvent e_OnHide;

	public WndEvent e_OnShowFinish;

	protected UIAlphaGroup[] mGroups;

	private bool m_IsRefresh;

	private UIEnum.WndType m_SelfWndType;

	[SerializeField]
	private TweenScale mTweener;

	[SerializeField]
	private UITweenBufferAlpha mAlphaTweener;

	public UIEnum.WndType SelfWndType
	{
		get
		{
			return m_SelfWndType;
		}
		protected set
		{
			m_SelfWndType = value;
		}
	}

	public virtual bool isShow
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			if (base.gameObject != null && base.gameObject.activeSelf != value)
			{
				if (value)
				{
					Show();
				}
				else
				{
					Hide();
				}
			}
		}
	}

	public virtual void OnCreate()
	{
		mTweener = base.gameObject.GetComponent<TweenScale>();
		if (mTweener != null)
		{
			mTweener.onFinished = OnTweenFinished;
		}
		mAlphaTweener = base.gameObject.GetComponent<UITweenBufferAlpha>();
		if (mAlphaTweener != null)
		{
			mAlphaTweener.onFinished = OnTweenFinished;
		}
	}

	public virtual void OnDelete()
	{
	}

	public virtual void Show()
	{
		if (!CanOpenWnd())
		{
			return;
		}
		if (!mInit)
		{
			InitWindow();
		}
		UpdateTween();
		PlayShowTween();
		PlayOpenSoundEffect();
		if (mInit)
		{
			if (base.gameObject != null)
			{
				base.gameObject.SetActive(value: true);
			}
			if (e_OnShow != null)
			{
				e_OnShow(this);
			}
		}
	}

	protected bool CanOpenWnd()
	{
		if (null != GameUI.Instance && GameUI.Instance.SystemUIIsOpen && this != GameUI.Instance.mSystemMenu && this != GameUI.Instance.mOption && this != GameUI.Instance.mSaveLoad && this != MessageBox_N.Instance && this != GameUI.Instance.mRevive)
		{
			return false;
		}
		return true;
	}

	protected void PlayShowTween()
	{
		m_IsRefresh = false;
		if (mTweener != null)
		{
			mTweener.Play(forward: true);
		}
		if (mAlphaTweener != null)
		{
			mAlphaTweener.Play(forward: true);
		}
		CheckOpenUI();
	}

	private bool PlayerHideTween()
	{
		bool result = false;
		if (mTweener != null)
		{
			if (base.gameObject.activeInHierarchy)
			{
				mTweener.Play(forward: false);
				result = true;
			}
			else
			{
				result = false;
			}
		}
		if (mAlphaTweener != null)
		{
			if (base.gameObject.activeInHierarchy)
			{
				mAlphaTweener.Play(forward: false);
				result = true;
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	protected void UpdateTween()
	{
		if (mAlphaTweener != null)
		{
			mAlphaTweener.refreshWidget = true;
		}
	}

	public bool IsOpen()
	{
		return isShow;
	}

	public void Hide()
	{
		if (!PlayerHideTween())
		{
			OnHide();
		}
	}

	private void OnTweenFinished(UITweener tween)
	{
		if (tween.direction == Direction.Reverse)
		{
			if (base.gameObject.activeSelf)
			{
				OnHide();
			}
		}
		else if (tween.direction == Direction.Forward && !m_IsRefresh)
		{
			OnShowFinish();
			m_IsRefresh = true;
		}
	}

	protected virtual void OnHide()
	{
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(value: false);
		}
		if (e_OnHide != null)
		{
			e_OnHide(this);
		}
	}

	protected virtual void OnShowFinish()
	{
		if (e_OnShowFinish != null)
		{
			e_OnShowFinish();
		}
	}

	protected virtual void OnClose()
	{
		Hide();
	}

	protected virtual void InitWindow()
	{
		mInit = true;
		mGroups = GetComponentsInChildren<UIAlphaGroup>(includeInactive: true);
		if (e_OnInit != null)
		{
			e_OnInit(this);
		}
	}

	public virtual void ChangeWindowShowState()
	{
		if (isShow)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	protected virtual void PlayOpenSoundEffect()
	{
		if (null != GameUI.Instance)
		{
			GameUI.Instance.PlayWndOpenAudioEffect(this);
		}
	}

	protected virtual void CheckOpenUI()
	{
		if (SelfWndType != 0)
		{
			InGameAidData.CheckOpenUI(SelfWndType);
		}
	}
}
