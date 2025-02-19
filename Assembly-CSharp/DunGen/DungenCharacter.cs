using UnityEngine;

namespace DunGen;

[AddComponentMenu("DunGen/Character")]
public class DungenCharacter : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private Tile currentTile;

	public Tile CurrentTile
	{
		get
		{
			return currentTile;
		}
		set
		{
			currentTile = value;
		}
	}

	public event CharacterTileChangedEvent OnTileChanged;

	internal void ForceRecheckTile()
	{
		Tile[] array = Object.FindObjectsOfType<Tile>();
		foreach (Tile tile in array)
		{
			if (tile.Placement.Bounds.Contains(base.transform.position))
			{
				HandleTileChange(tile);
				break;
			}
		}
	}

	protected virtual void OnTileChangedEvent(Tile previousTile, Tile newTile)
	{
	}

	internal void HandleTileChange(Tile newTile)
	{
		if (!(currentTile == newTile))
		{
			Tile previousTile = currentTile;
			currentTile = newTile;
			if (this.OnTileChanged != null)
			{
				this.OnTileChanged(this, previousTile, newTile);
			}
			OnTileChangedEvent(previousTile, newTile);
		}
	}
}
