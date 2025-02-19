using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class DienManager : MonoBehaviour
{
	public static List<Transform> doors;

	public static List<List<Transform>> mulDoors = new List<List<Transform>>();

	public static bool doorsCanTrigger = true;

	public Light directionLight;

	private void Start()
	{
		doors = new List<Transform>(GetComponentsInChildren<Transform>()).FindAll(delegate(Transform trans)
		{
			string text = trans.gameObject.name;
			if (text.Length < 24)
			{
				return false;
			}
			int result;
			return (text.Substring(0, 22) == "scene_Dien_viyus_ship_" && IsNumberic(text.Substring(22, 2), out result) && result >= 22 && result <= 26) ? true : false;
		});
		List<Transform> item = new List<Transform>(GetComponentsInChildren<Transform>()).FindAll(delegate(Transform trans)
		{
			string text2 = trans.gameObject.name;
			if (text2.Length < 24)
			{
				return false;
			}
			int result2;
			return (text2.Substring(0, 22) == "scene_Dien_viyus_ship_" && IsNumberic(text2.Substring(22, 2), out result2) && result2 >= 22 && result2 <= 26) ? true : false;
		});
		mulDoors.Add(item);
		doors.Reverse();
	}

	private bool IsNumberic(string s, out int result)
	{
		result = -1;
		try
		{
			result = Convert.ToInt32(s);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void Update()
	{
		CheckOpen();
	}

	private void CheckOpen()
	{
		if (MissionManager.Instance == null)
		{
			return;
		}
		if (PeGameMgr.IsSingle)
		{
			if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip0)
			{
				if (!MissionManager.Instance.HadCompleteMission(640))
				{
					return;
				}
			}
			else if (!MissionManager.Instance.HasMission(906) && !MissionManager.Instance.HadCompleteMission(906))
			{
				return;
			}
		}
		else
		{
			if (PlayerNetwork.mainPlayer == null)
			{
				return;
			}
			if (PlayerNetwork.mainPlayer._curSceneId == 2)
			{
				if (!MissionManager.Instance.HadCompleteMission(640))
				{
					return;
				}
			}
			else if (!MissionManager.Instance.HasMission(906) && !MissionManager.Instance.HadCompleteMission(906))
			{
				return;
			}
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		if (doorsCanTrigger)
		{
			if (PeGameMgr.IsSingle)
			{
				for (int i = 0; i < 5; i++)
				{
					if (Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.position, doors[i].position) <= 3f)
					{
						DoorOpen(doors[i]);
					}
				}
				return;
			}
			{
				foreach (List<Transform> mulDoor in mulDoors)
				{
					foreach (Transform item in mulDoor)
					{
						if (item != null && Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.position, item.position) <= 3f)
						{
							DoorOpen(item);
						}
					}
				}
				return;
			}
		}
		if (PeGameMgr.IsSingle)
		{
			for (int j = 0; j < 5; j++)
			{
				if (Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.position, doors[j].position) <= 3f && PeSingleton<PeCreature>.Instance.mainPlayer.position.z < doors[j].position.z)
				{
					DoorOpen(doors[j]);
				}
			}
			return;
		}
		foreach (List<Transform> mulDoor2 in mulDoors)
		{
			foreach (Transform item2 in mulDoor2)
			{
				if (item2 != null && Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.position, item2.position) <= 3f && PeSingleton<PeCreature>.Instance.mainPlayer.position.z < item2.position.z)
				{
					DoorOpen(item2);
				}
			}
		}
	}

	public static void DoorOpen(Transform door)
	{
		if (!(door == null))
		{
			door.GetComponent<Animator>().SetBool("IsOpen", value: true);
			door.GetComponent<BoxCollider>().enabled = false;
		}
	}

	public static void DoorClose(Transform door)
	{
		if (!(door == null))
		{
			door.GetComponent<Animator>().SetBool("IsOpen", value: false);
			door.GetComponent<BoxCollider>().enabled = true;
		}
	}
}
