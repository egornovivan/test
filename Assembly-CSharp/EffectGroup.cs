using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DestroyTimer))]
public class EffectGroup : MonoBehaviour
{
	public List<EffectDesc> Effect = new List<EffectDesc>();

	[HideInInspector]
	public Transform m_root;

	private float CurrentTime;

	private void OnEnable()
	{
		CurrentTime = 0f;
	}

	private void Update()
	{
		CurrentTime += Time.deltaTime;
		if (m_root != null)
		{
			foreach (EffectDesc item in Effect)
			{
				if (CurrentTime > item.DelayTime)
				{
					item.m_effroot = AiUtil.GetChild(m_root, item.Parent);
					item.Play();
				}
			}
			return;
		}
		Debug.LogError("Can't find the total parent or the model");
	}
}
