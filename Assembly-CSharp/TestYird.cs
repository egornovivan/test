using Pathea;
using UnityEngine;

public class TestYird : MonoBehaviour
{
	private CustomGameData customGameData;

	private void Start()
	{
		customGameData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(PeGameMgr.gameName);
		if (customGameData == null)
		{
			Debug.LogError("no custom game data found with name:" + PeGameMgr.gameName);
			Object.Destroy(this);
		}
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal(GUILayout.MaxWidth(200f));
		DrawCustomYird();
		GUILayout.EndHorizontal();
	}

	private void DrawCustomYird()
	{
		GUILayout.BeginVertical();
		customGameData.ForEach(delegate(YirdData item)
		{
			string text = item.name;
			if (PeGameMgr.yirdName != text && GUILayout.Button(text))
			{
				Travel(item);
				Object.Destroy(this);
			}
		});
		GUILayout.EndVertical();
	}

	private void Travel(YirdData yird)
	{
		PeSingleton<FastTravelMgr>.Instance.TravelTo(yird.name, yird.defaultPlayerPos);
	}
}
