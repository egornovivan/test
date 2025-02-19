using System;
using UnityEngine;

namespace Pathfinding;

[Obsolete("Use the RVO system instead")]
[RequireComponent(typeof(LocalAvoidance))]
public class LocalAvoidanceMover : MonoBehaviour
{
	public float targetPointDist = 10f;

	public float speed = 2f;

	private Vector3 targetPoint;

	private LocalAvoidance controller;

	private void Start()
	{
		targetPoint = base.transform.forward * targetPointDist + base.transform.position;
		controller = GetComponent<LocalAvoidance>();
	}

	private void Update()
	{
		if (controller != null)
		{
			controller.SimpleMove((targetPoint - base.transform.position).normalized * speed);
		}
	}
}
