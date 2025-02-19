using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class TeammateCtrl : MonoBehaviour
{
	private const int PerColumnCount = 8;

	[SerializeField]
	private TeammateItemCtrl mColumnPrefab;

	[SerializeField]
	private UIGrid mGrid;

	private List<PlayerNetwork> mTeammates = new List<PlayerNetwork>();

	private List<TeammateItemCtrl> mTeammateItemList = new List<TeammateItemCtrl>();

	private PeEntity mSelf;

	private int mSelfTeamID;

	private bool mDoDestroy;

	private void Awake()
	{
		if (PeGameMgr.IsMulti)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		PlayerNetwork.OnTeamChangedEventHandler += RefreshTeammate;
	}

	private void OnDisable()
	{
		PlayerNetwork.OnTeamChangedEventHandler -= RefreshTeammate;
	}

	public void RefreshTeammate()
	{
		if (null == PlayerNetwork.mainPlayer)
		{
			return;
		}
		mTeammates.Clear();
		PlayerNetwork.PlayerAction(delegate(PlayerNetwork p)
		{
			if (p.Id != PlayerNetwork.mainPlayerId && p.TeamId == PlayerNetwork.mainPlayer.TeamId)
			{
				mTeammates.Add(p);
			}
		});
		DestroyColumn();
		if (mTeammates.Count != 0)
		{
			CreateColumn(mTeammates);
		}
	}

	private void CreateColumn(List<PlayerNetwork> playerList)
	{
		if (playerList == null || playerList.Count <= 0)
		{
			return;
		}
		int num = playerList.Count / 8;
		int num2 = playerList.Count % 8;
		if (num > 0)
		{
			List<PlayerNetwork> list = null;
			for (int i = 0; i < num; i++)
			{
				list = playerList.GetRange(i * 8, 8);
				InstantiateColumn(list);
			}
		}
		if (num2 > 0)
		{
			List<PlayerNetwork> range = playerList.GetRange(num * 8, num2);
			InstantiateColumn(range);
		}
	}

	private void DestroyColumn()
	{
		for (int i = 0; i < mTeammateItemList.Count; i++)
		{
			Object.Destroy(mTeammateItemList[i].gameObject);
		}
		mTeammateItemList.Clear();
		mDoDestroy = true;
	}

	private void InstantiateColumn(List<PlayerNetwork> lis)
	{
		if (lis != null && lis.Count > 0)
		{
			TeammateItemCtrl teammateItemCtrl = Object.Instantiate(mColumnPrefab);
			teammateItemCtrl.transform.parent = mGrid.transform;
			teammateItemCtrl.transform.localPosition = Vector3.zero;
			teammateItemCtrl.transform.localScale = Vector3.one;
			teammateItemCtrl.SetGrid(lis);
			mTeammateItemList.Add(teammateItemCtrl);
		}
	}

	private void LateUpdate()
	{
		if (mDoDestroy)
		{
			mDoDestroy = false;
			mGrid.repositionNow = true;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.K))
		{
			RefreshTeammate();
		}
	}
}
