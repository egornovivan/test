using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class MousePicker : MonoLikeSingleton<MousePicker>
{
	public enum EPriority
	{
		Level0,
		Level1,
		Level2,
		Level3,
		Max
	}

	public interface IPickable
	{
		string tips { get; }

		EPriority priority { get; }

		float delayTime { get; }

		float CheckPick(Ray ray);

		void SetPickState(bool isOver);

		void PickObj();
	}

	private const int MaxPickDis = 100;

	private static int ObstacleLayer = 6144;

	private List<IPickable> mPickableList = new List<IPickable>();

	private IPickable mPickedObj;

	private float mHoveredTime;

	private bool mPickedObjChanged = true;

	private bool mEnable = true;

	private bool isOverHoveredTime
	{
		get
		{
			if (mPickedObj == null)
			{
				return false;
			}
			return mHoveredTime > mPickedObj.delayTime;
		}
	}

	public IPickable curPickObj => mPickedObj;

	public bool enable
	{
		get
		{
			return mEnable;
		}
		set
		{
			if (!value)
			{
				SetCurPickable(null);
				UpdateTis(value: false);
			}
			mEnable = value;
		}
	}

	public override void Update()
	{
		if (!enable || PeGameMgr.gamePause)
		{
			return;
		}
		mHoveredTime += Time.deltaTime;
		UpdateOpObject();
		if (isOverHoveredTime)
		{
			if (mPickedObjChanged)
			{
				HoverPicked(value: true);
				mPickedObjChanged = false;
			}
			ActiveFunction();
		}
	}

	private void UpdateOpObject()
	{
		IPickable pickable = CheckPickable();
		if (pickable != mPickedObj)
		{
			SetCurPickable(pickable);
		}
	}

	private void HoverPicked(bool value)
	{
		if (mPickedObj != null)
		{
			mPickedObj.SetPickState(value);
		}
		UpdateTis(value);
	}

	public void UpdateTis(bool value = true)
	{
		if (value && mPickedObj != null)
		{
			MouseActionWnd_N.Instance.SetText(mPickedObj.tips);
		}
		else
		{
			MouseActionWnd_N.Instance.Clear();
		}
	}

	private void SetCurPickable(IPickable pickable)
	{
		HoverPicked(value: false);
		mPickedObj = pickable;
		mHoveredTime = 0f;
		mPickedObjChanged = true;
	}

	private IPickable CheckPickable()
	{
		if (null != UICamera.hoveredObject)
		{
			return null;
		}
		Ray mouseRay = PeCamera.mouseRay;
		IPickable result = null;
		EPriority ePriority = EPriority.Level0;
		float maxDistance = GetMaxDistance(mouseRay);
		float num = maxDistance;
		for (int i = 0; i < mPickableList.Count; i++)
		{
			IPickable pickable = mPickableList[i];
			if (pickable == null)
			{
				continue;
			}
			float num2 = pickable.CheckPick(mouseRay);
			if (!(num2 > maxDistance))
			{
				if (pickable.priority > ePriority)
				{
					ePriority = pickable.priority;
					num = num2;
					result = pickable;
				}
				else if (pickable.priority == ePriority && num > num2)
				{
					num = num2;
					result = pickable;
				}
			}
		}
		return result;
	}

	private static float GetMaxDistance(Ray ray)
	{
		float num = 100f;
		if (Physics.Raycast(ray, out var hitInfo, 100f, ObstacleLayer) && hitInfo.distance < num)
		{
			num = hitInfo.distance;
		}
		return num + 10f;
	}

	private void ActiveFunction()
	{
		if (mPickedObj != null)
		{
			mPickedObj.PickObj();
		}
	}

	public void Add(IPickable opObj)
	{
		mPickableList.Add(opObj);
	}

	public bool Remove(IPickable p)
	{
		if (p == null)
		{
			return false;
		}
		if (p == mPickedObj)
		{
			if (null != MouseActionWnd_N.Instance)
			{
				MouseActionWnd_N.Instance.Clear();
			}
			mPickedObj = null;
		}
		return mPickableList.Remove(p);
	}
}
