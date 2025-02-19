using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PeMap;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;

public class BGManager : MonoBehaviour
{
	private const int BattleID = 701;

	private const int WaterID = 916;

	private const int WaterSurfaceSeaID = 918;

	private const int WaterSurfaceRiverID = 917;

	private const int CreationMusicID = 1162;

	private const int PlayerBaseMusicID = 1162;

	private const int NativeBaseMusicID = 1161;

	private const int DungeonIronMusicID = 1161;

	private const int DungeonCaveMusicID = 836;

	public static BGManager Instance;

	private static float MinBattleTime = 5f;

	private int mBgMusicID;

	private float mBattleBgTime;

	private bool mBattle;

	private bool mInWater;

	private bool mInWaterSurface;

	private bool mInitPlayer;

	private PeEntity mPlayer;

	private PeEntity mVehicle;

	private List<PeEntity> mPlayerEnemies;

	private List<PeEntity> mVehicleEnemies;

	private AudioController mBgBattleAudio;

	private AudioController mBgWaterAudio;

	private AudioController mBgWaterSurfaceSeaAudio;

	private AudioController mBgWaterSurfaceRiverAudio;

	private Dictionary<int, AudioController> mBgMusicDic = new Dictionary<int, AudioController>();

	public int bgMusic
	{
		set
		{
			if (mBgMusicID != value)
			{
				if (mBgMusicID > 0)
				{
					OnBgMusicExit(mBgMusicID);
				}
				mBgMusicID = value;
				if (mBgMusicID > 0)
				{
					OnBgMusicEnter(mBgMusicID);
				}
			}
		}
	}

	public bool battle
	{
		get
		{
			return mBattle;
		}
		set
		{
			if (mBattle != value)
			{
				mBattle = value;
				if (mBattle)
				{
					OnBattleEnter();
				}
				else
				{
					OnBattleExit();
				}
			}
		}
	}

	public bool inWater
	{
		get
		{
			return mInWater;
		}
		set
		{
			if (mInWater != value)
			{
				mInWater = value;
				if (mInWater)
				{
					OnPlayerWaterEnter();
				}
				else
				{
					OnPlayerWaterExit();
				}
			}
		}
	}

	public bool inWaterSurface
	{
		get
		{
			return mInWaterSurface;
		}
		set
		{
			if (mInWaterSurface != value)
			{
				mInWaterSurface = value;
				if (mInWaterSurface)
				{
					OnPlayerWaterSurfaceEnter();
				}
				else
				{
					OnPlayerWaterSurfaceExit();
				}
			}
		}
	}

	public AudioController bgAudio
	{
		get
		{
			if (mBgMusicDic.ContainsKey(mBgMusicID))
			{
				return mBgMusicDic[mBgMusicID];
			}
			return null;
		}
	}

	public AudioController bgBattleAudio => mBgBattleAudio;

	private int GetEnvironmentBgMusic()
	{
		if (VCEditor.s_Active)
		{
			return 1162;
		}
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (mainPlayer != null)
		{
			if (CSMain.Instance != null && CSMain.Instance.IsInAssemblyArea(mainPlayer.position))
			{
				return 1162;
			}
			if (PeGameMgr.IsAdventure)
			{
				if (RandomDunGenUtil.IsInDungeon(mainPlayer))
				{
					if (RandomDunGenUtil.GetDungeonType() == DungeonType.Iron)
					{
						return 1161;
					}
					return 836;
				}
				if (VArtifactUtil.IsInNativeCampArea(mainPlayer.position) >= 0)
				{
					return 1161;
				}
			}
			if (PeGameMgr.IsStory)
			{
				int mapSoundID = PeSingleton<StaticPoint.Mgr>.Instance.GetMapSoundID(mainPlayer.position);
				if (mapSoundID > 0)
				{
					return mapSoundID;
				}
			}
		}
		return 0;
	}

	protected virtual int GetCurrentBgMusicID()
	{
		return 0;
	}

	private IEnumerator UpdateTerrain()
	{
		while (true)
		{
			int bgMusicID = GetEnvironmentBgMusic();
			if (bgMusicID > 0)
			{
				bgMusic = bgMusicID;
			}
			else
			{
				bgMusic = GetCurrentBgMusicID();
			}
			yield return new WaitForSeconds(10f);
		}
	}

