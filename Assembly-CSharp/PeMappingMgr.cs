using System;
using System.IO;
using GraphMapping;
using Pathea;
using UnityEngine;

public class PeMappingMgr : MonoLikeSingleton<PeMappingMgr>
{
	public static bool inited;

	private Vector2 mWorldSize;

	public PeBiomeMapping mBiomeMap;

	public PeHeightMapping mHeightMap;

	public PeAiSpawnMapping mAiSpawnMap;

	private EBiome mBiome;

	private float mHeight;

	private int mAiSpawnID;

	private PeTrans mTrans;

	public EBiome Biome => mBiome;

	public float Height => mHeight;

	public int AiSpawnID => mAiSpawnID;

	private string filePath => "PeMappingData";

	private Vector2 GetPlayerPos()
	{
		if (mTrans == null)
		{
			PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
			if (null != mainPlayer)
			{
				mTrans = mainPlayer.GetCmpt<PeTrans>();
			}
		}
		if (null != mTrans)
		{
			return new Vector2(mTrans.position.x, mTrans.position.z);
		}
		return Vector2.zero;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		inited = false;
	}

	public override void Update()
	{
		if (!inited)
		{
			if (PeGameMgr.IsStory)
			{
				Debug.LogError("PeMapping has be used but it is not init!");
			}
		}
		else
		{
			base.Update();
			UpdateMappings(GetPlayerPos());
		}
	}

	public void UpdateMappings(Vector2 playerPos)
	{
		mBiome = mBiomeMap.GetBiom(playerPos, mWorldSize);
		mHeight = mHeightMap.GetHeight(playerPos, mWorldSize);
		mAiSpawnID = mAiSpawnMap.GetAiSpawnMapId(playerPos, mWorldSize);
	}

	public float GetTerrainHeight(Vector3 pos)
	{
		if (PeGameMgr.IsStory)
		{
			if (mHeightMap == null)
			{
				return 100f;
			}
			return mHeightMap.GetHeight(new Vector2(pos.x, pos.z), mWorldSize);
		}
		if (PeGameMgr.IsCustom)
		{
			return 100f;
		}
		return VFDataRTGen.GetBaseTerHeight(new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)));
	}

	public int GetAiSpawnMapId(Vector2 targetPos)
	{
		if (mAiSpawnMap == null)
		{
			Debug.LogError("Failed to read AiSpawnMap");
			return -1;
		}
		return mAiSpawnMap.GetAiSpawnMapId(targetPos, mWorldSize);
	}

	public void Init(Vector2 worldSzie)
	{
		mBiomeMap = new PeBiomeMapping();
		mHeightMap = new PeHeightMapping();
		mAiSpawnMap = new PeAiSpawnMapping();
		mWorldSize = worldSzie;
		LoadFile();
		inited = true;
	}

	private void SaveData(BinaryWriter bw)
	{
		byte[] array = mBiomeMap.Serialize();
		bw.Write(array.Length);
		bw.Write(array);
		array = mHeightMap.Serialize();
		bw.Write(array.Length);
		bw.Write(array);
		array = mAiSpawnMap.Serialize();
		bw.Write(array.Length);
		bw.Write(array);
	}

	private void ReadData(BinaryReader br)
	{
		int count = br.ReadInt32();
		mBiomeMap.Deserialize(br.ReadBytes(count));
		count = br.ReadInt32();
		mHeightMap.Deserialize(br.ReadBytes(count));
		count = br.ReadInt32();
		mAiSpawnMap.Deserialize(br.ReadBytes(count));
	}

	private bool LoadFile()
	{
		try
		{
			TextAsset textAsset = Resources.Load(filePath) as TextAsset;
			if (textAsset == null)
			{
				Debug.LogError("Load '" + filePath + "' failed!");
				return false;
			}
			MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			ReadData(binaryReader);
			binaryReader.Close();
			memoryStream.Close();
			return true;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
	}

	public bool SaveFile(string DirPath)
	{
		try
		{
			CheckDir(DirPath);
			using (FileStream fileStream = new FileStream(DirPath + filePath + ".bytes", FileMode.Create, FileAccess.Write))
			{
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				SaveData(binaryWriter);
				binaryWriter.Close();
				fileStream.Close();
			}
			return true;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
	}

	private void CheckDir(string DirPath)
	{
		if (!Directory.Exists(DirPath))
		{
			Directory.CreateDirectory(DirPath);
		}
	}
}
