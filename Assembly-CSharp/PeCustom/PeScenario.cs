using System;
using System.IO;
using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

public class PeScenario : PeCustomScene.SceneElement, Pathea.ISerializable, ISceneController, IMonoLike
{
	public const string ArchiveKey = "CustomScenario";

	private HashBinder mBinder = new HashBinder();

	private Scenario m_Scenario;

	private bool _isNew = true;

	private bool _initialized;

	public static bool s_ShowTools;

	private string mNPCid = string.Empty;

	public HashBinder Binder => mBinder;

	public Scenario core => m_Scenario;

	public int playerId { get; private set; }

	public TickMgr tickMgr { get; private set; }

	public CustomGUIMgr guiMgr { get; private set; }

	public StopwatchMgr stopwatchMgr { get; private set; }

	public MissionMgr missionMgr { get; private set; }

	public DialogMgr dialogMgr { get; private set; }

	public bool isRunning { get; private set; }

	void Pathea.ISerializable.Serialize(PeRecordWriter w)
	{
		if (_initialized)
		{
			w.Write(0);
			w.Write(m_Scenario.Export());
			missionMgr.Export(w.binaryWriter);
			stopwatchMgr.Export(w.binaryWriter);
			dialogMgr.Export(w.binaryWriter);
		}
	}

	public void Init(string file_path, int player_id)
	{
		if (!_initialized)
		{
			m_Scenario = Scenario.Create(file_path);
			if (m_Scenario != null)
			{
				playerId = player_id;
				InitSubSystems();
				PeSingleton<ArchiveMgr>.Instance.Register("CustomScenario", this);
				_initialized = true;
				m_Scenario.SetAsDebugTarget();
			}
		}
	}

	private void InitSubSystems()
	{
		tickMgr = new TickMgr();
		guiMgr = new CustomGUIMgr();
		stopwatchMgr = new StopwatchMgr();
		missionMgr = new MissionMgr(m_Scenario);
		dialogMgr = new DialogMgr();
	}

	public void OnNotification(ESceneNoification msg_type, params object[] data)
	{
		if (!_initialized)
		{
			return;
		}
		switch (msg_type)
		{
		case ESceneNoification.SceneBegin:
			InitUI();
			if (_isNew)
			{
				missionMgr.RunMission(0);
				Debug.Log("[Custom Scenario] Scene Begin ---- Run Mission (0)");
			}
			else
			{
				missionMgr.ResumeMission();
				Debug.Log("[Custom Scenario] Scene Begin ---- Resume Mission");
			}
			InitOther();
			isRunning = true;
			break;
		case ESceneNoification.SceneEnd:
			Close();
			break;
		}
	}

	public void InitUI()
	{
		GameUI.Instance.mStopwatchList.gameObject.SetActive(value: true);
		GameUI.Instance.mNpcDialog.Init();
		GameUI.Instance.mNPCSpeech.Init();
		GameUI.Instance.mMissionGoal.Init();
		GameUI.Instance.mCustomMissionTrack.Init();
	}

	public void InitOther()
	{
		if (PeSingleton<EntityMgr>.Instance != null && PeSingleton<EntityMgr>.Instance.eventor != null)
		{
			PeSingleton<EntityMgr>.Instance.eventor.Subscribe(OnMouseClickEntity);
		}
		CustomGameData curGameData = PeSingleton<CustomGameData.Mgr>.Instance.curGameData;
		foreach (ForceDesc mForceDesc in curGameData.mForceDescs)
		{
			Singleton<ForceSetting>.Instance.AddForceDesc(mForceDesc);
		}
		foreach (PlayerDesc mPlayerDesc in curGameData.mPlayerDescs)
		{
			Singleton<ForceSetting>.Instance.AddPlayerDesc(mPlayerDesc);
		}
	}

	public void CloseUI()
	{
		GameUI.Instance.mNpcDialog.Close();
		GameUI.Instance.mNPCSpeech.Close();
		GameUI.Instance.mMissionGoal.Close();
		GameUI.Instance.mCustomMissionTrack.Close();
	}

	private void OnMouseClickEntity(object sender, EntityMgr.RMouseClickEntityEvent e)
	{
		if (!(e.entity == null))
		{
			CommonCmpt commonCmpt = e.entity.commonCmpt;
			if (commonCmpt != null && commonCmpt.entityProto.proto == EEntityProto.Npc && GameUI.Instance.mNpcDialog.dialogInterpreter.SetNpoEntity(e.entity))
			{
				GameUI.Instance.mNpcDialog.Show();
			}
		}
	}

	public void Start()
	{
	}

	public void Update()
	{
		if (_initialized && isRunning)
		{
			m_Scenario.Update();
			tickMgr.Tick();
			stopwatchMgr.Update(Time.deltaTime);
			missionMgr.UpdateGoals();
		}
	}

	public void OnGUI()
	{
		if (!_initialized)
		{
			return;
		}
		guiMgr.DrawGUI();
		if (!s_ShowTools)
		{
			return;
		}
		mNPCid = GUI.TextArea(new Rect(100f, 270f, 80f, 20f), mNPCid);
		if (GUI.Button(new Rect(10f, 270f, 70f, 20f), "EntityId"))
		{
			OBJECT obj = default(OBJECT);
			obj.type = OBJECT.OBJECTTYPE.WorldObject;
			obj.Group = 0;
			obj.Id = Convert.ToInt32(mNPCid);
			PeEntity entity = PeScenarioUtility.GetEntity(obj);
			if (entity != null)
			{
				Vector3 position = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				position.z += 2f;
				position.y += 1f;
				entity.peTrans.position = position;
				entity.NpcCmpt.FixedPointPos = position;
			}
		}
	}

	public void OnDestroy()
	{
		Close();
	}

	public void Close()
	{
		if (_initialized)
		{
			m_Scenario.Close();
			m_Scenario = null;
			_initialized = false;
			isRunning = false;
		}
	}

	public void Restore()
	{
		if (!_initialized)
		{
			return;
		}
		byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData("CustomScenario");
		if (data != null)
		{
			_isNew = false;
			using (MemoryStream input = new MemoryStream(data))
			{
				BinaryReader binaryReader = new BinaryReader(input);
				binaryReader.ReadInt32();
				m_Scenario.Import(binaryReader);
				missionMgr.Import(binaryReader);
				stopwatchMgr.Import(binaryReader);
				dialogMgr.Import(binaryReader);
			}
			Debug.Log("[Custom Scenario] restore the Data");
		}
	}
}
