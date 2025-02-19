using UnityEngine;

namespace DunGen;

[AddComponentMenu("DunGen/Runtime Dungeon")]
public class RuntimeDungeon : MonoBehaviour
{
	public DungeonGenerator Generator = new DungeonGenerator();

	public bool GenerateOnStart = true;

	protected virtual void Start()
	{
		if (GenerateOnStart)
		{
			Generator.Generate();
		}
	}
}
