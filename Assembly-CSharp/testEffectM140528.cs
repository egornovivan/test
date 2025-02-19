using UnityEngine;

internal class testEffectM140528 : MonoBehaviour
{
	public float speed;

	public int index;

	public int AngleUnit = 180;

	public float delay = 0.1f;

	public float interval = 0.1f;

	public float heightMin = 0.3f;

	public float heightMax = 0.6f;

	public float offsetMin = 0.2f;

	public float offsetMax = 0.4f;

	public float circlePSCD = 3f;

	public GameObject target;

	private float timeNow;

	private Vector3 startPos;

	private Vector3 lastPos;

	private Vector3 forwardVector;

	private Vector3 frameVector;

	private float distanceNow;

	private float progress;

	private bool valid;

	private float transMag;

	private Vector3 subZ;

	private Vector3 fsubZ;

	private Vector3 subY = Vector3.up;

	private Vector3 fsubY = Vector3.up;

	private float angleNow;

	private int angleStart;

	public void Start()
	{
		startPos = base.transform.position;
		lastPos = startPos;
		angleStart = AngleUnit * index;
	}

	public void FixedUpdate()
	{
		if (!valid)
		{
			MissileSwitch();
		}
		if (valid)
		{
			forwardVector = target.transform.position - startPos;
			distanceNow += speed * Time.deltaTime;
			progress = distanceNow / forwardVector.magnitude;
			transMag = progress * 2f - 1f;
			transMag = (1f - transMag * transMag) * heightMin * forwardVector.magnitude;
			subZ = forwardVector;
			Vector3.OrthoNormalize(ref subZ, ref subY);
			frameVector = startPos + forwardVector * progress + subY * transMag - lastPos;
			lastPos = startPos + forwardVector * progress + subY * transMag;
			fsubZ = frameVector;
			Vector3.OrthoNormalize(ref fsubZ, ref fsubY);
			angleNow += circlePSCD * Time.deltaTime * 360f;
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, lastPos + Quaternion.AngleAxis((float)angleStart + angleNow, fsubZ) * (fsubY * offsetMin) - base.transform.position);
			base.transform.position = lastPos + Quaternion.AngleAxis((float)angleStart + angleNow, fsubZ) * (fsubY * offsetMin);
		}
	}

	private void MissileSwitch()
	{
		timeNow += Time.deltaTime;
		if (timeNow > delay + interval * (float)index - interval)
		{
			valid = true;
			timeNow = 0f;
		}
	}
}
