using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CustomData;
using ItemAsset;
using Pathea;
using Steamworks;
using uLink;
using UnityEngine;

public class SteamWorkShop : UnityEngine.MonoBehaviour
{
	private static SteamWorkShop _instance;

	private bool _steamInit;

	internal static List<string> Tags = new List<string> { "Item", "Weapon" };

	private static Dictionary<ulong, string> _SteamItems = new Dictionary<ulong, string>();

	private static List<RegisteredISO> _DownloadList = new List<RegisteredISO>();

	private static List<ulong> _CurDownloadList = new List<ulong>();

	private static ulong _CurDownload;

	public static List<CreationOriginData> CreationList = new List<CreationOriginData>();

	private static CallResult<RemoteStorageDownloadUGCResult_t> RemoteStorageDownloadPreUGCResult;

	public static string NewVersionTag = "2.3";

	private static Dictionary<ulong, SendIsoCache> SendIsoCacheMgr = new Dictionary<ulong, SendIsoCache>();

	public static Dictionary<string, FileSender> _FileSenderMgr = new Dictionary<string, FileSender>();

	internal static SteamWorkShop Instance => _instance;

	public static string _SteamPersonaName { get; set; }

	public static bool AddToIsoCache(SendIsoCache iso)
	{
		if (SendIsoCacheMgr.ContainsKey(iso.hash))
		{
			return false;
		}
		SendIsoCacheMgr.Add(iso.hash, iso);
		return true;
	}

	public static void RemoveIsoCache(SendIsoCache iso)
	{
		SendIsoCacheMgr.Remove(iso.hash);
	}

	public static void SendCacheIso(ulong hash)
	{
		if (SendIsoCacheMgr.ContainsKey(hash))
		{
			SendIsoCache sendIsoCache = SendIsoCacheMgr[hash];
			SteamFileItem steamFileItem = new SteamFileItem(sendIsoCache.callBackSteamUploadResult, sendIsoCache.name, sendIsoCache.desc, sendIsoCache.preData, sendIsoCache.data, sendIsoCache.hash, sendIsoCache.tags, sendIsoCache.bPublish, sendIsoCache.sendToServer, sendIsoCache.id, 0uL, free: true);
			steamFileItem.StartSend();
			RemoveIsoCache(sendIsoCache);
		}
	}

	public static SendIsoCache GetCacheIso(ulong hash)
	{
		if (SendIsoCacheMgr.ContainsKey(hash))
		{
			return SendIsoCacheMgr[hash];
		}
		return null;
	}

	public static string[] AddNewVersionTag(string[] tags)
	{
		for (int i = 0; i < tags.Length; i++)
		{
			if (tags[i] == NewVersionTag)
			{
				return tags;
			}
		}
		List<string> list = new List<string>();
		if (tags.Length > 0)
		{
			list.AddRange(tags);
		}
		list.Add(NewVersionTag);
		return list.ToArray();
	}

	public static CreationData GetCreation(int objId)
	{
		CreationOriginData creationOriginData = CreationList.Find((CreationOriginData iter) => iter.ObjectID == objId);
		if (creationOriginData != null)
		{
			PeSingleton<ItemProto.Mgr>.Instance.Remove(objId);
			CreationMgr.NewCreation(creationOriginData.ObjectID, creationOriginData.HashCode, creationOriginData.Seed);
			return CreationMgr.GetCreation(objId);
		}
		return null;
	}

	public static string GetCreationPath(int objId)
	{
		CreationOriginData creationOriginData = CreationList.Find((CreationOriginData iter) => iter.ObjectID == objId);
		if (creationOriginData != null)
		{
			string text = creationOriginData.HashCode.ToString("X").PadLeft(16, '0');
			string text2 = VCConfig.s_CreationPath + text + VCConfig.s_CreationFileExt;
			string text3 = VCConfig.s_CreationNetCachePath + text + VCConfig.s_CreationNetCacheFileExt;
			string text4 = string.Empty;
			if (File.Exists(text2))
			{
				text4 = text2;
			}
			else if (File.Exists(text3))
			{
				text4 = text3;
			}
			if (text4.Length == 0)
			{
				return null;
			}
			return text4;
		}
		return null;
	}

