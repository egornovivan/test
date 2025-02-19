using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : IgnoreTimeScale
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring
	}

	public Transform target;

	public Vector3 scale = Vector3.one;

	public float scrollWheelFactor;

	public bool restrictWithinPanel;

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	public float momentumAmount = 35f;

	private Plane mPlane;

	private Vector3 mLastPos;

	private UIPanel mPanel;

	private bool mPressed;

	private Vector3 mMomentum = Vector3.zero;

	private float mScroll;

	private Bounds mBounds;

	private UIBaseWnd baseWnd;

	private void Start()
	{
		if (target != null)
		{
			baseWnd = target.gameObject.GetComponent<UIBaseWnd>();
		}
		if (target != null)
		{
			Vector3 localPosition = target.localPosition;
			Vector3 vector = Vector3.zero;
			vector = ((!(base.gameObject.GetComponent<UISprite>() != null)) ? NGUIMath.CalculateRelativeWidgetBounds(target).size : new Vector3(base.transform.localScale.x, base.transform.localScale.y, 0f));
			UIAnchor componentInParent = target.GetComponentInParent<UIAnchor>();
			if (componentInParent != null)
			{
				localPosition += componentInParent.transform.localPosition;
			}
			float num = Screen.width / 2;
			float num2 = Screen.height / 2;
			Vector3 vector2 = vector / 2f;
			float num3 = ((!(num > vector2.x)) ? vector2.x : num);
			float num4 = ((!(0f - num < 0f - vector2.x)) ? (0f - vector2.x) : (0f - num));
			float num5 = ((!(num2 > vector2.y)) ? vector2.y : num2);
			float num6 = ((!(0f - num2 < 0f - vector2.y)) ? (0f - vector2.y) : (0f - num2));
			if (localPosition.x > num3)
			{
				localPosition.x = num3;
			}
			else if (localPosition.x < num4)
			{
				localPosition.x = num4;
			}
			if (localPosition.y > num5)
			{
				localPosition.y = num5;
			}
			else if (localPosition.y < num6)
			{
				localPosition.y = num6;
			}
			if (componentInParent != null)
			{
				localPosition -= componentInParent.transform.localPosition;
			}
			localPosition.x = Mathf.Round(localPosition.x);
			localPosition.y = Mathf.Round(localPosition.y);
			target.localPosition = localPosition;
		}
	}

	private void FindPanel()
	{
		mPanel = ((!(target != null)) ? null : UIPanel.Find(target.transform, createIfMissing: false));
		if (mPanel == null)
		{
			restrictWithinPanel = false;
		}
	}

	private void OnPress(bool pressed)
	{
		if (!base.enabled || !NGUITools.GetActive(base.gameObject) || !(target != null))
		{
			return;
		}
		mPressed = pressed;
		if (pressed)
		{
			if (UIStateMgr.Instance != null && baseWnd != null && UIStateMgr.Instance.GetTopWnd() != baseWnd)
			{
				baseWnd.TopMostWnd();
			}
			if (restrictWithinPanel && mPanel == null)
			{
				FindPanel();
			}
			if (restrictWithinPanel)
			{
				mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);
			}
			mMomentum = Vector3.zero;
			mScroll = 0f;
			SpringPosition component = target.GetComponent<SpringPosition>();
			if (component != null)
			{
				component.enabled = false;
			}
			mLastPos = UICamera.lastHit.point;
			Transform transform = UICamera.currentCamera.transform;
			mPlane = new Plane(((!(mPanel != null)) ? transform.rotation : mPanel.cachedTransform.rotation) * Vector3.back, mLastPos);
		}
		else if (restrictWithinPanel && mPanel.clipping != 0 && dragEffect == DragEffect.MomentumAndSpring)
		{
			mPanel.ConstrainTargetToBounds(target, ref mBounds, immediate: false);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (!Input.GetMouseButton(0) || !base.enabled || !NGUITools.GetActive(base.gameObject) || !(target != null))
		{
			return;
		}
		UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
		Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
		float enter = 0f;
		if (!mPlane.Raycast(ray, out enter))
		{
			return;
		}
		Vector3 point = ray.GetPoint(enter);
		Vector3 vector = point - mLastPos;
		mLastPos = point;
		if (vector.x != 0f || vector.y != 0f)
		{
			vector = target.InverseTransformDirection(vector);
			vector.Scale(scale);
			vector = target.TransformDirection(vector);
		}
		if (dragEffect != 0)
		{
			mMomentum = Vector3.Lerp(mMomentum, mMomentum + vector * (0.01f * momentumAmount), 0.67f);
		}
		if (restrictWithinPanel)
		{
			Vector3 localPosition = target.localPosition;
			target.position += vector;
			Vector3 localPosition2 = target.localPosition;
			Vector3 vector2 = Vector3.zero;
			vector2 = ((!(base.gameObject.GetComponent<UISprite>() != null)) ? NGUIMath.CalculateRelativeWidgetBounds(target).size : new Vector3(base.transform.localScale.x, base.transform.localScale.y, 0f));
			UIAnchor componentInParent = target.GetComponentInParent<UIAnchor>();
			if (componentInParent != null)
			{
				localPosition2 += componentInParent.transform.localPosition;
			}
			float num = Screen.width / 2;
			float num2 = Screen.height / 2;
			Vector3 vector3 = vector2 / 2f;
			float num3 = ((!(num > vector3.x)) ? vector3.x : num);
			float num4 = ((!(0f - num < 0f - vector3.x)) ? (0f - vector3.x) : (0f - num));
			float num5 = ((!(num2 > vector3.y)) ? vector3.y : num2);
			float num6 = ((!(0f - num2 < 0f - vector3.y)) ? (0f - vector3.y) : (0f - num2));
			if (localPosition2.x > num3)
			{
				localPosition2.x = num3;
			}
			else if (localPosition2.x < num4)
			{
				localPosition2.x = num4;
			}
			if (localPosition2.y > num5)
			{
				localPosition2.y = num5;
			}
			else if (localPosition2.y < num6)
			{
				localPosition2.y = num6;
			}
			if (componentInParent != null)
			{
				localPosition2 -= componentInParent.transform.localPosition;
			}
			localPosition2.x = Mathf.Round(localPosition2.x);
			localPosition2.y = Mathf.Round(localPosition2.y);
			target.localPosition = localPosition2;
			mBounds.center += target.localPosition - localPosition;
			if (dragEffect != DragEffect.MomentumAndSpring && mPanel.clipping != 0 && mPanel.ConstrainTargetToBounds(target, ref mBounds, immediate: true))
			{
				mMomentum = Vector3.zero;
				mScroll = 0f;
			}
		}
		else
		{
			target.position += vector;
		}
	}

	private void LateUpdate()
	{
		float deltaTime = UpdateRealTimeDelta();
		if (target == null)
		{
			return;
		}
		if (mPressed)
		{
			SpringPosition component = target.GetComponent<SpringPosition>();
			if (component != null)
			{
				component.enabled = false;
			}
			mScroll = 0f;
		}
		else
		{
			mMomentum += scale * ((0f - mScroll) * 0.05f);
			mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, deltaTime);
			if (mMomentum.magnitude > 0.0001f)
			{
				if (mPanel == null)
				{
					FindPanel();
				}
				if (mPanel != null)
				{
					target.position += NGUIMath.SpringDampen(ref mMomentum, 9f, deltaTime);
					if (!restrictWithinPanel || mPanel.clipping == UIDrawCall.Clipping.None)
					{
						return;
					}
					mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);
					if (!mPanel.ConstrainTargetToBounds(target, ref mBounds, dragEffect == DragEffect.None))
					{
						SpringPosition component2 = target.GetComponent<SpringPosition>();
						if (component2 != null)
						{
							component2.enabled = false;
						}
					}
					return;
				}
			}
			else
			{
				mScroll = 0f;
			}
		}
		NGUIMath.SpringDampen(ref mMomentum, 9f, deltaTime);
	}

	private void OnScroll(float delta)
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject))
		{
			if (Mathf.Sign(mScroll) != Mathf.Sign(delta))
			{
				mScroll = 0f;
			}
			mScroll += delta * scrollWheelFactor;
		}
	}
}
