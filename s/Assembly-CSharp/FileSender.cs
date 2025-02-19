using System;
using System.Collections.Generic;

public class FileSender
{
	public int m_FileSize;

	private byte[] m_Data;

	public string m_FileName;

	public int m_Sended;

	private byte[] m_SendBuffer = new byte[10240];

	public string m_IndexInList;

	public FileSender(string fileName, byte[] data, ulong fileHandle, Dictionary<string, FileSender> fileSenderList, string skey)
	{
		m_FileName = fileName;
		m_FileSize = data.Length;
		m_Sended = 0;
		m_Data = data;
		fileSenderList[skey] = this;
		m_IndexInList = skey;
	}

	public byte[] ReadData(ref int count)
	{
		Array.Clear(m_SendBuffer, 0, m_SendBuffer.Length);
		if (m_Sended < m_FileSize)
		{
			if (m_FileSize - m_Sended >= m_SendBuffer.Length)
			{
				Array.Copy(m_Data, m_Sended, m_SendBuffer, 0, m_SendBuffer.Length);
				count = m_SendBuffer.Length;
			}
			else
			{
				Array.Copy(m_Data, m_Sended, m_SendBuffer, 0, m_FileSize - m_Sended);
				count = m_FileSize - m_Sended;
			}
			return m_SendBuffer;
		}
		count = 0;
		return null;
	}
}
