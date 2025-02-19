using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace Pathea;

public class NpcThinkDb
{
	private static Dictionary<EThinkingType, NpcThinking> _sThinking;

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPCThinking");
		_sThinking = new Dictionary<EThinkingType, NpcThinking>();
		while (sqliteDataReader.Read())
		{
			NpcThinking npcThinking = new NpcThinking();
			npcThinking.ID = sqliteDataReader.GetInt32(0);
			npcThinking.Type = (EThinkingType)npcThinking.ID;
			npcThinking.Name = sqliteDataReader.GetString(1);
			for (int i = 2; i < sqliteDataReader.FieldCount; i++)
			{
				npcThinking.mThinkInfo.Add((EThinkingType)(i - 1), (EThinkingMask)sqliteDataReader.GetInt32(i));
			}
			_sThinking.Add(npcThinking.Type, npcThinking);
		}
	}

	public static void Release()
	{
		_sThinking = null;
	}

	public static NpcThinking Get(EThinkingType type)
	{
		return _sThinking[type];
	}

	public static NpcThinking Get(int id)
	{
		return Get((EThinkingType)id);
	}

	public static bool CanDo(PeEntity entity, EThinkingType type)
	{
		NpcCmpt npcCmpt = entity.NpcCmpt;
		if (npcCmpt == null)
		{
			return true;
		}
		return npcCmpt.ThinkAgent.CanDo(type);
	}

	public static bool CanDoing(PeEntity entity, EThinkingType _newtThink)
	{
		NpcCmpt npcCmpt = entity.NpcCmpt;
		if (npcCmpt == null)
		{
			return true;
		}
		if (!npcCmpt.ThinkAgent.CanDo(_newtThink))
		{
			EThinkingType nowDo = npcCmpt.ThinkAgent.GetNowDo();
			NpcThinking npcThinking = Get(_newtThink);
			if (npcThinking.GetMask(nowDo) == EThinkingMask.Block)
			{
				npcCmpt.ThinkAgent.RemoveThink(nowDo);
				npcCmpt.ThinkAgent.RemoveThink(_newtThink);
				npcCmpt.ThinkAgent.AddThink(nowDo);
				npcCmpt.ThinkAgent.AddThink(_newtThink);
				return true;
			}
			if (npcThinking.GetMask(nowDo) == EThinkingMask.Blocked)
			{
				npcCmpt.ThinkAgent.RemoveThink(_newtThink);
				npcCmpt.ThinkAgent.RemoveThink(nowDo);
				npcCmpt.ThinkAgent.AddThink(_newtThink);
				npcCmpt.ThinkAgent.AddThink(nowDo);
				return false;
			}
			if (npcThinking.GetMask(nowDo) == EThinkingMask.Delete)
			{
				npcCmpt.ThinkAgent.RemoveThink(nowDo);
				npcCmpt.ThinkAgent.AddThink(_newtThink);
				return true;
			}
			if (npcThinking.GetMask(nowDo) == EThinkingMask.Deleted)
			{
				return false;
			}
		}
		return true;
	}
}
