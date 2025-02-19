using System.Collections.Generic;
using UnityEngine;

public class AiGroupManager : MonoBehaviour
{
	private static AiGroupManager _instance;

	private static List<AIGroupNetWork> aiGroupList = new List<AIGroupNetWork>();

	public static AiGroupManager Self => _instance;

	public static List<AIGroupNetWork> AiGroupList => aiGroupList;

	private void Awake()
	{
		_instance = this;
	}
}
