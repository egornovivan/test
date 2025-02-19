using System;
using System.Collections.Generic;
using UnityEngine;

public class PECommandLine
{
	private class CmdLine
	{
		private Dictionary<string, string> mDicCmd = new Dictionary<string, string>(5);

		public bool Parse(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return false;
			}
			string[] array = line.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].StartsWith("-"))
				{
					continue;
				}
				if (!array[i + 1].StartsWith("-"))
				{
					if (!mDicCmd.ContainsKey(array[i]))
					{
						mDicCmd.Add(array[i], array[i + 1]);
					}
					i++;
				}
				else
				{
					mDicCmd.Add(array[i], null);
				}
			}
			return true;
		}

		public string GetCmd(string cmd)
		{
			if (mDicCmd.ContainsKey(cmd))
			{
				return mDicCmd[cmd];
			}
			return null;
		}

		public bool HasCmd(string cmd)
		{
			return mDicCmd.ContainsKey(cmd);
		}

		public void Print()
		{
			foreach (KeyValuePair<string, string> item in mDicCmd)
			{
				Debug.Log("[" + item.Key + "] = " + item.Value);
			}
		}
	}

	private static int _rw = 1280;

	private static int _rh = 720;

	private static bool _fs;

	private static string _oclPara;

	private static string _invitePara;

	public static int W => _rw;

	public static int H => _rh;

	public static bool FullScreen => _fs;

	public static string OclPara => _oclPara;

	public static string InvitePara => _invitePara;

	public static void ParseArgs()
	{
		string empty = string.Empty;
		empty = DateTime.Now.ToString("G") + " Steam Version:" + GameConfig.GameVersion;
		empty += " Win64";
		Debug.Log(empty);
		if (Application.isEditor)
		{
			return;
		}
		try
		{
			string commandLine = Environment.CommandLine;
			CmdLine cmdLine = new CmdLine();
			if (!cmdLine.Parse(commandLine))
			{
				Application.Quit();
				return;
			}
			if (!cmdLine.HasCmd("-from-launcher"))
			{
				Debug.LogError("The game must open with launcher");
				Application.Quit();
				return;
			}
			if (cmdLine.HasCmd("-rw") && cmdLine.HasCmd("-rh"))
			{
				_rw = Convert.ToInt32(cmdLine.GetCmd("-rw"));
				_rh = Convert.ToInt32(cmdLine.GetCmd("-rh"));
				if (_rw > 16384)
				{
					_rw = 16384;
				}
				else if (_rw < 1280)
				{
					_rw = 1280;
				}
				if (_rh > 4096)
				{
					_rh = 4096;
				}
				else if (_rh < 720)
				{
					_rh = 720;
				}
				_fs = cmdLine.HasCmd("-fs");
				Screen.fullScreen = _fs;
				if (_fs && SystemInfo.graphicsDeviceVersion.Contains("OpenGL"))
				{
					_rw = Screen.currentResolution.width;
					_rh = Screen.currentResolution.height;
				}
				Debug.Log("Request Game resolution from launcher[" + _rw + "X" + _rh + "] fs[" + _fs + "]");
				Screen.SetResolution(_rw, _rh, _fs);
			}
			else
			{
				_rw = Screen.currentResolution.width;
				_rh = Screen.currentResolution.height;
				_fs = true;
				Debug.Log("Request Game resolution Screen current resolution[" + _rw + "X" + _rh + "] fs[" + _fs + "]");
				Screen.SetResolution(_rw, _rh, _fs);
			}
			if (cmdLine.HasCmd("-ql"))
			{
				int index = Convert.ToInt32(cmdLine.GetCmd("-ql"));
				QualitySettings.SetQualityLevel(index, applyExpensiveChanges: true);
			}
			if (cmdLine.HasCmd("-language"))
			{
				string cmd = cmdLine.GetCmd("-language");
				SystemSettingData.Instance.mLanguage = cmd.ToLower();
				Debug.Log("Request Language: " + SystemSettingData.Instance.mLanguage);
			}
			if (cmdLine.HasCmd("-ocl"))
			{
				_oclPara = cmdLine.GetCmd("-ocl");
			}
			if (cmdLine.HasCmd("-inviteto"))
			{
				_invitePara = cmdLine.GetCmd("-inviteto");
			}
			if (cmdLine.HasCmd("-userpath"))
			{
				string cmd2 = cmdLine.GetCmd("-userpath");
				GameConfig.SetUserDataPath(cmd2);
				Debug.Log("Request UserDataPath: " + cmd2);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Process CommandLine excepution:  " + ex.ToString());
			Application.Quit();
		}
	}
}
