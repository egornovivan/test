using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using uLink;
using UnityEngine;

internal class SteamFileItem
{
	public int instanceId;

	private static int curinstanceId = 0;

	private string _RealFileName;

	private string _FileName;

	private string _PreFileName;

	private string _FileDescription;

	private int _FileSize;

	private ulong _hashCode;

	private string[] _Tags;

	private byte[] _PreData;

	internal byte[] _Data;

	private UGCHandle_t _UGCHandle;

	private PublishedFileId_t _PublishedFileId;

	public bool _SendToServer;

	private bool _Publish;

	private int _ID;

	public bool _free;

	private ExportStep _step;

	private CallResult<RemoteStorageFileShareResult_t> RemoteStoragePreFileShareResult;

	private CallResult<RemoteStorageFileShareResult_t> RemoteStorageFileShareResult;

	private CallResult<RemoteStoragePublishFileResult_t> RemoteStorageFilePublishResult;

	private SteamUploadEventHandler CallBackSteamUploadResult;

	private List<SteamAPICall_t> _handler = new List<SteamAPICall_t>();

	private static List<SteamFileItem> _mgr = new List<SteamFileItem>();

	public int ID => _ID;

	internal ulong FileID => _UGCHandle.m_UGCHandle;

	internal ulong PublishID => _PublishedFileId.m_PublishedFileId;

	internal ulong HashCode => _hashCode;

	internal string FileName => _FileName;

	internal string RealFileName => _RealFileName;

	private event EventHandler FileShareEvent;

	private event EventHandler FilePublishEvent;

	internal SteamFileItem()
	{
		RemoteStoragePreFileShareResult = CallResult<RemoteStorageFileShareResult_t>.Create(OnRemoteStoragePreFileShareResult);
		RemoteStorageFileShareResult = CallResult<RemoteStorageFileShareResult_t>.Create(OnRemoteStorageFileShareResult);
		RemoteStorageFilePublishResult = CallResult<RemoteStoragePublishFileResult_t>.Create(OnRemoteStorageFilePublishResult);
		this.FileShareEvent = (EventHandler)Delegate.Combine(this.FileShareEvent, new EventHandler(SteamWorkShop.OnFileSharedEvent));
		this.FilePublishEvent = (EventHandler)Delegate.Combine(this.FilePublishEvent, new EventHandler(SteamWorkShop.OnFilePublishEvent));
		_mgr.Add(this);
		curinstanceId++;
		instanceId = curinstanceId;
	}

	internal SteamFileItem(SteamUploadEventHandler callBackSteamUploadResult, string name, string desc, byte[] preData, byte[] data, ulong hashCode, string[] tags, bool publish = true, bool sendToServer = true, int id = -1, ulong fileId = 0, bool free = false)
		: this()
	{
		_ID = id;
		_RealFileName = name;
		_FileName = hashCode.ToString();
		_PreFileName = hashCode + "_preview";
		_Data = data;
		_FileDescription = desc;
		_hashCode = hashCode;
		_Tags = tags;
		_SendToServer = sendToServer;
		_Publish = publish;
		CallBackSteamUploadResult = callBackSteamUploadResult;
		byte[] bytes = Encoding.UTF8.GetBytes(SteamWorkShop._SteamPersonaName);
		int num = preData.Length + bytes.Length;
		_PreData = new byte[num];
		preData.CopyTo(_PreData, 0);
		bytes.CopyTo(_PreData, preData.Length);
		_UGCHandle = new UGCHandle_t(fileId);
		_free = free;
		_mgr.Add(this);
		curinstanceId++;
		instanceId = curinstanceId;
	}

	public void StartSend()
	{
		SteamPreFileWrite();
	}

	internal void SteamPreFileWrite()
	{
		try
		{
			if (_PreData != null && _PreData.Length > 0)
			{
				_step = ExportStep.PreFileSending;
				ShowMsg(_step);
				if (!IsShared(FileName))
				{
					if (IsWorking(FileName))
					{
						return;
					}
					if (!SteamRemoteStorage.FileWrite(_PreFileName, _PreData, _PreData.Length))
					{
						throw new Exception("File write failed. File Name: " + _PreFileName);
					}
				}
				else
				{
					OnFileShared();
				}
				SteamAPICall_t steamAPICall_t = SteamRemoteStorage.FileShare(_PreFileName);
				_handler.Add(steamAPICall_t);
				RemoteStoragePreFileShareResult.Set(steamAPICall_t);
				return;
			}
			throw new Exception("Invalid file data. File Name: " + _PreFileName);
		}
		catch (Exception)
		{
			if (CallBackSteamUploadResult != null)
			{
				CallBackSteamUploadResult(_ID, bOK: false, _hashCode);
			}
			_step = ExportStep.ExportFailed;
			ShowMsg(_step);
		}
	}

