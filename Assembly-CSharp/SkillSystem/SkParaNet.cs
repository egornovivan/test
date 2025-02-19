namespace SkillSystem;

public static class SkParaNet
{
	public static float[] ToFloatArray(ISkParaNet obj)
	{
		return obj.ToFloatArray();
	}

	public static ISkParaNet FromFloatArray(float[] data)
	{
		int num = (int)data[0];
		ISkParaNet skParaNet = null;
		switch (num)
		{
		case 0:
			skParaNet = new SkUseItemPara();
			skParaNet.FromFloatArray(data);
			break;
		case 1:
			skParaNet = new SkCarrierCanonPara();
			skParaNet.FromFloatArray(data);
			break;
		case 2:
			skParaNet = new SkCarrierCollisionPara();
			skParaNet.FromFloatArray(data);
			break;
		case 3:
			skParaNet = new ShootTargetPara();
			skParaNet.FromFloatArray(data);
			break;
		}
		return skParaNet;
	}
}
