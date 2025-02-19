using System.Collections.Generic;
using UnityEngine;

namespace EpsilonIndi;

public class PositionReset : MonoBehaviour
{
	public bool test;

	[SerializeField]
	private Transform sightpoint;

	[SerializeField]
	private Transform shipsight;

	[SerializeField]
	private Transform sun;

	[SerializeField]
	private Transform[] planets;

	private ShipSightPath sp;

	private ShipSightRotate sr;

	private float dt;

	private float rotateY;

	private float ssrotate;

	private List<OrbitalPath> ops = new List<OrbitalPath>();

	private List<SelfRotation> srs = new List<SelfRotation>();

	private Quaternion obj2cmr;

	private void Start()
	{
		sp = shipsight.GetComponent<ShipSightPath>();
		sr = shipsight.GetComponent<ShipSightRotate>();
		Transform[] array = planets;
		foreach (Transform transform in array)
		{
			if (transform.GetComponent<OrbitalPath>() != null)
			{
				ops.Add(transform.GetComponent<OrbitalPath>());
			}
			if (transform.GetComponent<SelfRotation>() != null)
			{
				srs.Add(transform.GetComponent<SelfRotation>());
			}
		}
	}

	private void FixedUpdate()
	{
		if (test)
		{
			TestUniverse();
			return;
		}
		dt = Time.deltaTime;
		obj2cmr = Quaternion.Inverse(sr.m_rotation * sightpoint.rotation);
		foreach (OrbitalPath op in ops)
		{
			op.transform.position = obj2cmr * (op.UpdatePosition(dt) - sp.m_position) + sightpoint.position;
		}
		foreach (SelfRotation sr in srs)
		{
			Quaternion rotationX = sr.m_rotationX;
			Quaternion quaternion = sr.UpdateRotateY(dt);
			sr.transform.rotation = obj2cmr * (rotationX * quaternion);
			CloudEmitter component = sr.GetComponent<CloudEmitter>();
			if (!component)
			{
				continue;
			}
			rotateY = component.transform.GetComponent<SelfRotation>().s_angle;
			ssrotate = this.sr.rotateY;
			component.relativeRotY = ssrotate - rotateY;
			List<CloudMotion> list = new List<CloudMotion>();
			foreach (CloudMotion cm in component.cms)
			{
				cm.transform.localRotation = cm.UpdateRotate(dt, rotateY);
				cm.transform.GetComponent<MeshRenderer>().material.SetVector("_SunDirect", sun.forward);
				if (cm.OutOfSight(ssrotate))
				{
					list.Add(cm);
				}
			}
			foreach (CloudMotion item in list)
			{
				component.cms.Remove(item);
				Object.Destroy(item.gameObject);
			}
			list.Clear();
		}
	}

	private void TestUniverse()
	{
		dt = Time.deltaTime;
		obj2cmr = Quaternion.Inverse(sr.m_rotation * sightpoint.rotation);
		foreach (OrbitalPath op in ops)
		{
			op.UpdatePosition(dt);
			op.TestUpdate();
		}
		foreach (SelfRotation sr in srs)
		{
			sr.TestUpdate();
			CloudEmitter component = sr.GetComponent<CloudEmitter>();
			if (!component)
			{
				continue;
			}
			rotateY = component.transform.GetComponent<SelfRotation>().s_angle;
			ssrotate = this.sr.rotateY;
			component.relativeRotY = ssrotate - rotateY;
			foreach (CloudMotion cm in component.cms)
			{
				cm.UpdateRotate(dt, rotateY);
				cm.TestUpdate();
				if (cm.OutOfSight(ssrotate))
				{
					component.cms.Remove(cm);
					Object.Destroy(cm.gameObject);
				}
			}
		}
	}
}
