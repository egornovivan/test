using System.Data;
using System.IO;
using PETools;

public class ConfigDbData : DbRecordData
{
	public int GameMode;

	public int GameType;

	public int MaxNewNpcId;

	public int MaxNewMonsterId;

	public int MaxNewItemId;

	public int MaxNewDoodadId;

	public int CurNewTeamId;

	public int TeamNum;

	public int NumPerTeam;

	public int TerrainType;

	public int VegetationType;

	public int ClimateType;

	public int TerrainHeight;

	public int MapSize;

	public int RiverDensity;

	public int RiverWidth;

	public int PlainHeight;

	public int Flatness;

	public int BridgeMaxHeight;

	public long ServerUID;

	public bool GameStarted;

	public string ServerName;

	public string MapSeed;

	public byte[] Data;

	public void ExportData()
	{
		mType = EDbOpType.OP_INSERT;
		GameMode = (int)ServerConfig.SceneMode;
		GameType = (int)ServerConfig.GameType;
		MaxNewNpcId = ServerConfig.MaxNewNpcID;
		MaxNewMonsterId = ServerConfig.MaxNewMonsterID;
		MaxNewItemId = ServerConfig.MaxNewItemID;
		MaxNewDoodadId = ServerConfig.MaxNewDoodadID;
		CurNewTeamId = GroupNetwork.CurNewTeamId;
		TeamNum = ServerConfig.TeamNum;
		NumPerTeam = ServerConfig.NumPerTeam;
		TerrainType = (int)ServerConfig.TerrainType;
		VegetationType = (int)ServerConfig.VegetationType;
		ClimateType = (int)ServerConfig.ClimateType;
		TerrainHeight = ServerConfig.TerrainHeight;
		MapSize = ServerConfig.MapSize;
		RiverDensity = ServerConfig.RiverDensity;
		RiverWidth = ServerConfig.RiverWidth;
		PlainHeight = ServerConfig.PlainHeight;
		Flatness = ServerConfig.Flatness;
		BridgeMaxHeight = ServerConfig.BridgeMaxHeight;
		ServerUID = ServerConfig.ServerUID;
		GameStarted = ServerConfig.GameStarted;
		ServerName = ServerConfig.ServerName;
		MapSeed = ServerConfig.MapSeed;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, ServerConfig.CreatedNpcItemBuildingIndex.Count);
			foreach (BuildingID key in ServerConfig.CreatedNpcItemBuildingIndex.Keys)
			{
				BufferHelper.Serialize(w, key.townId);
				BufferHelper.Serialize(w, key.buildingNo);
			}
			BufferHelper.Serialize(w, ServerConfig.mAliveBuildings.Keys.Count);
			foreach (int key2 in ServerConfig.mAliveBuildings.Keys)
			{
				BufferHelper.Serialize(w, key2);
				BufferHelper.Serialize(w, ServerConfig.mAliveBuildings[key2].Count);
				foreach (int item in ServerConfig.mAliveBuildings[key2])
				{
					BufferHelper.Serialize(w, item);
				}
			}
			BufferHelper.Serialize(w, GameTime.Timer.Second);
			if (ServerConfig.IsStory)
			{
				BufferHelper.Serialize(w, ServerConfig.FoundMapLable.Count);
				for (int i = 0; i < ServerConfig.FoundMapLable.Count; i++)
				{
					BufferHelper.Serialize(w, ServerConfig.FoundMapLable[i]);
				}
			}
			BufferHelper.Serialize(w, (int)VersionMgr.MAINTAIN_VER);
			BufferHelper.Serialize(w, (int)ServerConfig.MoneyType);
			BufferHelper.Serialize(w, ServerConfig.DropDeadPercent);
			BufferHelper.Serialize(w, ServerConfig.UID);
			BufferHelper.Serialize(w, ServerConfig.mirror);
			BufferHelper.Serialize(w, ServerConfig.rotation);
			BufferHelper.Serialize(w, ServerConfig.pickedLineIndex);
			BufferHelper.Serialize(w, ServerConfig.pickedLevelIndex);
			BufferHelper.Serialize(w, ServerConfig.AllyCount);
			BufferHelper.Serialize(w, BuildingInfoManager.Instance.allTownPos.Count);
			foreach (int key3 in BuildingInfoManager.Instance.allTownPos.Keys)
			{
				BufferHelper.Serialize(w, key3);
				BufferHelper.Serialize(w, BuildingInfoManager.Instance.allTownPos[key3]);
			}
		});
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO serverinfo(servername,ver,mode,type,seed,maxnpcid,maxmonsterid,maxitemid,maxdoodadid,curteamid,maxteamnum,numperteam,terraintype,vegetationtype,scenceclimate,serveruid,terrainheight,mapsize,riverdensity,riverwidth,plainheight,flatness,bridgemaxheight,gamestarted,blobdata) VALUES(@servername,@ver,@mode,@type,@seed,@maxnpcid,@maxmonsterid,@maxitemid,@maxdoodadid,@curteamid,@maxteamnum,@numperteam,@terraintype,@vegetationtype,@scenceclimate,@serveruid,@terrainheight,@mapsize,@riverdensity,@riverwidth,@plainheight,@flatness,@bridgemaxheight,@gamestarted,@blobdata);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@servername";
		dbDataParameter.DbType = DbType.String;
		dbDataParameter.Value = ServerName;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = ServerConfig.RecordVersion;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@mode";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = GameMode;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@type";
		dbDataParameter4.DbType = DbType.Int32;
		dbDataParameter4.Value = GameType;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@seed";
		dbDataParameter5.DbType = DbType.String;
		dbDataParameter5.Value = MapSeed;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@maxnpcid";
		dbDataParameter6.DbType = DbType.Int32;
		dbDataParameter6.Value = MaxNewNpcId;
		cmd.Parameters.Add(dbDataParameter6);
		IDbDataParameter dbDataParameter7 = cmd.CreateParameter();
		dbDataParameter7.ParameterName = "@maxmonsterid";
		dbDataParameter7.DbType = DbType.Int32;
		dbDataParameter7.Value = MaxNewMonsterId;
		cmd.Parameters.Add(dbDataParameter7);
		IDbDataParameter dbDataParameter8 = cmd.CreateParameter();
		dbDataParameter8.ParameterName = "@maxitemid";
		dbDataParameter8.DbType = DbType.Int32;
		dbDataParameter8.Value = MaxNewItemId;
		cmd.Parameters.Add(dbDataParameter8);
		IDbDataParameter dbDataParameter9 = cmd.CreateParameter();
		dbDataParameter9.ParameterName = "@maxdoodadid";
		dbDataParameter9.DbType = DbType.Int32;
		dbDataParameter9.Value = MaxNewDoodadId;
		cmd.Parameters.Add(dbDataParameter9);
		IDbDataParameter dbDataParameter10 = cmd.CreateParameter();
		dbDataParameter10.ParameterName = "@curteamid";
		dbDataParameter10.DbType = DbType.Int32;
		dbDataParameter10.Value = CurNewTeamId;
		cmd.Parameters.Add(dbDataParameter10);
		IDbDataParameter dbDataParameter11 = cmd.CreateParameter();
		dbDataParameter11.ParameterName = "@maxteamnum";
		dbDataParameter11.DbType = DbType.Int32;
		dbDataParameter11.Value = TeamNum;
		cmd.Parameters.Add(dbDataParameter11);
		IDbDataParameter dbDataParameter12 = cmd.CreateParameter();
		dbDataParameter12.ParameterName = "@numperteam";
		dbDataParameter12.DbType = DbType.Int32;
		dbDataParameter12.Value = NumPerTeam;
		cmd.Parameters.Add(dbDataParameter12);
		IDbDataParameter dbDataParameter13 = cmd.CreateParameter();
		dbDataParameter13.ParameterName = "@terraintype";
		dbDataParameter13.DbType = DbType.Int32;
		dbDataParameter13.Value = TerrainType;
		cmd.Parameters.Add(dbDataParameter13);
		IDbDataParameter dbDataParameter14 = cmd.CreateParameter();
		dbDataParameter14.ParameterName = "@vegetationtype";
		dbDataParameter14.DbType = DbType.Int32;
		dbDataParameter14.Value = VegetationType;
		cmd.Parameters.Add(dbDataParameter14);
		IDbDataParameter dbDataParameter15 = cmd.CreateParameter();
		dbDataParameter15.ParameterName = "@scenceclimate";
		dbDataParameter15.DbType = DbType.Int32;
		dbDataParameter15.Value = ClimateType;
		cmd.Parameters.Add(dbDataParameter15);
		IDbDataParameter dbDataParameter16 = cmd.CreateParameter();
		dbDataParameter16.ParameterName = "@serveruid";
		dbDataParameter16.DbType = DbType.Int64;
		dbDataParameter16.Value = ServerUID;
		cmd.Parameters.Add(dbDataParameter16);
		IDbDataParameter dbDataParameter17 = cmd.CreateParameter();
		dbDataParameter17.ParameterName = "@terrainheight";
		dbDataParameter17.DbType = DbType.Int32;
		dbDataParameter17.Value = TerrainHeight;
		cmd.Parameters.Add(dbDataParameter17);
		IDbDataParameter dbDataParameter18 = cmd.CreateParameter();
		dbDataParameter18.ParameterName = "@mapsize";
		dbDataParameter18.DbType = DbType.Int32;
		dbDataParameter18.Value = MapSize;
		cmd.Parameters.Add(dbDataParameter18);
		IDbDataParameter dbDataParameter19 = cmd.CreateParameter();
		dbDataParameter19.ParameterName = "@riverdensity";
		dbDataParameter19.DbType = DbType.Int32;
		dbDataParameter19.Value = RiverDensity;
		cmd.Parameters.Add(dbDataParameter19);
		IDbDataParameter dbDataParameter20 = cmd.CreateParameter();
		dbDataParameter20.ParameterName = "@riverwidth";
		dbDataParameter20.DbType = DbType.Int32;
		dbDataParameter20.Value = RiverWidth;
		cmd.Parameters.Add(dbDataParameter20);
		IDbDataParameter dbDataParameter21 = cmd.CreateParameter();
		dbDataParameter21.ParameterName = "@plainheight";
		dbDataParameter21.DbType = DbType.Int32;
		dbDataParameter21.Value = PlainHeight;
		cmd.Parameters.Add(dbDataParameter21);
		IDbDataParameter dbDataParameter22 = cmd.CreateParameter();
		dbDataParameter22.ParameterName = "@flatness";
		dbDataParameter22.DbType = DbType.Int32;
		dbDataParameter22.Value = Flatness;
		cmd.Parameters.Add(dbDataParameter22);
		IDbDataParameter dbDataParameter23 = cmd.CreateParameter();
		dbDataParameter23.ParameterName = "@bridgemaxheight";
		dbDataParameter23.DbType = DbType.Int32;
		dbDataParameter23.Value = BridgeMaxHeight;
		cmd.Parameters.Add(dbDataParameter23);
		IDbDataParameter dbDataParameter24 = cmd.CreateParameter();
		dbDataParameter24.ParameterName = "@gamestarted";
		dbDataParameter24.DbType = DbType.Boolean;
		dbDataParameter24.Value = GameStarted;
		cmd.Parameters.Add(dbDataParameter24);
		IDbDataParameter dbDataParameter25 = cmd.CreateParameter();
		dbDataParameter25.ParameterName = "@blobdata";
		dbDataParameter25.DbType = DbType.Int32;
		dbDataParameter25.Value = Data;
		cmd.Parameters.Add(dbDataParameter25);
		cmd.ExecuteNonQuery();
	}

	public override void Exce(IDbCommand cmd)
	{
		EDbOpType eDbOpType = mType;
		if (eDbOpType == EDbOpType.OP_INSERT)
		{
			Insert(cmd);
		}
	}
}