	private IEnumerator PauseBgAudio()
	{
		while (!(this is BGMainMenu))
		{
			if (bgAudio != null && bgAudio.length > Mathf.Epsilon)
			{
				float len = bgAudio.length;
				if (!bgAudio.isPlaying)
				{
					if (!mBattle)
					{
						bgAudio.PlayAudio(10f);
						yield return new WaitForSeconds((float)UnityEngine.Random.Range(2, 4) * len);
					}
				}
				else if (bgAudio.time >= len - 10f)
				{
					bgAudio.StopAudio(10f);
					yield return new WaitForSeconds((float)UnityEngine.Random.Range(1, 3) * len);
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private bool CalculateBattleState()
	{
		if (mPlayer != null)
		{
			for (int i = 0; i < mPlayerEnemies.Count; i++)
			{
				if (mPlayerEnemies[i] != null && PEUtil.SqrMagnitude(mPlayer.position, mPlayerEnemies[i].position, is3D: false) < 100f)
				{
					mBattleBgTime = Time.time;
				}
			}
		}
		if (mVehicle != null)
		{
			for (int j = 0; j < mVehicleEnemies.Count; j++)
			{
				if (mVehicleEnemies[j] != null && PEUtil.SqrMagnitude(mVehicle.position, mVehicleEnemies[j].position, is3D: false) < 100f)
				{
					mBattleBgTime = Time.time;
				}
			}
		}
		return Time.time - mBattleBgTime < MinBattleTime;
	}

	private bool IsInWaterSurfaceSea(Vector3 position)
	{
		if (inWaterSurface)
		{
			if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Adventure)
			{
				return VFDataRTGen.IsSea((int)position.x, (int)position.z);
			}
			if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story)
			{
				Vector2 targetPos = new Vector2(position.x, position.z);
				int aiSpawnMapId = PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(targetPos);
				return aiSpawnMapId == 25 || aiSpawnMapId == 26 || aiSpawnMapId == 22;
			}
		}
		return false;
	}

	private void OnPlayerWaterEnter()
	{
		if (mBgWaterAudio != null)
		{
			mBgWaterAudio.PlayAudio(5f);
		}
	}

	private void OnPlayerWaterExit()
	{
		if (mBgWaterAudio != null)
		{
			mBgWaterAudio.StopAudio(5f);
		}
	}

	private void OnPlayerWaterSurfaceEnter()
	{
		if (mPlayer == null || IsInWaterSurfaceSea(mPlayer.position))
		{
			if ((bool)mBgWaterSurfaceSeaAudio)
			{
				mBgWaterSurfaceSeaAudio.PlayAudio(5f);
			}
		}
		else if ((bool)mBgWaterSurfaceRiverAudio)
		{
			mBgWaterSurfaceRiverAudio.PlayAudio(5f);
		}
	}

	private void OnPlayerWaterSurfaceExit()
	{
		if ((bool)mBgWaterSurfaceSeaAudio)
		{
			mBgWaterSurfaceSeaAudio.StopAudio(5f);
		}
		if ((bool)mBgWaterSurfaceRiverAudio)
		{
			mBgWaterSurfaceRiverAudio.StopAudio(5f);
		}
	}

	private void OnBattleEnter()
	{
		if (bgAudio != null)
		{
			bgAudio.StopAudio(5f);
		}
		if (mBgBattleAudio != null)
		{
			mBgBattleAudio.PlayAudio(10f);
		}
	}

	private void OnBattleExit()
	{
		if (bgAudio != null)
		{
			bgAudio.PlayAudio(10f);
		}
		if (mBgBattleAudio != null)
		{
			mBgBattleAudio.PauseAudio(5f);
		}
	}

	private void OnBgMusicEnter(int id)
	{
		if (id <= 0)
		{
			return;
		}
		if (mBgMusicDic.ContainsKey(id) && mBgMusicDic[id] != null)
		{
			mBgMusicDic[id].PlayAudio();
			return;
		}
		AudioController audioController = AudioManager.instance.Create(PEUtil.MainCamTransform.position, id, base.transform, isPlay: false, isDelete: false);
		if (audioController != null)
		{
			audioController.PlayAudio(10f);
			mBgMusicDic.Add(id, audioController);
		}
	}

	private void OnBgMusicExit(int id)
	{
		if (id > 0 && mBgMusicDic.ContainsKey(id))
		{
			if (mBgMusicDic[id] != null)
			{
				mBgMusicDic[id].StopAudio(5f);
				mBgMusicDic[id].Delete(5f);
			}
			mBgMusicDic.Remove(id);
		}
	}

	private void OnPlayerGetOnCarrier(CarrierController carrier)
	{
		mVehicle = carrier.GetComponent<PeEntity>();
		if (mVehicle != null && mVehicle.peSkEntity != null)
		{
			mVehicle.peSkEntity.attackEvent += OnPlayerVehicleAttack;
			mVehicle.peSkEntity.onHpReduce += OnPlayerVehicleBeAttack;
			mVehicle.peSkEntity.OnBeEnemyEnter += OnVehicleEnemyEnter;
			mVehicle.peSkEntity.OnBeEnemyExit += OnVehicleEnemyExit;
		}
		mPlayerEnemies.Clear();
	}

	private void OnPlayerGetOffCarrier(CarrierController carrier)
	{
		if (mVehicle != null && mVehicle.peSkEntity != null)
		{
			mVehicle.peSkEntity.attackEvent -= OnPlayerVehicleAttack;
			mVehicle.peSkEntity.onHpReduce -= OnPlayerVehicleBeAttack;
			mVehicle.peSkEntity.OnBeEnemyEnter -= OnVehicleEnemyEnter;
			mVehicle.peSkEntity.OnBeEnemyExit -= OnVehicleEnemyExit;
		}
		mVehicle = null;
		mVehicleEnemies.Clear();
	}

	private void OnPlayerEnemyEnter(PeEntity enemy)
	{
		if (!mPlayerEnemies.Contains(enemy))
		{
			mPlayerEnemies.Add(enemy);
		}
	}

	private void OnPlayerEnemyExit(PeEntity enemy)
	{
		if (mPlayerEnemies.Contains(enemy))
		{
			mPlayerEnemies.Remove(enemy);
		}
	}

	private void OnVehicleEnemyEnter(PeEntity enemy)
	{
		if (!mVehicleEnemies.Contains(enemy))
		{
			mVehicleEnemies.Add(enemy);
		}
	}

	private void OnVehicleEnemyExit(PeEntity enemy)
	{
		if (mVehicleEnemies.Contains(enemy))
		{
			mVehicleEnemies.Remove(enemy);
		}
	}

	private void OnPlayerAttack(SkEntity hurt, float damage)
	{
		mBattleBgTime = Time.time;
	}

	private void OnPlayerBeAttack(SkEntity hurt, float damage)
	{
		if (!(hurt == null) && (!(mPlayer != null) || !(mPlayer.skEntity == hurt)))
		{
			mBattleBgTime = Time.time;
		}
	}

	private void OnPlayerVehicleAttack(SkEntity hurt, float damage)
	{
		mBattleBgTime = Time.time;
	}

	private void OnPlayerVehicleBeAttack(SkEntity hurt, float damage)
	{
		if (!(hurt == null) && (!(mVehicle != null) || !(mVehicle.skEntity == hurt)))
		{
			mBattleBgTime = Time.time;
		}
	}

	private void Awake()
	{
		Instance = this;
		mPlayerEnemies = new List<PeEntity>();
		mVehicleEnemies = new List<PeEntity>();
		Vector3 position = PEUtil.MainCamTransform.position;
		mBgBattleAudio = AudioManager.instance.Create(position, 701, base.transform, isPlay: false, isDelete: false);
		mBgWaterAudio = AudioManager.instance.Create(position, 916, base.transform, isPlay: false, isDelete: false);
		mBgWaterSurfaceSeaAudio = AudioManager.instance.Create(position, 918, base.transform, isPlay: false, isDelete: false);
		mBgWaterSurfaceRiverAudio = AudioManager.instance.Create(position, 917, base.transform, isPlay: false, isDelete: false);
		StartCoroutine(UpdateTerrain());
		StartCoroutine(PauseBgAudio());
	}

	private void Update()
	{
		if (mPlayer == null)
		{
			mPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		}
		if (mPlayer != null && !mInitPlayer)
		{
			mInitPlayer = true;
			if (mPlayer.passengerCmpt != null)
			{
				PassengerCmpt passengerCmpt = mPlayer.passengerCmpt;
				passengerCmpt.onGetOnCarrier = (Action<CarrierController>)Delegate.Combine(passengerCmpt.onGetOnCarrier, new Action<CarrierController>(OnPlayerGetOnCarrier));
				PassengerCmpt passengerCmpt2 = mPlayer.passengerCmpt;
				passengerCmpt2.onGetOffCarrier = (Action<CarrierController>)Delegate.Combine(passengerCmpt2.onGetOffCarrier, new Action<CarrierController>(OnPlayerGetOffCarrier));
			}
			mPlayer.peSkEntity.attackEvent += OnPlayerAttack;
			mPlayer.peSkEntity.onHpReduce += OnPlayerBeAttack;
			mPlayer.peSkEntity.OnBeEnemyEnter += OnPlayerEnemyEnter;
			mPlayer.peSkEntity.OnBeEnemyExit += OnPlayerEnemyExit;
		}
		if (mPlayer != null && mPlayer.biologyViewCmpt != null && mPlayer.biologyViewCmpt.monoPhyCtrl != null)
		{
			bool spineInWater = mPlayer.biologyViewCmpt.monoPhyCtrl.spineInWater;
			bool flag = (inWater = mPlayer.biologyViewCmpt.monoPhyCtrl.headInWater);
			inWaterSurface = spineInWater && !flag;
		}
		battle = CalculateBattleState();
	}
}
