using UnityEngine;

public class NetworkTestUI : MonoBehaviour
{
	private string strPlayerId = string.Empty;

	private string strNpcTeamplateId = string.Empty;

	private string strNpcId = string.Empty;

	private string strAiTemplateId = string.Empty;

	private string strMissionId = string.Empty;

	private TestType _type;

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(Screen.height / 4, Screen.width / 4, Screen.width / 2, Screen.height / 2));
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Player ID:", GUILayout.Width(60f));
		strPlayerId = GUILayout.TextField(strPlayerId, 32, GUILayout.Width(100f));
		GUILayout.EndHorizontal();
		if (GUILayout.Button("TestNPC"))
		{
			_type = TestType.TestNPC;
		}
		if (GUILayout.Button("TestAI"))
		{
			_type = TestType.TestAI;
		}
		OnDisplay();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	private void OnDisplay()
	{
		switch (_type)
		{
		case TestType.TestAI:
			OnGUIAi();
			break;
		case TestType.TestNPC:
			OnGUINpc();
			break;
		}
	}

	private void OnGUINpc()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("NpcTemplateID:", GUILayout.Width(100f));
		strNpcTeamplateId = GUILayout.TextField(strNpcTeamplateId, GUILayout.Width(100f));
		if (GUILayout.Button("CreateNpc"))
		{
			int num = int.Parse(strPlayerId);
			Player player = Player.GetPlayer(num);
			if (null == player)
			{
				return;
			}
			int templateId = int.Parse(strNpcTeamplateId);
			Vector3 pos = player.transform.position + Vector3.up * 0.5f;
			int num2 = SPTerrainEvent.CreateNpcWithoutLimit(num, player.WorldId, pos, templateId, 1, 1f, -1, isStand: false, 0f);
			if (num2 == -1)
			{
				return;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("NpcID:", GUILayout.Width(50f));
		strNpcId = GUILayout.TextField(strNpcId, GUILayout.Width(100f));
		if (GUILayout.Button("Recruit"))
		{
			int id = int.Parse(strPlayerId);
			Player player2 = Player.GetPlayer(id);
			if (null == player2)
			{
				return;
			}
			int id2 = int.Parse(strNpcId);
			AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id2);
			if (null == aiAdNpcNetwork)
			{
				return;
			}
			aiAdNpcNetwork.RecruitByPlayer(player2);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("NpcID:", GUILayout.Width(50f));
		strNpcId = GUILayout.TextField(strNpcId, GUILayout.Width(100f));
		if (GUILayout.Button("Dismiss"))
		{
			int id3 = int.Parse(strPlayerId);
			Player player3 = Player.GetPlayer(id3);
			if (null == player3)
			{
				return;
			}
			int id4 = int.Parse(strNpcId);
			AiAdNpcNetwork aiAdNpcNetwork2 = ObjNetInterface.Get<AiAdNpcNetwork>(id4);
			if (null == aiAdNpcNetwork2)
			{
				return;
			}
			aiAdNpcNetwork2.DismissByPlayer();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("NpcID:", GUILayout.Width(50f));
		strNpcId = GUILayout.TextField(strNpcId, GUILayout.Width(100f));
		GUILayout.Label("MissionID:", GUILayout.Width(70f));
		strMissionId = GUILayout.TextField(strMissionId, GUILayout.Width(100f));
		if (GUILayout.Button("AddMission"))
		{
			int id5 = int.Parse(strNpcId);
			AiAdNpcNetwork aiAdNpcNetwork3 = ObjNetInterface.Get<AiAdNpcNetwork>(id5);
			if (null == aiAdNpcNetwork3)
			{
				return;
			}
			int randomMission = int.Parse(strMissionId);
			NpcMissionData mission = aiAdNpcNetwork3.mission;
			if (mission == null)
			{
				return;
			}
			mission.m_RandomMission = randomMission;
			aiAdNpcNetwork3.SyncMission();
		}
		GUILayout.EndHorizontal();
	}

	private void OnGUIAi()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("AiTeamplateID:", GUILayout.Width(100f));
		strAiTemplateId = GUILayout.TextField(strAiTemplateId, GUILayout.Width(100f));
		if (GUILayout.Button("CreateAi"))
		{
			int num = int.Parse(strPlayerId);
			Player player = Player.GetPlayer(num);
			if (null == player)
			{
				return;
			}
			int aiId = int.Parse(strAiTemplateId);
			Vector3 pos = player.transform.position + new Vector3(0.2f, 0.2f, 0.2f);
			SPTerrainEvent.CreateMonsterWithoutLimit(num, player.WorldId, pos, aiId);
		}
		GUILayout.EndHorizontal();
	}
}
