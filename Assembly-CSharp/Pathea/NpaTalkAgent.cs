using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class NpaTalkAgent
{
	public class AgentInfo
	{
		public ENpcTalkType _type;

		public ENpcSpeakType _spType;

		public ETalkLevel _level;

		public bool _canLoop;

		public float _startTime;

		public float _loopTime;

		public bool _hasSend;

		public AgentInfo(ENpcTalkType type, ENpcSpeakType spType, bool canLoop = false)
		{
			_type = type;
			_spType = spType;
			_startTime = Time.time;
			_canLoop = canLoop;
			_loopTime = NpcRandomTalkDb.Get(type)._interval;
		}
	}

	private NpcCmpt m_npcCmpt;

	private EntityInfoCmpt m_entityInfo;

	private AgentInfo[] _msgs;

	private List<AgentInfo> _romveCases;

	public NpaTalkAgent(PeEntity entity)
	{
		_msgs = new AgentInfo[35];
		_romveCases = new List<AgentInfo>();
		m_npcCmpt = entity.GetComponent<NpcCmpt>();
		m_entityInfo = entity.GetComponent<EntityInfoCmpt>();
	}

	public void AddAgentInfo(AgentInfo info)
	{
		if (_msgs[(int)info._type] == null)
		{
			_msgs[(int)info._type] = info;
		}
	}

	public void AddAgentInfo(ENpcTalkType type, ENpcSpeakType spType, bool canLoop = false)
	{
		if (_msgs == null)
		{
			_msgs = new AgentInfo[35];
		}
		AddAgentInfo(new AgentInfo(type, spType, canLoop));
	}

	public bool RemoveAgentInfo(ENpcTalkType type)
	{
		if (_msgs == null || _msgs[(int)type] == null)
		{
			return false;
		}
		_msgs[(int)type] = null;
		return true;
	}

	private bool CampeareTime(AgentInfo keyInfo, float NowTime)
	{
		return NowTime - keyInfo._startTime >= keyInfo._loopTime;
	}

	public void RunAttrAgent(PeEntity peEntity)
	{
		List<NpcRandomTalkDb.Item> talkItems = NpcRandomTalkDb.GetTalkItems(peEntity);
		for (int i = 0; i < talkItems.Count; i++)
		{
			if (talkItems[i] != null && talkItems[i].Type != AttribType.Max && talkItems[i].Level != ETalkLevel.Max && !NpcEatDb.CanEatByAttr(peEntity, talkItems[i].Type, talkItems[i].TypeMax, bContinue: false))
			{
				AddAgentInfo(new AgentInfo(talkItems[i].TalkType, ENpcSpeakType.TopHead, canLoop: true));
			}
		}
		RunAgent();
	}

	public void RunAgent()
	{
		for (int i = 0; i < 35; i++)
		{
			if (_msgs[i] != null && !_msgs[i]._hasSend)
			{
				m_npcCmpt.SendTalkMsg((int)_msgs[i]._type, _msgs[i]._loopTime, _msgs[i]._spType);
				_msgs[i]._hasSend = true;
				_romveCases.Add(_msgs[i]);
			}
		}
		int count = _romveCases.Count;
		if (count <= 0)
		{
			return;
		}
		for (int j = 0; j < count; j++)
		{
			if (!_romveCases[j]._canLoop)
			{
				RemoveAgentInfo(_romveCases[j]._type);
				_romveCases.Remove(_romveCases[j]);
				break;
			}
			if (_romveCases[j]._canLoop && CampeareTime(_romveCases[j], Time.time))
			{
				RemoveAgentInfo(_romveCases[j]._type);
				_romveCases.Remove(_romveCases[j]);
				break;
			}
		}
	}
}
