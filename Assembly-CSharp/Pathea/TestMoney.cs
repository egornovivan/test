using UnityEngine;

namespace Pathea;

public class TestMoney : MonoBehaviour
{
	private Money mMoney;

	private void Start()
	{
		PackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
		mMoney = cmpt.money;
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(400f, 0f, 100f, 500f));
		GUILayout.BeginVertical();
		GUILayout.Label("Digital" + Money.Digital + ", currency:" + mMoney.current);
		if (GUILayout.Button("toggle money"))
		{
			Money.Digital = !Money.Digital;
		}
		if (GUILayout.Button("add 11 money"))
		{
			mMoney.current += 11;
		}
		if (GUILayout.Button("minus 10 money"))
		{
			mMoney.current -= 10;
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
