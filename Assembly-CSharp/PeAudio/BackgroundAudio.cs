using System;
using System.Collections.Generic;
using System.Xml;
using Pathea;
using Pathea.Maths;
using UnityEngine;

namespace PeAudio;

public class BackgroundAudio : MonoBehaviour
{
	private Dictionary<int, SectionDesc> _sectionDescs;

	private BGMInst bgm;

	private Dictionary<int, BGAInst> bga;

	private GameObject bga_group_go;

	private GameObject spot_group_go;

	private int prevSectionId;

	public int sectionId
	{
		get
		{
			switch (GlobalBehaviour.currentSceneName)
			{
			case "GameStart":
				return -1;
			case "GameMainMenu":
				return -2;
			case "GameRoleCustom":
				return -3;
			case "GameLobby":
				return -4;
			case "MLoginScene":
				return -5;
			case "PeGame":
				if (PeSingleton<PeCreature>.Instance != null && !(PeSingleton<PeCreature>.Instance.mainPlayer != null))
				{
				}
				break;
			}
			return 0;
		}
	}

	public string sectionName
	{
		get
		{
			int key = sectionId;
			if (_sectionDescs.ContainsKey(key))
			{
				return _sectionDescs[key].name;
			}
			return string.Empty;
		}
	}

	public SectionDesc currentSectionDesc
	{
		get
		{
			int key = sectionId;
			if (_sectionDescs.ContainsKey(key))
			{
				return _sectionDescs[key];
			}
			return null;
		}
	}

	private void Init()
	{
		LoadConfig();
		GameObject gameObject = new GameObject("BGM");
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		bga_group_go = new GameObject("BGA");
		bga_group_go.transform.parent = base.transform;
		bga_group_go.transform.localPosition = Vector3.zero;
		spot_group_go = new GameObject("SPOT");
		spot_group_go.transform.parent = base.transform;
		spot_group_go.transform.localPosition = Vector3.zero;
		bgm = gameObject.AddComponent<BGMInst>();
		bgm.audioSrc.path = string.Empty;
		bgm.audioSrc.playOnReset = true;
		bga = new Dictionary<int, BGAInst>();
	}

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		if (sectionId != prevSectionId)
		{
			float prewarm = 0f;
			float postwarm = 0f;
			float predamp = 0.05f;
			float postdamp = 0.05f;
			if (prevSectionId > 0 && sectionId > 0)
			{
				prewarm = 15f;
				postwarm = 10f;
				predamp = 0.005f;
				postdamp = 0.05f;
			}
			else if (prevSectionId > 0 && sectionId < 0)
			{
				prewarm = 0f;
				postwarm = 2f;
				predamp = 0.08f;
				postdamp = 0.08f;
			}
			else if (prevSectionId < 0 && sectionId > 0)
			{
				prewarm = 0f;
				postwarm = 20f;
				predamp = 0.02f;
				postdamp = 0.08f;
			}
			else if (prevSectionId < 0 && sectionId < 0)
			{
				prewarm = 0f;
				postwarm = 0f;
				predamp = 0.03f;
				postdamp = 0.03f;
			}
			if (currentSectionDesc != null)
			{
				bgm.ChangeBGM(currentSectionDesc.bgmDesc.path, prewarm, postwarm, predamp, postdamp);
			}
			else
			{
				bgm.ChangeBGM(string.Empty, prewarm, postwarm, predamp, postdamp);
			}
		}
		prevSectionId = sectionId;
	}

	private BGAInst CreateBGA(int id)
	{
		if (_sectionDescs.ContainsKey(id))
		{
			SectionDesc sectionDesc = _sectionDescs[id];
			GameObject gameObject = new GameObject("BGA " + id + " : " + sectionDesc.name);
			gameObject.transform.parent = bga_group_go.transform;
			gameObject.transform.localPosition = Vector3.zero;
			BGAInst bGAInst = gameObject.AddComponent<BGAInst>();
			bGAInst.audioSrc.path = sectionDesc.bgaDesc.path;
			bGAInst.audioSrc.playOnReset = false;
			bga[id] = bGAInst;
			return bGAInst;
		}
		return null;
	}

	private void CreateSPOT(int id)
	{
	}

	public void LoadConfig()
	{
		string filename = Application.streamingAssetsPath + "/ambient.xml";
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			_sectionDescs = new Dictionary<int, SectionDesc>();
			foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
			{
				if (!(childNode.Name == "SECTION"))
				{
					continue;
				}
				SectionDesc sectionDesc = new SectionDesc();
				int id = XmlConvert.ToInt32(childNode.Attributes["id"].Value);
				string value = childNode.Attributes["name"].Value;
				sectionDesc.id = id;
				sectionDesc.name = value;
				List<SPOTDesc> list = new List<SPOTDesc>();
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "BGM" && sectionDesc.bgmDesc == null)
					{
						sectionDesc.bgmDesc = new BGMDesc();
						sectionDesc.bgmDesc.path = childNode2.Attributes["path"].Value;
						sectionDesc.bgmDesc.musicCount = XmlConvert.ToInt32(childNode2.Attributes["musiccnt"].Value);
						sectionDesc.bgmDesc.changeTime = StrToRange(childNode2.Attributes["changetime"].Value);
					}
					else if (childNode2.Name == "BGA" && sectionDesc.bgaDesc == null)
					{
						sectionDesc.bgaDesc = new BGADesc();
						sectionDesc.bgaDesc.path = childNode2.Attributes["path"].Value;
					}
					else if (childNode2.Name == "SPOT")
					{
						SPOTDesc sPOTDesc = new SPOTDesc();
						sPOTDesc.path = childNode2.Attributes["path"].Value;
						sPOTDesc.density = XmlConvert.ToSingle(childNode2.Attributes["density"].Value);
						sPOTDesc.heightRange = StrToRange(childNode2.Attributes["height"].Value);
						sPOTDesc.minDistRange = StrToRange(childNode2.Attributes["mindistance"].Value);
						sPOTDesc.maxDistRange = StrToRange(childNode2.Attributes["maxdistance"].Value);
						list.Add(sPOTDesc);
					}
				}
				sectionDesc.spotDesc = new SPOTDesc[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					sectionDesc.spotDesc[i] = list[i];
				}
				_sectionDescs[sectionDesc.id] = sectionDesc;
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Load ambient.xml failed");
			_sectionDescs = new Dictionary<int, SectionDesc>();
		}
	}

	private static Range1D StrToRange(string s)
	{
		string[] array = s.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
		Range1D result = default(Range1D);
		if (array.Length == 1)
		{
			float num = Convert.ToSingle(array[0].Trim());
			result.SetMinMax(num, num);
		}
		else
		{
			if (array.Length <= 1)
			{
				throw new Exception("Invalid range string");
			}
			result.SetMinMax(Convert.ToSingle(array[0].Trim()), Convert.ToSingle(array[1].Trim()));
		}
		return result;
	}
}
