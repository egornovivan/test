using Pathea;
using UnityEngine;

public class PEAE_Anim : PEAbnormalEff
{
	private float original;

	private float nextRetryTime;

	public PeEntity entity { get; set; }

	public string effAnim { get; set; }

	public int actionType { get; set; }

	public override void Do()
	{
		if (!("0" == effAnim) && !(null == entity) && !(null == entity.motionMgr))
		{
			PEActionParamS param = PEActionParamS.param;
			param.str = effAnim;
			if (actionType == 0)
			{
				entity.motionMgr.DoAction(PEActionType.Leisure, param);
			}
			else
			{
				entity.motionMgr.DoAction(PEActionType.Abnormal, param);
			}
			nextRetryTime = Time.time + Random.Range(60f, 90f);
		}
	}

	public override void End()
	{
		if (!(null == entity) && !(null == entity.motionMgr))
		{
			entity.motionMgr.EndAction(PEActionType.Leisure);
		}
	}

	public override void Update()
	{
		if (Time.time > nextRetryTime)
		{
			Do();
		}
	}
}
