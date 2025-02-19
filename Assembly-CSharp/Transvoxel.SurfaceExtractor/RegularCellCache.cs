namespace Transvoxel.SurfaceExtractor;

internal class RegularCellCache
{
	private readonly ReuseCell[][] _cache;

	private int chunkSize;

	public ReuseCell this[int x, int y, int z]
	{
		get
		{
			return _cache[x & 1][y * chunkSize + z];
		}
		set
		{
			_cache[x & 1][y * chunkSize + z] = value;
		}
	}

	public ReuseCell this[IntVector3 v]
	{
		get
		{
			return this[v.x, v.y, v.z];
		}
		set
		{
			this[v.x, v.y, v.z] = value;
		}
	}

	public RegularCellCache(int chunksize)
	{
		chunkSize = chunksize;
		_cache = new ReuseCell[2][];
		_cache[0] = new ReuseCell[chunkSize * chunkSize];
		_cache[1] = new ReuseCell[chunkSize * chunkSize];
		for (int i = 0; i < chunkSize * chunkSize; i++)
		{
			_cache[0][i] = new ReuseCell(4);
			_cache[1][i] = new ReuseCell(4);
		}
	}

	public ReuseCell GetReusedIndex(IntVector3 pos, byte rDir)
	{
		int num = rDir & 1;
		int num2 = (rDir >> 1) & 1;
		int num3 = (rDir >> 2) & 1;
		int num4 = pos.x - num;
		int num5 = pos.y - num3;
		int num6 = pos.z - num2;
		return _cache[num4 & 1][num5 * chunkSize + num6];
	}

	internal void SetReusableIndex(IntVector3 pos, byte reuseIndex, ushort p)
	{
		_cache[pos.x & 1][pos.y * chunkSize + pos.z].Verts[reuseIndex] = p;
	}
}
