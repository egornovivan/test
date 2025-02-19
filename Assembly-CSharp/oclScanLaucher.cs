using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenCLNetWin;
using UnityEngine;

public static class oclScanLaucher
{
	public class OffsetData
	{
		public int ofs = 1;

		public int this[int pos] => ofsDataArray[ofs * pos];
	}

	public class SumData
	{
		public int this[int pos] => ofsDataArray[OFS_DATA_SIZE + pos];
	}

	private static Kernel ckScanLargeKernel;

	private static Kernel ckBlockAddiKernel;

	private static Kernel ckPrefixSumKernel;

	private static Kernel zeromemKernel;

	private static List<Mem> blockSumBufferList = new List<Mem>();

	private static List<Mem> outBufferList = new List<Mem>();

	private static int MaxGroupSize;

	private static int blockSize;

	private static int blockSizeShift;

	private static int blockSizeUnMask;

	private static List<int> listComputeArrayLen = new List<int>(8);

	private static int pass;

	private static int SUM_DATA_SIZE = 8;

	private static int OFS_DATA_SIZE;

	private static int[] ofsDataArray;

	private static GCHandle hofsDataArray;

	public static OffsetData ofsData = new OffsetData();

	public static SumData sumData = new SumData();

	public static Mem SumDataBuffer => outBufferList[1];

	public static void ClearOfsData()
	{
		Array.Clear(ofsDataArray, 0, ofsDataArray.Length);
	}

	public static int OffsetOfSumDataBuffer(int offset)
	{
		return OFS_DATA_SIZE + offset;
	}

