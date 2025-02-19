using UnityEngine;
using WhiteCat.Internal;

public class DrawTweenInterpolator : TweenBase
{
	private float _original;

	public float y
	{
		get
		{
			return base.transform.position.y;
		}
		set
		{
			base.transform.position = new Vector3(0f, value, 0f);
		}
	}

	public override void OnTween(float factor)
	{
		y = factor;
	}

	public override void OnRecord()
	{
		_original = y;
	}

	public override void OnRestore()
	{
		y = _original;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
		Gizmos.DrawLine(Vector3.zero, Vector3.up);
		Gizmos.DrawLine(Vector3.up, Vector2.one);
		Gizmos.DrawLine(Vector2.one, Vector3.right);
		Gizmos.DrawLine(Vector3.right, Vector3.zero);
		if ((bool)base.interpolator)
		{
			Vector3 vector = new Vector3(0f, base.interpolator.Interpolate(0f));
			Vector3 zero = Vector3.zero;
			Gizmos.color = Color.green;
			zero.x = 0.02f;
			while (zero.x <= 1f)
			{
				zero.y = base.interpolator.Interpolate(zero.x);
				Gizmos.DrawLine(vector, zero);
				vector = zero;
				zero.x += 0.02f;
			}
			Gizmos.color = Color.magenta;
			vector.x = 0f;
			vector.y = y;
			zero.x = base.interpolator.normalizedTime;
			zero.y = vector.y;
			Gizmos.DrawLine(vector, zero);
			vector.x = zero.x;
			vector.y = 0f;
			Gizmos.DrawLine(zero, vector);
		}
	}
}
