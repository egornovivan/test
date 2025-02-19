using System;
using System.Data;
using Mono.Data.SqliteClient;

public class PEDbOp
{
	private SqliteCommand _cmd;

	private Action<SqliteDataReader> _readerHandler;

	private byte _readOrWrite;

	public void BindParam(string key, object value)
	{
		_cmd.Parameters.Add(key, value);
	}

	public SqliteParameter BindParam(string key, DbType type)
	{
		return _cmd.Parameters.Add(key, type);
	}

	public void BindReaderHandler(Action<SqliteDataReader> handler)
	{
		_readerHandler = handler;
	}

	public void SetSqliteCmd(SqliteCommand cmd)
	{
		_cmd = cmd;
	}

	public void SetCmdText(string cmdText)
	{
		_cmd.CommandText = cmdText;
	}

	public void SetReadOrWrite(byte type)
	{
		_readOrWrite = type;
	}

	private void ExecReader()
	{
		AsyncSqlite.ExecuteReader(_cmd, _readerHandler);
	}

	private void ExecWriter()
	{
		AsyncSqlite.ExecuteNonQuery(_cmd);
	}

	public void Exec()
	{
		if (_readOrWrite == 0)
		{
			ExecReader();
		}
		else
		{
			ExecWriter();
		}
		Dispose();
	}

	public void Dispose()
	{
		if (!object.Equals(_cmd, null))
		{
			_cmd.Dispose();
			_cmd = null;
		}
	}
}
