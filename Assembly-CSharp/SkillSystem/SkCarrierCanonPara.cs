namespace SkillSystem;

public class SkCarrierCanonPara : ISkPara, ISkParaNet
{
	public int _idxCanon;

	public int TypeId => 1;

	public SkCarrierCanonPara()
	{
	}

	public SkCarrierCanonPara(int idx)
	{
		_idxCanon = idx;
	}

	public float[] ToFloatArray()
	{
		return new float[2]
		{
			(float)TypeId + 0.1f,
			(float)_idxCanon + 0.1f
		};
	}

	public void FromFloatArray(float[] data)
	{
		_idxCanon = (int)data[1];
	}
}
