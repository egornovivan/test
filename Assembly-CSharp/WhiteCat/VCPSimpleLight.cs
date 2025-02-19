using UnityEngine;

namespace WhiteCat;

public class VCPSimpleLight : VCSimpleObjectPart
{
	[SerializeField]
	private Light light;

	[SerializeField]
	private Renderer renderer;

	public Color color
	{
		get
		{
			return light.color;
		}
		set
		{
			light.color = value;
			renderer.material.SetColor("_Color", value);
			renderer.material.SetColor("_HoloColor", value);
		}
	}

	public override CmdList GetCmdList()
	{
		CmdList cmdList = base.GetCmdList();
		cmdList.Add((!light.enabled) ? "Turn On" : "Turn Off", delegate
		{
			light.enabled = !light.enabled;
		});
		return cmdList;
	}
}
