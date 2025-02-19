using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TownEditor : MonoBehaviour
{
	private class StoryTownData
	{
		public int mId;

		public int mBuildingId;

		public Vector3 mPosition;

		public int mRot;
	}

	private const int Version = 1;

	private static TownEditor mInstance;

	public Block45CurMan mPerfab;

	public float ActiveBuildingDistance = 1000f;

	private bool mIsActive;

	private List<StoryTownData> mTownDataList = new List<StoryTownData>();

	private List<StoryTownData> mUnactivedList = new List<StoryTownData>();

	private List<int> mActivelist = new List<int>();

	private List<int> mNpcList = new List<int>();

	private List<EditBuilding> mEditBuildingList = new List<EditBuilding>();

	private EditBuilding mCurrentOpBuilding;

	private bool mDragMode;

	private Vector3 mMousePos = Vector3.zero;

	public Transform viewTrans;

	public static TownEditor Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	public void SetOpBuildingPosition(Vector3 pos)
	{
		if ((bool)mCurrentOpBuilding)
		{
			if (mDragMode)
			{
				mCurrentOpBuilding.transform.position = pos + 0.5f * Vector3.down;
			}
			else
			{
				mMousePos = Input.mousePosition;
			}
		}
	}

	public void OnCreateBuilding(string fileName)
	{
		if (mPerfab == null)
		{
			mPerfab = Block45CurMan.self;
		}
		BlockBuilding building = BlockBuilding.GetBuilding(fileName);
		GameObject gameObject = new GameObject();
		gameObject.name = "EditBuilding";
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		EditBuilding editBuilding = gameObject.AddComponent<EditBuilding>();
		editBuilding.Init(building, mPerfab);
		mEditBuildingList.Add(editBuilding);
		OnBuildingSelected(editBuilding);
		mDragMode = true;
	}

	public void OnBuildingSelected(EditBuilding building)
	{
		if ((bool)mCurrentOpBuilding)
		{
			if (building == mCurrentOpBuilding)
			{
				mCurrentOpBuilding.mSelected = false;
				mCurrentOpBuilding = null;
				TownEditGui_N.Instance.SetOpBuild(null);
			}
			else
			{
				mCurrentOpBuilding.mSelected = false;
				mCurrentOpBuilding = building;
				mCurrentOpBuilding.mSelected = true;
				TownEditGui_N.Instance.SetOpBuild(mCurrentOpBuilding);
			}
		}
		else
		{
			mCurrentOpBuilding = building;
			mCurrentOpBuilding.mSelected = true;
			TownEditGui_N.Instance.SetOpBuild(mCurrentOpBuilding);
		}
		mDragMode = false;
	}

	public void OnBuildingDrag(EditBuilding building)
	{
		if (!(building == mCurrentOpBuilding))
		{
			return;
		}
		if (!mDragMode)
		{
			if (Vector3.Distance(Input.mousePosition, mMousePos) > 0f)
			{
				mDragMode = true;
			}
		}
		else
		{
			mMousePos = Input.mousePosition;
		}
	}

	public void PutBuildingDown()
	{
		if (mDragMode && null != mCurrentOpBuilding)
		{
			mDragMode = false;
			mCurrentOpBuilding.mSelected = false;
			mCurrentOpBuilding = null;
			TownEditGui_N.Instance.SetOpBuild(null);
		}
	}

	public void TurnBuilding()
	{
		if ((bool)mCurrentOpBuilding)
		{
			mCurrentOpBuilding.transform.rotation *= Quaternion.Euler(90f * Vector3.up);
		}
	}

	public void CancelSelect()
	{
		if ((bool)mCurrentOpBuilding)
		{
			mDragMode = false;
			mCurrentOpBuilding.mSelected = false;
			mCurrentOpBuilding = null;
		}
	}

	public void DeletBuilding()
	{
		if (null != mCurrentOpBuilding && mCurrentOpBuilding.DeletEnable)
		{
			mEditBuildingList.Remove(mCurrentOpBuilding);
			Object.Destroy(mCurrentOpBuilding.gameObject);
			mCurrentOpBuilding = null;
		}
	}

	public void ActiveEditor()
	{
		if (!mIsActive)
		{
			TownEditGui_N.Instance.Show();
			TownEditGui_N.Instance.ResetFileList();
			if (Block45Man.self.DataSource != null)
			{
				Block45Man.self.DataSource.Clear();
			}
			InitEdit();
			BGEffect.Instance.gameObject.SetActive(value: false);
			Block45Man.self.onPlayerPosReady(Camera.main.transform);
		}
	}

	public void Save()
	{
		if (!mIsActive)
		{
			return;
		}
		string path = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/StoryBuildings.txt";
		using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		if (binaryWriter == null)
		{
			Debug.LogError("On WriteRecord FileStream is null!");
			return;
		}
		binaryWriter.Write(1);
		binaryWriter.Write(mEditBuildingList.Count);
		foreach (EditBuilding mEditBuilding in mEditBuildingList)
		{
			binaryWriter.Write(mEditBuilding.mBlockBuilding.mId);
			binaryWriter.Write(mEditBuilding.transform.position.x);
			binaryWriter.Write(mEditBuilding.transform.position.y + 0.5f);
			binaryWriter.Write(mEditBuilding.transform.position.z);
			binaryWriter.Write(Mathf.RoundToInt(mEditBuilding.transform.eulerAngles.y / 90f));
		}
		binaryWriter.Close();
		fileStream.Close();
	}

	public void InitEdit()
	{
		mIsActive = true;
		if (mPerfab == null)
		{
			mPerfab = Block45CurMan.self;
		}
		for (int i = 0; i < mTownDataList.Count; i++)
		{
			BlockBuilding building = BlockBuilding.GetBuilding(mTownDataList[i].mBuildingId);
			GameObject gameObject = new GameObject();
			gameObject.name = "EditBuilding";
			gameObject.transform.position = mTownDataList[i].mPosition;
			gameObject.transform.rotation = Quaternion.Euler(0f, mTownDataList[i].mRot * 90, 0f);
			gameObject.transform.localScale = Vector3.one;
			EditBuilding editBuilding = gameObject.AddComponent<EditBuilding>();
			editBuilding.Init(building, mPerfab);
			mEditBuildingList.Add(editBuilding);
		}
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(1);
		bw.Write(mActivelist.Count);
		for (int i = 0; i < mActivelist.Count; i++)
		{
			bw.Write(mActivelist[i]);
		}
		bw.Write(mNpcList.Count);
		for (int j = 0; j < mNpcList.Count; j++)
		{
			NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(mNpcList[j]);
			if (missionData == null)
			{
				bw.Write(-1);
				continue;
			}
			bw.Write(mNpcList[j]);
			bw.Write(missionData.m_Rnpc_ID);
			bw.Write(missionData.m_QCID);
			bw.Write(missionData.m_CurMissionGroup);
			bw.Write(missionData.m_CurGroupTimes);
			bw.Write(missionData.mCurComMisNum);
			bw.Write(missionData.mCompletedMissionCount);
			bw.Write(missionData.m_RandomMission);
			bw.Write(missionData.m_RecruitMissionNum);
			bw.Write(missionData.m_MissionList.Count);
			for (int k = 0; k < missionData.m_MissionList.Count; k++)
			{
				bw.Write(missionData.m_MissionList[k]);
			}
			bw.Write(missionData.m_MissionListReply.Count);
			for (int l = 0; l < missionData.m_MissionListReply.Count; l++)
			{
				bw.Write(missionData.m_MissionListReply[l]);
			}
		}
	}

	private void Import(byte[] buffer)
	{
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			mActivelist.Add(binaryReader.ReadInt32());
		}
		num = binaryReader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			NpcMissionData npcMissionData = new NpcMissionData();
			int num2 = binaryReader.ReadInt32();
			if (num2 != -1)
			{
				npcMissionData.m_Rnpc_ID = binaryReader.ReadInt32();
				npcMissionData.m_QCID = binaryReader.ReadInt32();
				npcMissionData.m_CurMissionGroup = binaryReader.ReadInt32();
				npcMissionData.m_CurGroupTimes = binaryReader.ReadInt32();
				npcMissionData.mCurComMisNum = binaryReader.ReadByte();
				npcMissionData.mCompletedMissionCount = binaryReader.ReadInt32();
				npcMissionData.m_RandomMission = binaryReader.ReadInt32();
				npcMissionData.m_RecruitMissionNum = binaryReader.ReadInt32();
				int num3 = binaryReader.ReadInt32();
				for (int k = 0; k < num3; k++)
				{
					npcMissionData.m_MissionList.Add(binaryReader.ReadInt32());
				}
				num3 = binaryReader.ReadInt32();
				for (int l = 0; l < num3; l++)
				{
					npcMissionData.m_MissionListReply.Add(binaryReader.ReadInt32());
				}
				mNpcList.Add(num2);
				NpcMissionDataRepository.AddMissionData(num2, npcMissionData);
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public void LoadData()
	{
		string path = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/StoryBuildings.txt";
		if (File.Exists(path))
		{
			using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			int num2 = num;
			if (num2 == 1)
			{
				int num3 = binaryReader.ReadInt32();
				for (int i = 0; i < num3; i++)
				{
					StoryTownData storyTownData = new StoryTownData();
					storyTownData.mId = i;
					storyTownData.mBuildingId = binaryReader.ReadInt32();
					storyTownData.mPosition = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
					storyTownData.mRot = binaryReader.ReadInt32();
					mTownDataList.Add(storyTownData);
				}
			}
			binaryReader.Close();
		}
		else
		{
			path = "StoryBuildings";
			Object @object = Resources.Load(path);
			if ((bool)@object)
			{
				TextAsset textAsset = @object as TextAsset;
				MemoryStream input2 = new MemoryStream(textAsset.bytes);
				BinaryReader binaryReader2 = new BinaryReader(input2);
				int num4 = binaryReader2.ReadInt32();
				int num2 = num4;
				if (num2 == 1)
				{
					int num5 = binaryReader2.ReadInt32();
					for (int j = 0; j < num5; j++)
					{
						StoryTownData storyTownData2 = new StoryTownData();
						storyTownData2.mId = j;
						storyTownData2.mBuildingId = binaryReader2.ReadInt32();
						storyTownData2.mPosition = new Vector3(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
						storyTownData2.mRot = binaryReader2.ReadInt32();
						mTownDataList.Add(storyTownData2);
					}
				}
				binaryReader2.Close();
			}
		}
		for (int k = 0; k < mTownDataList.Count; k++)
		{
			if (!mActivelist.Contains(mTownDataList[k].mId))
			{
				mUnactivedList.Add(mTownDataList[k]);
			}
		}
	}

	private void Update()
	{
		if (null == viewTrans)
		{
			return;
		}
		foreach (StoryTownData mUnactived in mUnactivedList)
		{
			if (Vector3.Distance(viewTrans.position, mUnactived.mPosition) < ActiveBuildingDistance)
			{
				CreateBuilding(mUnactived);
				mActivelist.Add(mUnactived.mId);
				mUnactivedList.Remove(mUnactived);
				break;
			}
		}
	}

	private void CreateBuilding(StoryTownData sTD)
	{
	}
}
