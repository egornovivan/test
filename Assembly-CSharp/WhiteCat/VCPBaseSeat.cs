using System;
using UnityEngine;

namespace WhiteCat;

public abstract class VCPBaseSeat : VCPart
{
	[SerializeField]
	private Transform _pivotPoint;

	[SerializeField]
	private string _sitAnimName;

	[SerializeField]
	private GameObject _humanModel;

	private IVCPassenger _passenger;

	public IVCPassenger passenger => _passenger;

	public CarrierController drivingController => GetComponentInParent<CarrierController>();

	public event Action getOffCallback;

	public virtual void GetOn(IVCPassenger passenger)
	{
		base.enabled = true;
		_passenger = passenger;
		passenger.GetOn(_sitAnimName, this);
	}

	public void GetOff()
	{
		if (this.getOffCallback != null)
		{
			this.getOffCallback();
		}
		this.getOffCallback = null;
		_passenger.GetOff();
		_passenger = null;
		base.enabled = false;
	}

	public bool FindGetOffPosition(out Vector3 position)
	{
		position = Vector3.zero;
		CarrierController carrierController = drivingController;
		Transform transform = carrierController.transform;
		Bounds bounds = carrierController.creationController.bounds;
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Vector3 position2 = default(Vector3);
		position2.y = min.y - 2.5f;
		while (position2.y < max.y + 2.5f)
		{
			for (float num = 0.25f; num < (max.z - min.z) * 0.5f + 2.5f; num += 0.5f)
			{
				for (float num2 = 0.25f; num2 < (max.x - min.x) * 0.5f + 2.5f; num2 += 0.5f)
				{
					position2.z = (max.z + min.z) * 0.5f + num;
					position2.x = (max.x + min.x) * 0.5f - num2;
					if (CheckGetOffPosition(transform, position = transform.TransformPoint(position2)))
					{
						return true;
					}
					position2.x = (max.x + min.x) * 0.5f + num2;
					if (CheckGetOffPosition(transform, position = transform.TransformPoint(position2)))
					{
						return true;
					}
					position2.z = (max.z + min.z) * 0.5f - num;
					if (CheckGetOffPosition(transform, position = transform.TransformPoint(position2)))
					{
						return true;
					}
					position2.x = (max.x + min.x) * 0.5f - num2;
					if (CheckGetOffPosition(transform, position = transform.TransformPoint(position2)))
					{
						return true;
					}
				}
			}
			position2.y += 0.5f;
		}
		return false;
	}

	private bool CheckGetOffPosition(Transform carrier, Vector3 pos)
	{
		Vector3 end = pos;
		end.y += 1.5f;
		if (!Physics.CheckCapsule(pos, end, 0.55f, PEVCConfig.instance.getOffLayerMask, QueryTriggerInteraction.Ignore))
		{
			Vector3 direction = pos - base.transform.position;
			RaycastHit[] array = Physics.RaycastAll(base.transform.position, direction, direction.magnitude, PEVCConfig.instance.getOffLayerMask, QueryTriggerInteraction.Ignore);
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].transform.IsChildOf(carrier))
				{
					flag = false;
					break;
				}
			}
			if (flag && Physics.Raycast(pos, Vector3.down, 5f, PEVCConfig.instance.getOffLayerMask, QueryTriggerInteraction.Ignore))
			{
				return true;
			}
		}
		return false;
	}

	public void SyncPassenger()
	{
		if (_passenger != null)
		{
			_passenger.Sync(_pivotPoint.position, _pivotPoint.rotation);
		}
	}

	private void Update()
	{
		SyncPassenger();
	}

	public void DestroyHumanModel()
	{
		UnityEngine.Object.Destroy(_humanModel);
	}
}
