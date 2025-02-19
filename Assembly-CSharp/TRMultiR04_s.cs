using UnityEngine;

public class TRMultiR04_s : Trajectory
{
	private static Vector3[] originPos = new Vector3[3]
	{
		new Vector3(0f, -2.85f, 2.07f),
		new Vector3(0.002362892f, -6.941441f, 3.017968f),
		new Vector3(0.003171829f, -9.184122f, 1.870163f)
	};

	public float[] fwdAngle = new float[6] { 90f, 90f, 90f, 90f, 90f, 90f };

	public float speed = 80f;

	private void Start()
	{
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().useGravity = false;
		}
		if (fwdAngle.Length != 6)
		{
			Debug.LogWarning("length <> 6");
			return;
		}
		int num = Random.Range(0, 6);
		int num2 = Random.Range(0, 3);
		base.transform.rotation = Quaternion.identity;
		base.transform.position += Quaternion.AngleAxis((float)num * 60f, Vector3.up) * originPos[num2];
		Vector3 b = Quaternion.AngleAxis((float)num * 60f, Vector3.up) * Vector3.forward;
		base.transform.forward = Vector3.Slerp(Vector3.down, b, (fwdAngle[num2 * 2] + (fwdAngle[num2 * 2 + 1] - fwdAngle[num2 * 2]) * Random.value) / 90f);
	}

	public override Vector3 Track(float deltaTime)
	{
		return base.transform.forward * speed * deltaTime;
	}
}
