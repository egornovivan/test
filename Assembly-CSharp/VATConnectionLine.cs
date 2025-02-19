public class VATConnectionLine
{
	public IntVector2 leftPos;

	public IntVector2 rightPos;

	public VATConnectionLine(IntVector2 s, IntVector2 e)
	{
		if (s.x < e.x)
		{
			leftPos = s;
			rightPos = e;
		}
		else if (s.x > e.x)
		{
			leftPos = e;
			rightPos = s;
		}
		else if (s.y < e.y)
		{
			leftPos = s;
			rightPos = e;
		}
		else
		{
			leftPos = e;
			rightPos = s;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		VATConnectionLine vATConnectionLine = (VATConnectionLine)obj;
		if (vATConnectionLine != null)
		{
			return (leftPos.Equals(vATConnectionLine.leftPos) && rightPos.Equals(vATConnectionLine.rightPos)) || (leftPos.Equals(vATConnectionLine.rightPos) && rightPos.Equals(vATConnectionLine.leftPos));
		}
		return false;
	}

	public override int GetHashCode()
	{
		return leftPos.GetHashCode() + rightPos.GetHashCode();
	}
}
