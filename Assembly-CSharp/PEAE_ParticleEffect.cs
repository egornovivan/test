using System.Collections.Generic;
using Pathea;
using Pathea.Effect;
using UnityEngine;

public class PEAE_ParticleEffect : PEAbnormalEff
{
	private List<GameObject> mEffectObj = new List<GameObject>();

	private bool m_PlayEffect;

	public int[] effectID { get; set; }

	public PeEntity entity { get; set; }

	public override bool effectEnd
	{
		get
		{
			for (int num = mEffectObj.Count - 1; num >= 0; num--)
			{
				if (null != mEffectObj[num])
				{
					return false;
				}
				mEffectObj.RemoveAt(num);
			}
			return true;
		}
	}

	public override void Do()
	{
		Clear();
		for (int i = 0; i < effectID.Length; i++)
		{
			EffectBuilder.EffectRequest effectRequest = Singleton<EffectBuilder>.Instance.Register(effectID[i], null, entity.biologyViewCmpt.modelTrans);
			effectRequest.SpawnEvent += OnSpawn;
		}
		m_PlayEffect = true;
	}

	public override void End()
	{
		Clear();
	}

	private void Clear()
	{
		for (int i = 0; i < mEffectObj.Count; i++)
		{
			if (null != mEffectObj[i])
			{
				Object.Destroy(mEffectObj[i]);
			}
		}
		mEffectObj.Clear();
		m_PlayEffect = false;
	}

	private void OnSpawn(GameObject obj)
	{
		if (m_PlayEffect)
		{
			mEffectObj.Add(obj);
		}
		else
		{
			Object.Destroy(obj);
		}
	}
}
