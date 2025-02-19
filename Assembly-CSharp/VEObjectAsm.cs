using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;

public static class VEObjectAsm
{
	public class XMLMemberDesc
	{
		public string Name;

		public Type Type;

		public FieldInfo Field;

		public PropertyInfo Property;

		public int Order;

		public string Attr;

		public bool Necessary;

		public object DefaultValue;

		public static int Compare(XMLMemberDesc lhs, XMLMemberDesc rhs)
		{
			return lhs.Order - rhs.Order;
		}

		public object GetValue(object obj)
		{
			if (Field != null)
			{
				return Field.GetValue(obj);
			}
			if (Property != null)
			{
				return Property.GetValue(obj, null);
			}
			return null;
		}

		public void SetValue(object obj, object val)
		{
			if (Field != null)
			{
				Field.SetValue(obj, val);
			}
			else if (Property != null)
			{
				Property.SetValue(obj, val, null);
			}
		}
	}

	public class XMLMemberCollection
	{
		public string Name = string.Empty;

		public List<XMLMemberDesc> Members = new List<XMLMemberDesc>();
	}

	public static Type VEObjectBaseType;

	public static List<Type> VEObjectTypes;

	public static Dictionary<string, XMLMemberCollection> TypeDict;

	static VEObjectAsm()
	{
		VEObjectBaseType = typeof(VEObject);
		VEObjectTypes = null;
		TypeDict = null;
		Assembly assembly = Assembly.GetAssembly(VEObjectBaseType);
		Type[] types = assembly.GetTypes();
		VEObjectTypes = new List<Type>();
		Type[] array = types;
		foreach (Type type in array)
		{
			if (type.IsSubclassOf(VEObjectBaseType) && !type.IsAbstract)
			{
				VEObjectTypes.Add(type);
			}
		}
		TypeDict = new Dictionary<string, XMLMemberCollection>();
		foreach (Type vEObjectType in VEObjectTypes)
		{
			XMLMemberCollection xMLMemberCollection = new XMLMemberCollection
			{
				Name = vEObjectType.Name.ToUpper()
			};
			TypeDict[vEObjectType.FullName] = xMLMemberCollection;
			object[] customAttributes = vEObjectType.GetCustomAttributes(typeof(XMLObjectAttribute), inherit: false);
			if (customAttributes.Length > 0)
			{
				xMLMemberCollection.Name = (customAttributes[0] as XMLObjectAttribute).Name.ToUpper();
			}
			MemberInfo[] members = vEObjectType.GetMembers();
			MemberInfo[] array2 = members;
			foreach (MemberInfo memberInfo in array2)
			{
				FieldInfo fieldInfo = memberInfo as FieldInfo;
				PropertyInfo propertyInfo = memberInfo as PropertyInfo;
				if (fieldInfo != null || propertyInfo != null)
				{
					object[] customAttributes2 = memberInfo.GetCustomAttributes(typeof(XMLIOAttribute), inherit: true);
					XMLIOAttribute xMLIOAttribute = ((customAttributes2.Length <= 0) ? null : (customAttributes2[0] as XMLIOAttribute));
					if (xMLIOAttribute != null)
					{
						XMLMemberDesc item = new XMLMemberDesc
						{
							Name = memberInfo.Name,
							Type = ((fieldInfo == null) ? propertyInfo.PropertyType : fieldInfo.FieldType),
							Field = fieldInfo,
							Property = propertyInfo,
							Order = xMLIOAttribute.Order,
							Attr = xMLIOAttribute.Attr,
							Necessary = xMLIOAttribute.Necessary,
							DefaultValue = xMLIOAttribute.DefaultValue
						};
						xMLMemberCollection.Members.Add(item);
					}
				}
			}
			xMLMemberCollection.Members.Sort(XMLMemberDesc.Compare);
		}
	}

	public static string ToXML(VEObject obj)
	{
		string fullName = obj.GetType().FullName;
		if (!TypeDict.ContainsKey(fullName))
		{
			Debug.LogError("Object [" + obj.ID + "] ToXML failed: Unknown Type");
			return string.Empty;
		}
		XMLMemberCollection xMLMemberCollection = TypeDict[fullName];
		string text = "<" + xMLMemberCollection.Name + " ";
		foreach (XMLMemberDesc member in xMLMemberCollection.Members)
		{
			text += XMLIO.WriteValue(member.Attr, member.GetValue(obj), member.Type, member.Necessary, member.DefaultValue);
		}
		return text + "/>\r\n";
	}

	public static void Parse(VEObject obj, XmlElement xml)
	{
		string fullName = obj.GetType().FullName;
		if (!TypeDict.ContainsKey(fullName))
		{
			throw new Exception("Object [" + obj.ID + "] ToXML failed: Unknown Type");
		}
		XMLMemberCollection xMLMemberCollection = TypeDict[fullName];
		foreach (XMLMemberDesc member in xMLMemberCollection.Members)
		{
			object obj2 = XMLIO.ReadValue(xml, member.Attr, member.Type, member.Necessary, member.DefaultValue);
			if (obj2 != null)
			{
				member.SetValue(obj, obj2);
			}
		}
	}
}
