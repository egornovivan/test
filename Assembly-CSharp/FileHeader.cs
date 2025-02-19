public struct FileHeader
{
	public byte version;

	public byte cellLength;

	public byte chunkType;

	public byte chunkSize;

	public byte chunkPrefix;

	public byte chunkPostfix;

	public ushort chunkCountX;

	public ushort chunkCountY;

	public ushort chunkCountZ;

	public ushort voxelRes;

	public byte chunkOffsetDesc;
}
