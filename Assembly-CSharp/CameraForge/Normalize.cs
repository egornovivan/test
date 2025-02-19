namespace CameraForge;

[Menu("Math/Vector/Normalize", 1)]
public class Normalize : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Normalize()
	{
		X = new Slot("X");
	}

	public override Var Calculate()
	{
		X.Calculate();
		Var value = X.value;
		Var result = value;
		if (!value.isNull)
		{
			if (value.type == EVarType.Int)
			{
				if (value.value_i > 0)
				{
					return 1;
				}
				if (value.value_i < 0)
				{
					return -1;
				}
				return 0;
			}
			if (value.type == EVarType.Float)
			{
				return sign(value.value_f);
			}
			if (value.type == EVarType.Vector)
			{
				return value.value_v.normalized;
			}
		}
		return result;
	}

	private float sign(float x)
	{
		if (x > 0f)
		{
			return 1f;
		}
		if (x < 0f)
		{
			return -1f;
		}
		return 0f;
	}
}
