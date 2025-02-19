using UnityEngine;

public static class RoleHerderTexture
{
	private static Texture2D mRoleHerder;

	public static void SetTexture(Texture2D _texture)
	{
		mRoleHerder = _texture;
	}

	public static Texture2D GetTexture()
	{
		return mRoleHerder;
	}
}
