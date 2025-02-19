using System.Collections;
using Pathea;
using UnityEngine;

namespace TrainingScene;

public class FightingTask : MonoBehaviour
{
	private static FightingTask mInstance;

	private GameObject mMonster;

	private PESkEntity mPeSE;

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
	}

	private void Update()
	{
	}
}
