using System;
using Boo.Lang.Runtime;
using Mono.Data.SqliteClient;
using UnityScript.Lang;

[Serializable]
public class SqliteAccess
{
	private string connection;

	private SqliteConnection dbcon;

	private SqliteCommand dbcmd;

	private SqliteDataReader reader;

	public SqliteAccess(string p)
	{
		OpenDB(p);
	}

	public virtual void OpenDB(string p)
	{
		connection = "URI=file:" + p;
		dbcon = new SqliteConnection(connection);
		dbcon.Open();
	}

	public virtual SqliteDataReader ExecuteQuery(string q)
	{
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = q;
		reader = dbcmd.ExecuteReader();
		return reader;
	}

	public virtual UnityScript.Lang.Array GetQueryResult()
	{
		UnityScript.Lang.Array array = new UnityScript.Lang.Array();
		while (reader.Read())
		{
			UnityScript.Lang.Array array2 = new UnityScript.Lang.Array();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				array2.Add(reader.GetValue(i));
			}
			array.Add(array2);
		}
		return array;
	}

	public virtual UnityScript.Lang.Array GetQueryResultSingle()
	{
		UnityScript.Lang.Array array = new UnityScript.Lang.Array();
		while (reader.Read())
		{
			array.Add(reader.GetValue(0));
		}
		return array;
	}

	public virtual SqliteDataReader ReadFullTable(string tableName)
	{
		string text = null;
		text = "SELECT * FROM " + tableName;
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = text;
		reader = dbcmd.ExecuteReader();
		return reader;
	}

	public virtual void DeleteTableContents(string tableName)
	{
		string text = null;
		text = "DELETE FROM " + tableName;
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = text;
		reader = dbcmd.ExecuteReader();
	}

	public virtual void CreateTable(string name, UnityScript.Lang.Array col, UnityScript.Lang.Array colType)
	{
		if (!RuntimeServices.EqualityOperator(UnityRuntimeServices.GetProperty(col, "Length"), UnityRuntimeServices.GetProperty(colType, "Length")))
		{
			throw new SqliteSyntaxException("columns.Length != colType.Length");
		}
		string text = null;
		object obj = RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", "CREATE TABLE " + name + "(", col[0]), " "), colType[0]);
		if (!(obj is string))
		{
			obj = RuntimeServices.Coerce(obj, typeof(string));
		}
		text = (string)obj;
		for (int i = 1; i < col.length; i++)
		{
			object obj2 = RuntimeServices.InvokeBinaryOperator("op_Addition", text, RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", ", ", col[i]), " "), colType[i]));
			if (!(obj2 is string))
			{
				obj2 = RuntimeServices.Coerce(obj2, typeof(string));
			}
			text = (string)obj2;
		}
		text += ")";
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = text;
		reader = dbcmd.ExecuteReader();
	}

	public virtual void InsertInto(string tableName, UnityScript.Lang.Array values)
	{
		string text = null;
		object obj = RuntimeServices.InvokeBinaryOperator("op_Addition", "INSERT INTO " + tableName + " VALUES (", values[0]);
		if (!(obj is string))
		{
			obj = RuntimeServices.Coerce(obj, typeof(string));
		}
		text = (string)obj;
		for (int i = 1; i < values.length; i++)
		{
			object obj2 = RuntimeServices.InvokeBinaryOperator("op_Addition", text, RuntimeServices.InvokeBinaryOperator("op_Addition", ", ", values[i]));
			if (!(obj2 is string))
			{
				obj2 = RuntimeServices.Coerce(obj2, typeof(string));
			}
			text = (string)obj2;
		}
		text += ")";
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = text;
		reader = dbcmd.ExecuteReader();
	}

	public virtual void InsertIntoSpecific(string tableName, UnityScript.Lang.Array cols, UnityScript.Lang.Array values)
	{
		if (!RuntimeServices.EqualityOperator(UnityRuntimeServices.GetProperty(cols, "Length"), UnityRuntimeServices.GetProperty(values, "Length")))
		{
			throw new SqliteSyntaxException("columns.Length != values.Length");
		}
		string text = null;
		object obj = RuntimeServices.InvokeBinaryOperator("op_Addition", "INSERT INTO " + tableName + "(", cols[0]);
		if (!(obj is string))
		{
			obj = RuntimeServices.Coerce(obj, typeof(string));
		}
		text = (string)obj;
		for (int i = 1; i < cols.length; i++)
		{
			object obj2 = RuntimeServices.InvokeBinaryOperator("op_Addition", text, RuntimeServices.InvokeBinaryOperator("op_Addition", ", ", cols[i]));
			if (!(obj2 is string))
			{
				obj2 = RuntimeServices.Coerce(obj2, typeof(string));
			}
			text = (string)obj2;
		}
		object obj3 = RuntimeServices.InvokeBinaryOperator("op_Addition", text, RuntimeServices.InvokeBinaryOperator("op_Addition", ") VALUES (", values[0]));
		if (!(obj3 is string))
		{
			obj3 = RuntimeServices.Coerce(obj3, typeof(string));
		}
		text = (string)obj3;
		for (int i = 1; i < values.length; i++)
		{
			object obj4 = RuntimeServices.InvokeBinaryOperator("op_Addition", text, RuntimeServices.InvokeBinaryOperator("op_Addition", ", ", values[i]));
			if (!(obj4 is string))
			{
				obj4 = RuntimeServices.Coerce(obj4, typeof(string));
			}
			text = (string)obj4;
		}
		text += ")";
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = text;
		reader = dbcmd.ExecuteReader();
	}

	public virtual void InsertIntoSpecificSingle(string tableName, string col, string values)
	{
		string text = null;
		text = "INSERT INTO " + tableName + "(" + col + ")" + " VALUES (" + values + ")";
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = text;
		reader = dbcmd.ExecuteReader();
	}

	public virtual SqliteDataReader SelectWhere(string tableName, UnityScript.Lang.Array itemSToSelect, UnityScript.Lang.Array wCols, UnityScript.Lang.Array wOpers, UnityScript.Lang.Array wValues)
	{
		if (!RuntimeServices.EqualityOperator(UnityRuntimeServices.GetProperty(wCols, "Length"), UnityRuntimeServices.GetProperty(wOpers, "Length")) || !RuntimeServices.EqualityOperator(UnityRuntimeServices.GetProperty(wOpers, "Length"), UnityRuntimeServices.GetProperty(wValues, "Length")))
		{
			throw new SqliteSyntaxException("col.Length != operation.Length != values.Length");
		}
		object obj = null;
		obj = RuntimeServices.InvokeBinaryOperator("op_Addition", "SELECT ", itemSToSelect[0]);
		for (int i = 1; RuntimeServices.ToBool(RuntimeServices.InvokeBinaryOperator("op_LessThan", i, UnityRuntimeServices.GetProperty(itemSToSelect, "Length"))); i++)
		{
			obj = RuntimeServices.InvokeBinaryOperator("op_Addition", obj, RuntimeServices.InvokeBinaryOperator("op_Addition", ", ", itemSToSelect[i]));
		}
		obj = RuntimeServices.InvokeBinaryOperator("op_Addition", obj, RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", " FROM " + tableName + " WHERE ", wCols[0]), wOpers[0]), "'"), wValues[0]), "' "));
		for (int i = 1; RuntimeServices.ToBool(RuntimeServices.InvokeBinaryOperator("op_LessThan", i, UnityRuntimeServices.GetProperty(wCols, "Length"))); i++)
		{
			obj = RuntimeServices.InvokeBinaryOperator("op_Addition", obj, RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", RuntimeServices.InvokeBinaryOperator("op_Addition", " AND ", wCols[i]), wOpers[i]), "'"), wValues[0]), "' "));
		}
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		SqliteCommand sqliteCommand = dbcmd;
		object obj2 = obj;
		if (!(obj2 is string))
		{
			obj2 = RuntimeServices.Coerce(obj2, typeof(string));
		}
		sqliteCommand.CommandText = (string)obj2;
		reader = dbcmd.ExecuteReader();
		return reader;
	}

	public virtual SqliteDataReader SelectWhereSingle(string tableName, string itemToSelect, string wCol, string wOper, string wValue)
	{
		string text = null;
		text = "SELECT " + itemToSelect + " FROM " + tableName + " WHERE " + wCol + wOper + wValue;
		dbcmd = (SqliteCommand)dbcon.CreateCommand();
		dbcmd.CommandText = text;
		reader = dbcmd.ExecuteReader();
		return reader;
	}

	public virtual void CloseDB()
	{
		reader.Close();
		reader = null;
		dbcmd.Dispose();
		dbcmd = null;
		dbcon.Close();
		dbcon = null;
	}
}
