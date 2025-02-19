using System.Collections.Generic;

public class RectCacheDict<TValue>
{
	private Dictionary<IntVector2, TValue> _dict = new Dictionary<IntVector2, TValue>();

	private IntVector2[][] _posMap;

	private IntVector2 _lastRemovedKey;

	private TValue _lastRemovedValue;

	public IntVector2 LastRemovedKey => _lastRemovedKey;

	public TValue LastRemovedValue => _lastRemovedValue;

	public int MapSizeX { get; set; }

	public int MapSizeY { get; set; }

	public RectCacheDict(int sizeX, int sizeY)
	{
		MapSizeX = sizeX;
		MapSizeY = sizeY;
		_posMap = new IntVector2[MapSizeX][];
		for (int i = 0; i < MapSizeX; i++)
		{
			_posMap[i] = new IntVector2[MapSizeY];
		}
	}

	public void Add(IntVector2 key, TValue value)
	{
		int i;
		for (i = key.x; i >= MapSizeX; i -= MapSizeX)
		{
		}
		for (; i < 0; i += MapSizeX)
		{
		}
		int j;
		for (j = key.y; j >= MapSizeY; j -= MapSizeY)
		{
		}
		for (; j < 0; j += MapSizeY)
		{
		}
		_lastRemovedKey = _posMap[i][j];
		_posMap[i][j] = key;
		if (_lastRemovedKey != null)
		{
			_lastRemovedValue = _dict[_lastRemovedKey];
			_dict.Remove(_lastRemovedKey);
		}
		_dict.Add(key, value);
	}

	public bool TryGetValue(IntVector2 key, out TValue value)
	{
		return _dict.TryGetValue(key, out value);
	}
}
