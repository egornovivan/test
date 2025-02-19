public class SelBox
{
	public IntBox m_Box;

	public byte m_Val;

	public SelBox()
	{
	}

	public SelBox(long hash, byte val)
	{
		m_Box.Hash = hash;
		m_Val = val;
	}
}
