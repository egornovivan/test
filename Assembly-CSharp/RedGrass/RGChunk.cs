using System.Collections.Generic;
using Pathea.Graphic;
using Pathea.Maths;
using UnityEngine;

namespace RedGrass;

public class RGChunk
{
	private EvniAsset mEvni;

	public int xIndex;

	public int zIndex;

	private Dictionary<int, RedGrassInstance> mGrasses;

	private Dictionary<int, List<RedGrassInstance>> mHGrass;

	public static bool AnyDirty;

	private bool _dirty;

	private int mTimeStamp;

	private int mTimeStampOutofdate;

	public Dictionary<int, RedGrassInstance> grasses => mGrasses;

	public bool isEmpty => mGrasses.Count == 0;

	public Dictionary<int, List<RedGrassInstance>> HGrass => mHGrass;

	public bool Dirty
	{
		get
		{
			return _dirty;
		}
		set
		{
			_dirty = value;
			AnyDirty |= _dirty;
		}
	}

	public int SIZE => mEvni.CHUNKSIZE;

	public int SHIFT => mEvni.SHIFT;

	public int MASK => mEvni.MASK;

	public void UpdateTimeStamp()
	{
		mTimeStampOutofdate++;
	}

	public void SyncTimeStamp()
	{
		mTimeStamp = mTimeStampOutofdate;
	}

	public bool IsOutOfDate()
	{
		return mTimeStamp != mTimeStampOutofdate;
	}

	public void Init(int cx, int cz, EvniAsset evni)
	{
		mEvni = evni;
		xIndex = cx;
		zIndex = cz;
		if (mGrasses == null)
		{
			mGrasses = new Dictionary<int, RedGrassInstance>();
		}
		if (mHGrass == null)
		{
			mHGrass = new Dictionary<int, List<RedGrassInstance>>();
		}
	}

	public RedGrassInstance Read(int x, int y, int z)
	{
		int key = PosToKey(Mathf.FloorToInt(x) & MASK, Mathf.FloorToInt(z) & MASK);
		if (mGrasses.ContainsKey(key))
		{
			RedGrassInstance result = mGrasses[key];
			if ((int)result.Position.y == y)
			{
				return result;
			}
			if (mHGrass.ContainsKey(key))
			{
				return mHGrass[key].Find((RedGrassInstance item0) => (int)item0.Position.y == y);
			}
		}
		return default(RedGrassInstance);
	}

	public List<RedGrassInstance> Read(int x, int z, int ymin, int ymax)
	{
		int key = PosToKey(Mathf.FloorToInt(x) & MASK, Mathf.FloorToInt(z) & MASK);
		List<RedGrassInstance> list = new List<RedGrassInstance>();
		if (mGrasses.ContainsKey(key))
		{
			RedGrassInstance item = mGrasses[key];
			if ((int)item.Position.y >= ymin && (int)item.Position.y <= ymax)
			{
				list.Add(item);
			}
			if (mHGrass.ContainsKey(key))
			{
				foreach (RedGrassInstance item2 in mHGrass[key])
				{
					if ((int)item2.Position.y >= ymin && (int)item2.Position.y <= ymax)
					{
						list.Add(item2);
					}
				}
			}
		}
		return list;
	}

	public void Write(RedGrassInstance grass)
	{
		Vector3 position = grass.Position;
		int y = (int)position.y;
		int key = PosToKey(Mathf.FloorToInt(position.x) & MASK, Mathf.FloorToInt(position.z) & MASK);
		if (mGrasses.ContainsKey(key))
		{
			if ((int)mGrasses[key].Position.y == y)
			{
				mGrasses[key] = grass;
			}
			else if (mHGrass.ContainsKey(key))
			{
				List<RedGrassInstance> list = mHGrass[key];
				int num = list.FindIndex((RedGrassInstance item0) => (int)item0.Position.y == y);
				if (num != -1)
				{
					list[num] = grass;
				}
				else
				{
					list.Add(grass);
				}
			}
			else
			{
				List<RedGrassInstance> rGList = RGPoolSig.GetRGList();
				rGList.Add(grass);
				mHGrass[key] = rGList;
			}
		}
		else
		{
			mGrasses.Add(key, grass);
		}
		Dirty = true;
	}

	public bool Remove(int x, int y, int z)
	{
		int key = PosToKey(Mathf.FloorToInt(x) & MASK, Mathf.FloorToInt(z) & MASK);
		if (mGrasses.ContainsKey(key))
		{
			if ((int)mGrasses[key].Position.y == y)
			{
				if (mHGrass.ContainsKey(key))
				{
					List<RedGrassInstance> list = mHGrass[key];
					mGrasses[key] = list[0];
					list.RemoveAt(0);
					if (list.Count == 0)
					{
						RGPoolSig.RecycleRGList(list);
						mHGrass.Remove(key);
					}
					Dirty = true;
					return true;
				}
				Dirty = true;
				mGrasses.Remove(key);
				return true;
			}
			if (mHGrass.ContainsKey(key))
			{
				int num = mHGrass[key].FindIndex((RedGrassInstance item0) => (int)item0.Position.y == y);
				if (num != -1)
				{
					mHGrass[key].RemoveAt(num);
					if (mHGrass[key].Count == 0)
					{
						mHGrass.Remove(key);
					}
					Dirty = true;
					return true;
				}
			}
		}
		return false;
	}

	public void Free()
	{
		lock (this)
		{
			mGrasses.Clear();
			if (mHGrass.Count > 0)
			{
				foreach (KeyValuePair<int, List<RedGrassInstance>> item in mHGrass)
				{
					item.Value.Clear();
					RGPoolSig.RecycleRGList(item.Value);
				}
				mHGrass.Clear();
			}
			Dirty = true;
		}
	}

	public void DrawAreaInEditor()
	{
		INTVECTOR3 iNTVECTOR = new INTVECTOR3(mEvni.CHUNKSIZE, 0, mEvni.CHUNKSIZE);
		INTVECTOR3 iNTVECTOR2 = new INTVECTOR3(xIndex, 0, zIndex);
		Color yellow = Color.yellow;
		EditorGraphics.DrawXZRect(iNTVECTOR2 * iNTVECTOR, iNTVECTOR, yellow);
	}

	public int PosToKey(int dx, int dy)
	{
		return (dx << mEvni.SHIFT) | dy;
	}
}
