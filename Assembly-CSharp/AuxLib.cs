using System.IO;
using UnityEngine;

public class AuxLib
{
	private const int PXAxis = 0;

	private const int NXAxis = 1;

	private const int PZAxis = 2;

	private const int NZAxis = 3;

	private const int PYAxis = 4;

	private const int NYAxis = 5;

	private const int NoDir = 6;

	private Vector3 m_axisPX = new Vector3(1f, 0f, 0f);

	private Vector3 m_axisNX = new Vector3(-1f, 0f, 0f);

	private Vector3 m_axisPY = new Vector3(0f, 1f, 0f);

	private Vector3 m_axisNY = new Vector3(0f, -1f, 0f);

	private Vector3 m_axisPZ = new Vector3(0f, 0f, 1f);

	private Vector3 m_axisNZ = new Vector3(0f, 0f, -1f);

	private static AuxLib m_this;

	public static AuxLib Get()
	{
		if (m_this == null)
		{
			m_this = new AuxLib();
		}
		return m_this;
	}

	public float DistSqRayRay(ref Ray _ray0, ref Ray _ray1, ref float _s0, ref float _s1)
	{
		Vector3 lhs = _ray0.origin - _ray1.origin;
		float num = 0f - Vector3.Dot(_ray0.direction, _ray1.direction);
		float num2 = Vector3.Dot(lhs, _ray0.direction);
		float sqrMagnitude = lhs.sqrMagnitude;
		float num3 = Mathf.Abs(1f - num * num);
		float num6;
		if (num3 >= float.Epsilon)
		{
			float num4 = 0f - Vector3.Dot(lhs, _ray1.direction);
			_s0 = num * num4 - num2;
			_s1 = num * num2 - num4;
			if (_s0 >= 0f)
			{
				if (_s1 >= 0f)
				{
					float num5 = 1f / num3;
					_s0 *= num5;
					_s1 *= num5;
					num6 = _s0 * (_s0 + num * _s1 + 2f * num2) + _s1 * (num * _s0 + _s1 + 2f * num4) + sqrMagnitude;
				}
				else
				{
					_s1 = 0f;
					if (num2 >= 0f)
					{
						_s0 = 0f;
						num6 = sqrMagnitude;
					}
					else
					{
						_s0 = 0f - num2;
						num6 = num2 * _s0 + sqrMagnitude;
					}
				}
			}
			else if (_s1 >= 0f)
			{
				_s0 = 0f;
				if (num4 >= 0f)
				{
					_s1 = 0f;
					num6 = sqrMagnitude;
				}
				else
				{
					_s1 = 0f - num4;
					num6 = num4 * _s1 + sqrMagnitude;
				}
			}
			else if (num2 < 0f)
			{
				_s0 = 0f - num2;
				_s1 = 0f;
				num6 = num2 * _s0 + sqrMagnitude;
			}
			else
			{
				_s0 = 0f;
				if (num4 >= 0f)
				{
					_s1 = 0f;
					num6 = sqrMagnitude;
				}
				else
				{
					_s1 = 0f - num4;
					num6 = num4 * _s1 + sqrMagnitude;
				}
			}
		}
		else if (num > 0f)
		{
			_s1 = 0f;
			if (num2 >= 0f)
			{
				_s0 = 0f;
				num6 = sqrMagnitude;
			}
			else
			{
				_s0 = 0f - num2;
				num6 = num2 * _s0 + sqrMagnitude;
			}
		}
		else if (num2 >= 0f)
		{
			float num4 = 0f - Vector3.Dot(lhs, _ray1.direction);
			_s0 = 0f;
			_s1 = 0f - num4;
			num6 = num4 * _s1 + sqrMagnitude;
		}
		else
		{
			_s0 = 0f - num2;
			_s1 = 0f;
			num6 = num2 * _s0 + sqrMagnitude;
		}
		if (num6 < 0f)
		{
			num6 = 0f;
		}
		return num6;
	}

	public void GetNearestRayDir(ref Ray _screenRay, ref Vector3 _srcPos, ref Vector3 _dir, ref Vector3 _dstPos)
	{
		Ray[] array = new Ray[6]
		{
			new Ray(_srcPos, m_axisPX),
			new Ray(_srcPos, m_axisNX),
			new Ray(_srcPos, m_axisPZ),
			new Ray(_srcPos, m_axisNZ),
			new Ray(_srcPos, m_axisPY),
			new Ray(_srcPos, m_axisNY)
		};
		float num = float.MaxValue;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i != 6; i++)
		{
			if (array[i].direction.x != 0f || array[i].direction.y != 0f || array[i].direction.z != 0f)
			{
				float _s = 0f;
				float _s2 = 0f;
				float num4 = DistSqRayRay(ref array[i], ref _screenRay, ref _s, ref _s2);
				if (num > num4)
				{
					num = num4;
					num2 = _s;
					num3 = i;
				}
			}
		}
		_dir = array[num3].direction;
		_dstPos = array[num3].origin + array[num3].direction * num2;
	}

	public float RandomValue()
	{
		return 0.001f * (float)Random.Range(0, 1000);
	}

	public float RandomValue(float _fBaseValue)
	{
		int min = (int)(_fBaseValue * 1000f);
		return 0.001f * (float)Random.Range(min, 1000);
	}

	public Color RandomColor()
	{
		Color result = default(Color);
		result.r = RandomValue();
		result.g = RandomValue();
		result.b = RandomValue();
		return result;
	}

	public Color RandomColor(Color _baseClr)
	{
		Color result = default(Color);
		result.r = RandomValue(_baseClr.r);
		result.g = RandomValue(_baseClr.g);
		result.b = RandomValue(_baseClr.b);
		return result;
	}

	public void ReadIntVector3(BinaryReader _in, IntVector3 _value)
	{
		_value.x = _in.ReadInt32();
		_value.y = _in.ReadInt32();
		_value.z = _in.ReadInt32();
	}

	public void WriteIntVector3(BinaryWriter _out, IntVector3 _value)
	{
		_out.Write(_value.x);
		_out.Write(_value.y);
		_out.Write(_value.z);
	}

	public void ReadVector3(BinaryReader _in, ref Vector3 _value)
	{
		_value.x = _in.ReadSingle();
		_value.y = _in.ReadSingle();
		_value.z = _in.ReadSingle();
	}

	public void WriteVector3(BinaryWriter _out, ref Vector3 _value)
	{
		_out.Write(_value.x);
		_out.Write(_value.y);
		_out.Write(_value.z);
	}

	public void ReadColor(BinaryReader _in, ref Color _clr)
	{
		_clr.r = _in.ReadSingle();
		_clr.g = _in.ReadSingle();
		_clr.b = _in.ReadSingle();
		_clr.a = _in.ReadSingle();
	}

	public void WriteColor(BinaryWriter _out, ref Color _clr)
	{
		_out.Write(_clr.r);
		_out.Write(_clr.g);
		_out.Write(_clr.b);
		_out.Write(_clr.a);
	}
}
