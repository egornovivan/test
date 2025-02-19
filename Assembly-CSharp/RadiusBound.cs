using UnityEngine;

public class RadiusBound : IBoundInScene
{
	private float _r;

	private float _cx;

	private float _cy;

	private float _cz;

	public RadiusBound(float r, float cx, float cy, float cz)
	{
		_r = r;
		_cx = cx;
		_cy = cy;
		_cz = cz;
	}

	public RadiusBound(RadiusBound local, Transform parent)
	{
		_r = local._r * parent.lossyScale.x;
		Vector3 vector = parent.TransformPoint(local._cx, local._cy, local._cz);
		_cx = vector.x;
		_cy = vector.y;
		_cz = vector.z;
	}

	public bool Contains(Vector3 pos, bool tstY)
	{
		float num = (_cx - pos.x) * (_cx - pos.x) + (_cz - pos.z) * (_cz - pos.z);
		if (tstY)
		{
			num += (_cy - pos.y) * (_cy - pos.y);
		}
		return num < _r * _r;
	}

	public bool Intersection(ref Bounds bound, bool tstY)
	{
		Vector3 min = bound.min;
		Vector3 max = bound.max;
		if (tstY)
		{
			return _cx - _r <= max.x && _cx + _r >= min.x && _cy - _r <= max.y && _cy + _r >= min.y && _cz - _r <= max.z && _cz + _r >= min.z;
		}
		return max.x > _cx - _r && min.x < _cx + _r && max.z > _cz - _r && min.z < _cz + _r;
	}
}
