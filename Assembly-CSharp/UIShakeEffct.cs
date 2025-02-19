using Pathea.Effect;
using SkillSystem;
using UnityEngine;

public class UIShakeEffct : MonoBehaviour, ISkEffectEntity
{
	private SkInst m_Inst;

	public AnimationCurve mForceToWeight;

	public SkInst Inst
	{
		set
		{
			m_Inst = value;
		}
	}

	private void Start()
	{
		float attribute = m_Inst._target.GetAttribute(82);
		Play(mForceToWeight.Evaluate(attribute));
	}

	public void Play(float value)
	{
		if (UIEffctMgr.Instance != null)
		{
			UIEffctMgr.Instance.mShakeEffect.Play(value);
		}
		Object.Destroy(base.gameObject);
	}
}
