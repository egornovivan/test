using System;
using UnityEngine;

namespace Steer3D;

public class PathFollow : SteeringBehaviour
{
	public Vector3[] path = new Vector3[0];

	public float weight = 1f;

	public float slowingRadius = 5f;

	public float arriveRadius = 0.2f;

	public int followIndex;

	public bool drawPathInScene;

	public Vector3 followTarget
	{
		get
		{
			if (path.Length == 0)
			{
				return Vector3.zero;
			}
			int num = followIndex;
			if (num >= path.Length)
			{
				num = path.Length - 1;
			}
			return path[num];
		}
	}

	public override bool idle => !active || followIndex == path.Length;

	public void Reset(Vector3[] _path)
	{
		path = new Vector3[_path.Length];
		Array.Copy(_path, path, _path.Length);
		followIndex = 0;
	}

	public override void Behave()
	{
		if (!idle && followIndex < path.Length)
		{
			Vector3 vector = followTarget;
			Vector3 vector2 = vector - base.position;
			Vector3 normalized = vector2.normalized;
			float num = 1f;
			if (followIndex == path.Length - 1)
			{
				num = ((!(slowingRadius > arriveRadius)) ? ((!(vector2.magnitude > arriveRadius)) ? 0f : 1f) : Mathf.Clamp01(Mathf.InverseLerp(arriveRadius, slowingRadius, vector2.magnitude)));
			}
			normalized *= num;
			agent.AddDesiredVelocity(normalized, weight, 0.75f);
			if (vector2.magnitude < arriveRadius)
			{
				followIndex++;
			}
		}
	}

	private void LateUpdate()
	{
		if (drawPathInScene && path.Length > 0)
		{
			for (int i = 0; i < path.Length; i++)
			{
				Color color = ((i <= followIndex) ? ((i != followIndex) ? Color.green : Color.white) : Color.red);
				Debug.DrawLine(path[i] + Vector3.left * arriveRadius, path[i] + Vector3.right * arriveRadius, color);
				Debug.DrawLine(path[i] + Vector3.up * arriveRadius, path[i] + Vector3.down * arriveRadius, color);
				Debug.DrawLine(path[i] + Vector3.back * arriveRadius, path[i] + Vector3.forward * arriveRadius, color);
			}
			for (int j = 1; j < path.Length; j++)
			{
				Debug.DrawLine(color: (j <= followIndex) ? ((j != followIndex) ? Color.green : Color.white) : Color.red, start: path[j - 1], end: path[j]);
			}
		}
	}
}
