using System;
using UnityEngine;

public class S_TaskItem : MonoBehaviour
{
	private MeshRenderer smr;

	public float angle;

	public float Xmin = -0.25f;

	public float Xmax = 0.25f;

	public float Zmin = -0.16475f;

	public float Zmax = 0.6494f;

	public float cycle = 3f;

	public float speed = 5f;

	public float wide = 0.25f;

	private float ctime;

	private float coolingTime;

	private void Start()
	{
		smr = base.transform.GetComponent<MeshRenderer>();
		Material material = Resources.Load("Materials/TaskItem") as Material;
		Texture texture = smr.GetComponent<Renderer>().material.GetTexture(0);
		material.SetTexture(0, texture);
		smr.GetComponent<Renderer>().material = material;
		material.SetFloat("_Angle", Mathf.Clamp(angle / 180f * (float)Math.PI, 0f, 3.1416f));
		material.SetFloat("_Xmin", Xmin);
		material.SetFloat("_Xmax", Xmax);
		material.SetFloat("_Zmin", Zmin);
		material.SetFloat("_Zmax", Zmax);
		material.SetFloat("_Wide", wide);
		material.SetFloat("_Speed", speed);
	}

	private void Update()
	{
		coolingTime = (Mathf.Max(Xmax - Xmin, Zmax - Zmin) * 1.15f + wide) / speed * cycle;
		ctime += Time.deltaTime;
		if (ctime > coolingTime)
		{
			ctime -= coolingTime;
		}
		smr.GetComponent<Renderer>().material.SetFloat("_Dt", ctime);
	}
}
