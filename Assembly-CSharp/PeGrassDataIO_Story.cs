using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Pathea.Maths;
using RedGrass;
using UnityEngine;

public class PeGrassDataIO_Story : Chunk32DataIO
{
	private static string sOriginalSubTerrainDir;

	private object mDestoryLockObj = new object();

	private FileStream[] mOrgnGrassFile;

	private int[,] mOrgnOfsData;

	public static string[] mOrgnFilePath;

	private Queue<RGChunk> mReqs;

	private Thread mThread;

	private bool mRunning = true;

	private bool mActive;

	private bool mStart;

	private OnReqsFinished onReqsFinished;

	public static string originalSubTerrainDir
	{
		get
		{
			return sOriginalSubTerrainDir;
		}
		set
		{
			sOriginalSubTerrainDir = value + "/";
		}
	}

	public override void Init(EvniAsset evni)
	{
		base.Init(evni);
		mEvni.SetDensity(mEvni.Density);
		if (SystemSettingData.Instance.GrassLod > ELodType.LOD_3_TYPE_1)
		{
			SystemSettingData.Instance.GrassLod = ELodType.LOD_3_TYPE_1;
		}
		mEvni.SetLODType(SystemSettingData.Instance.GrassLod);
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
		mOrgnFilePath = new string[9]
		{
			originalSubTerrainDir + "subTerG_x0_y0.dat",
			originalSubTerrainDir + "subTerG_x1_y0.dat",
			originalSubTerrainDir + "subTerG_x2_y0.dat",
			originalSubTerrainDir + "subTerG_x0_y1.dat",
			originalSubTerrainDir + "subTerG_x1_y1.dat",
			originalSubTerrainDir + "subTerG_x2_y1.dat",
			originalSubTerrainDir + "subTerG_x0_y2.dat",
			originalSubTerrainDir + "subTerG_x1_y2.dat",
			originalSubTerrainDir + "subTerG_x2_y2.dat"
		};
		mOrgnGrassFile = new FileStream[evni.FileXZcount];
		for (int j = 0; j < evni.FileXZcount; j++)
		{
			if (File.Exists(mOrgnFilePath[j]))
			{
				mOrgnGrassFile[j] = new FileStream(mOrgnFilePath[j], FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			else
			{
				Debug.LogWarning("The path file: " + mOrgnFilePath[j] + "is missing!");
			}
		}
		mOrgnOfsData = new int[mEvni.FileXZcount, mEvni.XZTileCount];
		for (int k = 0; k < mEvni.FileXZcount; k++)
		{
			if (mOrgnGrassFile[k] != null)
			{
				BinaryReader binaryReader = new BinaryReader(mOrgnGrassFile[k]);
				for (int l = 0; l < mEvni.XZTileCount; l++)
				{
					mOrgnOfsData[k, l] = binaryReader.ReadInt32();
				}
			}
		}
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
		try
		{
			while (mReqs.Count != 0)
			{
				RGChunk rGChunk = null;
				lock (mReqs)
				{
					rGChunk = mReqs.Dequeue();
				}
				INTVECTOR3 iNTVECTOR = ChunkPosToPos32(rGChunk.xIndex, rGChunk.zIndex);
				int num = FindOrginFileIndex(iNTVECTOR);
				if (num == -1 || mOrgnGrassFile[num] == null)
				{
					continue;
				}
				BinaryReader binaryReader = new BinaryReader(mOrgnGrassFile[num]);
				int num2 = mEvni.CHUNKSIZE / mEvni.Tile;
				lock (rGChunk)
				{
					for (int i = iNTVECTOR.x; i < iNTVECTOR.x + num2; i++)
					{
						for (int j = iNTVECTOR.z; j < iNTVECTOR.z + num2; j++)
						{
							int x_ = i % mEvni.XTileCount;
							int z_ = j % mEvni.ZTileCount;
							int num3 = Pos32ToIndex32(x_, z_);
							if (mOrgnOfsData[num, num3] <= 0)
							{
								continue;
							}
							binaryReader.BaseStream.Seek(mOrgnOfsData[num, num3], SeekOrigin.Begin);
							int num4 = binaryReader.ReadInt32();
							for (int k = 0; k < num4; k++)
							{
								RedGrassInstance grass = default(RedGrassInstance);
								grass.ReadFromStream(binaryReader);
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
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
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

	public int FindOrginFileIndex(Vector3 chunk32Pos)
	{
		int result = -1;
		for (int i = 0; i < mEvni.FileXCount; i++)
		{
			int num = (mEvni.ZStart >> mEvni.Tile) + mEvni.ZTileCount * i;
			int num2 = num + mEvni.ZTileCount;
			for (int j = 0; j < mEvni.FlieZCount; j++)
			{
				int num3 = (mEvni.XStart >> mEvni.Tile) + mEvni.XTileCount * j;
				int num4 = num3 + mEvni.XTileCount;
				if ((float)num3 <= chunk32Pos.x && (float)num4 > chunk32Pos.x && (float)num <= chunk32Pos.z && (float)num2 > chunk32Pos.z)
				{
					result = i * mEvni.FlieZCount + j;
					break;
				}
			}
		}
		return result;
	}

	private void OnDestroy()
	{
		mRunning = false;
		lock (mDestoryLockObj)
		{
			FileStream[] array = mOrgnGrassFile;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.Close();
			}
		}
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
					lock (mDestoryLockObj)
					{
						INTVECTOR3 iNTVECTOR = ChunkPosToPos32(rGChunk.xIndex, rGChunk.zIndex);
						int num = FindOrginFileIndex(iNTVECTOR);
						if (num == -1 || mOrgnGrassFile[num] == null)
						{
							continue;
						}
						BinaryReader binaryReader = new BinaryReader(mOrgnGrassFile[num]);
						int num2 = mEvni.CHUNKSIZE / mEvni.Tile;
						lock (rGChunk)
						{
							for (int i = iNTVECTOR.x; i < iNTVECTOR.x + num2; i++)
							{
								for (int j = iNTVECTOR.z; j < iNTVECTOR.z + num2; j++)
								{
									int x_ = i % mEvni.XTileCount;
									int z_ = j % mEvni.ZTileCount;
									int num3 = Pos32ToIndex32(x_, z_);
									if (mOrgnOfsData[num, num3] <= 0)
									{
										continue;
									}
									binaryReader.BaseStream.Seek(mOrgnOfsData[num, num3], SeekOrigin.Begin);
									int num4 = binaryReader.ReadInt32();
									for (int k = 0; k < num4; k++)
									{
										RedGrassInstance grass = default(RedGrassInstance);
										grass.ReadFromStream(binaryReader);
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
				mStart = false;
				Thread.Sleep(10);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("<<<< Data IO Thread error >>>> \r\n" + ex);
		}
	}
}
