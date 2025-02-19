using System;

namespace AiAsset;

[Serializable]
public class DefaultActionArgument : ActionArgument
{
	public int skill;

	public string anim = string.Empty;

	public float rangeMin;

	public float rangeMax;
}
