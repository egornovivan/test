using System;
using UnityEngine;

namespace DunGen;

[Serializable]
public sealed class SerializableType
{
	[SerializeField]
	private string typeName;

	public Type Type
	{
		get
		{
			return (!string.IsNullOrEmpty(typeName)) ? Type.GetType(typeName) : null;
		}
		set
		{
			typeName = ((value != null) ? value.AssemblyQualifiedName : string.Empty);
		}
	}

	public SerializableType()
	{
	}

	public SerializableType(Type type)
	{
		Type = type;
	}

	public SerializableType(string assemblyQualifiedName)
	{
		typeName = assemblyQualifiedName;
	}

	public static implicit operator Type(SerializableType serializableType)
	{
		return serializableType.Type;
	}

	public static implicit operator SerializableType(Type type)
	{
		return new SerializableType(type);
	}
}
