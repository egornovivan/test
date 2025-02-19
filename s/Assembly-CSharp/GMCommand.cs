using System;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;
using Weather;

public class GMCommand
{
	private delegate void CMDEventHandler(Player player, string str);

	private static GMCommand _instance = new GMCommand();

	private List<ForbidInfo> _forbidList = new List<ForbidInfo>();

	private Player curPlayer;

	private Dictionary<string, CMDEventHandler> cmdArray = new Dictionary<string, CMDEventHandler>();

	public static GMCommand Self => _instance;

	private GMCommand()
	{
		_instance = this;
		Self.RegisterCMD();
	}

	private void RegisterCMD()
	{
		cmdArray["make"] = make;
		cmdArray["playerdroppercent"] = playerdroppercent;
		cmdArray["moveto"] = moveto;
		cmdArray["call"] = call;
		cmdArray["clearitem"] = clearitem;
		cmdArray["changew"] = changew;
		cmdArray["time"] = time;
		cmdArray["monster"] = monster;
		cmdArray["npc"] = npc;
		cmdArray["kickout"] = kickout;
		cmdArray["forbid"] = forbid;
		cmdArray["whosyourdaddy"] = whosyourdaddy;
		cmdArray["youaremydaddy"] = youaremydaddy;
		cmdArray["stat"] = statistics;
	}

	public bool ParsingCMD(Player player, string code)
	{
		if (player == null || code.Length == 0)
		{
			return false;
		}
		string text = code.Substring(0, 1);
		if (text == "/")
		{
			if (!player.IsGM)
			{
				return true;
			}
			curPlayer = player;
			string text2 = code.Substring(1);
			if (text2.Length > 0)
			{
				string[] array = text2.Split(' ');
				if (array.Count() > 1)
				{
					cmdArray[array[0]](player, array[1]);
				}
				else
				{
					if (array.Count() <= 0)
					{
						return false;
					}
					cmdArray[array[0]](player, string.Empty);
				}
				return true;
			}
		}
		return false;
	}

