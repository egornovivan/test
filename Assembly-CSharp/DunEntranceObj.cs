using Pathea;
using PeMap;
using UnityEngine;

public class DunEntranceObj : ISceneObjAgent
{
	private int id;

	private int dungeonId;

	private int level;

	private string path;

	private Object prefab;

	private GameObject gameObj;

	private Vector3 position;

	private Quaternion rotation;

	private bool needToActivate;

	private bool tstYOnActivate;

	private bool isShowOnMap;

	private bool showEnterOrNot;

	public bool ShowEnterOrNot
	{
		set
		{
			showEnterOrNot = value;
			if (gameObj != null)
			{
				gameObj.GetComponent<RandomDungenEntrance>().isShow = value;
			}
		}
	}

	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public int Level
	{
		get
		{
			return level;
		}
		set
		{
			level = value;
		}
	}

	public int DungeonId
	{
		get
		{
			return dungeonId;
		}
		set
		{
			dungeonId = value;
		}
	}

	public int ScenarioId { get; set; }

	public GameObject Go => gameObj;

	public Vector3 Pos => position;

	public IBoundInScene Bound => null;

	public bool NeedToActivate => needToActivate;

	public bool TstYOnActivate => tstYOnActivate;

	public DunEntranceObj(Object entrancePrefab, Vector3 pos)
	{
		prefab = entrancePrefab;
		position = pos;
		if (PeSingleton<MaskTile.Mgr>.Instance.GetIsKnowByPos(pos))
		{
			new DungeonEntranceLabel(pos);
			isShowOnMap = true;
		}
	}

	public void OnActivate()
	{
		Rigidbody component = gameObj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.useGravity = true;
		}
	}

	public void OnDeactivate()
	{
		Rigidbody component = gameObj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.useGravity = false;
		}
	}

	public void OnConstruct()
	{
		if (null != prefab)
		{
			gameObj = Object.Instantiate(prefab) as GameObject;
			gameObj.transform.position = position;
			gameObj.transform.rotation = rotation;
			RandomDungenEntrance component = gameObj.GetComponent<RandomDungenEntrance>();
			component.isShow = showEnterOrNot;
			component.SetLevel(level);
			component.SetDungeonId(dungeonId);
			Rigidbody component2 = gameObj.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.useGravity = false;
			}
			if (!isShowOnMap)
			{
				new DungeonEntranceLabel(position);
				isShowOnMap = true;
			}
		}
		else
		{
			Debug.LogError("entrance prefab not found");
		}
	}

	public void OnDestruct()
	{
		if (gameObj != null)
		{
			position = gameObj.transform.position;
			rotation = gameObj.transform.rotation;
			Object.Destroy(gameObj);
		}
	}

	public void DestroySelf()
	{
		if (gameObj != null)
		{
			Object.Destroy(gameObj);
		}
		SceneMan.RemoveSceneObj(this);
		DungeonEntranceLabel.Remove(position);
	}
}
