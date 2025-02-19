using System.Collections;
using Pathea;
using UnityEngine;

namespace TrainingScene;

public class FightingTask : MonoBehaviour
{
	private static FightingTask mInstance;

	private GameObject mMonster;

	private RequestCmpt mRC;

	private PESkEntity mPeSE;

	private Vector3[] mPath;

	private bool isDestroyed;

	public static FightingTask Instance => mInstance;

	private IEnumerator DestroyFightingScene()
	{
		yield return new WaitForSeconds(1f);
		if (mMonster == null)
		{
			yield return 0;
		}
		Object.Destroy(mMonster);
		mMonster = null;
		TrainingTaskManager.Instance.CloseMonsterPoint();
	}

	private IEnumerator GetMonster()
	{
		yield return new WaitForSeconds(2f);
		if (GameObject.Find("lepus hare_2146483648") == null)
		{
			yield return 0;
		}
		mMonster = GameObject.Find("lepus hare_2146483648");
		mPeSE = mMonster.GetComponent<PESkEntity>();
		if (mPeSE == null)
		{
			yield return 0;
		}
		mPeSE.SetAttribute(AttribType.CampID, 0f);
		mPeSE.SetAttribute(AttribType.DamageID, 0f);
	}

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
		mPath = new Vector3[18]
		{
			new Vector3(21.4f, 3.5f, 12.3f),
			new Vector3(24.1f, 3.5f, 12.3f),
			new Vector3(25.7f, 3.5f, 12.3f),
			new Vector3(26.1f, 3.5f, 10.6f),
			new Vector3(24.2f, 3.5f, 10.6f),
			new Vector3(22.3f, 3.5f, 10.7f),
			new Vector3(20f, 3.5f, 10.8f),
			new Vector3(18.1f, 3.5f, 10.9f),
			new Vector3(16f, 3.5f, 11f),
			new Vector3(13.5f, 3.5f, 11f),
			new Vector3(11f, 3.5f, 11.1f),
			new Vector3(8.5f, 3.5f, 11.2f),
			new Vector3(7.1f, 3.5f, 9.7f),
			new Vector3(5.8f, 3.5f, 10.1f),
			new Vector3(3.9f, 3.5f, 12.7f),
			new Vector3(6.4f, 3.5f, 15f),
			new Vector3(9.5f, 3.5f, 15.1f),
			new Vector3(9f, 3.5f, 12.9f)
		};
	}

	private void Update()
	{
	}
}