	private void make(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Count() == 2)
		{
			int num = int.Parse(array[0]);
			int count = int.Parse(array[1]);
			ItemProto itemData = ItemProto.GetItemData(num);
			if (itemData != null)
			{
				List<ItemObject> effItems = new List<ItemObject>(10);
				player.Package.AddSameItems(num, count, ref effItems);
				ChannelNetwork.SyncItemList(player.WorldId, effItems);
				player.SyncPackageIndex();
			}
		}
		else if (array.Count() == 1)
		{
			int num = int.Parse(array[0]);
			int count = 1;
			ItemProto itemData2 = ItemProto.GetItemData(num);
			if (itemData2 != null)
			{
				List<ItemObject> effItems2 = new List<ItemObject>(10);
				player.Package.AddSameItems(num, count, ref effItems2);
				ChannelNetwork.SyncItemList(player.WorldId, effItems2);
				player.SyncPackageIndex();
			}
		}
	}

	private void playerdroppercent(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Count() == 1)
		{
			int num = int.Parse(array[0]);
			if (num >= 0 && num <= 100)
			{
				ServerConfig.DropDeadPercent = num;
			}
			else
			{
				player.SyncErrorMsg("Bad Command!");
			}
		}
	}

	private void changew(Player player, string str)
	{
		WeatherManager.Instance.GMRandWeather();
	}

	private void time(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Count() == 1)
		{
			GameTime.Timer.Update(int.Parse(array[0]));
			GameTime.SyncTimeImmediately();
		}
	}

	private void monster(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Count() == 1)
		{
			Vector3 pos = player.transform.position + new Vector3(0.2f, 0.2f, 0.2f);
			SPTerrainEvent.CreateMonsterWithoutLimit(player.Id, player.WorldId, pos, Convert.ToInt32(array[0]));
		}
		else if (array.Count() == 2)
		{
			int num = Convert.ToInt32(array[1]);
			Vector3 pos2 = player.transform.position + new Vector3(0.2f, 0.2f, 0.2f);
			for (int i = 0; i < num; i++)
			{
				SPTerrainEvent.CreateMonsterWithoutLimit(player.Id, player.WorldId, pos2, Convert.ToInt32(array[0]));
			}
		}
	}

	private void npc(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Count() < 1)
		{
			return;
		}
		Vector3 pos = player.transform.position + new Vector3(0.2f, 0.2f, 0.2f);
		int num = Convert.ToInt32(array[0]);
		float scale = UnityEngine.Random.Range(0.5f, 1.5f);
		if (ServerConfig.IsStory)
		{
			if (NpcMissionDataRepository.GetMissionData(num) == null)
			{
				return;
			}
			if (array.Count() == 1)
			{
				SPTerrainEvent.CreateNpc(player.Id, player.WorldId, pos, num, 1, scale, -1, isStand: false, 0f, forcedServant: false, dontCheck: true);
			}
			else if (array.Count() == 2)
			{
				int num2 = Convert.ToInt32(array[1]);
				for (int i = 0; i < num2; i++)
				{
					SPTerrainEvent.CreateNpc(player.Id, player.WorldId, pos, num, 1, scale, -1, isStand: false, 0f, forcedServant: false, dontCheck: true);
				}
			}
		}
		else if (array.Count() == 1)
		{
			SPTerrainEvent.CreateNpc(player.Id, player.WorldId, pos, num, 1, scale, -1, isStand: false, 0f, forcedServant: false, dontCheck: true);
		}
		else if (array.Count() == 2)
		{
			int num3 = Convert.ToInt32(array[1]);
			for (int j = 0; j < num3; j++)
			{
				SPTerrainEvent.CreateNpc(player.Id, player.WorldId, pos, num, 1, scale, -1, isStand: false, 0f, forcedServant: false, dontCheck: true);
			}
		}
	}

	private void moveto(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Count() == 3)
		{
			int num = int.Parse(array[0]);
			int num2 = int.Parse(array[1]);
			int num3 = int.Parse(array[2]);
			Vector3 position = player.transform.position;
			position.x = num;
			position.y = num2;
			position.z = num3;
			player.transform.position = position;
			player.RPCOthers(EPacketType.PT_InGame_FastTransfer, position);
		}
	}

	private void call(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Count() == 1)
		{
			Player player2 = Player.GetPlayer(array[0]);
			if (player2 != null)
			{
				player2.transform.position = player.transform.position;
				player2.RPCOthers(EPacketType.PT_InGame_FastTransfer, player2.transform.position);
			}
		}
	}

	private void clearitem(Player player, string str)
	{
		player.Package.Clear();
		player.SyncPackageIndex();
	}

	private void kickout(Player player, string str)
	{
		Player playerByName = Player.GetPlayerByName(str);
		if (playerByName != null)
		{
			NetInterface.CloseConnection(playerByName.OwnerView.owner);
		}
	}

	private void forbid(Player player, string str)
	{
		string[] array = str.Split(',');
		if (array.Length != 2)
		{
			return;
		}
		Player playerByName = Player.GetPlayerByName(array[0]);
		if (playerByName != null)
		{
			ForbidInfo forbidInfo = GetForbid(playerByName.steamId);
			if (forbidInfo == null)
			{
				forbidInfo = new ForbidInfo();
			}
			forbidInfo._curTime = 0;
			forbidInfo._totalTime = Convert.ToInt32(array[1]);
			forbidInfo._steamId = playerByName.steamId;
			_forbidList.Add(forbidInfo);
			NetInterface.CloseConnection(playerByName.OwnerView.owner);
		}
	}

	private void whosyourdaddy(Player player, string str)
	{
		player.SetAllAttribute(AttribType.DebuffReduce10, 1f);
	}

	private void youaremydaddy(Player player, string str)
	{
		player.SetAllAttribute(AttribType.DebuffReduce10, 0f);
	}

	private void statistics(Player player, string str)
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.Statistics);
	}

	private ForbidInfo GetForbid(ulong steamId)
	{
		foreach (ForbidInfo forbid in _forbidList)
		{
			if (forbid._steamId == steamId)
			{
				return forbid;
			}
		}
		return null;
	}

	private uLink.NetworkPlayer GetCurPlayer()
	{
		return Player.GetPlayerPeer(curPlayer.Id);
	}

	public void SendStatisticsToGM(string str)
	{
		curPlayer.RPCPeer(GetCurPlayer(), EPacketType.PT_GM_Statistics, str);
	}

	public bool IsForbid(ulong steamId)
	{
		if (GetForbid(steamId) != null)
		{
			return true;
		}
		return false;
	}

	public void MyUpdate()
	{
		foreach (ForbidInfo forbid in _forbidList)
		{
			forbid._curTime++;
			if (forbid._curTime >= forbid._totalTime)
			{
				_forbidList.Remove(forbid);
				break;
			}
		}
	}
}
