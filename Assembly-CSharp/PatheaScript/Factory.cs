using System;
using System.Collections.Generic;
using System.Xml;

namespace PatheaScript;

public abstract class Factory
{
	private class EventInfo
	{
		public string proxyClass;

		public string eventClass;

		public override string ToString()
		{
			return $"event class:{eventClass}, proxy class:{proxyClass}";
		}
	}

	private const string ScriptFileExt = "xml";

	private Dictionary<string, EventInfo> mDicEvent;

	private Dictionary<string, string> mDicCondition;

	private Dictionary<string, string> mDicAction;

	public abstract string ScriptDirPath { get; }

	public abstract int EntryScriptId { get; }

	protected void RegisterEvent(string name, string proxyClass, string eventClass)
	{
		EventInfo eventInfo = new EventInfo();
		eventInfo.proxyClass = proxyClass;
		eventInfo.eventClass = eventClass;
		if (mDicEvent == null)
		{
			mDicEvent = new Dictionary<string, EventInfo>(5);
		}
		if (mDicEvent.ContainsKey(name))
		{
			Debug.LogWarning(string.Concat("event:", name, " [", mDicEvent[name], "] will be replaced by [", eventInfo, "]"));
		}
		mDicEvent[name] = eventInfo;
	}

	protected void RegisterCondition(string name, string conditionClass)
	{
		if (mDicCondition == null)
		{
			mDicCondition = new Dictionary<string, string>(5);
		}
		if (mDicCondition.ContainsKey(name))
		{
			Debug.LogWarning("condition:" + name + " [" + mDicCondition[name] + "] will be replaced by [" + conditionClass + "]");
		}
		mDicCondition[name] = conditionClass;
	}

	protected void RegisterAction(string name, string actionClass)
	{
		if (mDicAction == null)
		{
			mDicAction = new Dictionary<string, string>(5);
		}
		if (mDicAction.ContainsKey(name))
		{
			Debug.LogWarning("condition:" + name + " [" + mDicAction[name] + "] will be replaced by [" + actionClass + "]");
		}
		mDicAction[name] = actionClass;
	}

	private object CreateByName(string className)
	{
		Type type = Type.GetType(className);
		if (type == null)
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}

	public EventProxy CreateEventProxy(string name)
	{
		if (mDicEvent != null && mDicEvent.ContainsKey(name) && CreateByName(mDicEvent[name].proxyClass) is EventProxy result)
		{
			return result;
		}
		throw new Exception("no corresponding Event proxy to " + name);
	}

	public Event CreateEvent(string name)
	{
		if (mDicEvent != null && mDicEvent.ContainsKey(name) && CreateByName(mDicEvent[name].eventClass) is Event result)
		{
			return result;
		}
		throw new Exception("no corresponding Event to " + name);
	}

	public Condition CreateCondition(string name)
	{
		if (mDicCondition != null && mDicCondition.ContainsKey(name) && CreateByName(mDicCondition[name]) is Condition result)
		{
			return result;
		}
		throw new Exception("no corresponding Condition to " + name);
	}

	public virtual Action CreateAction(string name)
	{
		if (mDicAction != null && mDicAction.ContainsKey(name) && CreateByName(mDicAction[name]) is Action result)
		{
			return result;
		}
		throw new Exception("no corresponding Action to " + name);
	}

	public virtual Compare GetCompare(XmlNode xmlNode, string name)
	{
		return Util.GetInt(xmlNode, name) switch
		{
			1 => new Greater(), 
			2 => new GreaterEqual(), 
			3 => new Equal(), 
			4 => new NotEqual(), 
			5 => new LesserEqual(), 
			6 => new Lesser(), 
			_ => null, 
		};
	}

	public virtual Functor GetFunctor(XmlNode xmlNode, string name)
	{
		return Util.GetInt(xmlNode, name) switch
		{
			0 => new FunctorSet(), 
			1 => new FunctorPlus(), 
			2 => new FunctorMinus(), 
			3 => new FunctorDivide(), 
			4 => new FunctorMultiply(), 
			5 => new FunctorMod(), 
			6 => new FunctorPower(), 
			7 => new FunctorNot(), 
			_ => null, 
		};
	}

	public virtual bool Init()
	{
		RegisterEvent("MISSION END", "PatheaScript.EventProxyScriptEnd", "PatheaScript.EventScriptEnd");
		RegisterEvent("MISSION BEGIN", "PatheaScript.EventProxyScriptBegin", "PatheaScript.EventScriptBegin");
		RegisterCondition("ALWAYS", "PatheaScript.ConditionAlways");
		RegisterCondition("CHECK VAR", "PatheaScript.ConditionCheckVar");
		RegisterCondition("SWITCH", "PatheaScript.ConditionSwitch");
		RegisterAction("RUN MISSION", "PatheaScript.ActionLoadScript");
		RegisterAction("END MISSION", "PatheaScript.ActionEndScript");
		RegisterAction("SET VAR", "PatheaScript.ActionSetVar");
		RegisterAction("SET SWITCH", "PatheaScript.ActionSetSwitch");
		return true;
	}

	public string GetScriptPath(int id)
	{
		return ScriptDirPath + id + ".xml";
	}
}
