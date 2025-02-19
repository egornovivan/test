using UnityEngine;

namespace Pathfinding;

public class TargetMover : MonoBehaviour
{
	public LayerMask mask;

	public Transform target;

	private RichAI[] ais;

	private AIPath[] ais2;

	public bool onlyOnDoubleClick;

	private Camera cam;

	public void Start()
	{
		cam = Camera.main;
		ais = Object.FindObjectsOfType(typeof(RichAI)) as RichAI[];
		ais2 = Object.FindObjectsOfType(typeof(AIPath)) as AIPath[];
	}

	public void OnGUI()
	{
		if (onlyOnDoubleClick && cam != null && Event.current.type == EventType.MouseDown && Event.current.clickCount == 2)
		{
			UpdateTargetPosition();
		}
	}

	private void Update()
	{
		if (!onlyOnDoubleClick && cam != null)
		{
			UpdateTargetPosition();
		}
	}

	public void UpdateTargetPosition()
	{
		if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hitInfo, float.PositiveInfinity, mask) || !(hitInfo.point != target.position))
		{
			return;
		}
		target.position = hitInfo.point;
		if (ais != null && onlyOnDoubleClick)
		{
			for (int i = 0; i < ais.Length; i++)
			{
				if (ais[i] != null)
				{
					ais[i].UpdatePath();
				}
			}
		}
		if (ais2 == null || !onlyOnDoubleClick)
		{
			return;
		}
		for (int j = 0; j < ais2.Length; j++)
		{
			if (ais2[j] != null)
			{
				ais2[j].SearchPath();
			}
		}
	}
}
