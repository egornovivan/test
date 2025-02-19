public class PEAT_Event_IntArray : PEAT_Event
{
	public int[] intValues { get; set; }

	public void OnIntEvent(int value)
	{
		for (int i = 0; i < intValues.Length; i++)
		{
			if (intValues[i] == value)
			{
				base.validEvent = true;
				break;
			}
		}
	}
}