	public static VCIsoHeadData GetCreateionHead(int objId)
	{
		string creationPath = GetCreationPath(objId);
		VCIsoData.ExtractHeader(creationPath, out var iso_header);
		if (PeSingleton<ItemProto.Mgr>.Instance.Get(objId) != null)
		{
			return iso_header;
		}
		ItemProto itemProto = GenItemData(iso_header, objId);
		if (itemProto != null)
		{
			PeSingleton<ItemProto.Mgr>.Instance.Add(itemProto);
			byte[] iconTex = iso_header.IconTex;
			if (iconTex != null)
			{
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(iconTex);
				texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				itemProto.iconTex = texture2D;
			}
		}
		return iso_header;
	}

	public static CreationAttr GetCreationAttr(string xml)
	{
		CreationAttr creationAttr = new CreationAttr();
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			XmlNode xmlNode = xmlDocument.DocumentElement["ATTR"];
			XmlNode xmlNode2 = xmlNode["COMMON"];
			XmlNode xmlNode3 = xmlNode["PROP"];
			creationAttr.m_Type = (ECreation)XmlConvert.ToInt32(xmlNode2.Attributes["type"].Value);
			creationAttr.m_Volume = XmlConvert.ToSingle(xmlNode2.Attributes["vol"].Value);
			creationAttr.m_Weight = XmlConvert.ToSingle(xmlNode2.Attributes["weight"].Value);
			creationAttr.m_Durability = XmlConvert.ToSingle(xmlNode2.Attributes["dur"].Value);
			creationAttr.m_SellPrice = XmlConvert.ToSingle(xmlNode2.Attributes["sp"].Value);
			creationAttr.m_Attack = XmlConvert.ToSingle(xmlNode3.Attributes["atk"].Value);
			creationAttr.m_Defense = XmlConvert.ToSingle(xmlNode3.Attributes["def"].Value);
			creationAttr.m_MuzzleAtkInc = XmlConvert.ToSingle(xmlNode3.Attributes["inc"].Value);
			creationAttr.m_FireSpeed = XmlConvert.ToSingle(xmlNode3.Attributes["fs"].Value);
			creationAttr.m_Accuracy = XmlConvert.ToSingle(xmlNode3.Attributes["acc"].Value);
			creationAttr.m_DragCoef = XmlConvert.ToSingle(xmlNode3.Attributes["dc"].Value);
			creationAttr.m_MaxFuel = XmlConvert.ToSingle(xmlNode3.Attributes["fuel"].Value);
			foreach (XmlNode childNode in xmlNode2.ChildNodes)
			{
				int key = XmlConvert.ToInt32(childNode.Attributes["id"].Value);
				int value = XmlConvert.ToInt32(childNode.Attributes["cnt"].Value);
				creationAttr.m_Cost.Add(key, value);
			}
			return creationAttr;
		}
		catch (Exception ex)
		{
			LogManager.Warning(ex);
			creationAttr.m_Errors.Add("error");
			return creationAttr;
		}
	}

	private static ItemProto GenItemData(VCIsoHeadData headData, int objId)
	{
		CreationAttr creationAttr = GetCreationAttr(headData.Remarks);
		creationAttr.CheckCostId();
		return CreationData.StaticGenItemData(objId, headData, creationAttr);
	}

	public static void AddCreation(CreationOriginData data)
	{
		if (!CreationList.Contains(data) && data != null)
		{
			CreationList.Add(data);
		}
	}

	public static ulong GetFileHandle(ulong hashCode)
	{
		foreach (RegisteredISO download in _DownloadList)
		{
			if (download._hashCode == hashCode)
			{
				return download.UGCHandle;
			}
		}
		return 0uL;
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Update()
	{
		if (GameConfig.IsMultiMode)
		{
			CreationMgr.CheckForDel();
		}
	}

	private void Start()
	{
		LoadSteamItems();
		CreationMgr.Init();
		_CurDownload = 0uL;
		_DownloadList.Clear();
		StartCoroutine(DownLoadCoroutine());
		_SteamPersonaName = "$sinwa$" + SteamFriends.GetPersonaName();
		SteamFriends.SetListenForFriendsMessages(bInterceptEnabled: true);
	}

	public void OnDisconnectEvent(object sender, EventArgs e)
	{
		_CurDownload = 0uL;
		_DownloadList.Clear();
		_FileSenderMgr.Clear();
	}

	internal static void OnFileSharedEvent(object sender, EventArgs args)
	{
		SteamFileItem steamFileItem = (SteamFileItem)sender;
		if (!steamFileItem._SendToServer)
		{
			return;
		}
		if (null != PlayerNetwork.mainPlayer)
		{
			VCIsoData vCIsoData = new VCIsoData();
			vCIsoData.Import(steamFileItem._Data, new VCIsoOption(editor: false));
			IEnumerable<int> source = from component in vCIsoData.m_Components
				where VCUtils.IsSeat(component.m_Type)
				select (int)component.m_Type;
			if (source.Count() > 0)
			{
				NetworkManager.SyncServer(EPacketType.PT_Common_WorkshopShared, steamFileItem.FileID, steamFileItem.RealFileName, steamFileItem.HashCode, steamFileItem._free, steamFileItem.instanceId, true, source.ToArray());
			}
			else
			{
				NetworkManager.SyncServer(EPacketType.PT_Common_WorkshopShared, steamFileItem.FileID, steamFileItem.RealFileName, steamFileItem.HashCode, steamFileItem._free, steamFileItem.instanceId, false);
			}
		}
		string path = VCConfig.s_CreationNetCachePath + steamFileItem.HashCode.ToString("X").PadLeft(16, '0') + VCConfig.s_CreationNetCacheFileExt;
		using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			fileStream.Write(steamFileItem._Data, 0, steamFileItem._Data.Length);
		}
		_SteamItems[steamFileItem.HashCode] = steamFileItem.FileName;
	}

	internal static void OnFilePublishEvent(object sender, EventArgs args)
	{
		SteamFileItem steamFileItem = (SteamFileItem)sender;
		if (steamFileItem != null)
		{
			UIWorkShopCtrl.PublishFinishCellBack(steamFileItem.ID, steamFileItem.PublishID, steamFileItem.HashCode);
		}
	}

	internal static void LoadSteamItems()
	{
		_SteamItems.Clear();
		string[] files = Directory.GetFiles(VCConfig.s_CreationNetCachePath, "*" + VCConfig.s_CreationNetCacheFileExt);
		string[] array = files;
		foreach (string fileName in array)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			Stream stream = fileInfo.OpenRead();
			ulong key = CRC64.Compute(stream);
			string[] array2 = fileInfo.Name.Split('.');
			_SteamItems[key] = array2[0];
		}
	}

	internal static void SendFile(SteamUploadEventHandler callBackSteamUploadResult, string name, string desc, byte[] preData, byte[] data, string[] tags, bool sendToServer = true, int id = -1, ulong fileId = 0, bool free = false)
	{
		ulong num = CRC64.Compute(data);
		bool flag = false;
		try
		{
			if (string.IsNullOrEmpty(name))
			{
				VCEMsgBox.Show(VCEMsgBoxType.EXPORT_EMPTY_NAME);
				LogManager.Error("File name cannot be null.");
				return;
			}
			if (!SteamUser.BLoggedOn())
			{
				LogManager.Error("log back in steam...");
				return;
			}
			bool flag2 = !sendToServer;
			if (SteamRemoteStorage.FileExists(num + "_preview"))
			{
			}
			if (!SteamRemoteStorage.IsCloudEnabledForAccount())
			{
				throw new Exception("Account cloud disabled.");
			}
			if (!SteamRemoteStorage.IsCloudEnabledForApp())
			{
				throw new Exception("App cloud disabled.");
			}
			if (!flag2)
			{
				SteamFileItem steamFileItem = new SteamFileItem(callBackSteamUploadResult, name, desc, preData, data, num, tags, flag2, sendToServer, id, fileId, free);
				steamFileItem.StartSend();
			}
			else
			{
				SendIsoCache sendIsoCache = new SendIsoCache();
				sendIsoCache.id = id;
				sendIsoCache.hash = num;
				sendIsoCache.name = name;
				sendIsoCache.preData = preData;
				sendIsoCache.sendToServer = sendToServer;
				sendIsoCache.tags = tags;
				sendIsoCache.data = data;
				sendIsoCache.desc = desc;
				sendIsoCache.callBackSteamUploadResult = callBackSteamUploadResult;
				sendIsoCache.bPublish = flag2;
				if (!AddToIsoCache(sendIsoCache))
				{
					return;
				}
				LobbyInterface.LobbyRPC(ELobbyMsgType.UploadISO, num, SteamMgr.steamId.m_SteamID);
			}
			VCEMsgBox.Show(VCEMsgBoxType.EXPORT_NETWORK);
			flag = true;
		}
		catch (Exception ex)
		{
			VCEMsgBox.Show(VCEMsgBoxType.EXPORT_NETWORK_FAILED);
			Debug.LogWarning("workshop error :" + ex.Message);
			ToolTipsMgr.ShowText(ex.Message);
		}
		finally
		{
			if (!flag)
			{
				callBackSteamUploadResult?.Invoke(id, bOK: false, num);
			}
		}
	}

	private IEnumerator DownLoadCoroutine()
	{
		int lastDownloadBytes = 0;
		while (true)
		{
			if (_DownloadList.Count >= 1 && NetworkInterface.IsClient)
			{
				string isoName = _DownloadList[0]._isoName;
				if (_CurDownload == 0L)
				{
					_CurDownload = _DownloadList[0].UGCHandle;
					DownloadUGC();
					lastDownloadBytes = 0;
				}
				else
				{
					int downloadedBytes = 0;
					int totalBytes = 0;
					if (SteamRemoteStorage.GetUGCDownloadProgress(new UGCHandle_t(_CurDownload), out downloadedBytes, out totalBytes))
					{
						if (totalBytes != 0 && downloadedBytes != lastDownloadBytes)
						{
							int speed = downloadedBytes - lastDownloadBytes;
							lastDownloadBytes = downloadedBytes;
							RoomGui_N.UpdateDownLoadInfo(speed: speed / 1024, leftLoad: _DownloadList.Count);
							RoomGui_N.SetMapInfo("Downloading " + isoName + "...[" + downloadedBytes * 100 / totalBytes + "%]");
						}
					}
					else
					{
						foreach (KeyValuePair<string, FileSender> iter in _FileSenderMgr)
						{
							if (iter.Value.m_FileHandle == _CurDownload && iter.Value.m_FileSize > 0)
							{
								int speed3 = iter.Value.m_Sended - lastDownloadBytes;
								lastDownloadBytes = iter.Value.m_Sended;
								speed3 /= 1024;
								totalBytes = iter.Value.m_FileSize;
								RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, speed3);
								RoomGui_N.SetMapInfo("Downloading " + isoName + "...[" + downloadedBytes * 100 / totalBytes + "%]");
								break;
							}
						}
					}
				}
			}
			yield return new WaitForSeconds(0.02f);
		}
	}

	internal static void GetFiles()
	{
		int fileCount = SteamRemoteStorage.GetFileCount();
		Debug.Log("File count : " + fileCount);
		for (int i = 0; i < fileCount; i++)
		{
			SteamRemoteStorage.GetFileNameAndSize(i, out var _);
		}
	}

	internal static void DeleteFiles()
	{
		while (SteamRemoteStorage.GetFileCount() > 0)
		{
			int pnFileSizeInBytes;
			string fileNameAndSize = SteamRemoteStorage.GetFileNameAndSize(0, out pnFileSizeInBytes);
			SteamRemoteStorage.FileDelete(fileNameAndSize);
		}
	}

	internal static void DownloadUGC()
	{
		try
		{
			RegisteredISO registeredISO = _DownloadList.Find((RegisteredISO iter) => iter.UGCHandle == _CurDownload);
			if (registeredISO == null)
			{
				_DownloadList.RemoveAll((RegisteredISO iter) => iter.UGCHandle == _CurDownload);
				_CurDownload = 0uL;
				return;
			}
			if (!_SteamItems.ContainsKey(registeredISO._hashCode))
			{
				UGCHandle_t hContent = default(UGCHandle_t);
				hContent.m_UGCHandle = _CurDownload;
				SteamAPICall_t hAPICall = SteamRemoteStorage.UGCDownload(hContent, 0u);
				RemoteStorageDownloadPreUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnSteamUGCDownloadResult);
				RemoteStorageDownloadPreUGCResult.Set(hAPICall);
				RoomGui_N.SetMapInfo("Downloading " + registeredISO._isoName + "...[0%]");
			}
			else
			{
				_DownloadList.RemoveAll((RegisteredISO iter) => iter.UGCHandle == _CurDownload);
				NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, _CurDownload);
				_CurDownload = 0uL;
				if (_DownloadList.Count <= 0)
				{
					RoomGui_N.SetMapInfo("Download complete");
					if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
					{
						RoomGui_N.Instance.ActiveStartBtn();
					}
				}
				else
				{
					RoomGui_N.SetMapInfo("Downloading " + registeredISO._isoName + "...[100%]");
				}
			}
			RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
		}
		catch (Exception ex)
		{
			LogManager.Warning(ex.Message);
		}
	}

	private static void OnSteamUGCDownloadResult(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure)
	{
		Debug.Log(string.Concat("[SteamUGCDownloadResult] -- ", pCallback.m_eResult, " -- Name:", pCallback.m_pchFileName, " -- Size:", pCallback.m_nSizeInBytes));
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			RegisteredISO registeredISO = _DownloadList.Find((RegisteredISO iter) => iter.UGCHandle == pCallback.m_hFile.m_UGCHandle);
			if (registeredISO != null)
			{
				string path = VCConfig.s_CreationNetCachePath + registeredISO._hashCode.ToString("X").PadLeft(16, '0') + VCConfig.s_CreationNetCacheFileExt;
				byte[] array = new byte[pCallback.m_nSizeInBytes];
				int count = SteamRemoteStorage.UGCRead(pCallback.m_hFile, array, pCallback.m_nSizeInBytes, 0u, EUGCReadAction.k_EUGCRead_Close);
				using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					fileStream.Write(array, 0, count);
				}
				_DownloadList.RemoveAll((RegisteredISO iter) => iter.UGCHandle == pCallback.m_hFile.m_UGCHandle);
				_SteamItems[registeredISO._hashCode] = pCallback.m_pchFileName;
			}
			if (BaseNetwork.MainPlayer.NetworkState == ENetworkState.Null)
			{
				NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, pCallback.m_hFile.m_UGCHandle);
			}
			else
			{
				for (int i = 0; i < _CurDownloadList.Count; i++)
				{
					if (_CurDownloadList[i] == pCallback.m_hFile.m_UGCHandle)
					{
						NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, pCallback.m_hFile.m_UGCHandle);
						_CurDownloadList[i] = 0uL;
					}
				}
			}
			_CurDownload = 0uL;
			if (_DownloadList.Count <= 0)
			{
				RoomGui_N.SetMapInfo("Download complete");
				if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
				{
					RoomGui_N.Instance.ActiveStartBtn();
				}
			}
			else
			{
				RoomGui_N.SetMapInfo("Downloading " + pCallback.m_pchFileName + "...[100%]");
			}
			RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
		}
		else if (pCallback.m_eResult == EResult.k_EResultFileNotFound)
		{
			Debug.LogWarning(pCallback.m_hFile.m_UGCHandle + " does not exits.");
			NetworkManager.SyncServer(EPacketType.PT_Common_InvalidUGC, _DownloadList[0]._hashCode, pCallback.m_hFile.m_UGCHandle);
		}
		else if (pCallback.m_eResult == EResult.k_EResultTimeout)
		{
			NetworkManager.SyncServer(EPacketType.PT_Common_DownTimeOut, _DownloadList[0]._hashCode, pCallback.m_hFile.m_UGCHandle);
		}
		else
		{
			RoomGui_N.SetMapInfo("Download failed");
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000116), OnYesEvent, OnNoEvent);
		}
	}

	private static void OnYesEvent()
	{
		_CurDownload = 0uL;
	}

	private static void OnNoEvent()
	{
		_CurDownload = 0uL;
		_DownloadList.Clear();
		PeSceneCtrl.Instance.GotoLobbyScene();
	}

	public static void RPC_S2C_ResponseUGC(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num <= 0)
		{
			if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
			{
				RoomGui_N.Instance.ActiveStartBtn();
			}
			return;
		}
		RegisteredISO[] array = stream.Read<RegisteredISO[]>(new object[0]);
		RegisteredISO[] array2 = array;
		foreach (RegisteredISO registeredISO in array2)
		{
			if (_SteamItems.ContainsKey(registeredISO._hashCode))
			{
				_SteamItems[registeredISO._hashCode] = registeredISO._isoName;
			}
		}
		RoomGui_N.SetDownLoadInfo(array.Length);
		_DownloadList.Clear();
		_CurDownloadList.Clear();
		_DownloadList.AddRange(array);
	}

	public static void RPC_S2C_WorkShopShared(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RegisteredISO registeredISO = stream.Read<RegisteredISO>(new object[0]);
		if (_SteamItems.ContainsKey(registeredISO._hashCode))
		{
			_SteamItems[registeredISO._hashCode] = registeredISO._isoName;
		}
		_DownloadList.Add(registeredISO);
		_CurDownloadList.Add(registeredISO.UGCHandle);
	}

	public static void RPC_S2C_UGCGenerateList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CreationOriginData[] array = stream.Read<CreationOriginData[]>(new object[0]);
		CreationOriginData[] array2 = array;
		foreach (CreationOriginData data in array2)
		{
			AddCreation(data);
		}
	}

	public static void RPC_S2C_RequestUGCData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = array;
		int id;
		for (int i = 0; i < array2.Length; i++)
		{
			id = array2[i];
			CreationData creation = CreationMgr.GetCreation(id);
			if (creation == null)
			{
				CreationOriginData creationOriginData = CreationList.Find((CreationOriginData iter) => iter.ObjectID == id);
				if (creationOriginData != null)
				{
					CreationMgr.NewCreation(creationOriginData.ObjectID, creationOriginData.HashCode, creationOriginData.Seed);
					break;
				}
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000500));
				LogManager.Error("Creation item create failed. ID:" + id);
				GameClientNetwork.Disconnect();
				break;
			}
			ItemProto itemData = ItemProto.GetItemData(id);
			if (itemData != null)
			{
				byte[] buffer = ItemProto.GetBuffer(itemData);
				NetworkManager.SyncServer(EPacketType.PT_Common_UGCItem, id, buffer);
				IEnumerable<int> source = from component in creation.m_IsoData.m_Components
					where VCUtils.IsSeat(component.m_Type)
					select (int)component.m_Type;
				float durability = creation.m_Attribute.m_Durability;
				float maxFuel = creation.m_Attribute.m_MaxFuel;
				if (source.Count() >= 1)
				{
					NetworkManager.SyncServer(EPacketType.PT_Common_UGCData, id, durability, maxFuel, creation.m_Attribute.m_Cost.Keys.ToArray(), creation.m_Attribute.m_Cost.Values.ToArray(), true, source.ToArray());
				}
				else
				{
					NetworkManager.SyncServer(EPacketType.PT_Common_UGCData, id, durability, maxFuel, creation.m_Attribute.m_Cost.Keys.ToArray(), creation.m_Attribute.m_Cost.Values.ToArray(), false);
				}
			}
		}
	}

	public static void RPC_S2C_DownTimeOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<ulong>(new object[0]);
		RoomGui_N.SetMapInfo("Download failed");
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000116), OnYesEvent, OnNoEvent);
	}

	public static void RPC_S2C_FileDontExists(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ulong fileHandle = stream.Read<ulong>(new object[0]);
		_DownloadList.RemoveAll((RegisteredISO iter) => iter.UGCHandle == fileHandle);
		_CurDownload = 0uL;
		if (_DownloadList.Count <= 0)
		{
			RoomGui_N.SetMapInfo("Download complete");
			if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
			{
				RoomGui_N.Instance.ActiveStartBtn();
			}
		}
		RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
	}

	public static void RPC_S2C_SendFile(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		string skey = stream.Read<string>(new object[0]);
		if (array == null || num == 0 || !_FileSenderMgr.ContainsKey(skey) || _FileSenderMgr[skey] == null)
		{
			return;
		}
		bool bFinish = false;
		_FileSenderMgr[skey].WriteData(array, num, ref bFinish);
		if (bFinish)
		{
			RegisteredISO registeredISO = _DownloadList.Find((RegisteredISO iter) => iter.UGCHandle == _FileSenderMgr[skey].m_FileHandle);
			if (registeredISO != null)
			{
				string path = VCConfig.s_CreationNetCachePath + _FileSenderMgr[skey].m_FileName + VCConfig.s_CreationNetCacheFileExt;
				using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					fileStream.Write(_FileSenderMgr[skey].m_Data, 0, _FileSenderMgr[skey].m_FileSize);
				}
				_DownloadList.RemoveAll((RegisteredISO iter) => iter.UGCHandle == _FileSenderMgr[skey].m_FileHandle);
				Debug.Log(_DownloadList.Count() + "===========" + _FileSenderMgr[skey].m_FileName);
				_SteamItems[registeredISO._hashCode] = _FileSenderMgr[skey].m_FileName;
			}
			NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, _FileSenderMgr[skey].m_FileHandle);
			_CurDownload = 0uL;
			_FileSenderMgr.Remove(skey);
			if (_DownloadList.Count <= 0)
			{
				RoomGui_N.SetMapInfo("Download complete");
				if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
				{
					RoomGui_N.Instance.ActiveStartBtn();
				}
			}
			else
			{
				RoomGui_N.SetMapInfo("Downloading " + _FileSenderMgr[skey].m_FileName + "...[100%]");
			}
			RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
		}
		else
		{
			NetworkManager.SyncServer(EPacketType.PT_Common_UGCUpload, skey, num, true);
		}
	}

	public static void RPC_S2C_StartSendFile(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string fileName = stream.Read<string>(new object[0]);
		string skey = stream.Read<string>(new object[0]);
		int fileSize = stream.Read<int>(new object[0]);
		ulong fileHandle = stream.Read<ulong>(new object[0]);
		new FileSender(fileName, _FileSenderMgr, fileHandle, fileSize, skey);
	}

	public void GetRandIsos(int dungeonId, int amount, string tag)
	{
		DungeonIsos.AddItem(amount, dungeonId);
		SteamProcessMgr.Instance.RandomGetIsosFromWorkShop(GetRandomIsoListCallBackEventHandler, amount, dungeonId, tag);
	}

	private void GetRandomIsoListCallBackEventHandler(List<ulong> fileIDsList, List<ulong> publishIds, int dungeonId, bool bOK)
	{
		if (!bOK)
		{
			return;
		}
		if (PeGameMgr.IsSingle)
		{
			if (DungeonIsos.AddIsos(dungeonId, fileIDsList.ToArray(), publishIds.ToArray()))
			{
				for (int i = 0; i < fileIDsList.Count; i++)
				{
					SteamProcessMgr.Instance.GetPrimaryFile(Instance.GetPrimaryFileResultEventHandler, new UGCHandle_t(fileIDsList[i]), new PublishedFileId_t(publishIds[i]), i, dungeonId);
				}
			}
		}
		else
		{
			NetworkManager.SyncServer(EPacketType.PT_Common_SendRandIsoFileIds, dungeonId, fileIDsList.ToArray(), publishIds.ToArray());
		}
	}

	public static void RPC_S2C_GetRandIsoFileIds(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int dungeonId = stream.Read<int>(new object[0]);
		int amount = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		Instance.GetRandIsos(dungeonId, amount, text);
	}

	public static void RPC_S2C_SendRandIsoFileIds(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int dungeonId = stream.Read<int>(new object[0]);
		ulong[] array = stream.Read<ulong[]>(new object[0]);
		ulong[] array2 = stream.Read<ulong[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			SteamProcessMgr.Instance.GetPrimaryFile(Instance.GetPrimaryFileResultEventHandler, new UGCHandle_t(array[i]), new PublishedFileId_t(array2[i]), i, dungeonId);
		}
		DungeonIsos.AddIsos(dungeonId, array, array2);
	}

	public static void RPC_S2C_ExportRandIso(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int dungeonId = stream.Read<int>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		string fullpath = stream.Read<string>(new object[0]);
		ExportRandIso(fullpath, dungeonId, index);
	}

	public void GetPrimaryFileResultEventHandler(byte[] fileData, PublishedFileId_t publishedFileId, bool bOK, int index, int dungeonId)
	{
		string text = UIWorkShopCtrl.DownloadFileCallBack(fileData, publishedFileId, bOK);
		if (text.Length > 0)
		{
			if (PeGameMgr.IsSingle)
			{
				ExportRandIso(text, dungeonId, index);
				return;
			}
			NetworkManager.SyncServer(EPacketType.PT_Common_ExportRandIso, dungeonId, index, text);
		}
	}

	public static void ExportRandIso(string fullpath, int dungeonId, int index)
	{
		if (fullpath == string.Empty || !File.Exists(fullpath))
		{
			return;
		}
		ulong fileId = DungeonIsos.GetFileId(dungeonId, index);
		VCIsoData vCIsoData = new VCIsoData();
		using FileStream fileStream = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
		byte[] array = new byte[(int)fileStream.Length];
		fileStream.Read(array, 0, (int)fileStream.Length);
		fileStream.Close();
		vCIsoData.Import(array, new VCIsoOption(editor: true));
		if (PeGameMgr.IsMulti)
		{
			ulong num = CRC64.Compute(array);
			VCGameMediator.SendIsoDataToServer(vCIsoData.m_HeadInfo.Name, vCIsoData.m_HeadInfo.SteamDesc, vCIsoData.m_HeadInfo.SteamPreview, array, AddNewVersionTag(vCIsoData.m_HeadInfo.ScenePaths()), sendToServer: true, fileId, free: true);
			NetworkManager.SyncServer(EPacketType.PT_Common_SendRandIsoHash, dungeonId, index, num);
			return;
		}
		CreationData creationData = new CreationData();
		creationData.m_ObjectID = CreationMgr.QueryNewId();
		creationData.m_RandomSeed = UnityEngine.Random.value;
		creationData.m_Resource = array;
		creationData.ReadRes();
		ulong isoCode = CRC64.Compute(array);
		creationData.GenCreationAttr();
		if (creationData.m_Attribute.m_Type == ECreation.Null)
		{
			Debug.LogWarning("Creation is not a valid type !");
			creationData.Destroy();
		}
		else if (creationData.SaveRes())
		{
			creationData.BuildPrefab();
			creationData.Register();
			CreationMgr.AddCreation(creationData);
			creationData.SendToPlayer(out var item, pay: false);
			Debug.Log("Make creation succeed !");
			RandomDungenMgr.Instance.ReceiveIsoObj(dungeonId, isoCode, item.instanceId);
		}
		else
		{
			Debug.LogWarning("Save creation resource file failed !");
			creationData.Destroy();
		}
	}
}
