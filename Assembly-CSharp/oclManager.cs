using System;
using System.Collections.Generic;
using OpenCLNetWin;
using UnityEngine;

public static class oclManager
{
	private const string OptCpuMask = "Cpu";

	private const string OptOclMask = "OpenCL";

	private static readonly string OclSourcePath = GameConfig.OclSourcePath;

	private static readonly string OclBinaryPath = GameConfig.OclBinaryPath;

	public static OpenCLManager ActiveOclMan;

	public static CommandQueue ActiveOclCQ;

	public static string CurOclOpt = "Cpu";

	private static List<string> oclOptList = null;

	private static List<OpenCLManager> instances = null;

	public static List<string> OclOptionList
	{
		get
		{
			if (oclOptList == null)
			{
				CreateOclOptList();
			}
			return oclOptList;
		}
	}

	private static void CreateOclManagers()
	{
		instances = new List<OpenCLManager>();
		int numberOfPlatforms = OpenCL.NumberOfPlatforms;
		Debug.Log("[OCLLOG]:Platform Count:" + numberOfPlatforms);
		if (numberOfPlatforms <= 0)
		{
			Debug.LogError("[OCLLOG]:OpenCLIs[NOT]Available");
			return;
		}
		for (int i = 0; i < numberOfPlatforms; i++)
		{
			Platform platform = OpenCL.GetPlatform(i);
			Device[] array = platform.QueryDevices(OpenCLNetWin.DeviceType.ALL);
			int num = array.Length;
			if (num <= 0)
			{
				Debug.LogError("[OCLLOG]:No OpenCL devices found that matched filter criteria on Platform_" + platform.Name);
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				try
				{
					Debug.Log("[OCLLOG]:Creating OCL on Platform_" + platform.Name + "+Device_" + array[j].Name);
					OpenCLManager openCLManager = new OpenCLManager();
					openCLManager.SourcePath = OclSourcePath;
					openCLManager.BinaryPath = OclBinaryPath;
					IntPtr[] contextProperties = new IntPtr[3]
					{
						(IntPtr)4228L,
						platform,
						IntPtr.Zero
					};
					Device[] devices = new Device[1] { array[j] };
					openCLManager.CreateContext(platform, contextProperties, devices);
					instances.Add(openCLManager);
				}
				catch (Exception ex)
				{
					Debug.Log("[OCLLOG]Exception at Platform_" + platform.Name + "+Device_" + array[j].Name + ":" + ex.Message);
				}
			}
		}
	}

	private static void CreateOclOptList()
	{
		oclOptList = new List<string>();
		try
		{
			if (instances == null)
			{
				CreateOclManagers();
			}
			int num = 0;
			foreach (OpenCLManager instance in instances)
			{
				string text = ((instance.Platform.Name.Length <= 16) ? instance.Platform.Name.PadRight(16, ' ') : instance.Platform.Name.Substring(0, 16));
				CommandQueue[] cQ = instance.CQ;
				foreach (CommandQueue commandQueue in cQ)
				{
					string item = ("OpenCL|" + text + "|" + commandQueue.Device.Name).Replace(' ', '_');
					if (commandQueue.Device.DeviceType == OpenCLNetWin.DeviceType.GPU)
					{
						oclOptList.Insert(num++, item);
					}
					else
					{
						oclOptList.Add(item);
					}
				}
			}
		}
		catch
		{
			Debug.Log("[OCLLOG]Exception at CreateOclOptList");
		}
		oclOptList.Add("Cpu");
	}

	public static bool InitOclFromOpt()
	{
		ActiveOclMan = null;
		ActiveOclCQ = null;
		if (string.Compare("Cpu", 0, CurOclOpt, 0, "Cpu".Length) == 0)
		{
			Debug.Log("[OCLLOG]Succeed to init with " + CurOclOpt);
			return true;
		}
		if (instances == null)
		{
			CreateOclManagers();
		}
		try
		{
			foreach (OpenCLManager instance in instances)
			{
				string text = ((instance.Platform.Name.Length <= 16) ? instance.Platform.Name.PadRight(16, ' ') : instance.Platform.Name.Substring(0, 16));
				CommandQueue[] cQ = instance.CQ;
				foreach (CommandQueue commandQueue in cQ)
				{
					string strA = ("OpenCL|" + text + "|" + commandQueue.Device.Name).Replace(' ', '_');
					if (string.Compare(strA, CurOclOpt) == 0)
					{
						ActiveOclMan = instance;
						ActiveOclCQ = commandQueue;
						if (oclMarchingCube.InitMC())
						{
							Debug.Log("[OCLLOG]Succeed to init with " + CurOclOpt);
							return true;
						}
						throw new Exception("Failed to init with " + CurOclOpt);
					}
				}
			}
		}
		catch
		{
		}
		Debug.Log("[OCLLOG]Fallback to CPU because of failure to init with " + CurOclOpt);
		ActiveOclMan = null;
		ActiveOclCQ = null;
		CurOclOpt = "Cpu";
		return false;
	}

	public static void Dispose()
	{
		try
		{
			if (instances == null)
			{
				return;
			}
			foreach (OpenCLManager instance in instances)
			{
				instance.Dispose();
			}
			instances.Clear();
			instances = null;
		}
		catch (Exception ex)
		{
			Debug.Log("[OCLLOG]Fallback to dispose OpenCL:" + ex.Message);
		}
	}
}
