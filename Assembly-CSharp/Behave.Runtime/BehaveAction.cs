using System;
using System.Reflection;

namespace Behave.Runtime;

public class BehaveAction : Attribute
{
	private Type m_Type;

	private Type m_DataType;

	private string m_Name;

	public string name => m_Name;

	public Type type => m_Type;

	public Type dataType => m_DataType;

	public BehaveAction(Type argType, string argName)
	{
		m_Type = argType;
		m_DataType = argType.GetNestedType("Data", BindingFlags.Instance | BindingFlags.NonPublic);
		m_Name = argName;
	}
}
