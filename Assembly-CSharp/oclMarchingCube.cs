using System;
using System.Collections;
using System.Runtime.InteropServices;
using OpenCLNetWin;
using UnityEngine;

public static class oclMarchingCube
{
	private const int VOLUME_VOXEL_X_LEN_SHIFT = 7;

	private const int VOLUME_VOXEL_Y_LEN_SHIFT = 7;

	private const int VOLUME_VOXEL_Z_LEN_SHIFT = 6;

	private const int VOLUME_VOXEL_X_LEN = 128;

	private const int VOLUME_VOXEL_Y_LEN = 128;

	private const int VOLUME_VOXEL_Z_LEN = 64;

	private const int MAX_VOXELS = 1048576;

	private const int MAX_VERTS = 3986432;

	private const int VERT_DATA_SIZE = 28;

	private const int MAX_MEMORY_FOR_VERTS = 111620096;

	private const int VOXEL_SIZE = 2;

	private const ChannelOrder CH_ORDER = ChannelOrder.RG;

	private const int CHUNK_VOXEL_DIM_LEN_SHIFT = 5;

	private const int CHUNK_VOXEL_DIM_LEN = 32;

	private const uint CHUNK_VOXEL_DIM_LEN_MASK = 31u;

	public const int MAX_CHUNKS = 32;

	private const int CHUNK_VOXEL_NUM = 32768;

	private const int CHUNK_VOXEL_DIM_LEN_REAL = 35;

	private const int CHUNK_VOXEL_NUM_REAL = 42875;

	private const int VOLUME_VOXEL_X_LEN_REAL = 140;

	private const int VOLUME_VOXEL_Y_LEN_REAL = 140;

	private const int VOLUME_VOXEL_Z_LEN_REAL = 70;

	private const int NTHREADS_SHIFT = 5;

	private const int NTHREADS = 32;

	private const int NTHREADS_MASK = 31;

	private static readonly UInt4 CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS = new UInt4(5u, 10u, 15u, 0u);

	private static readonly UInt4 VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS = new UInt4(2u, 4u, 5u, 0u);

	private static readonly UInt4 VOLUME_CHUNK_DIM_LEN = new UInt4(4u, 4u, 2u, 0u);

	private static readonly UInt4 VOLUME_CHUNK_DIM_LEN_MASK = new UInt4(VOLUME_CHUNK_DIM_LEN.S0 - 1, VOLUME_CHUNK_DIM_LEN.S1 - 1, VOLUME_CHUNK_DIM_LEN.S2 - 1, 0u);

	private static OpenCLManager OCLManager;

	private static CommandQueue OCLComQueue;

	private static bool bImageFormatSupported;

	private static Kernel classifyVoxelKernel;

	private static Kernel compactVoxelsKernel;

	private static Kernel generateTriangles2Kernel;

	private static Mem d_pos;

	private static Mem d_norm01;

	private static Mem d_norm2t;

	private static Mem d_volume;

	private static Mem d_voxelVerts;

	private static Mem d_voxelVertsScan;

	private static Mem d_voxelOccupied;

	private static Mem d_voxelOccupiedScan;

	private static Mem d_compVoxelArray;

	public static GCHandle hPosArray;

	public static GCHandle hNorm01Array;

	public static GCHandle hNorm2tArray;

	private static byte[] volumeZeroConst;

	private static int[] localWorkSize = new int[3] { 32, 1, 1 };

	private static int[] globalWorkSize = new int[3] { 32, 32, 32 };

	public static int numChunks = 0;

	private static int activeVoxels = 0;

	private static int totalVerts = 0;

	private static float isoValue = 0.5f;

	private static int oclMCStatus = 0;

	public static bool IsInitialed => oclMCStatus != 0;

	public static bool IsAvailable => oclMCStatus == 1;

