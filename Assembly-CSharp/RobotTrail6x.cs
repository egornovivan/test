using System.Collections.Generic;
using UnityEngine;

public class RobotTrail6x : MonoBehaviour
{
	public List<WeaponTrail> trails = new List<WeaponTrail>();

	private float tempT;

	private float t;

	private void Start()
	{
		for (int i = 0; i < trails.Count; i++)
		{
			trails[i].FadeOut(0f);
		}
	}

	private void Update()
	{
		StartAllTrails();
		t = Mathf.Clamp(Time.deltaTime * 1f, 0f, 0.066f);
		for (int i = 0; i < trails.Count; i++)
		{
			trails[i].Itterate(Time.time - t + tempT);
		}
		tempT -= t;
		for (int j = 0; j < trails.Count; j++)
		{
			trails[j].UpdateTrail(Time.time, t);
		}
	}

	public void StartAllTrails()
	{
		for (int i = 0; i < trails.Count; i++)
		{
			trails[i].ClearTrail();
			trails[i].StartTrail(0.2f, 0.2f);
		}
	}
}
