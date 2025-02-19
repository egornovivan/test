using System;
using UnityEngine;

namespace PeUIEffect;

[Serializable]
public abstract class UIEffect : MonoBehaviour
{
	public delegate void EffectEvent(UIEffect effect);

	[SerializeField]
	protected bool m_Runing;

	[SerializeField]
	protected bool mForward = true;

	public bool Forward => mForward;

	public event EffectEvent e_OnPlay;

	public event EffectEvent e_OnEnd;

	public virtual void Play(bool forward)
	{
		mForward = forward;
		Play();
	}

	public virtual void Play()
	{
		m_Runing = true;
		if (this.e_OnPlay != null)
		{
			this.e_OnPlay(this);
		}
	}

	public virtual void End()
	{
		m_Runing = false;
		if (this.e_OnEnd != null)
		{
			this.e_OnEnd(this);
		}
	}
}
