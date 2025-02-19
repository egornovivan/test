using System;
using uLink;
using uLobby;
using UnityEngine;

public class LobbyInterface : UnityEngine.MonoBehaviour
{
	protected MessageHandlers msgHandlers;

	public void SetHandlers(MessageHandlers handlers)
	{
		msgHandlers = handlers;
	}

	public bool CheckHandler(ELobbyMsgType msgType)
	{
		return msgHandlers != null && msgHandlers.CheckHandler(msgType);
	}

	public static void LobbyRPC(params object[] obj)
	{
		try
		{
			Lobby.RPC("RPC_LobbyMsg", Lobby.lobby, obj);
		}
		catch (Exception ex)
		{
			if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
	}

	[RPC]
	protected void RPC_LobbyMsg(uLink.BitStream stream, LobbyMessageInfo info)
	{
		ELobbyMsgType eLobbyMsgType = ELobbyMsgType.Max;
		try
		{
			eLobbyMsgType = stream.Read<ELobbyMsgType>(new object[0]);
			if (CheckHandler(eLobbyMsgType))
			{
				msgHandlers[eLobbyMsgType](stream, info);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Message:[{0}] does not implement", eLobbyMsgType);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Message:[{0}]\r\n{1}\r\n{2}", eLobbyMsgType, ex.Message, ex.StackTrace);
			}
		}
	}
}
