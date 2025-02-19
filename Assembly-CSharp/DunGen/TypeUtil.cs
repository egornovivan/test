using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DunGen.Adapters;

namespace DunGen;

public static class TypeUtil
{
	public static bool GetAdapterTypesInfo(Type parentType, out Type[] types, out string[] names, bool includeEmptySlotAtBeginning = false)
	{
		List<Type> list = parentType.GetValidSubtypes().ToList();
		if (includeEmptySlotAtBeginning)
		{
			list.Insert(0, null);
		}
		types = list.ToArray();
		names = new string[types.Length];
		for (int i = 0; i < names.Length; i++)
		{
			Type type = types[i];
			if (type == null)
			{
				names[i] = "None";
				continue;
			}
			AdapterDisplayName adapterDisplayName = type.GetCustomAttributes(typeof(AdapterDisplayName), inherit: false).FirstOrDefault() as AdapterDisplayName;
			names[i] = ((adapterDisplayName == null) ? StringUtil.SplitCamelCase(type.Name) : adapterDisplayName.Name);
		}
		return (!includeEmptySlotAtBeginning) ? (types.Length > 0) : (types.Length > 1);
	}

	public static bool IsValidSubtypeOf(this Type childType, Type parentType)
	{
		if (childType == null || parentType == null)
		{
			return false;
		}
		return !childType.IsAbstract && parentType.IsAssignableFrom(childType);
	}

	public static IEnumerable<Type> GetValidSubtypes(this Type parentType)
	{
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (type.IsValidSubtypeOf(parentType))
			{
				yield return type;
			}
		}
	}
}
