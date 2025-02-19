using UnityEngine;

public class BuildingInfo
{
	public Transform tran;

	public int occupyId;

	public bool IsOccupy;

	public BuildingInfo(int Id, Transform _tran)
	{
		tran = _tran;
		occupyId = Id;
		IsOccupy = false;
	}
}
