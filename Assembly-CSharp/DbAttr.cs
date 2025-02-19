using System;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using UnityEngine;

public class DbAttr
{
	private const int c_cntAttribType = 97;

	public float[] attributeArray = new float[97];

	private static readonly int[] c_scalableAttrIndices = new int[4] { 0, 1, 25, 27 };

	public DbAttr Clone()
	{
		DbAttr dbAttr = new DbAttr();
		Array.Copy(attributeArray, dbAttr.attributeArray, 97);
		return dbAttr;
	}

	public void ReadFromDb(SqliteDataReader reader)
	{
		for (int i = 0; i < 97; i++)
		{
			AttribType attribType = (AttribType)i;
			string fieldName = attribType.ToString();
			try
			{
				attributeArray[i] = Db.GetFloat(reader, fieldName);
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
				attributeArray[i] = 0f;
			}
		}
	}

	public PESkEntity.Attr[] ToAliveAttr()
	{
		PESkEntity.Attr[] array = new PESkEntity.Attr[97];
		for (int i = 0; i < 97; i++)
		{
			array[i] = new PESkEntity.Attr
			{
				m_Type = (AttribType)i,
				m_Value = attributeArray[i]
			};
		}
		return array;
	}

	public PESkEntity.Attr[] ToAliveAttrWithScale(float fScale)
	{
		PESkEntity.Attr[] array = new PESkEntity.Attr[97];
		for (int i = 0; i < 97; i++)
		{
			array[i] = new PESkEntity.Attr
			{
				m_Type = (AttribType)i,
				m_Value = attributeArray[i]
			};
		}
		for (int j = 0; j < c_scalableAttrIndices.Length; j++)
		{
			int num = c_scalableAttrIndices[j];
			array[num].m_Value *= fScale;
		}
		return array;
	}
}
