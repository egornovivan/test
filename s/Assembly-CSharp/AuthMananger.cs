using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

[DisallowMultipleComponent]
public class AuthMananger : MonoBehaviour
{
	private List<BanAccount> banList = new List<BanAccount>();

	private Dictionary<ulong, BanAccount> banDic = new Dictionary<ulong, BanAccount>();

	[RuntimeInitializeOnLoadMethod]
	private static void OnLoad()
	{
		GameObject gameObject = new GameObject("AuthManager");
		gameObject.AddComponent<AuthMananger>();
	}

	private void Start()
	{
		SkNetworkInterface.OnPEACCheckEventHandler += OnPEACCheck;
	}

	private void OnDestroy()
	{
		SkNetworkInterface.OnPEACCheckEventHandler -= OnPEACCheck;
	}

	private bool AddBanAccount(ulong steamId, long id)
	{
		long num = DateTime.Now.Ticks / 10000000;
		if (!banDic.ContainsKey(steamId))
		{
			BanAccount banAccount = default(BanAccount);
			banAccount.steamId = steamId;
			banAccount.cheatTimes = 1;
			banAccount.acTime = num + 20;
			banDic.Add(steamId, banAccount);
			banList.Add(banAccount);
			return true;
		}
		BanAccount value = banDic[steamId];
		if (num < value.acTime)
		{
			return false;
		}
		value.acTime = num + 20;
		value.cheatTimes++;
		banDic[steamId] = value;
		return true;
	}

	private void OnPEACCheck(SkNetworkInterface skNet, int casterId)
	{
		AttribType type = AttribType.Max;
		int curValue = 0;
		if (skNet is Player)
		{
			Player player = (Player)skNet;
			if (CheckCheater(player, ref type, ref curValue) && AddBanAccount(player.steamId, player.Id))
			{
				LobbyInterface.LobbyRPC(ELobbyMsgType.CheatingCheck, player.steamId, player.Id, player.roleName, (int)type, curValue);
				player.RPCOwner(EPacketType.PT_CheatingChecked);
				if (LogFilter.logWarn)
				{
					Debug.LogWarningFormat("Cheating:Name:{0}, Type{1}, Value:{2}", player.roleName, (int)type, curValue);
				}
			}
		}
		Player player2 = Player.GetPlayer(casterId);
		if (null != player2 && CheckCheater(player2, ref type, ref curValue) && AddBanAccount(player2.steamId, player2.Id))
		{
			LobbyInterface.LobbyRPC(ELobbyMsgType.CheatingCheck, player2.steamId, player2.Id, player2.roleName, (int)type, curValue);
			player2.RPCOwner(EPacketType.PT_CheatingChecked);
			if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Cheating:Name:{0}, Type{1}, Value:{2}", player2.roleName, (int)type, curValue);
			}
		}
	}

	private bool CheckCheater(Player cheater, ref AttribType type, ref int curValue)
	{
		if (null != cheater && null != cheater._skEntity)
		{
			curValue = (int)cheater._skEntity.GetAttribute(AttribType.Hp);
			type = AttribType.Hp;
			if (curValue >= 1500)
			{
				return true;
			}
			curValue = (int)cheater._skEntity.GetAttribute(AttribType.Def);
			type = AttribType.Def;
			if (curValue >= 1500)
			{
				return true;
			}
			curValue = (int)cheater._skEntity.GetAttribute(AttribType.Atk);
			type = AttribType.Atk;
			if (curValue >= 1500)
			{
				return true;
			}
		}
		return false;
	}
}
