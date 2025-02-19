using System;

[Serializable]
public class GraphicOptionGroup
{
	public float Fastest;

	public float Fast;

	public float Normal;

	public float Good;

	public float Fantastic;

	public float Level(int l)
	{
		return l switch
		{
			1 => Fastest, 
			2 => Fast, 
			3 => Normal, 
			4 => Good, 
			5 => Fantastic, 
			_ => Fastest, 
		};
	}
}
