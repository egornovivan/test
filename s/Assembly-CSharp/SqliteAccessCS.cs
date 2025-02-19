using System.Collections;
using Mono.Data.SqliteClient;
using UnityEngine;

public class SqliteAccessCS
{
	private SqliteConnection dbConnection;

	private SqliteCommand dbCommand;

	private SqliteDataReader reader;

	public SqliteAccessCS(string connectionString)
	{
		OpenDB(connectionString);
	}

	public void OpenDB(string connectionString)
	{
		dbConnection = new SqliteConnection("URI=file:" + connectionString);
		dbConnection.Open();
		Debug.Log("Connected to db");
	}

	public SqliteDataReader ExecuteQuery(string sqlQuery)
	{
		dbCommand = (SqliteCommand)dbConnection.CreateCommand();
		dbCommand.CommandText = sqlQuery;
		reader = dbCommand.ExecuteReader();
		return reader;
	}

	public ArrayList GetQueryResult()
	{
		ArrayList arrayList = new ArrayList();
		while (reader.Read())
		{
			ArrayList arrayList2 = new ArrayList();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				arrayList2.Add(reader.GetValue(i));
			}
			arrayList.Add(arrayList2);
		}
		return arrayList;
	}

	public ArrayList GetQueryResultSingle()
	{
		ArrayList arrayList = new ArrayList();
		while (reader.Read())
		{
			arrayList.Add(reader.GetValue(0));
		}
		return arrayList;
	}

	public SqliteDataReader ReadFullTable(string tableName)
	{
		string sqlQuery = "SELECT * FROM " + tableName;
		return ExecuteQuery(sqlQuery);
	}

	public SqliteDataReader DeleteTableContents(string tableName)
	{
		string sqlQuery = "DELETE FROM " + tableName;
		return ExecuteQuery(sqlQuery);
	}

	public SqliteDataReader CreateTable(string name, string[] col, string[] colType)
	{
		if (col.Length != colType.Length)
		{
			throw new SqliteSyntaxException("columns.Length != colType.Length");
		}
		string text = "CREATE TABLE " + name + " (" + col[0] + " " + colType[0];
		for (int i = 1; i < col.Length; i++)
		{
			string text2 = text;
			text = text2 + ", " + col[i] + " " + colType[i];
		}
		text += ")";
		return ExecuteQuery(text);
	}

	public SqliteDataReader InsertInto(string tableName, string[] values)
	{
		string text = "INSERT INTO " + tableName + " VALUES (" + '"' + values[0] + '"';
		for (int i = 1; i < values.Length; i++)
		{
			string text2 = text;
			text = text2 + ", " + '"' + values[i] + '"';
		}
		text += ")";
		return ExecuteQuery(text);
	}

	public SqliteDataReader InsertIntoSpecific(string tableName, string[] cols, string[] values)
	{
		if (cols.Length != values.Length)
		{
			throw new SqliteSyntaxException("columns.Length != values.Length");
		}
		string text = "INSERT INTO " + tableName + "(" + cols[0];
		for (int i = 1; i < cols.Length; i++)
		{
			text = text + ", " + cols[i];
		}
		text = text + ") VALUES (" + values[0];
		for (int j = 1; j < values.Length; j++)
		{
			text = text + ", " + values[j];
		}
		text += ")";
		return ExecuteQuery(text);
	}

	public SqliteDataReader InsertIntoSpecificSingle(string tableName, string col, string values)
	{
		string sqlQuery = "INSERT INTO " + tableName + "(" + col + ") VALUES (" + values + ")";
		return ExecuteQuery(sqlQuery);
	}

	public SqliteDataReader SelectWhere(string tableName, string[] items, string[] cols, string[] operation, string[] values)
	{
		if (cols.Length != operation.Length || operation.Length != values.Length)
		{
			throw new SqliteSyntaxException("col.Length != operation.Length != values.Length");
		}
		string text = "SELECT " + '"' + items[0] + '"';
		string text2;
		for (int i = 1; i < items.Length; i++)
		{
			text2 = text;
			text = text2 + ", " + '"' + items[i] + '"';
		}
		text2 = text;
		text = text2 + " FROM " + tableName + " WHERE " + cols[0] + operation[0] + '"' + values[0] + '"';
		for (int j = 1; j < cols.Length; j++)
		{
			text2 = text;
			text = text2 + " AND " + cols[j] + operation[j] + '"' + values[0] + '"';
		}
		return ExecuteQuery(text);
	}

	public SqliteDataReader SelectWhereSingle(string tableName, string item, string col, string operation, string values)
	{
		string sqlQuery = "SELECT " + item + " FROM " + tableName + " WHERE " + col + operation + values;
		return ExecuteQuery(sqlQuery);
	}

	public void CloseDB()
	{
		if (dbCommand != null)
		{
			dbCommand.Dispose();
		}
		dbCommand = null;
		if (reader != null)
		{
			reader.Dispose();
		}
		reader = null;
		if (dbConnection != null)
		{
			dbConnection.Close();
		}
		dbConnection = null;
		Debug.Log("Disconnected from db.");
	}
}
