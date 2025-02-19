using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class ActionProcess
{
	private delegate bool ActionEvent(string param, SkNetworkInterface skNet, params object[] args);

	public static ActionProcess _self = new ActionProcess();

	private Dictionary<string, ActionEvent> ActionMap = new Dictionary<string, ActionEvent>();

	private Dictionary<int, ActionData> _actionDataSet = new Dictionary<int, ActionData>();

	private Dictionary<int, ActionData> ActionDataSet => _actionDataSet;

	public ActionProcess()
	{
		_self = this;
		Register();
	}

	private void Register()
	{
		ActionMap.Add("iscoopmode", IsCoopMode);
		ActionMap.Add("isvsmode", IsVsMode);
		ActionMap.Add("iswinner", IsWinner);
		ActionMap.Add("isloser", IsLoser);
		ActionMap.Add("addlobbyexp", AddLobbyExp);
		ActionMap.Add("addcasterlobbyexp", AddCasterLobbyExp);
	}

	public void StartActionProcess(int idAction, SkNetworkInterface skNet, params object[] args)
	{
		if (idAction == 0)
		{
			return;
		}
		int num = 0;
		while (idAction != 0)
		{
			if (!ActionDataSet.ContainsKey(idAction))
			{
				Debug.LogError("idAction was not exist, idAction = " + idAction);
				break;
			}
			ActionData actionData = ActionDataSet[idAction];
			if (actionData == null)
			{
				Debug.LogError("ActionData is null, idAction = " + idAction);
				break;
			}
			idAction = ((!Process(actionData._type, actionData._param, skNet, args)) ? actionData._id_nextfail : actionData._id_next);
			if (num++ >= 64)
			{
				Debug.LogError("Too many actions idAction = " + idAction);
				break;
			}
		}
	}

	private bool Process(string cmd, string param, SkNetworkInterface skNet, params object[] args)
	{
		if (!ActionMap.ContainsKey(cmd.ToLower()))
		{
			return false;
		}
		return ActionMap[cmd.ToLower()](param, skNet, args);
	}

	public void LoadInfo()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("actionsystem");
		while (sqliteDataReader.Read())
		{
			ActionData actionData = new ActionData();
			actionData._id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			actionData._id_next = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id_next")));
			actionData._id_nextfail = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id_nextfail")));
			actionData._type = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("type"));
			actionData._param = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("param"));
			_actionDataSet.Add(actionData._id, actionData);
		}
	}

	private bool IsCoopMode(string param, SkNetworkInterface skNet, params object[] args)
	{
		if (!ServerConfig.IsCooperation)
		{
			return true;
		}
		return false;
	}

	private bool IsVsMode(string param, SkNetworkInterface skNet, params object[] args)
	{
		if (!ServerConfig.IsVS)
		{
			return true;
		}
		return false;
	}

	private bool IsWinner(string param, SkNetworkInterface skNet, params object[] args)
	{
		if (args == null || args.Length < 1)
		{
			return false;
		}
		if (skNet == null || !(skNet is Player))
		{
			return false;
		}
		int num = (int)args[0];
		if (skNet.TeamId != num)
		{
			return false;
		}
		return true;
	}

	private bool IsLoser(string param, SkNetworkInterface skNet, params object[] args)
	{
		if (args.Length != 1)
		{
			return false;
		}
		if (skNet == null || !(skNet is Player))
		{
			return false;
		}
		int num = (int)args[0];
		if (skNet.TeamId == num)
		{
			return false;
		}
		return true;
	}

	private bool AddLobbyExp(string param, SkNetworkInterface skNet, params object[] args)
	{
		if (skNet == null || !(skNet is Player))
		{
			return false;
		}
		float exp = Convert.ToSingle(param);
		((Player)skNet).AddLobbyExp(exp);
		return true;
	}

	private bool AddCasterLobbyExp(string param, SkNetworkInterface skNet, params object[] args)
	{
		if (args == null || args.Length < 1)
		{
			return false;
		}
		int id = (int)args[0];
		Player player = Player.GetPlayer(id);
		if (player != null)
		{
			float exp = Convert.ToSingle(param);
			player.AddLobbyExp(exp);
			return true;
		}
		return false;
	}
}
