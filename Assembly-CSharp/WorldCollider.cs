using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class WorldCollider : MonoBehaviour
{
	private static WorldCollider s_Instance;

	private static Rect s_Rect;

	public Vector3[] storyBoarderLine;

	public Vector3[] demoBoarderLine;

	public float wallThickness = 3f;

	private List<GameObject> m_BoarderObj;

	public static WorldCollider Instance => s_Instance;

	private static Rect g_Rect
	{
		get
		{
			if (null != s_Instance)
			{
				return s_Rect;
			}
			return new Rect(0f, 500f, 18000f, 17500f);
		}
	}

	public static bool IsPointInWorld(Vector3 point)
	{
		return g_Rect.Contains(new Vector2(point.x, point.z));
	}

	private void Awake()
	{
		s_Instance = this;
		m_BoarderObj = new List<GameObject>();
	}

	private void Start()
	{
		if (PeGameMgr.IsSingleStory || PeGameMgr.IsMultiStory)
		{
			if (storyBoarderLine.Length >= 2)
			{
				ResetBoarder(storyBoarderLine);
			}
		}
		else if (PeGameMgr.randomMap)
		{
			ResetBoarder(new Vector3[4]
			{
				new Vector3(RandomMapConfig.Instance.boundaryWest - RandomMapConfig.Instance.BorderOffset, 0f, RandomMapConfig.Instance.boundarySouth - RandomMapConfig.Instance.BorderOffset),
				new Vector3(RandomMapConfig.Instance.boundaryWest - RandomMapConfig.Instance.BorderOffset, 0f, RandomMapConfig.Instance.boundaryNorth + RandomMapConfig.Instance.BorderOffset),
				new Vector3(RandomMapConfig.Instance.boundaryEast + RandomMapConfig.Instance.BorderOffset, 0f, RandomMapConfig.Instance.boundaryNorth + RandomMapConfig.Instance.BorderOffset),
				new Vector3(RandomMapConfig.Instance.boundaryEast + RandomMapConfig.Instance.BorderOffset, 0f, RandomMapConfig.Instance.boundarySouth - RandomMapConfig.Instance.BorderOffset)
			});
		}
	}

	private void CreatWallCube(Vector3 pos1, Vector3 pos2)
	{
		Vector3 vector = (pos1 + pos2) * 0.5f;
		Vector3 vector2 = pos2 - pos1;
		GameObject gameObject = new GameObject();
		BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.center = Vector3.zero;
		boxCollider.size = Vector3.one;
		gameObject.transform.parent = base.transform;
		gameObject.transform.position = new Vector3(vector.x, 1472f, vector.z);
		gameObject.transform.rotation = Quaternion.LookRotation(vector2.normalized, Vector3.up);
		gameObject.transform.localScale = new Vector3(wallThickness, 2944f, vector2.magnitude);
		m_BoarderObj.Add(gameObject);
	}

	public void ResetBoarder(Vector3[] boarderLine)
	{
		for (int i = 0; i < m_BoarderObj.Count; i++)
		{
			Object.Destroy(m_BoarderObj[i]);
		}
		m_BoarderObj.Clear();
		Vector3 vector = boarderLine[0];
		Vector3 vector2 = boarderLine[0];
		CreatWallCube(boarderLine[boarderLine.Length - 1], boarderLine[0]);
		for (int j = 0; j < boarderLine.Length - 1; j++)
		{
			vector.x = Mathf.Min(vector.x, boarderLine[j].x, boarderLine[j + 1].x);
			vector.z = Mathf.Min(vector.z, boarderLine[j].z, boarderLine[j + 1].z);
			vector2.x = Mathf.Max(vector2.x, boarderLine[j].x, boarderLine[j + 1].x);
			vector2.z = Mathf.Max(vector2.z, boarderLine[j].z, boarderLine[j + 1].z);
			CreatWallCube(boarderLine[j], boarderLine[j + 1]);
		}
		s_Rect.Set(vector.x, vector.z, vector2.x - vector.x, vector2.z - vector.z);
	}
}
