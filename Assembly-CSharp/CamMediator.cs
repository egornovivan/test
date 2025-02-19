using Pathea;
using UnityEngine;

public static class CamMediator
{
	private static Texture2D m_AimTex;

	private static Transform mTrans;

	public static Transform Character
	{
		get
		{
			if (null == mTrans)
			{
				PeTrans cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PeTrans>();
				if (null != cmpt)
				{
					mTrans = cmpt.trans;
				}
			}
			return mTrans;
		}
	}

	public static Camera MainCamera => Camera.main;

	public static Texture2D AimTexture
	{
		get
		{
			if (m_AimTex == null)
			{
				m_AimTex = Resources.Load("GUI/Atlases/Tex/HitPoint") as Texture2D;
			}
			return m_AimTex;
		}
	}

	public static void DrawAimTextureGUI(Vector2 pos)
	{
		if (!(AimTexture == null))
		{
			Texture2D aimTexture = AimTexture;
			GUI.DrawTexture(new Rect(Mathf.Floor((float)Screen.width * pos.x - (float)aimTexture.width * 0.5f), Mathf.Floor((float)Screen.height * (1f - pos.y) - (float)aimTexture.height * 0.5f), aimTexture.width, aimTexture.height), aimTexture);
		}
	}
}
