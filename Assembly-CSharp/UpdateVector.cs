public class UpdateVector
{
	public const int length = 4;

	public byte xyz0;

	public byte xyz1;

	public byte voxelData0;

	public byte voxelData1;

	public int Data
	{
		get
		{
			int num = voxelData1;
			num <<= 8;
			num |= voxelData0;
			num <<= 8;
			num |= xyz1;
			num <<= 8;
			return num | xyz0;
		}
		set
		{
			xyz0 = (byte)value;
			xyz1 = (byte)(value >> 8);
			voxelData0 = (byte)(value >> 16);
			voxelData1 = (byte)(value >> 24);
		}
	}
}
