using Pathea;

namespace ItemAsset;

public class Property : Cmpt
{
	private float mFactor = 1f;

	public int buffId => base.protoData.buffId;

	public void SetFactor(float factor)
	{
		mFactor = factor;
	}

	public float GetProperty(AttribType property)
	{
		return GetRawProperty(property) * mFactor;
	}

	public float GetRawProperty(AttribType property)
	{
		return base.protoData.propertyList.GetProperty(property);
	}
}
