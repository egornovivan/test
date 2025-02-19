using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class LSubTerrIO
{
	public static string OriginalSubTerrainDir;

	private Thread _thread;

	private FileStream _orgnSubTerrFile;

	private int[] _orgnOfsData;

	private int[] _orgnLenData;

	private int[] _orgnUcmpLenData;

	private int _curDataIdxInBuff = -1;

	private int _curDataLenInBuff;

	private byte[] _zippedBuff;

	private byte[] _unzippedBuff;

	private List<int> _lstNodesIdx = new List<int>();

	private List<LSubTerrain> _lstNodes = new List<LSubTerrain>();

	public static string s_orgnFilePath => Path.Combine(OriginalSubTerrainDir, "subter.dat");

	public byte[] DataBuff => _unzippedBuff;

	public int DataIdx => _curDataIdxInBuff;

	public int DataLen => _curDataLenInBuff;

	private int CntInProcess
	{
		get
		{
			lock (this)
			{
				return _lstNodesIdx.Count;
			}
		}
	}

	private LSubTerrIO()
	{
	}

	[DllImport("lz4_dll")]
	public static extern int LZ4_compress(byte[] source, byte[] dest, int isize);

	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress(byte[] source, byte[] dest, int osize);

	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress_unknownOutputSize(byte[] source, byte[] dest, int isize, int maxOutputSize);

	public static LSubTerrIO CreateInst()
	{
		LSubTerrIO lSubTerrIO = new LSubTerrIO();
		lSubTerrIO._orgnOfsData = new int[5184];
		lSubTerrIO._orgnLenData = new int[5184];
		lSubTerrIO._orgnUcmpLenData = new int[5184];
		try
		{
			lSubTerrIO._orgnSubTerrFile = new FileStream(s_orgnFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		}
		catch
		{
			Debug.LogWarning("No Layered subTerrain File, No Trees");
			lSubTerrIO._orgnSubTerrFile = null;
		}
		if (lSubTerrIO._orgnSubTerrFile != null)
		{
			BinaryReader binaryReader = new BinaryReader(lSubTerrIO._orgnSubTerrFile);
			int num = 0;
			int num2 = 0;
			lSubTerrIO._orgnOfsData[0] = binaryReader.ReadInt32();
			lSubTerrIO._orgnUcmpLenData[0] = binaryReader.ReadInt32();
			for (int i = 1; i < 5184; i++)
			{
				lSubTerrIO._orgnOfsData[i] = binaryReader.ReadInt32();
				lSubTerrIO._orgnLenData[i - 1] = lSubTerrIO._orgnOfsData[i] - lSubTerrIO._orgnOfsData[i - 1];
				if (lSubTerrIO._orgnOfsData[i - 1] > num)
				{
					num = lSubTerrIO._orgnOfsData[i - 1];
				}
				lSubTerrIO._orgnUcmpLenData[i] = binaryReader.ReadInt32();
				if (lSubTerrIO._orgnUcmpLenData[i] > num2)
				{
					num2 = lSubTerrIO._orgnUcmpLenData[i];
				}
			}
			lSubTerrIO._orgnLenData[5183] = (int)lSubTerrIO._orgnSubTerrFile.Length - lSubTerrIO._orgnOfsData[5183];
			if (lSubTerrIO._orgnLenData[5183] > num)
			{
				num = lSubTerrIO._orgnLenData[5183];
			}
			lSubTerrIO._curDataIdxInBuff = -1;
			lSubTerrIO._curDataLenInBuff = 0;
			lSubTerrIO._zippedBuff = new byte[num];
			lSubTerrIO._unzippedBuff = new byte[num2 + 1];
			lSubTerrIO._thread = new Thread(lSubTerrIO.ProcessReqs);
			lSubTerrIO._thread.Start();
		}
		return lSubTerrIO;
	}

	public static void DestroyInst(LSubTerrIO io)
	{
		if (io != null && io._orgnSubTerrFile != null)
		{
			io._orgnSubTerrFile.Close();
			io._orgnSubTerrFile = null;
		}
	}

	public void ReadOrgDataToBuff(int index)
	{
		if (_orgnSubTerrFile != null)
		{
			try
			{
				_orgnSubTerrFile.Seek(_orgnOfsData[index], SeekOrigin.Begin);
				_orgnSubTerrFile.Read(_zippedBuff, 0, _orgnLenData[index]);
				_curDataLenInBuff = LZ4_uncompress_unknownOutputSize(_zippedBuff, _unzippedBuff, _orgnLenData[index], _unzippedBuff.Length);
				_curDataIdxInBuff = index;
			}
			catch
			{
			}
		}
	}

	public bool TryFill(IntVector3 ipos, Dictionary<int, LSubTerrain> dicNodes)
	{
		int numDataExpands = LSubTerrainMgr.Instance.NumDataExpands;
		for (int i = ipos.x - numDataExpands; i <= ipos.x + numDataExpands; i++)
		{
			for (int j = ipos.z - numDataExpands; j <= ipos.z + numDataExpands; j++)
			{
				if (i >= 0 && i < 72 && j >= 0 && j < 72)
				{
					int num = LSubTerrUtils.PosToIndex(i, j);
					if (!dicNodes.ContainsKey(num))
					{
						LSubTerrain lSubTerrain = new LSubTerrain();
						lSubTerrain.Index = num;
						ReadOrgDataToBuff(lSubTerrain.Index);
						lSubTerrain.ApplyData(DataBuff, DataLen);
						dicNodes.Add(num, lSubTerrain);
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool TryFill_T(IntVector3 ipos, Dictionary<int, LSubTerrain> dicNodes)
	{
		if (CntInProcess > 0)
		{
			return false;
		}
		int count = _lstNodes.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				int index = _lstNodes[i].Index;
				if (dicNodes.ContainsKey(index))
				{
					Debug.LogError("Adding an LSubTerrain node but it already exist in the map, the old node will be replaced !");
					dicNodes[index] = _lstNodes[i];
				}
				else
				{
					dicNodes.Add(index, _lstNodes[i]);
				}
			}
			_lstNodes.Clear();
		}
		bool result = true;
		int numDataExpands = LSubTerrainMgr.Instance.NumDataExpands;
		lock (_lstNodesIdx)
		{
			for (int j = ipos.x - numDataExpands; j <= ipos.x + numDataExpands; j++)
			{
				for (int k = ipos.z - numDataExpands; k <= ipos.z + numDataExpands; k++)
				{
					if (j >= 0 && j < 72 && k >= 0 && k < 72)
					{
						int num = LSubTerrUtils.PosToIndex(j, k);
						if (!dicNodes.ContainsKey(num))
						{
							_lstNodesIdx.Add(num);
							result = false;
						}
					}
				}
			}
			return result;
		}
	}

	private void ProcessReqs()
	{
		while (LSubTerrainMgr.Instance != null)
		{
			int cntInProcess = CntInProcess;
			if (cntInProcess > 0)
			{
				try
				{
					for (int i = 0; i < cntInProcess; i++)
					{
						LSubTerrain lSubTerrain = new LSubTerrain();
						lSubTerrain.Index = _lstNodesIdx[i];
						ReadOrgDataToBuff(lSubTerrain.Index);
						lSubTerrain.ApplyData(DataBuff, DataLen);
						_lstNodes.Add(lSubTerrain);
					}
				}
				catch
				{
				}
				lock (_lstNodesIdx)
				{
					_lstNodesIdx.Clear();
				}
			}
			Thread.Sleep(1);
		}
	}
}
