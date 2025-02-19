using System;
using UnityEngine;

public class TRTrack : Trajectory
{
	public float speed;

	public float circlePerSec = 1f;

	public float wavesPerSec = 2f;

	public float maxMagnitude = 0.3f;

	private new Transform target;

	private Vector3 startPos;

	private float startTime;

	private Vector3 dynamicDir;

	private float transAngle;

	private float transMag;

	private Vector3 subX;

	private Vector3 subY;

	private float random1 = -999f;

	private Vector3 offset = Vector3.zero;

	private Vector3 mainPath = Vector3.zero;

	private bool init = true;

	private float Rand1
	{
		get
		{
			if (random1 < -1.1f)
			{
				random1 = ((!(UnityEngine.Random.value > 0.5f)) ? (-1f) : 1f);
			}
			return random1;
		}
	}

	private void Update()
	{
		if (init)
		{
			Emit(m_Target);
			init = false;
		}
	}

	public void Emit(Transform target)
	{
		this.target = target;
		startPos = base.transform.position;
		startTime = Time.time;
		dynamicDir = (GetPredictPosition(this.target, startPos, speed) - base.transform.position).normalized;
	}

	public override Vector3 Track(float deltaTime)
	{
		transAngle += circlePerSec * 360f * deltaTime;
		transAngle -= (float)(int)(transAngle / 360f) * 360f;
		transMag = Mathf.Sin((Time.time - startTime) * wavesPerSec * 2f * (float)Math.PI) * maxMagnitude;
		subX = Vector3.Cross(Vector3.up, new Vector3(dynamicDir.x, 0f, dynamicDir.z)).normalized * Rand1;
		subY = Vector3.Cross(dynamicDir, subX) * Rand1;
		offset.x = (int)(transAngle / 90f);
		if (offset.x == 0f)
		{
			offset = Vector3.Slerp(subY, subX, transAngle / 90f);
		}
		else if (offset.x == 1f)
		{
			offset = Vector3.Slerp(subX, -subY, (transAngle - 90f) / 90f);
		}
		else if (offset.x == 2f)
		{
			offset = Vector3.Slerp(-subY, -subX, (transAngle - 180f) / 90f);
		}
		else
		{
			offset = Vector3.Slerp(-subX, subY, (transAngle - 270f) / 90f);
		}
		offset *= transMag;
		mainPath += dynamicDir * speed * deltaTime;
		return startPos + mainPath + offset - base.transform.position;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		return Quaternion.FromToRotation(Vector3.forward, base.moveVector);
	}
}
