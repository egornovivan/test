using System.Collections.Generic;
using Pathea.Maths;
using UnityEngine;

namespace RedGrass;

public class RGDataSource
{
	public class ReqsOutput
	{
		public List<RGChunk> reqsChunk;

		public List<RGChunk> discardChunk;

		public ReqsOutput()
		{
			reqsChunk = new List<RGChunk>();
			discardChunk = new List<RGChunk>();
		}

		public void Clear()
		{
			reqsChunk.Clear();
			discardChunk.Clear();
		}

		public bool IsEmpty()
		{
			return reqsChunk.Count == 0 && discardChunk.Count == 0;
		}
	}

	private RGChunks mChunks;

	private EvniAsset mEvni;

	private ChunkDataIO mIO;

	public INTVECTOR2 mCenter = new INTVECTOR2(-100, -100);

	private ReqsOutput mReqsOutput = new ReqsOutput();

	public RGChunks Chunks => mChunks;

	public int SIZE => mEvni.CHUNKSIZE;

	public int SHIFT => mEvni.SHIFT;

	public int MASK => mEvni.MASK;

	public ChunkDataIO dataIO => mIO;

	public bool IsProcessReqs { get; private set; }

	public ReqsOutput reqsOutput => mReqsOutput;

	public RGChunk Node(int x, int z)
	{
		return mChunks[x, z];
	}

	public void Init(EvniAsset evni, ChunkDataIO io)
	{
		mChunks = new RGChunks();
		mEvni = evni;
		mIO = io;
		mIO.Init(evni);
	}

	public void Free()
	{
		Clear();
		mChunks = null;
	}

	public void Clear()
	{
		mChunks.Clear();
	}

	public RedGrassInstance Read(int x, int y, int z)
	{
		int x2 = x >> SHIFT;
		int z2 = z >> SHIFT;
		return mChunks[x2, z2]?.Read(x, y, z) ?? default(RedGrassInstance);
	}

	public List<RedGrassInstance> Read(int x, int z, int ymin, int ymax)
	{
		int x2 = x >> SHIFT;
		int z2 = z >> SHIFT;
		RGChunk rGChunk = mChunks[x2, z2];
		if (rGChunk != null)
		{
			return rGChunk.Read(x, z, ymin, ymax);
		}
		return new List<RedGrassInstance>();
	}

	public bool Write(RedGrassInstance rgi)
	{
		Vector3 position = rgi.Position;
		int num = (int)position.x >> SHIFT;
		int num2 = (int)position.z >> SHIFT;
		RGChunk rGChunk = mChunks[num, num2];
		if (rGChunk != null)
		{
			if (rgi.Density < 0.001f)
			{
				rGChunk.Remove((int)position.x, (int)position.y, (int)position.z);
			}
			else
			{
				rGChunk.Write(rgi);
			}
		}
		else if (rgi.Density > 0.001f)
		{
			rGChunk = RGPoolSig.GetChunk();
			rGChunk.Init(num, num2, mEvni);
			mChunks[num, num2] = rGChunk;
			rGChunk.Write(rgi);
		}
		return true;
	}

	public bool Remove(int x, int y, int z)
	{
		int x2 = x >> SHIFT;
		int z2 = z >> SHIFT;
		return mChunks[x2, z2]?.Remove(x, y, z) ?? false;
	}

	public void RefreshImmediately(int cx, int cz)
	{
		mIO.StopProcess();
		Clear();
		mReqsOutput.Clear();
		int dataExpandNum = mEvni.DataExpandNum;
		for (int i = cx - dataExpandNum; i <= cx + dataExpandNum; i++)
		{
			for (int j = cz - dataExpandNum; j <= cz + dataExpandNum; j++)
			{
				RGChunk chunk = RGPoolSig.GetChunk();
				chunk.Init(i, j, mEvni);
				mReqsOutput.reqsChunk.Add(chunk);
			}
		}
		mIO.AddReqs(mReqsOutput.reqsChunk);
		mIO.ProcessReqsImmediatly();
		foreach (RGChunk item in mReqsOutput.reqsChunk)
		{
			if (!item.isEmpty)
			{
				mChunks[item.xIndex, item.zIndex] = item;
			}
		}
		mCenter = new INTVECTOR2(cx, cz);
	}

	public void ReqsUpdate(int cx, int cz)
	{
		mReqsOutput.Clear();
		IsProcessReqs = true;
		int dataExpandNum = mEvni.DataExpandNum;
		for (int i = cx - dataExpandNum; i <= cx + dataExpandNum; i++)
		{
			for (int j = cz - dataExpandNum; j <= cz + dataExpandNum; j++)
			{
				if (!mChunks.Contains(i, j))
				{
					RGChunk chunk = RGPoolSig.GetChunk();
					chunk.Init(i, j, mEvni);
					mReqsOutput.reqsChunk.Add(chunk);
				}
			}
		}
		if (mReqsOutput.reqsChunk.Count != 0)
		{
			mIO.AddReqs(mReqsOutput.reqsChunk);
		}
		int num = cx + dataExpandNum;
		int num2 = cx - dataExpandNum;
		int num3 = cz + dataExpandNum;
		int num4 = cz - dataExpandNum;
		for (int k = mCenter.x - dataExpandNum; k <= mCenter.x + dataExpandNum; k++)
		{
			for (int l = mCenter.y - dataExpandNum; l <= mCenter.y + dataExpandNum; l++)
			{
				if (k < num2 || k > num || l < num4 || l > num3)
				{
					RGChunk rGChunk = mChunks[k, l];
					if (rGChunk != null)
					{
						mReqsOutput.discardChunk.Add(rGChunk);
					}
				}
			}
		}
		if (mReqsOutput.reqsChunk.Count != 0 || mReqsOutput.discardChunk.Count != 0)
		{
			mIO.Active(delegate
			{
				IsProcessReqs = false;
			});
		}
	}

	public void SumbmitReqs(INTVECTOR2 center)
	{
		for (int i = 0; i < mReqsOutput.discardChunk.Count; i++)
		{
			mReqsOutput.discardChunk[i].Free();
			if (mChunks.Remove(mReqsOutput.discardChunk[i].xIndex, mReqsOutput.discardChunk[i].zIndex))
			{
				RGPoolSig.RecycleChunk(mReqsOutput.discardChunk[i]);
			}
		}
		for (int j = 0; j < mReqsOutput.reqsChunk.Count; j++)
		{
			if (mReqsOutput.reqsChunk[j].isEmpty)
			{
				RGPoolSig.RecycleChunk(mReqsOutput.reqsChunk[j]);
			}
			else
			{
				mChunks[mReqsOutput.reqsChunk[j].xIndex, mReqsOutput.reqsChunk[j].zIndex] = mReqsOutput.reqsChunk[j];
			}
		}
		mCenter = center;
		mReqsOutput.Clear();
	}
}
