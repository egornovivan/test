public class PackBase
{
	protected virtual void PushIn(params int[] ids)
	{
	}

	protected virtual void PopOut(params int[] ids)
	{
	}

	public static PackBase operator +(PackBase pack, int id)
	{
		pack.PushIn(id);
		return pack;
	}

	public static PackBase operator -(PackBase pack, int id)
	{
		pack.PopOut(id);
		return pack;
	}

	public static PackBase operator +(PackBase pack, int[] ids)
	{
		pack.PushIn(ids);
		return pack;
	}

	public static PackBase operator -(PackBase pack, int[] ids)
	{
		pack.PopOut(ids);
		return pack;
	}
}
