using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace PETools;

public static class DbReader
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DbFieldAttribute : Attribute
	{
		public string FieldName;

		public bool EnumValue;

		public DbFieldAttribute(string fieldName, bool enumValue = false)
		{
			FieldName = fieldName;
			EnumValue = enumValue;
		}
	}

	public static List<T> Read<T>(string tableName, int capacity = 20) where T : class, new()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable(tableName);
		if (sqliteDataReader == null)
		{
			return null;
		}
		return Read<T>(sqliteDataReader, capacity);
	}

	public static List<T> Read<T>(SqliteDataReader reader, int capacity = 20) where T : class, new()
	{
		GetFieldProperty<T>();
		Type typeFromHandle = typeof(T);
		FieldInfo[] fieldInfos = GetFieldInfos(typeFromHandle);
		PropertyInfo[] propertyInfos = GetPropertyInfos(typeFromHandle);
		List<T> list = new List<T>(capacity);
		while (reader.Read())
		{
			T item = BuildItem<T>(reader, fieldInfos, propertyInfos);
			list.Add(item);
		}
		return list;
	}

	public static T ReadItem<T>(SqliteDataReader reader) where T : class, new()
	{
		Type typeFromHandle = typeof(T);
		FieldInfo[] fieldInfos = GetFieldInfos(typeFromHandle);
		PropertyInfo[] propertyInfos = GetPropertyInfos(typeFromHandle);
		return BuildItem<T>(reader, fieldInfos, propertyInfos);
	}

	private static MemberInfo[] GetFieldProperty<T>() where T : class, new()
	{
		Type typeFromHandle = typeof(T);
		MemberInfo[] members = typeFromHandle.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		typeFromHandle.GetFields(BindingFlags.Instance);
		return members;
	}

	private static FieldInfo[] GetFieldInfos(Type t)
	{
		return t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	private static PropertyInfo[] GetPropertyInfos(Type t)
	{
		return t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	private static DbFieldAttribute GetDbFieldAttribute(MemberInfo memInfo)
	{
		object[] customAttributes = memInfo.GetCustomAttributes(typeof(DbFieldAttribute), inherit: true);
		return (customAttributes.Length <= 0) ? null : (customAttributes[0] as DbFieldAttribute);
	}

	private static T BuildItem<T>(SqliteDataReader reader, FieldInfo[] fieldInfos, PropertyInfo[] propertyInfos) where T : class, new()
	{
		T val = new T();
		foreach (FieldInfo fieldInfo in fieldInfos)
		{
			DbFieldAttribute dbFieldAttribute = GetDbFieldAttribute(fieldInfo);
			if (dbFieldAttribute != null)
			{
				object value = GetValue(reader, fieldInfo.FieldType, dbFieldAttribute);
				fieldInfo.SetValue(val, value);
			}
		}
		foreach (PropertyInfo propertyInfo in propertyInfos)
		{
			DbFieldAttribute dbFieldAttribute2 = GetDbFieldAttribute(propertyInfo);
			if (dbFieldAttribute2 != null)
			{
				object value2 = GetValue(reader, propertyInfo.PropertyType, dbFieldAttribute2);
				propertyInfo.SetValue(val, value2, null);
			}
		}
		return val;
	}

	private static object GetValue(SqliteDataReader reader, Type fieldType, DbFieldAttribute attr)
	{
		object result = null;
		if (fieldType == typeof(int))
		{
			result = Db.GetInt(reader, attr.FieldName);
		}
		else if (fieldType == typeof(int[]))
		{
			result = Db.GetIntArray(reader, attr.FieldName);
		}
		else if (fieldType == typeof(float))
		{
			result = Db.GetFloat(reader, attr.FieldName);
		}
		else if (fieldType == typeof(float[]))
		{
			result = Db.GetFloatArray(reader, attr.FieldName);
		}
		else if (fieldType == typeof(string))
		{
			result = Db.GetString(reader, attr.FieldName);
		}
		else if (fieldType == typeof(bool))
		{
			result = Db.GetBool(reader, attr.FieldName);
		}
		else if (fieldType == typeof(Color))
		{
			result = Db.GetColor(reader, attr.FieldName);
		}
		else if (fieldType.IsEnum)
		{
			result = ((!attr.EnumValue) ? Enum.Parse(fieldType, Db.GetString(reader, attr.FieldName)) : ((object)Db.GetInt(reader, attr.FieldName)));
		}
		else if (fieldType == typeof(Vector3))
		{
			result = Db.GetVector3(reader, attr.FieldName);
		}
		else if (fieldType == typeof(Vector3[]))
		{
			result = Db.GetVector3Array(reader, attr.FieldName);
		}
		else if (fieldType == typeof(Quaternion))
		{
			result = Db.GetQuaternion(reader, attr.FieldName);
		}
		else if (fieldType == typeof(Quaternion[]))
		{
			result = Db.GetQuaternionArray(reader, attr.FieldName);
		}
		else
		{
			Debug.LogError("not supported value type:" + fieldType);
		}
		return result;
	}
}
