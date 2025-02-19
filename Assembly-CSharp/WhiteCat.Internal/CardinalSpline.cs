using System;
using UnityEngine;

namespace WhiteCat.Internal;

[Serializable]
public class CardinalSpline : PathSpline
{
	[SerializeField]
	private float _tension;

	public float tension
	{
		get
		{
			return _tension;
		}
		set
		{
			_tension = Mathf.Clamp(value, 0.001f, 1000f);
		}
	}

	public CardinalSpline(float error, float tension)
		: base(error)
	{
		this.tension = tension;
	}
}
