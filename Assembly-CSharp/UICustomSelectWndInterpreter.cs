using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class UICustomSelectWndInterpreter : MonoBehaviour
{
	public class MapItemDescs
	{
		public bool IsDir;

		public string UID;

		public string Name;

		public string Path;
	}

	[SerializeField]
	private UICustomGameSelectWnd selectWnd;

	public int pathCharCount = 300;

	public static bool ignoreIntegrityCheck;

	private bool mIsProcessCheck;

	private Dictionary<string, ScenarioMapDesc> mMapDescs = new Dictionary<string, ScenarioMapDesc>(10);

	private DirectoryInfo mBackDir;

	private string mCurPath;

	private List<MapItemDescs> mMapItems = new List<MapItemDescs>(10);

	private void Awake()
	{
		Init();
	}

	public void Init()
	{
		selectWnd.onInit += OnWndInit;
		selectWnd.onOpen += OnWndOpen;
		selectWnd.onClose += OnWndClose;
		selectWnd.onBack += OnWndBack;
		selectWnd.onMapItemClick += OnWndMapItemClick;
		selectWnd.onMapItemDoubleClick += OnWndMapItemDoubleClick;
		selectWnd.onMapItemSetContent += OnWndMapItemSetContent;
		selectWnd.onMapItemClearContent += OnWndMapItemClearContent;
		selectWnd.onPlayerSelectedChanged += OnWndPlayerSelectChanged;
		selectWnd.onStartBtnClick += OnWndStartClick;
	}

	private void OnWndInit()
	{
	}

	private void OnWndOpen()
	{
		if (mCurPath == null)
		{
			mCurPath = GameConfig.CustomDataDir;
		}
		UpdateMapItem(mCurPath);
	}

	private void OnWndClose()
	{
	}

	private bool OnWndBack()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(mCurPath);
		bool flag = false;
		if (directoryInfo.Parent != null && Path.GetFileName(mCurPath) != Path.GetFileName(GameConfig.CustomDataDir))
		{
			mCurPath = directoryInfo.Parent.FullName;
			flag = true;
		}
		else
		{
			mCurPath = GameConfig.CustomDataDir;
			flag = false;
		}
		UpdateMapItem(mCurPath);
		return flag;
	}

	private void OnWndMapItemClick(UICustomGameSelectWnd.CMapInfo mapInfo, UICustomGameSelectWnd.CPlayerInfo playerInfo, UICustomMapItem item)
	{
		if (mMapItems.Count <= item.index)
		{
			return;
		}
		MapItemDescs mapItemDescs = mMapItems[item.index];
		CustomGameData customGameData = null;
		try
		{
			customGameData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(mapItemDescs.UID, mapItemDescs.Path);
		}
		catch
		{
			mapInfo.texture.mainTexture = null;
			mapInfo.name.text = "crap";
			mapInfo.size.text = "crap";
		}
		finally
		{
			mapInfo.texture.mainTexture = customGameData.screenshot;
			mapInfo.name.text = customGameData.name.ToString();
			mapInfo.size.text = customGameData.size.x + "X" + customGameData.size.z;
			PlayerDesc[] humanDescs = customGameData.humanDescs;
			if (humanDescs.Length > 0 && playerInfo.playerList != null)
			{
				playerInfo.playerList.items.Clear();
				PlayerDesc[] array = humanDescs;
				foreach (PlayerDesc playerDesc in array)
				{
					playerInfo.playerList.items.Add(playerDesc.Name);
				}
				playerInfo.playerList.selection = humanDescs[0].Name;
			}
			else
			{
				playerInfo.playerList.items.Clear();
				playerInfo.playerList.selection = " ";
			}
		}
	}

	private void OnWndMapItemDoubleClick(UICustomMapItem item)
	{
		if (item.IsFile)
		{
			mCurPath = mMapItems[item.index].Path;
			UpdateMapItem(mCurPath);
		}
	}

	private void OnWndPlayerSelectChanged(int playerIndex)
	{
		if (playerIndex != -1)
		{
			MapItemDescs mapItemDescs = mMapItems[selectWnd.selectedItem.index];
			CustomGameData customData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(mapItemDescs.UID, mapItemDescs.Path);
			if (!customData.DeterminePlayer(playerIndex))
			{
			}
		}
	}

	private bool OnWndStartClick()
	{
		if (mIsProcessCheck)
		{
			return true;
		}
		if (selectWnd.selectedItem != null && !selectWnd.selectedItem.IsFile)
		{
			MapItemDescs mapItemDescs = mMapItems[selectWnd.selectedItem.index];
			CustomGameData customData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(mapItemDescs.UID, mapItemDescs.Path);
			if (customData == null || customData.humanDescs.Length == 0)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000859));
				return true;
			}
			MapItemDescs mapItemDescs2 = mMapItems[selectWnd.selectedItem.index];
			ScenarioIntegrityCheck check = ScenarioMapUtils.CheckIntegrityByPath(mapItemDescs2.Path);
			StartCoroutine(ProcessIntegrityCheck(check));
			selectWnd.HintBox.Msg = "Checking";
			selectWnd.HintBox.isProcessing = true;
			selectWnd.HintBox.Open();
		}
		return true;
	}

	private void OnWndMapItemSetContent(int index, UICustomMapItem item)
	{
		try
		{
			item.IsFile = mMapItems[index].IsDir;
			item.nameStr = mMapItems[index].Name;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
		}
	}

	private void OnWndMapItemClearContent(UICustomMapItem item)
	{
	}

	private IEnumerator ProcessIntegrityCheck(ScenarioIntegrityCheck check)
	{
		mIsProcessCheck = true;
		while (true)
		{
			if (check.integrated == true)
			{
				try
				{
					PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
					PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Custom;
					PeGameMgr.gameName = selectWnd.selectedItem.nameStr;
					PeGameMgr.mapUID = mMapItems[selectWnd.selectedItem.index].UID;
				}
				catch
				{
					Debug.Log("This map is wrong, please chose anther one");
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
					selectWnd.HintBox.Close();
					break;
				}
				selectWnd.HintBox.Msg = "Correct";
				selectWnd.HintBox.isProcessing = false;
				yield return new WaitForSeconds(0.5f);
				PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.RoleScene);
				Debug.Log("Check Correct");
				break;
			}
			if (check.integrated == false)
			{
				if (!ignoreIntegrityCheck)
				{
					Debug.Log("This map is wrong, please chose anther one");
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
					selectWnd.HintBox.Close();
					break;
				}
				try
				{
					PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
					PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Custom;
					PeGameMgr.gameName = selectWnd.selectedItem.nameStr;
					PeGameMgr.mapUID = mMapItems[selectWnd.selectedItem.index].UID;
					PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.RoleScene);
				}
				catch
				{
					Debug.Log("This map is wrong, please chose anther one");
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
					selectWnd.HintBox.Close();
					break;
				}
			}
			yield return 0;
		}
		mIsProcessCheck = false;
	}

	private void UpdateMapItem(string dir)
	{
		string text = dir;
		if (text.Length > pathCharCount)
		{
			text = text.Substring(0, Mathf.Max(3, pathCharCount - 3));
			text += "...";
		}
		selectWnd.Path = text;
		mMapItems.Clear();
		GetMapItemDescs(mMapItems, dir);
		selectWnd.CreateMapItem(mMapItems.Count);
	}

	private void GetMapItemDescs(List<MapItemDescs> mapItemDesces, string dir)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(dir);
		if (!directoryInfo.Exists)
		{
			Debug.LogWarning("The dir[" + GameConfig.CustomDataDir + "] is not exsit");
			return;
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		if (directories == null || directories.Length == 0)
		{
			return;
		}
		mMapDescs.Clear();
		ScenarioMapDesc[] mapList = ScenarioMapUtils.GetMapList(dir);
		for (int i = 0; i < mapList.Length; i++)
		{
			mMapDescs.Add(mapList[i].Name, mapList[i]);
		}
		DirectoryInfo[] array = directories;
		foreach (DirectoryInfo directoryInfo2 in array)
		{
			MapItemDescs mapItemDescs = new MapItemDescs();
			if (mMapDescs.ContainsKey(directoryInfo2.Name))
			{
				ScenarioMapDesc scenarioMapDesc = mMapDescs[directoryInfo2.Name];
				mapItemDescs.IsDir = false;
				mapItemDescs.Name = scenarioMapDesc.Name;
				mapItemDescs.Path = scenarioMapDesc.Path;
				mapItemDescs.UID = scenarioMapDesc.UID;
			}
			else
			{
				mapItemDescs.IsDir = true;
				mapItemDescs.Name = directoryInfo2.Name;
				mapItemDescs.Path = directoryInfo2.FullName;
				mapItemDescs.UID = null;
			}
			mapItemDesces.Add(mapItemDescs);
		}
	}
}
