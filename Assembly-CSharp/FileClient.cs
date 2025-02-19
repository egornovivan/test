using System;
using System.IO;
using System.Net.Sockets;

public class FileClient : IDisposable
{
	private TcpClient _client;

	private Stream _stream;

	internal FileData _fileData = new FileData();

	private byte[] _buffer = new byte[FileConst.BlockSize];

	private string _path;

	private string _ext;

	private string _account;

	private long _pos;

	private EFileStatus _status;

	private bool _disposed;

	internal TcpClient Client => _client;

	internal string FilePath => _path;

	internal string FileExt => _ext;

	internal long Position => _pos;

	internal ulong HashCode => _fileData.HashCode;

	internal string FileName => _fileData.FileName;

	internal long FileLength => _fileData.FileLength;

	internal string Account => _account;

	internal event CommonEventHandler SendFileEvent;

	internal event CommonEventHandler DisposedEvent;

	internal event CommonEventHandler CompleteEvent;

	internal event CommonEventHandler DataReceivedEvent;

	internal FileClient()
	{
		_status = EFileStatus.NULL;
		_disposed = false;
		this.CompleteEvent = (CommonEventHandler)Delegate.Combine(this.CompleteEvent, new CommonEventHandler(OnComplete));
	}

	internal void SetPath(string path, string ext)
	{
		_ext = ext;
		_path = path;
	}

	internal void SetAccount(string account)
	{
		_account = account;
	}

	private void Receive()
	{
		try
		{
			if (this.DataReceivedEvent == null)
			{
				throw new Exception("null == DataReceivedEvent");
			}
			this.DataReceivedEvent(this, null);
		}
		catch (Exception ex)
		{
			Close();
			LogManager.Error(ex);
		}
	}

	internal void SendFile(string filePath, string host, int port)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(filePath);
			SendFile(fileInfo, host, port);
		}
		catch (Exception ex)
		{
			Close();
			LogManager.Error(ex);
		}
	}

	internal void SendFile(FileInfo fileInfo, string host, int port)
	{
		try
		{
			_fileData.FileName = fileInfo.Name;
			_fileData.FileLength = fileInfo.Length;
			_stream = fileInfo.OpenRead();
			_client = new TcpClient();
			_client.BeginConnect(host, port, EndSendConnect, null);
			LogManager.Info("Connect to the iso server ip:", host);
		}
		catch (Exception ex)
		{
			Close();
			LogManager.Error(ex);
		}
	}

	internal void SendFile(string name, byte[] data, string host, int port)
	{
		try
		{
			_status = EFileStatus.OPEN;
			_fileData.FileLength = data.Length;
			_fileData.FileName = name + ".~vcres";
			_stream = new MemoryStream(data);
			_client = new TcpClient();
			_client.BeginConnect(host, port, EndSendConnect, null);
			LogManager.Info("Connect to the iso server ip:", host);
		}
		catch (Exception ex)
		{
			Close();
			LogManager.Error(ex);
		}
	}

	private void EndSendConnect(IAsyncResult ar)
	{
		try
		{
			_client.EndConnect(ar);
			_status = EFileStatus.CHECKING;
			_fileData.HashCode = CRC64.Compute(_stream);
			_stream.Position = 0L;
			_status = EFileStatus.SENDING;
			NetworkStream stream = _client.GetStream();
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			byte value = 221;
			binaryWriter.Write(value);
			binaryWriter.Write(_fileData.HashCode);
			binaryWriter.Write(_fileData.FileName);
			binaryWriter.Write(_fileData.FileLength);
			binaryWriter.Write(_account);
			ReadStream();
		}
		catch (Exception ex)
		{
			Close();
			LogManager.Error(ex);
		}
	}

	private void ReadStream()
	{
		try
		{
			int num = _stream.Read(_buffer, 0, FileConst.BlockSize);
			if (num == 0)
			{
				if (this.CompleteEvent != null)
				{
					this.CompleteEvent(this, new FileCompleteEventArgs(success: true));
				}
			}
			else
			{
				_pos += num;
				NetworkStream stream = _client.GetStream();
				BinaryWriter binaryWriter = new BinaryWriter(stream);
				binaryWriter.Write(_buffer, 0, num);
				ReadStream();
			}
		}
		catch (Exception ex)
		{
			Close();
			LogManager.Error(ex);
		}
	}

	private void OnComplete(object sender, EventArgs args)
	{
		Close();
	}

	internal void DownloadFile(byte head, ulong hashCode, string host, int port)
	{
		try
		{
			_fileData.HashCode = hashCode;
			_client = new TcpClient();
			_client.BeginConnect(host, port, EndRecvConnect, head);
			LogManager.Info("Connect to the iso server ip:", host);
		}
		catch (Exception ex)
		{
			Close();
			throw ex;
		}
	}

	private void EndRecvConnect(IAsyncResult ar)
	{
		try
		{
			byte value = (byte)ar.AsyncState;
			_client.EndConnect(ar);
			NetworkStream stream = _client.GetStream();
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(value);
			binaryWriter.Write(_fileData.HashCode);
			Receive();
		}
		catch (Exception ex)
		{
			Close();
			throw ex;
		}
	}

	internal void ReceiveFile()
	{
		try
		{
			NetworkStream stream = _client.GetStream();
			BinaryReader binaryReader = new BinaryReader(stream);
			_fileData.HashCode = binaryReader.ReadUInt64();
			_fileData.FileName = binaryReader.ReadString();
			_fileData.FileLength = binaryReader.ReadInt64();
			string fileName = _path + _fileData.FileName + ".tmp";
			FileInfo fileInfo = new FileInfo(fileName);
			_stream = fileInfo.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
			stream.BeginRead(_buffer, 0, FileConst.BlockSize, RecveiveCallback, null);
		}
		catch (Exception ex)
		{
			Close();
			throw ex;
		}
	}

	private void RecveiveCallback(IAsyncResult ar)
	{
		try
		{
			NetworkStream stream = _client.GetStream();
			int num = stream.EndRead(ar);
			_stream.Write(_buffer, 0, num);
			_pos += num;
			if (_pos < _fileData.FileLength)
			{
				stream.BeginRead(_buffer, 0, FileConst.BlockSize, RecveiveCallback, null);
				return;
			}
			_stream.Position = 0L;
			ulong num2 = CRC64.Compute(_stream);
			bool flag = num2 == _fileData.HashCode;
			if (_stream != null)
			{
				_stream.Close();
				_stream = null;
			}
			if (flag)
			{
				string text = _path + _fileData.FileName;
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				string sourceFileName = _path + _fileData.FileName + ".tmp";
				File.Move(sourceFileName, text);
			}
			if (this.CompleteEvent != null)
			{
				this.CompleteEvent(this, new FileCompleteEventArgs(flag));
			}
		}
		catch (Exception ex)
		{
			Close();
			throw ex;
		}
	}

	internal void Close()
	{
		Dispose();
	}

	protected virtual void Dispose(bool dispose)
	{
		_disposed = true;
		if (this.DisposedEvent != null)
		{
			this.DisposedEvent(this, null);
		}
		if (_stream != null)
		{
			_stream.Close();
			_stream = null;
		}
		if (_client != null)
		{
			_client.Client.Close();
			_client.Close();
			_client = null;
		}
	}

	public void Dispose()
	{
		Dispose(dispose: true);
	}
}
