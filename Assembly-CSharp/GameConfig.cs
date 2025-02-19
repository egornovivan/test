using System;
using System.IO;
using Pathea;
using uLink;
using UnityEngine;

public class GameConfig
{
	public const int MaxFixedUpdate = 2;

	public const string StartSceneName = "GameStart";

	public const string RoleSceneName = "GameRoleCustom";

	public const string LobbySceneName = "GameLobby";

	public const string MainMenuSceneName = "GameMainMenu";

	public const string AdventureSceneName = "GameAdventure";

	public const string BuildSceneName = "GameBuild";

	public const string MainSceneName = "GameStory";

	public const string ClientSceneName = "GameClient";

	public const string CreationSceneName = "CreationSystem";

	public const string MultiRoleSceneName = "MLoginScene";

	public const string TutorialSceneName = "GameTraining";

	public const string AssetBundleDir = "AssetBundles";

	public const string MapDataDir_Zip = "VoxelData/zData";

	public const string Network_MapDataDir_Zip = "VoxelData/networkvoxel";

	public const string MapDataDir_Plant = "VoxelData/SubTerrains";

	public const string DataBaseFile = "DataBase/localData";

	public const string ConfigDataDir = "/PlanetExplorers/Config";

	public const string CreateSystemData = "/PlanetExplorers/CreateData";

	public const string VCSystemData = "/PlanetExplorers/VoxelCreationData";

	public const string UserDataDir = "UserData/0";

	public const float NetMinUpdateValue = 3f;

	public const float NetUpdateInterval = 3f;

	public const float NPCControlDistance = 4.5f;

	public static bool IsInVCE = false;

	private static string _strGameVersion = null;

	public static readonly string AssetsManifest_Base = "BaseAssets";

	public static readonly string AssetsManifest_Dynamic = "DynamicAssets";

	public static readonly string AssetsManifest_Item = "Item";

	public static readonly string AssetsManifest_Monster = "Monster";

	public static readonly string AssetsManifest_Tower = "Tower";

	public static readonly string AssetsManifest_Puja = "Native";

	public static readonly string AssetsManifest_Alien = "Alien";

	public static readonly string AssetsManifest_Group = "Group";

	public static readonly string AssetsManifest_Player = "Player";

	public static readonly string AssetsManifest_Npc = "Npc";

	public static readonly string DataBaseI18NPath = PEDataPath + "i18n.db";

	public static readonly string AssetBundlePath = PEDataPath + "AssetBundles/";

	public static readonly string OclSourcePath = Application.dataPath + "/Resources/OclKernel";

	public static readonly string OclBinaryPath = PEDataPath + "clCache";

	public static readonly string VoxelCacheDataPath = PEDataPath + "voxelCache/";

	public static readonly string MergedUserDataPath = PEDataPath + "UserData/0/Merged/";

	private static string _userDataPath = string.Empty;

	public static int GroundLayer = 6144;

	public static int SceneLayer = 620544;

	public static int ProjectileDamageLayer = 34174976;

	public static readonly string RadioSoundsPath = Path.Combine(PEDataPath, "CustomSounds/");

	public static readonly string OSTSoundsPath = Path.Combine(PEDataPath, "Soundtrack/");

	public static string PEDataPath => Application.dataPath + "/../";

	public static string GameVersion
	{
		get
		{
			if (_strGameVersion == null)
			{
				string path = PEDataPath + "ConfigFiles/version.txt";
				_strGameVersion = string.Empty;
				if (File.Exists(path))
				{
					using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
					StreamReader streamReader = new StreamReader(fileStream);
					streamReader.BaseStream.Seek(0L, SeekOrigin.Begin);
					_strGameVersion = streamReader.ReadLine();
					streamReader.Close();
					fileStream.Close();
				}
			}
			return _strGameVersion;
		}
	}

	public static string CustomDataDir => Path.Combine(PEDataPath, "CustomGames");

	public static bool IsMultiMode => PeGameMgr.IsMulti;

	public static bool IsMultiClient => uLink.Network.isClient;

	public static bool IsMultiServer => uLink.Network.isServer;

	public static bool IsNight => GameTime.Timer.CycleInDay < -0.1;

	public static void SetUserDataPath(string userDataPath)
	{
		if (!string.IsNullOrEmpty(userDataPath))
		{
			try
			{
				if (Directory.Exists(userDataPath))
				{
					_userDataPath = userDataPath;
					return;
				}
				throw new Exception("Path Not Exists!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[UserDataPath]Failed to set " + userDataPath + ":" + ex);
			}
		}
		_userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
	}

	public static string GetUserDataPath()
	{
		if (string.IsNullOrEmpty(_userDataPath))
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}
		return _userDataPath;
	}

	public static string GetPeUserDataPath()
	{
		return Path.Combine(GetUserDataPath(), "PlanetExplorers");
	}
}
