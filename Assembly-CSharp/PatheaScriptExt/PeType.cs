using System;
using System.Collections.Generic;
using System.Xml;
using Pathea;
using PatheaScript;

namespace PatheaScriptExt;

public static class PeType
{
	public static List<PeEntity> GetPlayer(int playerId)
	{
		List<PeEntity> list = new List<PeEntity>(1);
		if (playerId == 0)
		{
			list.Add(GetMainPlayer());
			return list;
		}
		throw new Exception("not found player type.");
	}

	public static PeEntity GetMainPlayer()
	{
		return PeSingleton<PeCreature>.Instance.mainPlayer;
	}

	public static VarRef GetPlayerId(XmlNode xmlNode, Trigger trigger)
	{
		return PatheaScript.Util.GetVarRefOrValue(xmlNode, "player", VarValue.EType.Int, trigger);
	}

	public static VarRef GetItemId(XmlNode xmlNode, Trigger trigger)
	{
		return PatheaScript.Util.GetVarRefOrValue(xmlNode, "item", VarValue.EType.Int, trigger);
	}

	public static VarRef GetMonsterId(XmlNode xmlNode, Trigger trigger)
	{
		return PatheaScript.Util.GetVarRefOrValue(xmlNode, "monster", VarValue.EType.Int, trigger);
	}

	public static VarRef GetNpcId(XmlNode xmlNode, Trigger trigger)
	{
		return PatheaScript.Util.GetVarRefOrValue(xmlNode, "npc", VarValue.EType.Int, trigger);
	}

	public static VarRef GetEffectId(XmlNode xmlNode, Trigger trigger)
	{
		return PatheaScript.Util.GetVarRefOrValue(xmlNode, "effect", VarValue.EType.Int, trigger);
	}

	public static VarRef GetLocationId(XmlNode xmlNode, Trigger trigger)
	{
		return PatheaScript.Util.GetVarRefOrValue(xmlNode, "location", VarValue.EType.Int, trigger);
	}

	public static VarRef GetUIId(XmlNode xmlNode, Trigger trigger)
	{
		return PatheaScript.Util.GetVarRefOrValue(xmlNode, "ui", VarValue.EType.Int, trigger);
	}

	public static EStatus GetStatusType(XmlNode xmlNode)
	{
		return PatheaScript.Util.GetInt(xmlNode, "stat") switch
		{
			-1 => EStatus.Any, 
			1 => EStatus.HitPoint, 
			2 => EStatus.Stamina, 
			3 => EStatus.Oxygen, 
			_ => EStatus.Max, 
		};
	}
}
