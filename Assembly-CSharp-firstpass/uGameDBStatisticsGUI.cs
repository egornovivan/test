using uGameDB;
using UnityEngine;

public class uGameDBStatisticsGUI : MonoBehaviour
{
	private const int WindowMargin = 10;

	public void OnGUI()
	{
		GUI.depth = 0;
		GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 20, Screen.height - 20));
		DrawStatisticsBox();
		GUILayout.EndArea();
	}

	public static void DrawStatisticsBox(int width = 300)
	{
		GUILayout.BeginVertical("Box", GUILayout.Width(width));
		GUILayout.Label("uGameDB Statistics");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Status:", GUILayout.Width(width / 2));
		GUILayout.Label((!Database.isConnected) ? "Not Connected" : "Connected");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Pending Requests:", GUILayout.Width(width / 2));
		GUILayout.Label(Database.pendingRequestCount.ToString());
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Queued Requests:", GUILayout.Width(width / 2));
		GUILayout.Label(Database.queuedRequestCount.ToString());
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Nodes (online/total):", GUILayout.Width(width / 2));
		GUILayout.Label(Database.onlineNodeCount + "/" + Database.nodeCount);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("VClock cache hit ratio:", GUILayout.Width(width / 2));
		GUILayout.Label(Database.vectorClockCacheHitRatio.ToString("F2") + " (" + Database.vectorClockCacheHits + " hits, " + Database.vectorClockCacheMisses + " misses)");
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}
}
