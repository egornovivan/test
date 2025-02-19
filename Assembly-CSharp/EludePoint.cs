using PETools;
using UnityEngine;

public class EludePoint
{
	private Vector3 m_Position;

	private Vector3 m_Direction;

	private Vector3 m_FaceDirection;

	private bool m_Dirty;

	public Vector3 Position => m_Position;

	public Vector3 Direction => m_Direction;

	public Vector3 FaceDirection => m_FaceDirection;

	public bool Dirty
	{
		get
		{
			return m_Dirty;
		}
		set
		{
			m_Dirty = value;
		}
	}

	public EludePoint(Vector3 argPos, Vector3 argDir, Vector3 argFace)
	{
		m_Position = argPos;
		m_Direction = argDir;
		m_FaceDirection = argFace;
	}

	public bool Elude(Vector3 pos)
	{
		return PEUtil.SqrMagnitudeH(m_Position, pos) < 1f;
	}

	public bool CanElude(Vector3 pos)
	{
		int layerMask = 73728;
		return Physics.Raycast(m_Position, pos - m_Position, Vector3.Distance(pos, m_Position), layerMask);
	}
}