	private static void launch_classifyVoxel(int[] gridDim, int[] threadsDim, Mem voxelVerts, Mem voxelOccupied, Mem volume, uint chunkSizeReal, UInt4 chunkSizeShiftPlus, UInt4 volumeChunkShiftPlus, UInt4 volumeChunkMask, float isoValue)
	{
		classifyVoxelKernel.SetArg(0, voxelVerts);
		classifyVoxelKernel.SetArg(1, voxelOccupied);
		classifyVoxelKernel.SetArg(2, volume);
		classifyVoxelKernel.SetArg(3, chunkSizeReal);
		classifyVoxelKernel.SetArg(4, chunkSizeShiftPlus);
		classifyVoxelKernel.SetArg(5, volumeChunkShiftPlus);
		classifyVoxelKernel.SetArg(6, volumeChunkMask);
		classifyVoxelKernel.SetArg(7, isoValue);
		OCLComQueue.EnqueueNDRangeKernel(classifyVoxelKernel, gridDim.Length, null, gridDim, threadsDim);
	}

	private static void launch_compactVoxels(int[] gridDim, int[] threadsDim, Mem compVoxelArray, Mem voxelOccupied, Mem voxelOccupiedScan, UInt4 chunkSizeShiftPlus)
	{
		compactVoxelsKernel.SetArg(0, compVoxelArray);
		compactVoxelsKernel.SetArg(1, voxelOccupied);
		compactVoxelsKernel.SetArg(2, voxelOccupiedScan);
		compactVoxelsKernel.SetArg(3, chunkSizeShiftPlus);
		OCLComQueue.EnqueueNDRangeKernel(compactVoxelsKernel, gridDim.Length, null, gridDim, threadsDim);
	}

	private static void launch_generateTriangles2(int[] gridDim, int[] threadsDim, Mem pos, Mem norm01, Mem norm2t, Mem compactedVoxelArray, Mem numVertsScanned, Mem volume, uint chunkSizeReal, uint chunkSizeMask, UInt4 chunkSizeShiftPlus, UInt4 volumeChunkShiftPlus, UInt4 volumeChunkMask, float isoValue, uint nActiveVoxels, uint nMaxVerts)
	{
		int num = 0;
		generateTriangles2Kernel.SetArg(num++, pos);
		generateTriangles2Kernel.SetArg(num++, norm01);
		generateTriangles2Kernel.SetArg(num++, norm2t);
		generateTriangles2Kernel.SetArg(num++, compactedVoxelArray);
		generateTriangles2Kernel.SetArg(num++, numVertsScanned);
		generateTriangles2Kernel.SetArg(num++, volume);
		generateTriangles2Kernel.SetArg(num++, chunkSizeReal);
		generateTriangles2Kernel.SetArg(num++, chunkSizeMask);
		generateTriangles2Kernel.SetArg(num++, chunkSizeShiftPlus);
		generateTriangles2Kernel.SetArg(num++, volumeChunkShiftPlus);
		generateTriangles2Kernel.SetArg(num++, volumeChunkMask);
		generateTriangles2Kernel.SetArg(num++, isoValue);
		generateTriangles2Kernel.SetArg(num++, nActiveVoxels);
		generateTriangles2Kernel.SetArg(num++, nMaxVerts);
		OCLComQueue.EnqueueNDRangeKernel(generateTriangles2Kernel, gridDim.Length, null, gridDim, threadsDim);
	}

