using System.Collections.Generic;
using UnityEngine;

public class AnimationEffectPlayer : MonoBehaviour
{
	public GameObject model;

	public List<AnimationEffect> AnimationEffects = new List<AnimationEffect>();

	private bool islog;

	private void OnEnable()
	{
		islog = false;
	}

	private void Update()
	{
		if (model != null && model.GetComponent<Animation>() != null)
		{
			foreach (AnimationEffect animationEffect in AnimationEffects)
			{
				if (animationEffect != null)
				{
					if (model.GetComponent<Animation>().IsPlaying(animationEffect.AnimationName) && !animationEffect.LastIsPlaying)
					{
						PlayEffectGroup(animationEffect.EffectGroupRes);
					}
					animationEffect.LastIsPlaying = model.GetComponent<Animation>().IsPlaying(animationEffect.AnimationName);
				}
			}
			return;
		}
		if (model == null && !islog)
		{
			Debug.LogError("The model is null!!!");
			islog = true;
		}
	}

	private void PlayEffectGroup(GameObject effgroupres)
	{
		if (!(effgroupres == null))
		{
			GameObject gameObject = Object.Instantiate(effgroupres);
			gameObject.transform.parent = base.transform;
			if (gameObject.GetComponent<EffectGroup>() == null)
			{
				Debug.LogError("There is no EffectGroup to get Root");
			}
			else if (model != null)
			{
				gameObject.GetComponent<EffectGroup>().m_root = model.transform;
			}
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
		}
	}
}
