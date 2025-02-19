public class CVFVoxel
{
	public VFVoxel _value;

	public CVFVoxel(VFVoxel v)
	{
		_value = v;
	}

	public static implicit operator VFVoxel(CVFVoxel cv)
	{
		return cv._value;
	}

	public static implicit operator CVFVoxel(VFVoxel v)
	{
		return new CVFVoxel(v);
	}
}
