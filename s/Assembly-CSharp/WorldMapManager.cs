using System.Collections;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
	private void Start()
	{
		if (ServerConfig.IsVS)
		{
			StartCoroutine(AutoIncrease());
		}
	}

	private IEnumerator AutoIncrease()
	{
		while (true)
		{
			yield return new WaitForSeconds(BattleConstData.Instance._site_interval);
			BattleManager.Update();
		}
	}
}
