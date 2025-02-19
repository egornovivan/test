using System;
using System.Collections.Generic;
using System.Reflection;

namespace ScenarioRTL;

internal static class Asm
{
	internal class StatementObj
	{
		public string className;

		public Type type;

		public bool recyclable;
	}

	private static bool _initialised;

	private static Assembly _asm;

	private static Dictionary<string, StatementObj> _actionTypes;

	private static Dictionary<string, StatementObj> _conditionTypes;

	private static Dictionary<string, StatementObj> _eventListenerTypes;

	internal static void Init()
	{
		if (_initialised)
		{
			return;
		}
		Assembly assembly = Assembly.GetAssembly(typeof(Mission));
		Type[] types = assembly.GetTypes();
		_asm = assembly;
		_initialised = true;
		_actionTypes = new Dictionary<string, StatementObj>();
		_conditionTypes = new Dictionary<string, StatementObj>();
		_eventListenerTypes = new Dictionary<string, StatementObj>();
		Type[] array = types;
		foreach (Type type in array)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(StatementAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				StatementAttribute statementAttribute = customAttributes[0] as StatementAttribute;
				if (type.IsSubclassOf(typeof(Action)))
				{
					StatementObj statementObj = new StatementObj();
					statementObj.className = statementAttribute.className;
					statementObj.type = type;
					statementObj.recyclable = statementAttribute.recyclable;
					_actionTypes.Add(statementObj.className, statementObj);
				}
				else if (type.IsSubclassOf(typeof(Condition)))
				{
					StatementObj statementObj2 = new StatementObj();
					statementObj2.className = statementAttribute.className;
					statementObj2.type = type;
					statementObj2.recyclable = statementAttribute.recyclable;
					_conditionTypes.Add(statementObj2.className, statementObj2);
				}
				else if (type.IsSubclassOf(typeof(EventListener)))
				{
					StatementObj statementObj3 = new StatementObj();
					statementObj3.className = statementAttribute.className;
					statementObj3.type = type;
					statementObj3.recyclable = statementAttribute.recyclable;
					_eventListenerTypes.Add(statementObj3.className, statementObj3);
				}
			}
		}
	}

	internal static Action CreateActionInstance(string class_name)
	{
		if (!_actionTypes.ContainsKey(class_name))
		{
			return null;
		}
		return _asm.CreateInstance(_actionTypes[class_name].type.FullName) as Action;
	}

	internal static Condition CreateConditionInstance(string class_name)
	{
		if (!_conditionTypes.ContainsKey(class_name))
		{
			return null;
		}
		return _asm.CreateInstance(_conditionTypes[class_name].type.FullName) as Condition;
	}

	internal static EventListener CreateEventListenerInstance(string class_name)
	{
		if (!_eventListenerTypes.ContainsKey(class_name))
		{
			return null;
		}
		return _asm.CreateInstance(_eventListenerTypes[class_name].type.FullName) as EventListener;
	}

	internal static bool ActionIsRecyclable(string class_name)
	{
		if (!_actionTypes.ContainsKey(class_name))
		{
			return false;
		}
		return _actionTypes[class_name].recyclable;
	}
}
