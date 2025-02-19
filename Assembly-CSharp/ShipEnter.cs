using Pathea;
using UnityEngine;

public class ShipEnter : MonoBehaviour
{
	private void OnTriggerEnter(Collider o)
	{
		if (PeGameMgr.IsCustom || o.gameObject.layer != 10)
		{
			return;
		}
		PeEntity componentInParent = o.gameObject.GetComponentInParent<PeEntity>();
		if (componentInParent == null || componentInParent != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			return;
		}
		if (base.transform.parent.gameObject.name.Substring(0, 24) == "scene_Dien_viyus_ship_on")
		{
			Vector3 position = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			if (Vector3.Distance(position, new Vector3(16545.25f, 213.93f, 10645.7f)) < 150f)
			{
				MissionManager.Instance.transPoint = new Vector3(16545.25f, 23f, 10548f);
				MissionManager.Instance.yirdName = "DienShip1";
			}
			else if (Vector3.Distance(position, new Vector3(2876f, 365.6f, 9750.3f)) < 150f)
			{
				MissionManager.Instance.transPoint = new Vector3(2876f, 283.6f, 9652.3f);
				MissionManager.Instance.yirdName = "DienShip2";
			}
			else if (Vector3.Distance(position, new Vector3(13765.5f, 175.7f, 15242.7f)) < 150f)
			{
				MissionManager.Instance.transPoint = new Vector3(13765.5f, 93.7f, 15144.7f);
				MissionManager.Instance.yirdName = "DienShip3";
			}
			else if (Vector3.Distance(position, new Vector3(12547.7f, 623.7f, 13485.5f)) < 150f)
			{
				MissionManager.Instance.transPoint = new Vector3(12547.7f, 541.7f, 13387.5f);
				MissionManager.Instance.yirdName = "DienShip4";
			}
			else if (Vector3.Distance(position, new Vector3(7750.4f, 449.7f, 14712.8f)) < 150f)
			{
				MissionManager.Instance.transPoint = new Vector3(7750.4f, 367.7f, 14614.8f);
				MissionManager.Instance.yirdName = "DienShip5";
			}
			else
			{
				MissionManager.Instance.transPoint = new Vector3(14798.09f, 20.98818f, 8246.396f);
				MissionManager.Instance.yirdName = "DienShip0";
			}
			MissionManager.Instance.SceneTranslate();
		}
		else if (base.transform.parent.gameObject.name.Substring(0, 24) == "scene_Epiphany_L1Outside")
		{
			if (MissionManager.Instance.HadCompleteMission(607) && (!MissionManager.Instance.HadCompleteMission(617) || MissionManager.Instance.HadCompleteMission(618)))
			{
				MissionManager.Instance.transPoint = new Vector3(9649.354f, 90.488f, 12744.77f);
				MissionManager.Instance.yirdName = "L1Ship";
				MissionManager.Instance.SceneTranslate();
			}
		}
		else if (base.transform.parent.gameObject.name.Substring(0, 27) == "scene_paja_port_shipoutside")
		{
			if (MissionManager.Instance.HadCompleteMission(800))
			{
				if (PeGameMgr.IsMulti)
				{
					MissionManager.Instance.transPoint = new Vector3(1593.278f, -350.949f, 8021.335f);
				}
				else
				{
					MissionManager.Instance.transPoint = new Vector3(1593.278f, 149.051f, 8021.335f);
				}
				MissionManager.Instance.yirdName = "PajaShip";
				MissionManager.Instance.SceneTranslate();
			}
		}
		else if (base.transform.parent.gameObject.name.Substring(0, 26) == "scene_paja_launch_center03")
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.transPoint = new Vector3(1607f, -342f, 10411f);
			}
			else
			{
				MissionManager.Instance.transPoint = new Vector3(1607f, 158f, 10411f);
			}
			MissionManager.Instance.yirdName = "LaunchCenter";
			MissionManager.Instance.SceneTranslate();
		}
		else if (base.transform.parent.gameObject.name.Substring(0, 26) == "scene_paja_launch_center01")
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.transPoint = new Vector3(2019f, -354f, 10402f);
			}
			else
			{
				MissionManager.Instance.transPoint = new Vector3(2019f, 146f, 10402f);
			}
			MissionManager.Instance.yirdName = "LaunchCenter";
			MissionManager.Instance.SceneTranslate();
		}
	}
}
