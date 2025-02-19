using UnityEngine;

public abstract class WaveHandler : MonoBehaviour
{
	public delegate void GoNotify(GameObject go);

	public event GoNotify onRecycle;

	public abstract void Init(WaveTracer tracer);

	protected void Recycle()
	{
		if (this.onRecycle != null)
		{
			this.onRecycle(base.gameObject);
		}
	}
}
