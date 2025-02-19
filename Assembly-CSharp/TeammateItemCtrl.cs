using System.Collections.Generic;
using UnityEngine;

public class TeammateItemCtrl : MonoBehaviour
{
	private List<PlayerNetwork> mPlayerList;

	[SerializeField]
	private UIGrid mGrid;

	[SerializeField]
	private TeammateGrid mPrefab;

	public void SetGrid(List<PlayerNetwork> _lis)
	{
		mPlayerList = _lis;
		CreatGridList(_lis);
	}

	private void CreatGridList(List<PlayerNetwork> _lis)
	{
		for (int i = 0; i < _lis.Count; i++)
		{
			InstantiateGrid(_lis[i]);
		}
		mGrid.repositionNow = true;
	}

	private void InstantiateGrid(PlayerNetwork _pn)
	{
		TeammateGrid teammateGrid = Object.Instantiate(mPrefab);
		teammateGrid.transform.parent = mGrid.transform;
		teammateGrid.transform.localPosition = Vector3.zero;
		teammateGrid.transform.localScale = Vector3.one;
		teammateGrid.SetInfo(_pn);
	}
}