	public static bool InitMC()
	{
		try
		{
			OCLManager = oclManager.ActiveOclMan;
			OCLComQueue = oclManager.ActiveOclCQ;
			try
			{
				oclScanLaucher.initScan(OCLManager, OCLComQueue, 20, 32768);
			}
			catch (Exception ex)
			{
				string text = "[OCLLOG]Kernel Error: ";
				if (ex is OpenCLBuildException ex2)
				{
					for (int i = 0; i < ex2.BuildLogs.Count; i++)
					{
						text += ex2.BuildLogs[i];
					}
				}
				else
				{
					text += ex.Message;
				}
				Debug.LogError(text);
				throw;
			}
			ImageFormat imageFormat = new ImageFormat(ChannelOrder.RG, ChannelType.UNORM_INT8);
			bImageFormatSupported = !OCLComQueue.Device.Name.Contains("RV7") && OCLComQueue.Context.SupportsImageFormat(MemFlags.READ_ONLY, MemObjectType.IMAGE3D, imageFormat.ChannelOrder, imageFormat.ChannelType);
			OCLManager.BuildOptions = "-cl-mad-enable";
			OCLManager.Defines = string.Empty;
			Program program = null;
			while (true)
			{
				try
				{
					string text2 = ((!bImageFormatSupported) ? ("OclKernel/marchingCubes_kernel_u" + 2 + "b") : "OclKernel/marchingCubes_kernel_img");
					TextAsset textAsset = Resources.Load(text2) as TextAsset;
					Debug.Log("[OCLLOG]Build kernel:" + text2);
					program = OCLManager.CompileSource(textAsset.text);
					textAsset = null;
				}
				catch (Exception ex3)
				{
					string text3 = "[OCLLOG]Kernel Error: ";
					if (ex3 is OpenCLBuildException ex4)
					{
						for (int j = 0; j < ex4.BuildLogs.Count; j++)
						{
							text3 += ex4.BuildLogs[j];
						}
					}
					else
					{
						text3 += ex3.Message;
					}
					Debug.LogError(text3);
					if (bImageFormatSupported)
					{
						bImageFormatSupported = false;
						Debug.Log("[OCLLOG]Try to build kernel without img support:");
						continue;
					}
					throw;
				}
				break;
			}
			classifyVoxelKernel = program.CreateKernel("classifyVoxel");
			compactVoxelsKernel = program.CreateKernel("compactVoxels");
			generateTriangles2Kernel = program.CreateKernel("generateTriangles2_vec3");
			program = null;
			Debug.Log("[OCLLOG]All kernels are ready.");
			if (bImageFormatSupported)
			{
				d_volume = OCLManager.Context.CreateImage3D((MemFlags)20uL, imageFormat, 140, 140, 70, 0, 0, IntPtr.Zero);
			}
			else
			{
				isoValue = 128f;
				d_volume = OCLManager.Context.CreateBuffer((MemFlags)20uL, 2744000L);
			}
			Vector3[] value = new Vector3[3986432];
			Vector2[] value2 = new Vector2[3986432];
			Vector2[] value3 = new Vector2[3986432];
			hPosArray = GCHandle.Alloc(value, GCHandleType.Pinned);
			hNorm01Array = GCHandle.Alloc(value2, GCHandleType.Pinned);
			hNorm2tArray = GCHandle.Alloc(value3, GCHandleType.Pinned);
			d_pos = OCLManager.Context.CreateBuffer((MemFlags)10uL, 47837184L, hPosArray.AddrOfPinnedObject());
			d_norm01 = OCLManager.Context.CreateBuffer((MemFlags)10uL, 31891456L, hNorm01Array.AddrOfPinnedObject());
			d_norm2t = OCLManager.Context.CreateBuffer((MemFlags)10uL, 31891456L, hNorm2tArray.AddrOfPinnedObject());
			uint num = 4194304u;
			d_voxelVerts = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, num);
			d_voxelVertsScan = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, num);
			d_voxelOccupied = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, num);
			d_voxelOccupiedScan = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, num);
			d_compVoxelArray = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, num);
			oclMCStatus = 1;
		}
		catch
		{
			oclMCStatus = -1;
			Debug.LogError("[OCLLOG]OclMarchingCubes is not available.");
			return false;
		}
		volumeZeroConst = new byte[171500];
		numChunks = 0;
		return true;
	}

	public static void AddChunkVolumeData<T>(T[] volumeData)
	{
		int[] array = new int[3] { 35, 35, 35 };
		int[] array2 = new int[3];
		long num = 35L;
		long num2 = numChunks;
		UInt4 vOLUME_CHUNK_DIM_LEN_MASK = VOLUME_CHUNK_DIM_LEN_MASK;
		array2[0] = (int)(num * (num2 & vOLUME_CHUNK_DIM_LEN_MASK.S0));
		long num3 = 35L;
		int num4 = numChunks;
		UInt4 vOLUME_CHUNK_DIM_LEN_SHIFT_PLUS = VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS;
		long num5 = num4 >> (int)vOLUME_CHUNK_DIM_LEN_SHIFT_PLUS.S0;
		UInt4 vOLUME_CHUNK_DIM_LEN_MASK2 = VOLUME_CHUNK_DIM_LEN_MASK;
		array2[1] = (int)(num3 * (num5 & vOLUME_CHUNK_DIM_LEN_MASK2.S1));
		long num6 = 35L;
		int num7 = numChunks;
		UInt4 vOLUME_CHUNK_DIM_LEN_SHIFT_PLUS2 = VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS;
		long num8 = num7 >> (int)vOLUME_CHUNK_DIM_LEN_SHIFT_PLUS2.S1;
		UInt4 vOLUME_CHUNK_DIM_LEN_MASK3 = VOLUME_CHUNK_DIM_LEN_MASK;
		array2[2] = (int)(num6 * (num8 & vOLUME_CHUNK_DIM_LEN_MASK3.S2));
		int[] array3 = array2;
		int num9 = 35;
		int num10 = 1225;
		GCHandle gCHandle = GCHandle.Alloc(volumeData, GCHandleType.Pinned);
		IntPtr ptr = gCHandle.AddrOfPinnedObject();
		if (bImageFormatSupported)
		{
			OCLComQueue.EnqueueWriteImage(d_volume, blockingWrite: false, array3, array, 0, 0, ptr);
		}
		else
		{
			array3[0] *= 2;
			array[0] *= 2;
			OCLComQueue.EnqueueWriteBufferRect(d_volume, blocking_write: false, array3, new int[3], array, 280, 39200, num9 * 2, num10 * 2, ptr);
		}
		gCHandle.Free();
		numChunks++;
	}

	public static void Cleanup()
	{
		if (OCLManager != null)
		{
			oclScanLaucher.closeScan();
			hPosArray.Free();
			hNorm01Array.Free();
			hNorm2tArray.Free();
			if (d_pos != null)
			{
				d_pos.Dispose();
				d_pos = null;
			}
			if (d_norm01 != null)
			{
				d_norm01.Dispose();
				d_norm01 = null;
			}
			if (d_norm2t != null)
			{
				d_norm2t.Dispose();
				d_norm2t = null;
			}
			if (d_volume != null)
			{
				d_volume.Dispose();
				d_volume = null;
			}
			if (d_voxelVerts != null)
			{
				d_voxelVerts.Dispose();
				d_voxelVerts = null;
			}
			if (d_voxelVertsScan != null)
			{
				d_voxelVertsScan.Dispose();
				d_voxelVertsScan = null;
			}
			if (d_voxelOccupied != null)
			{
				d_voxelOccupied.Dispose();
				d_voxelOccupied = null;
			}
			if (d_voxelOccupiedScan != null)
			{
				d_voxelOccupiedScan.Dispose();
				d_voxelOccupiedScan = null;
			}
			if (d_compVoxelArray != null)
			{
				d_compVoxelArray.Dispose();
				d_compVoxelArray = null;
			}
			if (classifyVoxelKernel != null)
			{
				classifyVoxelKernel.Dispose();
				classifyVoxelKernel = null;
			}
			if (compactVoxelsKernel != null)
			{
				compactVoxelsKernel.Dispose();
				compactVoxelsKernel = null;
			}
			if (generateTriangles2Kernel != null)
			{
				generateTriangles2Kernel.Dispose();
				generateTriangles2Kernel = null;
			}
		}
	}

	public static void computeIsosurface()
	{
		IEnumerator enumerator = computeIsosurfaceAsyn();
		while (enumerator.MoveNext())
		{
		}
	}

	public static IEnumerator computeIsosurfaceAsyn()
	{
		if (numChunks == 0)
		{
			yield break;
		}
		int numVoxels = numChunks * 32768;
		int groupSize = oclScanLaucher.prepareScan(numVoxels);
		int numVoxelsToScan;
		for (numVoxelsToScan = 4 * groupSize; numVoxelsToScan < numVoxels; numVoxelsToScan <<= 1)
		{
		}
		while ((numVoxels & (4 * groupSize - 1)) != 0)
		{
			AddChunkVolumeData(volumeZeroConst);
			numVoxels = numChunks * 32768;
		}
		globalWorkSize[0] = numChunks * 32;
		launch_classifyVoxel(globalWorkSize, localWorkSize, d_voxelVerts, d_voxelOccupied, d_volume, 35u, CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_MASK, isoValue);
		oclScanLaucher.scanExclusiveLarge_1Batch(OCLComQueue, d_voxelOccupiedScan, d_voxelOccupied, numVoxels, numVoxelsToScan, 1);
		launch_compactVoxels(globalWorkSize, localWorkSize, d_compVoxelArray, d_voxelOccupied, d_voxelOccupiedScan, CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS);
		oclScanLaucher.scanExclusiveLarge_1Batch(OCLComQueue, d_voxelVertsScan, d_voxelVerts, numVoxels, numVoxelsToScan, 0);
		OpenCLNetWin.Event evRead = oclScanLaucher.UpdateOfsnSumDataAsyn(OCLComQueue, 32768);
		OCLComQueue.Flush();
		while (evRead.ExecutionStatus != 0)
		{
			yield return 0;
		}
		activeVoxels = oclScanLaucher.sumData[1];
		totalVerts = oclScanLaucher.sumData[0];
		if (totalVerts > 0)
		{
			if (totalVerts > 3986432)
			{
				string errStr = "[OCL]Error: total verts is out of bounds:" + totalVerts;
				for (int i = 0; i < numChunks; i++)
				{
					errStr = errStr + Environment.NewLine + oclScanLaucher.ofsData[i];
				}
				string text = errStr;
				errStr = text + Environment.NewLine + activeVoxels + "//" + totalVerts;
				Debug.LogWarning(errStr);
				oclScanLaucher.ClearOfsData();
				numChunks = 0;
				throw new ArgumentOutOfRangeException("totalVerts", totalVerts, errStr);
			}
			int[] grid2 = new int[2] { activeVoxels, 1 };
			while (grid2[0] > 65535)
			{
				grid2[0] = grid2[0] + 1 >> 1;
				grid2[1] <<= 1;
			}
			grid2[0] = (grid2[0] + 31) & -32;
			launch_generateTriangles2(grid2, new int[2] { 32, 1 }, d_pos, d_norm01, d_norm2t, d_compVoxelArray, d_voxelVertsScan, d_volume, 35u, 31u, CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_MASK, isoValue, (uint)activeVoxels, (uint)totalVerts);
			OCLComQueue.EnqueueReadBuffer(d_pos, blockingRead: false, 0, totalVerts * 3 * 4, hPosArray.AddrOfPinnedObject());
			OCLComQueue.EnqueueReadBuffer(d_norm01, blockingRead: false, 0, totalVerts * 2 * 4, hNorm01Array.AddrOfPinnedObject());
			OCLComQueue.EnqueueReadBuffer(d_norm2t, blockingRead: false, 0, totalVerts * 2 * 4, hNorm2tArray.AddrOfPinnedObject(), 0, null, out evRead);
			OCLComQueue.Flush();
			while (evRead.ExecutionStatus != 0)
			{
				yield return 0;
			}
		}
		numChunks = 0;
	}

	public static void Reset()
	{
		numChunks = 0;
	}
}
