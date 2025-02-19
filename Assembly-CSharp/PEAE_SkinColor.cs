using Pathea;
using UnityEngine;

public class PEAE_SkinColor : PEAbnormalEff
{
	public AvatarCmpt avatar { get; set; }

	public Color color { get; set; }

	public override void Do()
	{
		if (null != avatar)
		{
			avatar.apperaData.subSkinColor = color;
			avatar.UpdateSmr();
		}
	}

	public override void End()
	{
		if (null != avatar)
		{
			avatar.apperaData.subSkinColor = Color.black;
			avatar.UpdateSmr();
		}
	}
}
