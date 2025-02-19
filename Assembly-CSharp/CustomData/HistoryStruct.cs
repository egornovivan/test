using uLink;

namespace CustomData;

public class HistoryStruct
{
	public int m_Day;

	public string m_Value;

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		HistoryStruct historyStruct = new HistoryStruct();
		historyStruct.m_Day = stream.Read<int>(new object[0]);
		historyStruct.m_Value = stream.Read<string>(new object[0]);
		return historyStruct;
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		HistoryStruct historyStruct = (HistoryStruct)value;
		stream.Write(historyStruct.m_Day);
		stream.Write(historyStruct.m_Value);
	}
}
