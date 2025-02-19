public class EffectSpwanPoint : SpawnPoint
{
	public EffectSpwanPoint()
	{
	}

	public EffectSpwanPoint(WEEffect obj)
	{
		ID = obj.ID;
		Name = obj.ObjectName;
		Position = obj.Position;
		Rotation = obj.Rotation;
		Scale = obj.Scale;
		Prototype = obj.Prototype;
		PlayerIndex = -1;
		IsTarget = false;
		Visible = true;
	}

	public EffectSpwanPoint(EffectSpwanPoint sp)
		: base(sp)
	{
	}
}
