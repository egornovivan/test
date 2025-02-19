public interface ISkParaNet : ISkPara
{
	int TypeId { get; }

	float[] ToFloatArray();

	void FromFloatArray(float[] data);
}
