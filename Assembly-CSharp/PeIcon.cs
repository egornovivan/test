using UnityEngine;

public class PeIcon
{
	private string mIconString;

	private Texture mIconTexture;

	public PeIcon(string iconString)
	{
		mIconString = iconString;
		mIconTexture = null;
	}

	public PeIcon(Texture iconTex)
	{
		mIconTexture = iconTex;
		mIconString = null;
	}
}
