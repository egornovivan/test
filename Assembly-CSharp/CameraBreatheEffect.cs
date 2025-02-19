using System;
using UnityEngine;

public class CameraBreatheEffect : CamEffect
{
	public Vector3 PosAmp = Vector3.zero;

	public Vector3 PosT = Vector3.one;

	public Vector3 PosPhase = Vector3.zero;

	public Vector3 RotAmp = Vector3.zero;

	public Vector3 RotT = Vector3.one;

	public Vector3 RotPhase = Vector3.zero;

	private float t;

	public override void Do()
	{
		t += Mathf.Clamp(Time.deltaTime, 0.001f, 0.025f);
		PosT.x = Mathf.Abs(PosT.x);
		PosT.y = Mathf.Abs(PosT.y);
		PosT.z = Mathf.Abs(PosT.z);
		RotT.x = Mathf.Abs(RotT.x);
		RotT.y = Mathf.Abs(RotT.y);
		RotT.z = Mathf.Abs(RotT.z);
		if (PosT.x < 0.05f)
		{
			PosT.x = 0.05f;
		}
		if (PosT.y < 0.05f)
		{
			PosT.y = 0.05f;
		}
		if (PosT.z < 0.05f)
		{
			PosT.z = 0.05f;
		}
		if (RotT.x < 0.05f)
		{
			RotT.x = 0.05f;
		}
		if (RotT.y < 0.05f)
		{
			RotT.y = 0.05f;
		}
		if (RotT.z < 0.05f)
		{
			RotT.z = 0.05f;
		}
		float num = (float)Math.PI * 2f;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		zero.x = PosAmp.x * Mathf.Sin((t / PosT.x + PosPhase.x) * num);
		zero.y = PosAmp.y * Mathf.Sin((t / PosT.y + PosPhase.y) * num);
		zero.z = PosAmp.z * Mathf.Sin((t / PosT.z + PosPhase.z) * num);
		zero2.x = RotAmp.x * Mathf.Sin((t / RotT.x + RotPhase.x) * num);
		zero2.y = RotAmp.y * Mathf.Sin((t / RotT.y + RotPhase.y) * num);
		zero2.z = RotAmp.z * Mathf.Sin((t / RotT.z + RotPhase.z) * num);
		m_TargetCam.transform.position += zero;
		m_TargetCam.transform.eulerAngles += zero2;
	}
}
