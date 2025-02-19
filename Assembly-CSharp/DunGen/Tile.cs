using System.Collections.Generic;
using System.Linq;
using DunGen.Graph;
using UnityEngine;

namespace DunGen;

[AddComponentMenu("DunGen/Tile")]
public class Tile : MonoBehaviour
{
	public bool AllowRotation = true;

	public bool AllowImmediateRepeats;

	[SerializeField]
	private TilePlacementData placement;

	[SerializeField]
	private bool isVisible = true;

	[SerializeField]
	private DungeonArchetype archetype;

	[SerializeField]
	private TileSet tileSet;

	[SerializeField]
	private FlowNodeReference node;

	[SerializeField]
	private FlowLineReference line;

	public TilePlacementData Placement
	{
		get
		{
			return placement;
		}
		internal set
		{
			placement = value;
		}
	}

	public DungeonArchetype Archetype
	{
		get
		{
			return archetype;
		}
		internal set
		{
			archetype = value;
		}
	}

	public TileSet TileSet
	{
		get
		{
			return tileSet;
		}
		internal set
		{
			tileSet = value;
		}
	}

	public GraphNode Node
	{
		get
		{
			return (node != null) ? node.Node : null;
		}
		internal set
		{
			if (value == null)
			{
				node = null;
			}
			else
			{
				node = new FlowNodeReference(value.Graph, value);
			}
		}
	}

	public GraphLine Line
	{
		get
		{
			return (line != null) ? line.Line : null;
		}
		internal set
		{
			if (value == null)
			{
				line = null;
			}
			else
			{
				line = new FlowLineReference(value.Graph, value);
			}
		}
	}

	public Dungeon Dungeon { get; internal set; }

	public bool IsVisible => isVisible;

	internal void AddTriggerVolume()
	{
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.center = base.transform.InverseTransformPoint(Placement.Bounds.center);
		boxCollider.size = base.transform.InverseTransformDirection(Placement.Bounds.size);
		boxCollider.isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(other == null))
		{
			DungenCharacter component = other.gameObject.GetComponent<DungenCharacter>();
			if (component != null)
			{
				component.HandleTileChange(this);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (placement != null)
		{
			Bounds bounds = placement.Bounds;
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(bounds.center, bounds.size);
		}
	}

	public IEnumerable<Tile> GetAdjactedTiles()
	{
		return Placement.UsedDoorways.Select((Doorway x) => x.ConnectedDoorway.Tile).Distinct();
	}

	public bool IsAdjacentTo(Tile other)
	{
		foreach (Doorway usedDoorway in Placement.UsedDoorways)
		{
			if (usedDoorway.ConnectedDoorway.Tile == other)
			{
				return true;
			}
		}
		return false;
	}

	public void Show()
	{
		if (!isVisible)
		{
			SetVisibility(isVisible: true);
		}
	}

	public void Hide()
	{
		if (isVisible)
		{
			SetVisibility(isVisible: false);
		}
	}

	public void SetVisibility(bool isVisible)
	{
		if (this.isVisible != isVisible)
		{
			this.isVisible = isVisible;
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = isVisible;
			}
		}
	}
}
