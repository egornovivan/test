using System;
using System.Collections;
using UnityEngine;

public class SPGroupStatic : SPGroup
{
	public override IEnumerator SpawnGroup()
	{
		foreach (Transform ite in base.transform)
		{
			if (!(ite.tag != "AIPoint"))
			{
				while (!AiUtil.CheckCorrectPosition(ite.position, AiUtil.groundedLayer))
				{
					yield return new WaitForSeconds(0.5f);
				}
				int res = Convert.ToInt32(ite.name);
				Instantiate(res, ite.position, ite.rotation);
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield return new WaitForSeconds(0.5f);
	}

	private void OnDrawGizmos()
	{
		foreach (Transform item in base.transform)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(item.position, 0.5f);
		}
	}
}
