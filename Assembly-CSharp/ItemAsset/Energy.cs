namespace ItemAsset;

public class Energy : OneFloat
{
	public PeFloatRangeNum energy => base.floatValue;

	public override float GetRawMax()
	{
		return itemObj.protoData.engergyMax;
	}

	public override string ProcessTooltip(string text)
	{
		string text2 = base.ProcessTooltip(text);
		text2 = text2.Replace("$powerMax$", ((int)base.valueMax).ToString());
		return text2.Replace("$100000006$", ((int)energy.current).ToString());
	}
}