	public static void initScan(OpenCLManager OCLManager, CommandQueue cqCommandQueue, int numDataShift, int chunkSize)
	{
		OCLManager.BuildOptions = string.Empty;
		OCLManager.Defines = string.Empty;
		Program program;
		try
		{
			TextAsset textAsset = Resources.Load("OclKernel/ScanLargeArrays_Kernels") as TextAsset;
			Debug.Log("[OCLLOG]Build kernel:Scan");
			program = OCLManager.CompileSource(textAsset.text);
			textAsset = null;
		}
		catch (OpenCLBuildException ex)
		{
			string text = "[OCLLOG]Kernel Error: ";
			for (int i = 0; i < ex.BuildLogs.Count; i++)
			{
				text += ex.BuildLogs[i];
			}
			Debug.LogError(text);
			throw;
		}
		ckScanLargeKernel = program.CreateKernel("ScanLargeArrays");
		ckBlockAddiKernel = program.CreateKernel("blockAddition");
		ckPrefixSumKernel = program.CreateKernel("prefixSum");
		zeromemKernel = program.CreateKernel("zeromem");
		program = null;
		MaxGroupSize = (int)cqCommandQueue.Device.MaxWorkGroupSize;
		int num = cqCommandQueue.Device.MaxWorkItemSizes[0].ToInt32();
		int num2 = cqCommandQueue.Device.MaxWorkItemSizes[1].ToInt32();
		int num3 = cqCommandQueue.Device.MaxWorkItemSizes[2].ToInt32();
		Debug.Log("[OCLLOG]SCAN MaxGroup:" + MaxGroupSize + " MaxWorkItem:" + num + "," + num2 + "," + num3);
		if (cqCommandQueue.Device.Name.Contains("RV7"))
		{
			num = (MaxGroupSize = 32);
			Debug.Log("[OCLLOG]SCAN RV7xx lower MaxGroup:" + MaxGroupSize + "MaxWorkItem:" + num);
		}
		if (num > chunkSize)
		{
			num = chunkSize;
		}
		blockSize = 1;
		blockSizeShift = 0;
		while (blockSize < num)
		{
			blockSize <<= 1;
			blockSizeShift++;
		}
		blockSizeUnMask = ~(blockSize - 1);
		bool flag = false;
		blockSumBufferList.Add(null);
		outBufferList.Add(null);
		if (numDataShift < blockSizeShift)
		{
			numDataShift = blockSizeShift;
		}
		do
		{
			numDataShift -= blockSizeShift;
			Mem item = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, (1 << numDataShift) * 4);
			blockSumBufferList.Add(item);
			Mem item2;
			if (!flag)
			{
				OFS_DATA_SIZE = 1 << numDataShift;
				ofsDataArray = new int[OFS_DATA_SIZE + SUM_DATA_SIZE];
				hofsDataArray = GCHandle.Alloc(ofsDataArray, GCHandleType.Pinned);
				item2 = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, (OFS_DATA_SIZE + SUM_DATA_SIZE) * 4);
				flag = true;
			}
			else
			{
				item2 = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, (1 << numDataShift) * 4);
			}
			outBufferList.Add(item2);
		}
		while (numDataShift > blockSizeShift);
	}

	public static int prepareScan(int numData)
	{
		listComputeArrayLen.Clear();
		do
		{
			numData = (numData + blockSize - 1) & blockSizeUnMask;
			listComputeArrayLen.Add(numData);
			numData >>= blockSizeShift;
		}
		while (numData > blockSize);
		pass = listComputeArrayLen.Count;
		int num = 0;
		while (numData > 1)
		{
			num++;
			numData >>= 1;
		}
		listComputeArrayLen.Add(1 << num + 1);
		return blockSize;
	}

	public static void closeScan()
	{
		hofsDataArray.Free();
		if (blockSumBufferList.Count > 0)
		{
			blockSumBufferList[0] = null;
		}
		for (int i = 1; i < blockSumBufferList.Count; i++)
		{
			blockSumBufferList[i].Dispose();
			blockSumBufferList[i] = null;
		}
		blockSumBufferList.Clear();
		if (outBufferList.Count > 0)
		{
			outBufferList[0] = null;
		}
		for (int j = 1; j < outBufferList.Count; j++)
		{
			outBufferList[j].Dispose();
			outBufferList[j] = null;
		}
		outBufferList.Clear();
		if (ckScanLargeKernel != null)
		{
			ckScanLargeKernel.Dispose();
			ckScanLargeKernel = null;
		}
		if (ckBlockAddiKernel != null)
		{
			ckBlockAddiKernel.Dispose();
			ckBlockAddiKernel = null;
		}
		if (ckPrefixSumKernel != null)
		{
			ckPrefixSumKernel.Dispose();
			ckPrefixSumKernel = null;
		}
		if (zeromemKernel != null)
		{
			zeromemKernel.Dispose();
			zeromemKernel = null;
		}
	}

	public static OpenCLNetWin.Event UpdateOfsnSumDataAsyn(CommandQueue cqCommandQueue, int chunkSize)
	{
		ofsData.ofs = chunkSize >> blockSizeShift;
		OpenCLNetWin.Event _event = null;
		cqCommandQueue.EnqueueReadBuffer(SumDataBuffer, blockingRead: false, 0, ofsDataArray.Length * 4, hofsDataArray.AddrOfPinnedObject(), 0, null, out _event);
		return _event;
	}

	private static void zero_mem(CommandQueue cqCommandQueue, Mem uArray, int offset, int size)
	{
		zeromemKernel.SetArg(0, uArray);
		zeromemKernel.SetArg(1, offset);
		cqCommandQueue.EnqueueNDRangeKernel(zeromemKernel, 1, null, new int[1] { size }, new int[1] { 1 });
	}

	private static void ScanLargeArray(CommandQueue cqCommandQueue, int len, Mem inputBuffer, Mem outputBuffer, Mem blockSumBuffer)
	{
		int num = blockSize * MaxGroupSize;
		int[] array = new int[1];
		int[] array2 = new int[1];
		array[0] = blockSize / 2;
		array2[0] = ((len <= num) ? (len / 2) : (num / 2));
		ckScanLargeKernel.SetArg(0, outputBuffer);
		ckScanLargeKernel.SetArg(1, inputBuffer);
		ckScanLargeKernel.SetArg(2, (IntPtr)(blockSize * 4), IntPtr.Zero);
		ckScanLargeKernel.SetArg(3, blockSize);
		ckScanLargeKernel.SetArg(4, 0);
		ckScanLargeKernel.SetArg(5, 0);
		ckScanLargeKernel.SetArg(6, blockSumBuffer);
		cqCommandQueue.EnqueueNDRangeKernel(ckScanLargeKernel, 1, null, array2, array);
		int num2 = 1;
		while (len > num)
		{
			len -= num;
			array2[0] = ((len <= num) ? (len / 2) : (num / 2));
			ckScanLargeKernel.SetArg(4, num2 * num);
			ckScanLargeKernel.SetArg(5, num2 * MaxGroupSize);
			cqCommandQueue.EnqueueNDRangeKernel(ckScanLargeKernel, 1, null, array2, array);
			num2++;
		}
	}

	private static void PrefixSum(CommandQueue cqCommandQueue, int len, Mem inputBuffer, Mem outputBuffer, Mem sumDataBuffer, int sumDataOffset)
	{
		int[] array = new int[1];
		int[] array2 = new int[1];
		array[0] = len / 2;
		array2[0] = len / 2;
		ckPrefixSumKernel.SetArg(0, outputBuffer);
		ckPrefixSumKernel.SetArg(1, inputBuffer);
		ckPrefixSumKernel.SetArg(2, (IntPtr)(len * 4), IntPtr.Zero);
		ckPrefixSumKernel.SetArg(3, len);
		ckPrefixSumKernel.SetArg(4, sumDataBuffer);
		ckPrefixSumKernel.SetArg(5, sumDataOffset);
		cqCommandQueue.EnqueueNDRangeKernel(ckPrefixSumKernel, 1, null, array2, array);
	}

	private static void BlockAddition(CommandQueue cqCommandQueue, int len, Mem inputBuffer, Mem outputBuffer)
	{
		int num = blockSize * MaxGroupSize;
		int[] array = new int[1];
		int[] array2 = new int[1];
		array[0] = blockSize;
		array2[0] = ((len <= num) ? len : num);
		ckBlockAddiKernel.SetArg(0, inputBuffer);
		ckBlockAddiKernel.SetArg(1, outputBuffer);
		ckBlockAddiKernel.SetArg(2, 0);
		ckBlockAddiKernel.SetArg(3, 0);
		cqCommandQueue.EnqueueNDRangeKernel(ckBlockAddiKernel, 1, null, array2, array);
		int num2 = 1;
		while (len > num)
		{
			len -= num;
			array2[0] = ((len <= num) ? len : num);
			ckBlockAddiKernel.SetArg(2, num2 * num);
			ckBlockAddiKernel.SetArg(3, num2 * MaxGroupSize);
			cqCommandQueue.EnqueueNDRangeKernel(ckBlockAddiKernel, 1, null, array2, array);
			num2++;
		}
	}

	public static void scanExclusiveLarge_1Batch(CommandQueue cqCommandQueue, Mem d_Dst, Mem d_Src, int arrayLen, int arrayLenPower2, int sumDataPos)
	{
		blockSumBufferList[0] = d_Src;
		outBufferList[0] = d_Dst;
		for (int i = 0; i < pass; i++)
		{
			if (listComputeArrayLen[i] > arrayLen)
			{
				zero_mem(cqCommandQueue, blockSumBufferList[i], arrayLen, listComputeArrayLen[i] - arrayLen);
			}
			arrayLen = listComputeArrayLen[i] >> blockSizeShift;
			ScanLargeArray(cqCommandQueue, listComputeArrayLen[i], blockSumBufferList[i], outBufferList[i], blockSumBufferList[i + 1]);
		}
		if (listComputeArrayLen[pass] > arrayLen)
		{
			zero_mem(cqCommandQueue, blockSumBufferList[pass], arrayLen, listComputeArrayLen[pass] - arrayLen);
		}
		PrefixSum(cqCommandQueue, listComputeArrayLen[pass], blockSumBufferList[pass], outBufferList[pass], outBufferList[1], sumDataPos + OFS_DATA_SIZE);
		for (int num = pass; num > 0; num--)
		{
			BlockAddition(cqCommandQueue, listComputeArrayLen[num - 1], outBufferList[num], outBufferList[num - 1]);
		}
	}
}
