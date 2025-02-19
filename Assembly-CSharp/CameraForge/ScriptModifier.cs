using UnityEngine;

namespace CameraForge;

public abstract class ScriptModifier : PoseNode
{
	public Slot Name;

	public Slot Col;

	public PoseSlot Prev;

	public ScriptModifier()
	{
		Name = new Slot("Name");
		Name.value = string.Empty;
		Col = new Slot("Color");
		Col.value = new Color(0.7f, 0.8f, 1f, 1f);
		Prev = new PoseSlot("Prev");
	}
}
