using System;

namespace FMOD.Studio;

public abstract class HandleBase
{
	protected IntPtr rawPtr;

	public HandleBase(IntPtr newPtr)
	{
		rawPtr = newPtr;
	}

	public bool isValid()
	{
		return rawPtr != IntPtr.Zero && isValidInternal();
	}

	protected abstract bool isValidInternal();

	public IntPtr getRaw()
	{
		return rawPtr;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as HandleBase);
	}

	public bool Equals(HandleBase p)
	{
		return (object)p != null && rawPtr == p.rawPtr;
	}

	public override int GetHashCode()
	{
		return rawPtr.ToInt32();
	}

	public static bool operator ==(HandleBase a, HandleBase b)
	{
		if (object.ReferenceEquals(a, b))
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.rawPtr == b.rawPtr;
	}

	public static bool operator !=(HandleBase a, HandleBase b)
	{
		return !(a == b);
	}
}
