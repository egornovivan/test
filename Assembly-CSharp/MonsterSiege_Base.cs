using Pathea;
using UnityEngine;

public class MonsterSiege_Base : MonoBehaviour
{
	public static bool MonsterSiegeBasePause;

	[SerializeField]
	private CSBuildingLogic assembly;

	[SerializeField]
	private float minHours;

	[SerializeField]
	private float maxHours;

	private int _lvl;

	private float _lastHour;

	private float _nextHour;

	private bool _Init;

	private PETimer m_Timer;

	private CSDataMonsterSiege m_Data;

	private EntityMonsterBeacon m_Beacon;

	private TowerInfoUIData m_UIData;

	private int lvl
	{
		set
		{
			_lvl = value;
			Export();
		}
	}

	private float lastHour
	{
		set
		{
			_lastHour = value;
			Export();
		}
	}

	private float nextHour
	{
		set
		{
			_nextHour = value;
			Export();
		}
	}

	private void Init()
	{
		if (!_Init)
		{
			_Init = true;
			m_Timer = assembly.m_Entity.m_Creator.Timer;
			m_Data = assembly.m_Entity.m_Creator.m_DataInst.m_Siege;
			assembly.m_Entity.AddEventListener(OnEntityEventListener);
			Import();
			if (_nextHour == 0f)
			{
				CreateNextSiege();
			}
		}
	}

	private void Import()
	{
		_lvl = m_Data.lvl;
		_lastHour = m_Data.lastHour;
		_nextHour = m_Data.nextHour;
		CreateMonsterBeacon();
	}

	private void Export()
	{
		m_Data.lvl = _lvl;
		m_Data.lastHour = _lastHour;
		m_Data.nextHour = _nextHour;
	}

	private void CreateNextSiege()
	{
		lastHour = (int)m_Timer.Hour;
		nextHour = Random.Range(minHours, maxHours);
	}

	private void CreateMonsterBeacon()
	{
		if (_lvl > 0 && assembly != null && assembly.m_Entity != null)
		{
			if (m_Beacon != null)
			{
				m_Beacon.Delete();
			}
			m_UIData = new TowerInfoUIData();
			m_Beacon = EntityMonsterBeacon.CreateMonsterBeaconByTDID(_lvl, base.transform, m_UIData);
			m_Beacon.gameObject.AddComponent<MonsterSiege>().SetCreator(assembly.m_Entity.m_Creator as CSMgCreator, m_UIData);
		}
	}

	private void CalculateLvl()
	{
		int num = 0;
		if (assembly != null && assembly.m_Entity != null && assembly.m_Entity is CSAssembly)
		{
			num = (assembly.m_Entity as CSAssembly).Level;
		}
		lvl = Mathf.Clamp(Random.Range(num, num + 3), 1, 5);
		CreateMonsterBeacon();
		CreateNextSiege();
	}

	private void OnEntityEventListener(int event_id, CSEntity entity, object arg)
	{
		if (event_id == 1)
		{
			lvl = 0;
			lastHour = 0f;
			nextHour = 0f;
			if (m_Beacon != null)
			{
				m_Beacon.Delete();
			}
		}
	}

	private void Update()
	{
		if (!MonsterSiegeBasePause && !PeGameMgr.IsMulti && !EntityMonsterBeacon.IsRunning() && !PeGameMgr.IsBuild && !(assembly == null) && assembly.m_Entity != null && assembly.m_Entity is CSAssembly)
		{
			Init();
			if (_lvl > 0 && m_Beacon == null)
			{
				lvl = 0;
			}
			if (m_Timer.Hour - (double)_lastHour >= (double)_nextHour)
			{
				CalculateLvl();
			}
		}
	}

	private void OnDestroy()
	{
		if (m_Beacon != null)
		{
			m_Beacon.Delete();
		}
		if (assembly != null && assembly.m_Entity != null)
		{
			assembly.m_Entity.AddEventListener(OnEntityEventListener);
		}
	}
}