	internal void SteamFileWrite()
	{
		try
		{
			ShowMsg(ExportStep.RealFileSending);
			if (!SteamRemoteStorage.FileWrite(_FileName, _Data, _Data.Length))
			{
				throw new Exception("File write failed. File Name:" + _FileName);
			}
			SteamAPICall_t steamAPICall_t = default(SteamAPICall_t);
			_step = ExportStep.Shareing;
			ShowMsg(_step);
			steamAPICall_t = SteamRemoteStorage.FileShare(_FileName);
			_handler.Add(steamAPICall_t);
			RemoteStorageFileShareResult.Set(steamAPICall_t);
		}
		catch (Exception)
		{
			if (CallBackSteamUploadResult != null)
			{
				CallBackSteamUploadResult(_ID, bOK: false, _hashCode);
			}
			SteamRemoteStorage.FileDelete(_PreFileName);
			_step = ExportStep.ExportFailed;
			ShowMsg(_step);
		}
	}

	private void OnRemoteStoragePreFileShareResult(RemoteStorageFileShareResult_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult != EResult.k_EResultOK)
			{
				throw new Exception("Upload iso data failed.");
			}
			SteamFileWrite();
		}
		catch (Exception)
		{
			Debug.Log("workshop error:" + pCallback.m_eResult);
			if (CallBackSteamUploadResult != null)
			{
				CallBackSteamUploadResult(_ID, bOK: false, _hashCode);
			}
			_step = ExportStep.ExportFailed;
			ShowMsg(_step);
		}
	}

	private void OnRemoteStorageFileShareResult(RemoteStorageFileShareResult_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult != EResult.k_EResultOK)
			{
				if (CallBackSteamUploadResult != null)
				{
					CallBackSteamUploadResult(_ID, bOK: false, _hashCode);
				}
				_step = ExportStep.ExportFailed;
				ShowMsg(_step);
				return;
			}
			_UGCHandle = pCallback.m_hFile;
			OnFileShared();
			if (CallBackSteamUploadResult != null)
			{
				CallBackSteamUploadResult(_ID, bOK: true, _hashCode);
			}
			if (_Publish)
			{
				SteamAPICall_t steamAPICall_t = SteamRemoteStorage.PublishWorkshopFile(_FileName, _PreFileName, SteamUtils.GetAppID(), _RealFileName, _FileDescription, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic, _Tags, EWorkshopFileType.k_EWorkshopFileTypeFirst);
				_handler.Add(steamAPICall_t);
				RemoteStorageFilePublishResult.Set(steamAPICall_t);
			}
		}
		catch (Exception)
		{
			Debug.Log("workshop error:" + pCallback.m_eResult);
			if (CallBackSteamUploadResult != null)
			{
				CallBackSteamUploadResult(_ID, bOK: false, _hashCode);
			}
			_step = ExportStep.ExportFailed;
			ShowMsg(_step);
		}
	}

	private void OnFileShared()
	{
		foreach (SteamFileItem item in _mgr)
		{
			if (item._FileName == FileName && item._step < ExportStep.WaitingOtherPlayer)
			{
				item._UGCHandle = _UGCHandle;
				item._step = ExportStep.WaitingOtherPlayer;
				item.ShowMsg(item._step);
				if (item.FileShareEvent != null && item._SendToServer)
				{
					item.FileShareEvent(item, EventArgs.Empty);
				}
			}
		}
	}

	private void OnRemoteStorageFilePublishResult(RemoteStoragePublishFileResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			_PublishedFileId = pCallback.m_nPublishedFileId;
			LobbyInterface.LobbyRPC(ELobbyMsgType.UploadISOSuccess, _hashCode, SteamMgr.steamId.m_SteamID);
		}
		if (pCallback.m_eResult == EResult.k_EResultInsufficientPrivilege)
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000491));
		}
		OnFilePublished();
	}

	private void OnFilePublished()
	{
		if (this.FilePublishEvent != null)
		{
			this.FilePublishEvent(this, EventArgs.Empty);
		}
		_mgr.Remove(this);
	}

	public static void RPC_S2C_IsoExportSuccessed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		foreach (SteamFileItem item in _mgr)
		{
			if (item.instanceId == num)
			{
				if (flag)
				{
					IsoUploadInfoWndCtrl.Instance.UpdateIsoState(num, item.RealFileName, 5);
				}
				else
				{
					IsoUploadInfoWndCtrl.Instance.UpdateIsoState(num, item.RealFileName, 7);
				}
				break;
			}
		}
	}

	private bool IsShared(string filename)
	{
		foreach (SteamFileItem item in _mgr)
		{
			if (item._FileName == filename && item._step == ExportStep.ExportSuccessed)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWorking(string filename)
	{
		if (_step > ExportStep.PreFileSending)
		{
			return true;
		}
		return false;
	}

	private void SetUGCHandle(string filename)
	{
		foreach (SteamFileItem item in _mgr)
		{
			if (item._FileName == filename && item._step == ExportStep.ExportSuccessed)
			{
				_UGCHandle = item._UGCHandle;
			}
		}
	}

	private void ShowMsg(ExportStep step)
	{
		if (!_free)
		{
			IsoUploadInfoWndCtrl.Instance.UpdateIsoState(instanceId, RealFileName, (int)step);
		}
	}
}
