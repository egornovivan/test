using System;
using uLink;
using uLobby;
using UnityEngine;

public class ServerRegistered
{
	internal long ServerUID;

	internal int ServerID;

	internal int LimitedConn;

	internal int CurConn;

	internal int ServerStatus;

	internal int GameType;

	internal int GameMode;

	internal int PasswordStatus;

	internal int Ping;

	internal int Port;

	internal string ServerVersion;

	internal string ServerName;

	internal string ServerMasterAccount;

	internal string ServerMasterName;

	internal string IPAddress;

	internal string UID;

	internal string MapName;

	internal bool UseProxy;

	internal bool IsLan;

	internal string QueryInformation => ServerID + ServerName.ToLower() + ServerMasterName.ToLower();

	public override bool Equals(object obj)
	{
		if (!(obj is ServerRegistered))
		{
			return false;
		}
		return ServerID.Equals(((ServerRegistered)obj).ServerID);
	}

	public override int GetHashCode()
	{
		return ServerID;
	}

	public override string ToString()
	{
		return $"name:{ServerName}, id:{ServerID}, mode:{GameMode}, type:{GameType}, master:{ServerMasterName} proxy:{UseProxy}";
	}

	public virtual void AnalyseServer(uLink.HostData data, bool isLan)
	{
		try
		{
			IPAddress = data.ipAddress;
			Port = data.port;
			LimitedConn = data.playerLimit;
			CurConn = data.connectedPlayers;
			Ping = data.ping;
			ServerName = data.gameName;
			UseProxy = data.useProxy;
			IsLan = isLan;
			string[] array = data.comment.Split(',');
			int.TryParse(array[0], out ServerStatus);
			int.TryParse(array[1], out GameMode);
			int.TryParse(array[2], out GameType);
			ServerMasterName = MyServer.RecoverStr(array[3]);
			ServerVersion = array[4];
			int.TryParse(array[5], out PasswordStatus);
			int.TryParse(array[6], out ServerID);
			long.TryParse(array[7], out ServerUID);
			if (GameMode == 4)
			{
				UID = array[8];
				MapName = array[9];
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogWarningFormat("{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
	}

	public void AnalyseServer(ServerInfo data)
	{
		try
		{
			IPAddress = data.host;
			Port = data.port;
			uLink.BitStream remainingBitStream = data.data.GetRemainingBitStream();
			LimitedConn = remainingBitStream.Read<int>(new object[0]);
			CurConn = remainingBitStream.Read<int>(new object[0]);
			ServerStatus = remainingBitStream.Read<int>(new object[0]);
			GameMode = remainingBitStream.Read<int>(new object[0]);
			GameType = remainingBitStream.Read<int>(new object[0]);
			ServerName = remainingBitStream.Read<string>(new object[0]);
			ServerMasterName = remainingBitStream.Read<string>(new object[0]);
			ServerVersion = remainingBitStream.Read<string>(new object[0]);
			PasswordStatus = remainingBitStream.Read<int>(new object[0]);
			ServerID = remainingBitStream.Read<int>(new object[0]);
			ServerUID = remainingBitStream.Read<long>(new object[0]);
			if (GameMode == 4)
			{
				UID = remainingBitStream.Read<string>(new object[0]);
				MapName = remainingBitStream.Read<string>(new object[0]);
			}
			UseProxy = false;
			IsLan = false;
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogWarningFormat("{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
	}
}
