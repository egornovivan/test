public class DHElem
{
	public enum FunctorType
	{
		FUNCTOR_
	}

	public const int AxisSize = 1;

	public const int SquareSize = 1;

	public const int CubicSize = 1;

	public VFVoxel[] vData = new VFVoxel[1];

	public byte Volume
	{
		get
		{
			return vData[0].Volume;
		}
		set
		{
			int num = vData.Length;
			for (int i = 0; i < num; i++)
			{
				vData[i].Volume = value;
			}
		}
	}

	public byte Type
	{
		get
		{
			return vData[0].Type;
		}
		set
		{
			int num = vData.Length;
			for (int i = 0; i < num; i++)
			{
				vData[i].Type = value;
			}
		}
	}

	public DHElem(byte type)
	{
		int num = vData.Length;
		for (int i = 0; i < num; i++)
		{
			ref VFVoxel reference = ref vData[i];
			reference = new VFVoxel(byte.MaxValue, type);
		}
	}

	public DHElem(byte vol, byte type)
	{
		int num = vData.Length;
		for (int i = 0; i < num; i++)
		{
			ref VFVoxel reference = ref vData[i];
			reference = new VFVoxel(vol, type);
		}
	}
}
