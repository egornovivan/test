using Pathea;
using UnityEngine;

public class TestUseItemEffect : MonoBehaviour
{
	[SerializeField]
	private bool use;

	private void Update()
	{
		if (use)
		{
			use = false;
			UseItemCmpt cmpt = PeSingleton<MainPlayer>.Instance.entity.GetCmpt<UseItemCmpt>();
			cmpt.LearnEffectAndSound();
		}
	}
}
