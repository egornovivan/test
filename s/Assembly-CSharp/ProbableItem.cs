using System;

public struct ProbableItem
{
	public int protoId;

	public int numMin;

	public int numMax;

	public float probability;

	public ProbableItem ParseString(string str)
	{
		string[] array = str.Split(',');
		protoId = Convert.ToInt32(array[0]);
		string[] array2 = array[1].Split('-');
		numMin = Convert.ToInt32(array2[0]);
		numMax = Convert.ToInt32(array2[1]);
		probability = Convert.ToSingle(array[2]);
		return this;
	}
}
