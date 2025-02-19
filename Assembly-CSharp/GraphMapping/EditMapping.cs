using Pathea;
using UnityEngine;

namespace GraphMapping;

public class EditMapping : MonoBehaviour
{
	public Texture2D mBiomeTex;

	public Texture2D mHeightTex;

	public Texture2D mAiSpawnTex;

	public bool mSaveData;

	public bool mTest;

	public Vector2 testPos = new Vector2(2000f, 2000f);

	public Vector2 testWorldSize = new Vector2(18432f, 18432f);

	private bool SaveData()
	{
		PeSingleton<PeMappingMgr>.Instance.mBiomeMap.LoadTexData(mBiomeTex);
		PeSingleton<PeMappingMgr>.Instance.mHeightMap.LoadTexData(mHeightTex);
		PeSingleton<PeMappingMgr>.Instance.mAiSpawnMap.LoadTexData(mAiSpawnTex);
		return PeSingleton<PeMappingMgr>.Instance.SaveFile("D:/PeGraphMapping/");
	}

	private void Awake()
	{
		PeSingleton<PeMappingMgr>.Instance.Init(testWorldSize);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (mSaveData)
		{
			string message = ((!SaveData()) ? "ReLoad texture failed!" : "ReLoad texture suceess!");
			Debug.Log(message);
			mSaveData = false;
		}
		else if (mTest)
		{
			int aiSpawnMapId = PeSingleton<PeMappingMgr>.Instance.mAiSpawnMap.GetAiSpawnMapId(testPos, testWorldSize);
			Debug.Log(aiSpawnMapId);
			mTest = false;
		}
	}
}
