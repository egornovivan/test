using System;
using System.Collections.Generic;
using System.Threading;
using Pathea.Maths;
using RedGrass;
using UnityEngine;

public class PeGrassDataIO_Random : Chunk32DataIO, IRefreshDataDetect
{
	private Queue<RGChunk> mReqs;

	private bool mRunning = true;

	private bool mActive;

	private bool mStart;

	private Thread mThread;

	private OnReqsFinished onReqsFinished;

	public override void Init(EvniAsset evni)
	{
		base.Init(evni);
		mEvni.SetDensity(mEvni.Density);
		mEvni.SetLODType(ELodType.LOD_2_TYPE_2);
		mEvni.LODDensities = new float[mEvni.MaxLOD + 1];
		for (int i = 0; i < mEvni.MaxLOD + 1; i++)
		{
			if (i == 0)
			{
				mEvni.LODDensities[i] = 1f;
			}
			else
			{
				mEvni.LODDensities[i] = 0.4f / (float)(1 << i - 1);
			}
		}
		mReqs = new Queue<RGChunk>(100);
		if (mThread == null)
		{
			mThread = new Thread(Run);
			mThread.Name = " Story mode Grass IO Thread";
			mThread.Start();
		}
	}

	public override bool AddReqs(RGChunk chunk)
	{
		if (mActive)
		{
			Debug.LogError("The data IO is already acitve");
			return false;
		}
		lock (mReqs)
		{
			mReqs.Enqueue(chunk);
		}
		return true;
	}

	public override bool AddReqs(List<RGChunk> chunks)
	{
		if (mActive)
		{
			Debug.LogError("The data IO is already acitve");
			return false;
		}
		lock (mReqs)
		{
			foreach (RGChunk chunk in chunks)
			{
				mReqs.Enqueue(chunk);
			}
		}
		return true;
	}

	public override void ClearReqs()
	{
	}

	public override void StopProcess()
	{
		mActive = false;
		lock (mReqs)
		{
			mReqs.Clear();
		}
	}

	public override void ProcessReqsImmediatly()
	{
	}

	public override bool Active(OnReqsFinished call_back)
	{
		if (IsEmpty())
		{
			mActive = false;
			return false;
		}
		mActive = true;
		onReqsFinished = call_back;
		mStart = true;
		return true;
	}

	public override void Deactive()
	{
		mActive = false;
	}

	public override bool isActive()
	{
		return mActive;
	}

	private bool IsEmpty()
	{
		bool flag = false;
		lock (mReqs)
		{
			return mReqs.Count == 0;
		}
	}

	public bool CanRefresh(RGScene scene)
	{
		bool result = false;
		if (Time.frameCount % 16 == 0)
		{
			result = !scene.dataIO.isActive();
		}
		return result;
	}

	private void OnDestroy()
	{
		mRunning = false;
	}

	private void Update()
	{
		if (mActive && !mStart)
		{
			mActive = false;
			if (onReqsFinished != null)
			{
				onReqsFinished();
			}
		}
	}

	private void Run()
	{
		try
		{
			while (mRunning)
			{
				if (!mActive)
				{
					Thread.Sleep(10);
					continue;
				}
				Monitor.TryEnter(VFDataRTGen.s_dicGrassInstList);
				try
				{
					while (true)
					{
						RGChunk rGChunk = null;
						lock (mReqs)
						{
							if (mReqs.Count == 0)
							{
								break;
							}
							rGChunk = mReqs.Dequeue();
							goto IL_0063;
						}
						IL_0063:
						INTVECTOR3 iNTVECTOR = ChunkPosToPos32(rGChunk.xIndex, rGChunk.zIndex);
						int num = mEvni.CHUNKSIZE / mEvni.Tile;
						lock (rGChunk)
						{
							for (int i = iNTVECTOR.x; i < iNTVECTOR.x + num; i++)
							{
								for (int j = iNTVECTOR.z; j < iNTVECTOR.z + num; j++)
								{
									IntVector2 key = new IntVector2(i, j);
									if (!VFDataRTGen.s_dicGrassInstList.ContainsKey(key))
									{
										continue;
									}
									foreach (VoxelGrassInstance item in VFDataRTGen.s_dicGrassInstList[key])
									{
										RedGrassInstance grass = default(RedGrassInstance);
										grass.CopyTo(item);
										Vector3 position = grass.Position;
										if (!GrassDataSL.m_mapDelPos.ContainsKey(new INTVECTOR3((int)position.x, (int)position.y, (int)position.z)))
										{
											rGChunk.Write(grass);
										}
									}
								}
							}
						}
					}
				}
				finally
				{
					Monitor.Exit(VFDataRTGen.s_dicGrassInstList);
				}
				mStart = false;
				Thread.Sleep(10);
			}
		}
		catch (Exception)
		{
		}
	}
}
