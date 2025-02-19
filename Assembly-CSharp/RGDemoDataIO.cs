using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Pathea.Maths;
using RedGrass;
using UnityEngine;

public class RGDemoDataIO : Chunk32DataIO
{
	public static readonly string s_orgnFilePath = "D:/grassinst_test.dat";

	private FileStream mOrgnGrassFile;

	private int[] mOrgnOfsData;

	private int[] mOrgnLenData;

	private Queue<RGChunk> mReqs;

	private Thread mThread;

	private bool mRunning = true;

	private bool mActive;

	private bool mStart;

	private OnReqsFinished onReqsFinished;

	public override void Init(EvniAsset evni)
	{
		base.Init(evni);
		mReqs = new Queue<RGChunk>(100);
		mOrgnOfsData = new int[evni.XZTileCount];
		mOrgnLenData = new int[evni.XZTileCount];
		if (File.Exists(s_orgnFilePath))
		{
			mOrgnGrassFile = new FileStream(s_orgnFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			BinaryReader binaryReader = new BinaryReader(mOrgnGrassFile);
			for (int i = 0; i < evni.XZTileCount; i++)
			{
				mOrgnOfsData[i] = binaryReader.ReadInt32();
				mOrgnLenData[i] = binaryReader.ReadInt32();
			}
		}
		if (mThread == null)
		{
			mThread = new Thread(Run);
			mThread.Name = "Red Grass IO Thread";
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

	public override void ProcessReqsImmediatly()
	{
		try
		{
			while (mReqs.Count != 0)
			{
				RGChunk rGChunk = null;
				lock (mReqs)
				{
					rGChunk = mReqs.Dequeue();
				}
				BinaryReader binaryReader = new BinaryReader(mOrgnGrassFile);
				INTVECTOR3 iNTVECTOR = ChunkPosToPos32(rGChunk.xIndex, rGChunk.zIndex);
				int num = mEvni.CHUNKSIZE / mEvni.Tile;
				lock (rGChunk)
				{
					for (int i = iNTVECTOR.x; i < iNTVECTOR.x + num; i++)
					{
						for (int j = iNTVECTOR.z; j < iNTVECTOR.z + num; j++)
						{
							int num2 = Pos32ToIndex32(i, j);
							binaryReader.BaseStream.Seek(mOrgnOfsData[num2], SeekOrigin.Begin);
							int num3 = mOrgnLenData[num2];
							for (int k = 0; k < num3; k++)
							{
								RedGrassInstance grass = default(RedGrassInstance);
								grass.ReadFromStream(binaryReader);
								rGChunk.Write(grass);
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
		}
	}

	public override void StopProcess()
	{
		mActive = false;
		lock (mReqs)
		{
			mReqs.Clear();
		}
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
						goto IL_0058;
					}
					IL_0058:
					if (mOrgnGrassFile == null)
					{
						continue;
					}
					BinaryReader binaryReader = new BinaryReader(mOrgnGrassFile);
					INTVECTOR3 iNTVECTOR = ChunkPosToPos32(rGChunk.xIndex, rGChunk.zIndex);
					int num = mEvni.CHUNKSIZE / mEvni.Tile;
					lock (rGChunk)
					{
						for (int i = iNTVECTOR.x; i < iNTVECTOR.x + num; i++)
						{
							for (int j = iNTVECTOR.z; j < iNTVECTOR.z + num; j++)
							{
								int num2 = Pos32ToIndex32(i, j);
								binaryReader.BaseStream.Seek(mOrgnOfsData[num2], SeekOrigin.Begin);
								int num3 = mOrgnLenData[num2];
								for (int k = 0; k < num3; k++)
								{
									RedGrassInstance grass = default(RedGrassInstance);
									grass.ReadFromStream(binaryReader);
									rGChunk.Write(grass);
								}
							}
						}
					}
				}
				mStart = false;
				Thread.Sleep(10);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("<<<< Data IO Thread error >>>> \r\n" + ex);
		}
	}
}
