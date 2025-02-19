namespace CSRecord;

public class CSProcessingData : CSObjectData
{
	public bool isAuto;

	public ProcessingTask[] mTaskTable;

	public int TaskCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < mTaskTable.Length; i++)
			{
				if (HasLine(i))
				{
					num++;
				}
			}
			return num;
		}
	}

	public CSProcessingData()
	{
		dType = 9;
		isAuto = false;
		mTaskTable = new ProcessingTask[4];
	}

	public bool HasLine(int index)
	{
		if (mTaskTable[index] == null)
		{
			return false;
		}
		if (mTaskTable[index].itemList == null)
		{
			return false;
		}
		if (mTaskTable[index].itemList.Find((ItemIdCount item) => item != null) == null)
		{
			return false;
		}
		return true;
	}
}
