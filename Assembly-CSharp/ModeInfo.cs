using UnityEngine;

public class ModeInfo
{
	public PlayerModel mModel;

	public MLRoleInfo mRoleInfo;

	public GameObject mMode
	{
		get
		{
			if (mRoleInfo != null)
			{
				return mModel.mMode;
			}
			return null;
		}
	}
}
