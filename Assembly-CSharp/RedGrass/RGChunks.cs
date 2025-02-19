using System.Collections.Generic;

namespace RedGrass;

public class RGChunks
{
	private Dictionary<int, RGChunk> _dicData = new Dictionary<int, RGChunk>(4096);

	public RGChunk this[int x, int z]
	{
		get
		{
			RGChunk value = null;
			int key = Utils.PosToIndex(x, z);
			_dicData.TryGetValue(key, out value);
			return value;
		}
		set
		{
			int key = Utils.PosToIndex(x, z);
			_dicData[key] = value;
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<int, RGChunk> dicDatum in _dicData)
		{
			dicDatum.Value.Free();
			RGPoolSig.RecycleChunk(dicDatum.Value);
		}
		_dicData.Clear();
	}

	public bool Contains(int x, int z)
	{
		int key = Utils.PosToIndex(x, z);
		return _dicData.ContainsKey(key);
	}

	public bool Remove(int x, int z)
	{
		int key = Utils.PosToIndex(x, z);
		return _dicData.Remove(key);
	}
}
