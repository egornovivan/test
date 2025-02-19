using System;

namespace Pathfinding;

[Serializable]
public class TagMask
{
	public int tagsChange;

	public int tagsSet;

	public TagMask()
	{
	}

	public TagMask(int change, int set)
	{
		tagsChange = change;
		tagsSet = set;
	}

	public void SetValues(object boxedTagMask)
	{
		TagMask tagMask = (TagMask)boxedTagMask;
		tagsChange = tagMask.tagsChange;
		tagsSet = tagMask.tagsSet;
	}

	public override string ToString()
	{
		return string.Empty + Convert.ToString(tagsChange, 2) + "\n" + Convert.ToString(tagsSet, 2);
	}
}
