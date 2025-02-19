using System.Collections.Generic;
using Pathea;
using PeEvent;
using UnityEngine;

public class MousePickable : MonoBehaviour, MousePicker.IPickable
{
	public class RMouseClickEvent : EventArg
	{
		public MousePickable mousePickable;
	}

	public float operateDistance = 19f;

	private Event<RMouseClickEvent> mEventor = new Event<RMouseClickEvent>();

	private MousePicker.EPriority mPriority;

	[SerializeField]
	protected List<Collider> mCollider = new List<Collider>(1);

	private PeTrans _peTrans;

	string MousePicker.IPickable.tips => tipsText;

	MousePicker.EPriority MousePicker.IPickable.priority => priority;

	float MousePicker.IPickable.delayTime => 0f;

	public Event<RMouseClickEvent> eventor => mEventor;

	public MousePicker.EPriority priority
	{
		get
		{
			return mPriority;
		}
		set
		{
			mPriority = value;
		}
	}

	protected virtual string tipsText => null;

	float MousePicker.IPickable.CheckPick(Ray ray)
	{
		float dis = 0f;
		if (CheckPick(ray, out dis))
		{
			return dis;
		}
		return float.MaxValue;
	}

	void MousePicker.IPickable.SetPickState(bool isOver)
	{
		SetPickState(isOver);
	}

	void MousePicker.IPickable.PickObj()
	{
		CheckOperate();
	}

	public void ClearCollider()
	{
		mCollider.Clear();
	}

	public void CollectColliders()
	{
		_peTrans = GetComponent<PeTrans>();
		ClearCollider();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		Collider[] array = componentsInChildren;
		foreach (Collider item in array)
		{
			mCollider.Add(item);
		}
	}

	public bool DistanceInRange(Vector3 pos, float dist)
	{
		float num = dist * 1.25f;
		Vector3 vector = ((!(_peTrans == null)) ? (pos - _peTrans.position) : (pos - base.transform.position));
		if (vector.x > num || vector.x < 0f - num || vector.y > num || vector.y < 0f - num || vector.z > num || vector.z < 0f - num)
		{
			return false;
		}
		float num2 = dist * dist;
		if (_peTrans != null)
		{
			Bounds bound = _peTrans.bound;
			bound.center = _peTrans.trans.TransformPoint(bound.center);
			return num2 >= bound.SqrDistance(pos);
		}
		for (int i = 0; i < mCollider.Count; i++)
		{
			Collider collider = mCollider[i];
			if (collider != null && num2 >= collider.bounds.SqrDistance(pos))
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		PeSingleton<MousePicker>.Instance.Add(this);
		OnStart();
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnDestroy()
	{
		ClearCollider();
		PeSingleton<MousePicker>.Instance.Remove(this);
	}

	protected virtual bool CheckPick(Ray ray, out float dis)
	{
		if (null != PeSingleton<MainPlayer>.Instance.entity && DistanceInRange(PeSingleton<MainPlayer>.Instance.entity.position, operateDistance))
		{
			for (int i = 0; i < mCollider.Count; i++)
			{
				Collider collider = mCollider[i];
				if (null != collider && collider.Raycast(ray, out var hitInfo, 100f))
				{
					dis = hitInfo.distance;
					return true;
				}
			}
		}
		dis = 0f;
		return false;
	}

	protected virtual void CheckOperate()
	{
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
		{
			RMouseClickEvent rMouseClickEvent = new RMouseClickEvent();
			rMouseClickEvent.mousePickable = this;
			eventor.Dispatch(rMouseClickEvent);
		}
	}

	protected virtual void SetPickState(bool isOver)
	{
		OutlineObject component = GetComponent<OutlineObject>();
		if (!(null == component))
		{
			if (isOver)
			{
				component.color = new Color(0f, 0.5f, 1f, 1f);
			}
			else
			{
				Object.Destroy(component);
			}
		}
	}
}
