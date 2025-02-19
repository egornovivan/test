using UnityEngine;

public class CircleTran
{
	private Vector3 _centerPos;

	private Vector3 _Postion;

	private Quaternion _Rotation;

	private bool _IsOccpied;

	private int _OccpiedId;

	public Vector3 CenterPos => _centerPos;

	public Vector3 Postion => _Postion;

	public Quaternion Rotation => _Rotation;

	public bool IsOccpied
	{
		get
		{
			return _IsOccpied;
		}
		set
		{
			_IsOccpied = value;
		}
	}

	public int OccpiedId => _OccpiedId;

	public CircleTran(Vector3 Center, Vector3 postion, Quaternion rotation, int occpiedId = 0, bool isOccpied = false)
	{
		_centerPos = Center;
		_Postion = postion;
		_Rotation = rotation;
		_OccpiedId = occpiedId;
		_IsOccpied = isOccpied;
	}

	public void Occpy(int Id)
	{
		_OccpiedId = Id;
		_IsOccpied = true;
	}

	public void Release()
	{
		_OccpiedId = 0;
		_IsOccpied = false;
	}

	public bool CantainId(int Id)
	{
		return _OccpiedId == Id;
	}
}
