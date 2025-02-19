public class PEAbnormalHit
{
	public bool preHit { get; set; }

	public virtual float HitRate()
	{
		return 0f;
	}

	public virtual void Update()
	{
	}

	public virtual void Clear()
	{
	}
}
