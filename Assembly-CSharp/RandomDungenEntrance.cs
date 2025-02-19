using Pathea;
using UnityEngine;

public class RandomDungenEntrance : MonoBehaviour
{
	public int level = 1;

	public int dungeonId = 1;

	public bool isShow;

	private MessageBox_N.Message ob;

	private Vector3 pos => base.transform.position;

	private void OnTriggerEnter(Collider target)
	{
		if ((!PeGameMgr.IsSingleAdventure || !(PeGameMgr.yirdName == AdventureScene.MainAdventure.ToString())) && !PeGameMgr.IsMultiAdventure)
		{
			return;
		}
		Debug.Log("enter dungen");
		if (!(null == target.GetComponentInParent<MainPlayerCmpt>()) && !isShow)
		{
			isShow = true;
			if (level >= 100)
			{
				ob = MessageBox_N.ShowYNBox(PELocalization.GetString(8000713), SceneTranslate, SetFalse);
			}
			else
			{
				ob = MessageBox_N.ShowYNBox(CSUtils.GetNoFormatString(PELocalization.GetString(8000534), level.ToString()), SceneTranslate, SetFalse);
			}
		}
	}

	private void OnTriggerExit(Collider target)
	{
		if (ob != null)
		{
			MessageBox_N.CancelMessage(ob);
		}
		isShow = false;
	}

	public void SceneTranslate()
	{
		RandomDungenMgr.Instance.EnterDungen(base.transform.position, dungeonId);
		SetFalse();
	}

	public void SetFalse()
	{
		isShow = false;
	}

	public void SetLevel(int level)
	{
		this.level = level;
	}

	public void SetDungeonId(int id)
	{
		dungeonId = id;
	}

	private void OnDestroy()
	{
		if (ob != null)
		{
			MessageBox_N.CancelMessage(ob);
		}
	}

	private void Start()
	{
	}

	private void Awake()
	{
	}
}
