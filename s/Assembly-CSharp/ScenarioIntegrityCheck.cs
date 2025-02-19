using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

public class ScenarioIntegrityCheck
{
	public class Signature
	{
		public string code;

		public string sig;

		public Signature(string _code)
		{
			code = _code;
			sig = string.Empty;
		}

		public Signature(string _code, string _sig)
		{
			code = _code;
			sig = _sig;
		}

		public static int Compare(Signature lhs, Signature rhs)
		{
			return string.Compare(lhs.code, rhs.code);
		}
	}

	private bool used;

	private string path = string.Empty;

	private string uid = string.Empty;

	private string[] worldFiles = new string[16]
	{
		"map_x0_y0.voxelform", "map_x0_y0_1.voxelform", "map_x0_y0_2.voxelform", "map_x0_y0_3.voxelform", "map_x0_y0_4.voxelform", "water_x0_y0.voxelform", "water_x0_y0_1.voxelform", "water_x0_y0_2.voxelform", "water_x0_y0_3.voxelform", "water_x0_y0_4.voxelform",
		"subter.dat", "subTerG_x0_y0.dat", "ProjectSettings.dat", "Minimap.png", "ProjectData.xml", "WorldEntity.xml"
	};

	private string[] worldFileCodes = new string[16]
	{
		"T0", "T1", "T2", "T3", "T4", "W0", "W1", "W2", "W3", "W4",
		"TR", "GR", "PJ", "MM", "PD", "WD"
	};

	private string sp = ",sdof3.21d';;dasmv.23vla.D.FAdavaz;;s";

	private List<Signature> signatures;

	public bool? integrated { get; private set; }

	public string error { get; private set; }

	public ScenarioIntegrityCheck(string scenario_path)
	{
		if (used)
		{
			return;
		}
		try
		{
			used = true;
			path = scenario_path;
			integrated = null;
			error = string.Empty;
			if (string.IsNullOrEmpty(path))
			{
				integrated = false;
				error = "Map file doesn't exist";
				return;
			}
			path = scenario_path + "/";
			string text = path + "MAP.uid";
			if (!File.Exists(text))
			{
				integrated = false;
				error = "Map uid file doesn't exist";
				return;
			}
			uid = File.ReadAllText(text, Encoding.UTF8);
			uid = uid.Trim();
			if (uid.Length != 32)
			{
				integrated = false;
				error = "Invalid UID";
			}
			else
			{
				Thread thread = new Thread(CheckThread);
				thread.Start();
			}
		}
		catch (Exception ex)
		{
			error = "TryCatch " + ex.ToString();
			integrated = false;
		}
	}

	private void CheckThread()
	{
		try
		{
			signatures = new List<Signature>();
			string text = path + "Scenario/";
			string text2 = path + "Worlds/";
			string text3 = text + "Missions/";
			string filepath = text + "ForceSettings.xml";
			string text4 = text + "WorldSettings.xml";
			if (!AddFileSig("_WS", text4))
			{
				integrated = false;
				error = "WorldSettings.xml corrupt";
			}
			else if (!AddFileSig("_FS", filepath))
			{
				integrated = false;
				error = "ForceSettings.xml corrupt";
			}
			else if (Directory.Exists(text3))
			{
				string[] files = Directory.GetFiles(text3);
				string[] array = files;
				foreach (string text5 in array)
				{
					FileInfo fileInfo = new FileInfo(text5);
					int result = 0;
					if (fileInfo.Name.Length < 5)
					{
						integrated = false;
						error = "Corrupt mission filename (5)";
						return;
					}
					string text6 = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
					if (int.TryParse(text6, out result))
					{
						if (!AddFileSig("M" + text6, text5))
						{
							integrated = false;
							error = "Mission " + text6 + " corrupt";
							return;
						}
						continue;
					}
					integrated = false;
					error = "Corrupt mission filename (x)";
					return;
				}
				if (Directory.Exists(text2))
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(text4);
					List<string> list = new List<string>();
					foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
					{
						if (childNode.Name == "WORLD")
						{
							list.Add(childNode.Attributes["path"].Value);
							continue;
						}
						integrated = false;
						error = "WorldSettings.xml corrupt";
						return;
					}
					for (int j = 0; j < list.Count; j++)
					{
						string text7 = text2 + list[j] + "/";
						for (int k = 0; k < worldFiles.Length; k++)
						{
							string filepath2 = text7 + worldFiles[k];
							AddFileSig("W" + j + worldFileCodes[k], filepath2);
						}
					}
					signatures.Sort(Signature.Compare);
					string text8 = string.Empty;
					for (int l = 0; l < signatures.Count; l++)
					{
						text8 += signatures[l].code;
						text8 += ":";
						text8 += signatures[l].sig;
						text8 += ";";
					}
					string text9 = MD5Hash.MD5Encoding(text8 + sp);
					if (uid == text9)
					{
						integrated = true;
						return;
					}
					integrated = false;
					error = "One or more file has been tampered";
				}
				else
				{
					integrated = false;
					error = "World folder lost";
				}
			}
			else
			{
				integrated = false;
				error = "Mission folder lost";
			}
		}
		catch (Exception ex)
		{
			error = "TryCatch " + ex.ToString();
			integrated = false;
		}
	}

	private bool AddFileSig(string code, string filepath)
	{
		Signature signature = new Signature(code);
		if (File.Exists(filepath))
		{
			try
			{
				using FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
				signature.sig = MD5Hash.MD5Encoding(stream);
				signatures.Add(signature);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		return false;
	}
}
