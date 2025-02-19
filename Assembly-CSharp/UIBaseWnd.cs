using System.Collections.Generic;
using UnityEngine;

public abstract class UIBaseWnd : UIBaseWidget
{
	[SerializeField]
	private UIWndActivate mActivate;

	[SerializeField]
	private float testAlpha = 1f;

	public UIAnchor mAnchor;

	private bool deActive;

	public Rect rect
	{
		get
		{
			if (mActivate == null)
			{
				return new Rect(0f, 0f, 0f, 0f);
			}
			Transform transform = mActivate.mCollider.gameObject.transform;
			float num = transform.localScale.x - 6f;
			float num2 = transform.localScale.y - 6f;
			Vector3 vector = ((!(mAnchor == null)) ? mAnchor.transform.localPosition : Vector3.zero);
			float num3 = base.transform.localPosition.x + mActivate.BgLocalPos.x - num / 2f + vector.x;
			float num4 = base.transform.localPosition.y + mActivate.BgLocalPos.y - num2 / 2f + vector.y;
			float width = num3 + num;
			float height = num4 + num2;
			return new Rect(num3, num4, width, height);
		}
	}

	public bool Active => !deActive;

	public bool IsCoverForTopsWnd(List<Rect> rectList)
	{
		foreach (Rect rect in rectList)
		{
			if (CrossAlgorithm(rect, this.rect))
			{
				return true;
			}
		}
		return false;
	}

	protected override void InitWindow()
	{
		if (UIRecentDataMgr.Instance != null)
		{
			base.InitWindow();
			ResetWndPostion();
		}
	}

	public override void Show()
	{
		base.Show();
		TopMostWnd();
		UIStateMgr.RecordOpenHistory(this);
	}

	protected override void OnHide()
	{
		base.OnHide();
		SaveWndPostion();
		UIStateMgr.RemoveOpenHistory(this);
	}

	public override void OnCreate()
	{
		base.OnCreate();
	}

	protected override void OnClose()
	{
		base.OnClose();
		UIStateMgr.RemoveOpenHistory(this);
	}

	public virtual void DeActiveWnd()
	{
		if (!deActive)
		{
			deActive = true;
			if (mGroups != null)
			{
				UIAlphaGroup[] array = mGroups;
				foreach (UIAlphaGroup uIAlphaGroup in array)
				{
					uIAlphaGroup.State = 1;
				}
			}
		}
		if (mActivate != null)
		{
			mActivate.Deactivate();
		}
	}

	public virtual void ActiveWnd()
	{
		if (!isShow && !GameUI.Instance.mNPCTalk.isShow)
		{
			Show();
		}
		if (deActive)
		{
			deActive = false;
			if (mGroups != null)
			{
				UIAlphaGroup[] array = mGroups;
				foreach (UIAlphaGroup uIAlphaGroup in array)
				{
					uIAlphaGroup.State = 0;
				}
			}
		}
		if (mActivate != null)
		{
			mActivate.Activate();
		}
	}

	protected virtual void LateUpdate()
	{
	}

	public virtual void RepostionDepth()
	{
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, mDepth);
	}

	public virtual void TopMostWnd()
	{
		if (null != GameUI.Instance)
		{
			UIBaseWnd topWnd = UIStateMgr.Instance.GetTopWnd();
			if (topWnd == this)
			{
				return;
			}
			if (null != topWnd)
			{
				topWnd.RepostionDepth();
			}
		}
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 20f);
	}

	private void ResetWndPostion()
	{
		if (!(base.gameObject == null) && !(UIRecentDataMgr.Instance == null))
		{
			Vector3 defaltValue = base.gameObject.transform.localPosition;
			defaltValue = new Vector3((int)defaltValue.x, (int)defaltValue.y, (int)((mDepth != 0f) ? mDepth : defaltValue.z));
			base.gameObject.transform.localPosition = UIRecentDataMgr.Instance.GetVector3Value(base.gameObject.name, defaltValue);
		}
	}

	public void SaveWndPostion()
	{
		if (!(base.gameObject == null) && !(UIRecentDataMgr.Instance == null))
		{
			Vector3 localPosition = base.gameObject.transform.localPosition;
			localPosition = new Vector3((int)localPosition.x, (int)localPosition.y, (int)localPosition.z);
			UIRecentDataMgr.Instance.SetVector3Value(base.gameObject.name, localPosition);
			UIRecentDataMgr.Instance.SaveUIRecentData();
		}
	}

	private bool CrossAlgorithm(Rect r1, Rect r2)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		num = ((!(r1.x >= r2.x)) ? r2.x : r1.x);
		num2 = ((!(r1.y >= r2.y)) ? r2.y : r1.y);
		num3 = ((!(r1.width <= r2.width)) ? r2.width : r1.width);
		num4 = ((!(r1.height <= r2.height)) ? r2.height : r1.height);
		if (num > num3 || num2 > num4)
		{
			return false;
		}
		return true;
	}
}
