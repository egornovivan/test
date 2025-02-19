using Pathea;
using UnityEngine;

public class PEAE_CameraEffect : PEAbnormalEff
{
	private float m_ElapseTime;

	public PeEntity entity { get; set; }

	public AbnormalData.EffCamera effCamera { get; set; }

	private Vector3 effectPos { get; set; }

	private float effectStrength { get; set; }

	private float effectTime { get; set; }

	public override bool effectEnd => m_ElapseTime >= effectTime;

	public override void Do()
	{
		if (null == entity)
		{
			return;
		}
		if (entity != PeSingleton<MainPlayer>.Instance.entity)
		{
			m_ElapseTime = effectTime;
			return;
		}
		m_ElapseTime = 0f;
		switch (effCamera.type)
		{
		case 0:
			PeCameraImageEffect.ScreenMask(Mathf.RoundToInt(effectStrength), show: true, effectTime);
			break;
		case 1:
			PeCameraImageEffect.SetDizzyStrength(effCamera.value * effectStrength);
			break;
		case 2:
			PeCameraImageEffect.FlashlightExplode(effCamera.value * effectStrength);
			break;
		case 3:
			PeCameraImageEffect.SetFoodPoisonStrength(effCamera.value);
			break;
		case 4:
			PeCameraImageEffect.SetInjuredPoisonStrength(effCamera.value);
			break;
		case 5:
			PeCameraImageEffect.SetGRVInfestStrength(effCamera.value);
			break;
		}
	}

	public override void End()
	{
		switch (effCamera.type)
		{
		case 0:
			PeCameraImageEffect.ScreenMask(0, show: false);
			break;
		case 1:
			PeCameraImageEffect.SetDizzyStrength(0f);
			break;
		case 2:
			PeCameraImageEffect.FlashlightExplode(0f);
			break;
		case 3:
			PeCameraImageEffect.SetFoodPoisonStrength(0f);
			break;
		case 4:
			PeCameraImageEffect.SetInjuredPoisonStrength(0f);
			break;
		case 5:
			PeCameraImageEffect.SetGRVInfestStrength(0f);
			break;
		}
	}

	public void OnAbnormalAttack(PEAbnormalAttack attack, Vector3 effectPos)
	{
		bool flag = false;
		switch (effCamera.type)
		{
		case 0:
			flag = attack.type == PEAbnormalAttackType.BlurredVision;
			break;
		case 1:
			flag = attack.type == PEAbnormalAttackType.Dazzling;
			break;
		case 2:
			flag = attack.type == PEAbnormalAttackType.Flashlight;
			break;
		}
		if (flag)
		{
			this.effectPos = effectPos;
			effectStrength = attack.strength;
			effectTime = attack.duration;
			int type = effCamera.type;
			if ((type == 1 || type == 2) && attack.radius > float.Epsilon)
			{
				effectStrength *= 0.5f + 0.5f * (entity.position - effectPos).magnitude / attack.radius;
			}
		}
	}

	public override void Update()
	{
		m_ElapseTime += Time.deltaTime;
		int type = effCamera.type;
		if (type == 1 && effectTime > 0f && m_ElapseTime <= effectTime)
		{
			PeCameraImageEffect.SetDizzyStrength(effCamera.value * effectStrength * (1f - m_ElapseTime / effectTime));
		}
	}
}
