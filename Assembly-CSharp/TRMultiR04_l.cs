using UnityEngine;

public class TRMultiR04_l : Trajectory
{
	private static Vector3[] originPos = new Vector3[2]
	{
		new Vector3(1.7f, -1.71f, 0f),
		new Vector3(1.27f, -6.06f, 0f)
	};

	private static Vector3 offset = new Vector3(-0.04f, -0.375f, 0f);

	public float[] fwdAngle = new float[4] { 90f, 90f, 90f, 90f };

	public float speed = 80f;

	public byte index;

	private int rand1;

	private int rand2;

	private float rand3;

	private Vector3 axis;

	public void Emit(byte index)
	{
		this.index = index;
	}

	private void Start()
	{
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().useGravity = false;
		}
		if (fwdAngle.Length != 4)
		{
			Debug.LogWarning("length <> 4");
			return;
		}
		Random.seed = (int)(Time.time % 10f * 1000f);
		rand1 = Random.Range(0, 6);
		rand2 = Random.Range(0, 2);
		rand3 = Random.value;
		base.transform.rotation = Quaternion.identity;
		base.transform.position += Quaternion.AngleAxis((float)rand1 * 60f, Vector3.up) * originPos[rand2] + offset * (index - 1);
		axis = Quaternion.AngleAxis((float)rand1 * 60f, Vector3.up) * Vector3.right;
		base.transform.forward = Vector3.Slerp(Vector3.down, axis, (fwdAngle[rand2 * 2] + (fwdAngle[rand2 * 2 + 1] - fwdAngle[rand2 * 2]) * rand3) / 90f);
	}

	public override Vector3 Track(float deltaTime)
	{
		return base.transform.forward * speed * deltaTime;
	}
}
