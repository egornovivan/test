using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class ActionEventsMgr
{
	public static ActionEventsMgr _self = new ActionEventsMgr();

	private List<ActionEventsData> _actionEventsSet = new List<ActionEventsData>();

	public void LoadInfo()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("actionevents");
		while (sqliteDataReader.Read())
		{
			ActionEventsData actionEventsData = new ActionEventsData();
			actionEventsData._id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			actionEventsData._operator = (OperatorEnum)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("operator")));
			actionEventsData._opportunity = (ActionOpportunity)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("opportunity")));
			actionEventsData._actionId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("actionId")));
			_actionEventsSet.Add(actionEventsData);
		}
	}

	public void ProcessAction(OperatorEnum oper, ActionOpportunity opp, SkNetworkInterface skNet, params object[] args)
	{
		foreach (ActionEventsData item in _actionEventsSet)
		{
			if (item._operator == oper && item._opportunity == opp)
			{
				ActionProcess._self.StartActionProcess(item._actionId, skNet, args);
				break;
			}
		}
	}
}
