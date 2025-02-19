using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Pathea;
using Pathfinding;
using PETools;
using UnityEngine;

public class PathfinderCave : MonoBehaviour
{
	[Serializable]
	public class CaveData
	{
		public Vector2 center = Vector2.zero;

		public Vector2 size = Vector2.zero;

		private static List<CaveData> s_CaveData = new List<CaveData>();

		public Vector2 min => center - size * 0.5f;

		public Vector2 max => center + size * 0.5f;

		public static CaveData Get(Vector2 v2)
		{
			float num = 0f;
			CaveData result = null;
			if (s_CaveData.Count > 0)
			{
				result = s_CaveData[0];
				num = Vector2.SqrMagnitude(v2 - s_CaveData[0].center);
				foreach (CaveData s_CaveDatum in s_CaveData)
				{
					float num2 = Vector2.SqrMagnitude(v2 - s_CaveDatum.center);
					if (num2 < num)
					{
						result = s_CaveDatum;
						num = num2;
					}
				}
			}
			return result;
		}

		public static bool GetCenter(Vector2 v, out Vector2 v1, out Vector2 v2)
		{
			v1 = Vector2.zero;
			v2 = Vector2.zero;
			CaveData[] array = s_CaveData.FindAll((CaveData ret) => Vector2.SqrMagnitude(v - ret.center) <= 262144f).ToArray();
			if (array != null && array.Length > 0)
			{
				Vector2 vector = array[0].min;
				Vector2 vector2 = array[0].max;
				CaveData[] array2 = array;
				foreach (CaveData caveData in array2)
				{
					vector = Vector2.Min(vector, caveData.min);
					vector2 = Vector2.Max(vector2, caveData.max);
				}
				v1 = (vector2 + vector) * 0.5f;
				v2 = vector2 - vector;
				return true;
			}
			return false;
		}

		public static void Add(CaveData[] datas)
		{
			s_CaveData.AddRange(datas);
		}

		public static void Load(string path)
		{
			TextAsset textAsset = Resources.Load(path) as TextAsset;
			if (!(textAsset != null))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.IgnoreComments = true;
			xmlReaderSettings.IgnoreWhitespace = true;
			XmlReader xmlReader = XmlReader.Create(new StringReader(textAsset.text), xmlReaderSettings);
			xmlDocument.Load(xmlReader);
			XmlElement xmlElement = xmlDocument.SelectSingleNode("CaveInfo") as XmlElement;
			foreach (XmlElement item in xmlElement.GetElementsByTagName("Cave"))
			{
				float attributeFloat = XmlUtil.GetAttributeFloat(item, "centerX");
				float attributeFloat2 = XmlUtil.GetAttributeFloat(item, "centerZ");
				float x = XmlUtil.GetAttributeFloat(item, "caveWidthX") + 32f;
				float y = XmlUtil.GetAttributeFloat(item, "caveWidthz") + 32f;
				CaveData caveData = new CaveData();
				caveData.center = new Vector2(attributeFloat, attributeFloat2);
				caveData.size = new Vector2(x, y);
				s_CaveData.Add(caveData);
			}
			xmlReader.Close();
		}
	}

	public CaveData[] storyCaves;

	private LayerGridGraph layerGridGraph;

	private void Awake()
	{
		if (PeGameMgr.IsStory)
		{
			CaveData.Add(storyCaves);
		}
		CaveData.Load("Cave/CaveData");
		if (AstarPath.active == null)
		{
			throw new Exception("There is no AstarPath object in the scene");
		}
		layerGridGraph = AstarPath.active.astarData.layerGridGraph;
		if (layerGridGraph == null)
		{
			throw new Exception("The AstarPath object has no GridGraph");
		}
		UpdateCavePosition();
	}

	private void Update()
	{
		UpdateCavePosition();
	}

	private void UpdateCavePosition()
	{
		if (layerGridGraph == null || !(PeSingleton<PeCreature>.Instance.mainPlayer != null))
		{
			return;
		}
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		Vector2 v = new Vector2(mainPlayer.position.x, mainPlayer.position.z);
		CaveData caveData = CaveData.Get(v);
		if (caveData != null)
		{
			Vector3 vector = new Vector3(caveData.center.x, 0f, caveData.center.y);
			Vector3 vector2 = vector - layerGridGraph.center;
			if (vector2 != Vector3.zero)
			{
				layerGridGraph.center += vector2;
				layerGridGraph.GenerateMatrix();
			}
		}
	}
}
