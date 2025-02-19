using UnityEngine;

public class TRParabola : Trajectory
{
	public float speed;

	public float heightScale;

	public bool followRotate;

	public float selfRotateW;

	private Vector3 startPos;

	private float totalLenth;

	private float maxMagnitude;

	private Vector3 subZ;

	private Vector3 subY;

	private float progressOffset;

	private float progress;

	private float transMag;

	private Vector3 offset;

	private void Start()
	{
		Emit((!m_Target) ? m_TargetPosition : GetTargetCenter(m_Target));
	}

	public void Emit(Vector3 target)
	{
		startPos = base.transform.position;
		Vector3 vector = target - startPos;
		totalLenth = vector.magnitude;
		maxMagnitude = totalLenth * heightScale;
		subZ = vector.normalized;
		Vector3 normalized = Vector3.Cross(Vector3.up, new Vector3(subZ.x, 0f, subZ.z)).normalized;
		subY = Vector3.Cross(subZ, normalized);
	}

	public override Vector3 Track(float deltaTime)
	{
		progressOffset += speed * Time.deltaTime;
		progress = progressOffset / totalLenth;
		transMag = progress * 2f - 1f;
		transMag = (1f - transMag * transMag) * maxMagnitude;
		offset = subY * transMag;
		return startPos + progressOffset * subZ + offset - base.transform.position;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		if (followRotate)
		{
			return Quaternion.FromToRotation(Vector3.forward, base.moveVector);
		}
		return base.transform.rotation;
	}
}
