using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;

namespace PeMap;

public class MapIcon
{
	public class Mgr : MonoLikeSingleton<Mgr>
	{
		public List<MapIcon> iconList;

		protected override void OnInit()
		{
			base.OnInit();
			Load();
		}

		public void Load()
		{
			iconList = new List<MapIcon>();
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Icon");
			while (sqliteDataReader.Read())
			{
				MapIcon mapIcon = new MapIcon();
				mapIcon.id = Convert.ToInt32(sqliteDataReader.GetString(0));
				mapIcon.iconName = sqliteDataReader.GetString(1);
				mapIcon.iconType = (EMapIcon)Convert.ToInt32(sqliteDataReader.GetString(2));
				iconList.Add(mapIcon);
			}
		}
	}

	public const int None = 0;

	public const int UserDefine1 = 1;

	public const int UserDefine2 = 2;

	public const int UserDefine3 = 3;

	public const int UserDefine4 = 4;

	public const int Camp = 5;

	public const int WorldMapVehiclesTag = 6;

	public const int Player = 7;

	public const int Turret = 8;

	public const int Servant = 9;

	public const int FlagIcon = 10;

	public const int AllyPlayer = 11;

	public const int OppositePlayer = 12;

	public const int TaskTarget = 13;

	public const int Npc = 14;

	public const int TaskUnCmplt = 26;

	public const int PlayerBase = 16;

	public const int MonsterBoss = 17;

	public const int Monster = 18;

	public const int PujaBase = 19;

	public const int Puja = 20;

	public const int PujaBoss = 21;

	public const int PajaBase = 22;

	public const int Paja = 23;

	public const int PajaBoss = 24;

	public const int TaskGetable = 25;

	public const int TaskCmplt = 26;

	public const int AdventureCamp = 27;

	public const int Nutral = 28;

	public const int CrashSite = 29;

	public const int GlantTree = 30;

	public const int RockFormation = 31;

	public const int MonsterNest = 32;

	public const int Relic = 33;

	public const int LargeDoline = 34;

	public const int Lake = 35;

	public const int BrokenDam = 36;

	public const int DesertCity = 37;

	public const int Waterfall = 38;

	public const int VirusSpacecraft = 39;

	public const int PujaGate = 40;

	public const int L1_Ship = 41;

	public const int ServantDeadPlace = 43;

	public const int RandomDungeonEntrance = 44;

	public const int MonsterBeacon = 45;

	public const int HumanBrokenBase = 46;

	public const int PajaBrokenBase = 47;

	public const int PujaBrokenBase = 48;

	public const int Alien = 18;

	public const int unknownCamp = 26;

	public int id;

	public string iconName;

	public EMapIcon iconType;
}
