using System;
using System.Collections.Generic;
using uLink;
using uLobby;
using UnityEngine;

public class MessageHandlers
{
	public Dictionary<ELobbyMsgType, Action<uLink.BitStream, LobbyMessageInfo>> msgHandlers;

	public Action<uLink.BitStream, LobbyMessageInfo> this[ELobbyMsgType msgType] => msgHandlers[msgType];

	public MessageHandlers()
	{
		msgHandlers = new Dictionary<ELobbyMsgType, Action<uLink.BitStream, LobbyMessageInfo>>();
	}

	public bool CheckHandler(ELobbyMsgType msgType)
	{
		return msgHandlers.ContainsKey(msgType);
	}

	public void RegisterHandler(ELobbyMsgType msgType, Action<uLink.BitStream, LobbyMessageInfo> handler)
	{
		if (msgHandlers.ContainsKey(msgType))
		{
			if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Replace msg handler:{0}", msgType);
			}
			msgHandlers.Remove(msgType);
		}
		if (LogFilter.logDev)
		{
			Debug.LogWarningFormat("Register msg handler:{0}", msgType);
		}
		msgHandlers.Add(msgType, handler);
	}

	public void RegisterHandlerSafe(ELobbyMsgType msgType, Action<uLink.BitStream, LobbyMessageInfo> handler)
	{
		if (msgHandlers.ContainsKey(msgType))
		{
			if (LogFilter.logError)
			{
				Debug.LogWarningFormat("Duplicate msg handler:{0}", msgType);
			}
			return;
		}
		if (LogFilter.logDev)
		{
			Debug.LogWarningFormat("Register msg handler:{0}", msgType);
		}
		msgHandlers.Add(msgType, handler);
	}
}
