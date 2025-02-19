namespace Transvoxel.SurfaceExtractor;

internal class TransitionCache
{
	private readonly ReuseCell[] _cache;

	public ReuseCell this[int x, int y]
	{
		get
		{
			return _cache[x + (y & 1) * 32];
		}
		set
		{
			_cache[x + (y & 1) * 32] = value;
		}
	}

	public TransitionCache()
	{
		_cache = new ReuseCell[64];
		for (int i = 0; i < 64; i++)
		{
			_cache[i] = new ReuseCell(12);
		}
	}
}
