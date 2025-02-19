namespace ItemAsset;

public class Energy : OneFloat
{
	public PeFloatRangeNum energy => base.floatValue;

	public override float GetRawMax()
	{
		return itemObj.protoData.engergyMax;
	}
}
