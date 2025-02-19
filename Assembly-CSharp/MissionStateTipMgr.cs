using UnityEngine;

public class MissionStateTipMgr : MonoBehaviour
{
	private static MissionStateTipMgr instance;

	[SerializeField]
	private GameObject mPrefab;

	[SerializeField]
	private MissionStateTipParent[] mParents;

	public static MissionStateTipMgr Instance => instance;

	public void ShowMissionTip(string _state, string _content)
	{
		GameObject gameObject = CreatItem(GetParent().mParent, mPrefab);
		MissionStateTipItem component = gameObject.GetComponent<MissionStateTipItem>();
		component.SetContent(_state, _content);
	}

	private GameObject CreatItem(Transform _parent, GameObject _prefab)
	{
		GameObject gameObject = Object.Instantiate(_prefab);
		gameObject.transform.parent = _parent;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	private MissionStateTipParent GetParent()
	{
		MissionStateTipParent[] array = mParents;
		foreach (MissionStateTipParent missionStateTipParent in array)
		{
			if (missionStateTipParent.IsFree())
			{
				return missionStateTipParent;
			}
		}
		mParents[0].DeleteChild();
		return mParents[0];
	}

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
