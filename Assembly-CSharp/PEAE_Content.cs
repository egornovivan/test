using Pathea;
using UnityEngine;

public class PEAE_Content : PEAbnormalEff
{
	public int[] contentIDs { get; set; }

	public EntityInfoCmpt entityInfo { get; set; }

	public override void Do()
	{
		if (null != entityInfo && null != entityInfo.faceTex)
		{
			PeTipMsg.Register(PELocalization.GetString(contentIDs[Random.Range(0, contentIDs.Length)]), entityInfo.faceTex, PeTipMsg.EMsgLevel.Norm);
		}
	}
}
