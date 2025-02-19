using System;
using System.Collections.Generic;

public class FileSender
{
	public ulong m_FileHandle;

	public int m_FileSize;

	public byte[] m_Data;

	public string m_FileName;

	public int m_Sended;

	public string m_IndexInList;

	public FileSender(string fileName, Dictionary<string, FileSender> fileSenderList, ulong fileHandle, int fileSize, string skey)
	{
		m_FileName = fileName;
		m_FileSize = fileSize;
		m_Sended = 0;
		m_FileHandle = fileHandle;
		fileSenderList[skey] = this;
		m_IndexInList = skey;
		m_Data = new byte[fileSize];
	}

	public void WriteData(byte[] data, int count, ref bool bFinish)
	{
		Array.Copy(data, 0, m_Data, m_Sended, count);
		m_Sended += count;
		if (m_Data.Length == m_Sended)
		{
			bFinish = true;
		}
		else
		{
			bFinish = false;
		}
	}
}
