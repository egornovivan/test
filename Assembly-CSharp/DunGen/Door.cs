using System;
using UnityEngine;

namespace DunGen;

[Serializable]
public class Door : MonoBehaviour
{
	[HideInInspector]
	public Dungeon Dungeon;

	[HideInInspector]
	public Doorway DoorwayA;

	[HideInInspector]
	public Doorway DoorwayB;

	[HideInInspector]
	public Tile TileA;

	[HideInInspector]
	public Tile TileB;

	[SerializeField]
	private bool isOpen;

	public virtual bool IsOpen
	{
		get
		{
			return isOpen;
		}
		set
		{
			isOpen = value;
			if (Dungeon != null && Dungeon.Culling != null)
			{
				Dungeon.Culling.ChangeDoorState(this, isOpen);
			}
		}
	}
}
