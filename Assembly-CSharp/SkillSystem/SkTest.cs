using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem;

public class SkTest : MonoBehaviour
{
	private void Awake()
	{
		SkData skData = new SkData();
		skData._condToLoop = SkCond.Create("lasthit");
		skData._pretimeOfMain = new float[1] { 0.5f };
		skData._postimeOfMain = new float[1] { 0.5f };
		skData._events = new List<List<SkTriggerEvent>>();
		skData._events.Add(new List<SkTriggerEvent>());
		SkTriggerEvent skTriggerEvent = new SkTriggerEvent();
		skTriggerEvent._cond = SkCond.Create("CondToDig");
		skTriggerEvent._modsTarget = SkAttribsModifier.Create("mad,0,1,-1");
		skData._events[0].Add(skTriggerEvent);
		SkData.s_SkillTbl = new Dictionary<int, SkData>();
		SkData.s_SkillTbl.Add(0, skData);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.Mouse0))
		{
			VFVoxelTerrain.self.StartSkill(VFVoxelTerrain.self, 0);
		}
	}
}
