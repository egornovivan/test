using System.Collections.Generic;
using UnityEngine;

namespace NovaEnv;

public class ScreenDrop : MonoBehaviour
{
	[HideInInspector]
	public Executor Executor;

	private LineRenderer liner;

	public Transform drop;

	public float InitStrength = -0.1f;

	public float DryingSpeed = 0.02f;

	private Vector3 vel = Vector3.zero;

	private List<Vector3> verts = new List<Vector3>();

	public Vector3 pos
	{
		get
		{
			return drop.localPosition;
		}
		set
		{
			drop.localPosition = value;
		}
	}

	private void Start()
	{
		liner = GetComponent<LineRenderer>();
		liner.SetVertexCount(2);
		liner.SetPosition(0, new Vector3(0f, 0.5f, 0f));
		liner.SetPosition(1, new Vector3(0f, 0.5f, 0f));
		verts.Add(pos + base.transform.InverseTransformDirection(drop.up) * 0.5f);
	}

	private void Update()
	{
		if (Random.value < 0.02f)
		{
			Slip();
		}
		pos += vel * Time.deltaTime;
		verts[verts.Count - 1] = pos + base.transform.InverseTransformDirection(drop.up) * 0.5f;
		liner.SetVertexCount(verts.Count);
		int num = 0;
		int num2 = verts.Count - 1;
		while (num2 >= 0)
		{
			liner.SetPosition(num, verts[num2]);
			num2--;
			num++;
		}
		InitStrength *= Mathf.Clamp01(1f - DryingSpeed);
		liner.material.SetFloat("_DistortionStrength", InitStrength);
		drop.GetComponent<Renderer>().material.SetFloat("_DistortionStrength", InitStrength);
		if (Mathf.Abs(InitStrength) < 0.001f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Slip()
	{
		Vector3 windDirection = Executor.Wind.WindDirection;
		windDirection.y = 0f;
		windDirection = base.transform.parent.InverseTransformDirection(windDirection);
		windDirection.y = 0f;
		windDirection = windDirection * 0.07f + Vector3.down;
		windDirection += Random.insideUnitSphere * 0.3f;
		windDirection.z = 0f;
		vel = windDirection * (Random.value * 25f + 0.2f) * Mathf.Clamp01(Executor.WetCoef - 0.2f);
		drop.localRotation = Quaternion.LookRotation(Vector3.forward, -vel.normalized);
		verts.Add(pos + base.transform.InverseTransformDirection(drop.up) * 0.5f);
	}
}
