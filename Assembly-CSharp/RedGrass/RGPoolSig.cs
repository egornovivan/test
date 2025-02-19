using System.Collections.Generic;

namespace RedGrass;

public class RGPoolSig
{
	private const int CHUNK_POOL_COUNT = 1280;

	private const int RGLIST_POO_COUNT = 100;

	private static RGPoolSig _self;

	private RGSingletonPool<RGChunk> mChunkPool;

	private RGListPool mRGListPool;

	public static RGPoolSig Self
	{
		get
		{
			if (_self == null)
			{
				_self = new RGPoolSig();
			}
			return _self;
		}
	}

	public RGPoolSig()
	{
		mChunkPool = new RGSingletonPool<RGChunk>(1280);
		mRGListPool = new RGListPool(100, 10);
	}

	public static void Init()
	{
		if (_self == null)
		{
			_self = new RGPoolSig();
		}
	}

	public static void Destroy()
	{
		if (_self != null)
		{
			_self.mRGListPool.Destroy();
		}
	}

	public static RGChunk GetChunk()
	{
		return _self.mChunkPool.Get();
	}

	public static void RecycleChunk(RGChunk chunk)
	{
		chunk.Free();
		_self.mChunkPool.Recycle(chunk);
	}

	public static List<RedGrassInstance> GetRGList()
	{
		return _self.mRGListPool.Get();
	}

	public static void RecycleRGList(Dictionary<int, List<RedGrassInstance>> grasses)
	{
		_self.mRGListPool.Recycle(grasses);
	}

	public static void RecycleRGList(List<RedGrassInstance> list)
	{
		_self.mRGListPool.Recycle(list);
	}
}
