using UnityEngine;

public class testEffectFactory : MonoBehaviour
{
	public bool loop;

	public float cycleTime;

	public int explodeNum;

	public float interval;

	public GameObject unit;

	public GameObject target;

	private float pastTime;

	private float unitTime;

	private int totalNum = 1;

	private GameObject unitObj;

	private GameObject targetObj;

	private void Start()
	{
		if (!(null == target))
		{
			targetObj = Object.Instantiate(target, base.transform.position, base.transform.rotation) as GameObject;
			unitObj = Object.Instantiate(unit, base.transform.position, base.transform.rotation) as GameObject;
			targetObj.transform.parent = base.transform;
			unitObj.transform.parent = base.transform;
			if (targetObj.GetComponent<testEffectTarget>() != null)
			{
				targetObj.GetComponent<testEffectTarget>().attacker = unitObj;
				targetObj.GetComponent<testEffectTarget>().InitPos();
			}
		}
	}

	private void FixedUpdate()
	{
		if (!loop)
		{
			pastTime += Time.deltaTime;
			if (pastTime > cycleTime && cycleTime != 0f)
			{
				pastTime = 0f;
				targetObj = Object.Instantiate(target, base.transform.position, base.transform.rotation) as GameObject;
				unitObj = Object.Instantiate(unit, base.transform.position, base.transform.rotation) as GameObject;
				targetObj.transform.parent = base.transform;
				unitObj.transform.parent = base.transform;
				if (targetObj.GetComponent<testEffectTarget>() != null)
				{
					targetObj.GetComponent<testEffectTarget>().attacker = unitObj;
					targetObj.GetComponent<testEffectTarget>().InitPos();
				}
				totalNum = 1;
				unitTime = 0f - Time.deltaTime;
			}
			if (totalNum >= explodeNum)
			{
				return;
			}
		}
		if (unitTime >= interval)
		{
			targetObj = Object.Instantiate(target, base.transform.position, base.transform.rotation) as GameObject;
			unitObj = Object.Instantiate(unit, base.transform.position, base.transform.rotation) as GameObject;
			targetObj.transform.parent = base.transform;
			unitObj.transform.parent = base.transform;
			if (targetObj.GetComponent<testEffectTarget>() != null)
			{
				targetObj.GetComponent<testEffectTarget>().attacker = unitObj;
				targetObj.GetComponent<testEffectTarget>().InitPos();
			}
			unitTime -= interval;
			totalNum++;
		}
		else
		{
			unitTime += Time.deltaTime;
		}
	}
}
