using System;
using System.Reflection;

namespace WhiteCat.ReflectionExtension;

public static class ReflectionExtension
{
	public static FieldInfo GetFieldInfo(this object instance, string fieldName)
	{
		Type type = instance.GetType();
		FieldInfo fieldInfo = null;
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		while (type != null)
		{
			fieldInfo = type.GetField(fieldName, bindingAttr);
			if (fieldInfo != null)
			{
				return fieldInfo;
			}
			type = type.BaseType;
		}
		return null;
	}

	public static PropertyInfo GetPropertyInfo(this object instance, string propertyName)
	{
		Type type = instance.GetType();
		PropertyInfo propertyInfo = null;
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		while (type != null)
		{
			propertyInfo = type.GetProperty(propertyName, bindingAttr);
			if (propertyInfo != null)
			{
				return propertyInfo;
			}
			type = type.BaseType;
		}
		return null;
	}

	public static MethodInfo GetMethodInfo(this object instance, string methodName)
	{
		Type type = instance.GetType();
		MethodInfo methodInfo = null;
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		while (type != null)
		{
			methodInfo = type.GetMethod(methodName, bindingAttr);
			if (methodInfo != null)
			{
				return methodInfo;
			}
			type = type.BaseType;
		}
		return null;
	}

	public static object GetFieldValue(this object instance, string fieldName)
	{
		return instance.GetFieldInfo(fieldName).GetValue(instance);
	}

	public static void SetFieldValue(this object instance, string fieldName, object value)
	{
		instance.GetFieldInfo(fieldName).SetValue(instance, value);
	}

	public static object GetPropertyValue(this object instance, string propertyName)
	{
		return instance.GetPropertyInfo(propertyName).GetValue(instance, null);
	}

	public static void SetPropertyValue(this object instance, string propertyName, object value)
	{
		instance.GetPropertyInfo(propertyName).SetValue(instance, value, null);
	}

	public static object InvokeMethod(this object instance, string methodName, params object[] param)
	{
		return instance.GetMethodInfo(methodName).Invoke(instance, param);
	}
}
