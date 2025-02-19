using Pathea;
using PeCustom;
using UnityEngine;

public class ObjectDebugInfoShower : MonoBehaviour
{
	private GameObject _go;

	public GUISkin gskin;

	public LayerMask layermask;

	private PeEntity entity;

	private GUIStyle bold;

	private GUIStyle alignright;

	private GameObject go
	{
		get
		{
			return _go;
		}
		set
		{
			if (_go != value)
			{
				GameObject prev = _go;
				_go = value;
				GOChange(prev, value);
			}
		}
	}

	private void GOChange(GameObject prev, GameObject now)
	{
		if (now != null)
		{
			entity = now.GetComponentInParent<PeEntity>();
		}
		else
		{
			entity = null;
		}
	}

	private void Update()
	{
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 256f, layermask))
		{
			go = hitInfo.collider.gameObject;
		}
		else
		{
			go = null;
		}
	}

	private void Start()
	{
		bold = new GUIStyle(gskin.label);
		bold.fontStyle = FontStyle.Bold;
		bold.wordWrap = false;
		alignright = new GUIStyle(gskin.label);
		alignright.alignment = TextAnchor.LowerRight;
	}

	private void OnGUI()
	{
		GUI.skin = gskin;
		if (!(go != null))
		{
			return;
		}
		float num = 220f;
		float height = ((!(entity == null)) ? 80 : 30);
		GUI.BeginGroup(new Rect(Input.mousePosition.x + 10f, (float)Screen.height - Input.mousePosition.y, num, height));
		GUI.Box(new Rect(0f, 0f, num, height), string.Empty);
		if (entity != null)
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer != entity)
			{
				GUI.color = Color.yellow;
			}
			else
			{
				GUI.color = Color.green;
			}
			GUI.Label(new Rect(5f, 5f, num - 10f, 20f), entity.gameObject.name, bold);
			GUI.color = Color.white;
			GUI.Label(new Rect(8f, 30f, num - 16f, 20f), "Entity ID:");
			if (PeSingleton<PeCreature>.Instance.mainPlayer != entity)
			{
				GUI.Label(new Rect(8f, 55f, num - 16f, 20f), "Scenario ID:");
			}
			else
			{
				GUI.Label(new Rect(8f, 55f, num - 16f, 20f), "Player ID:");
			}
			GUI.Label(new Rect(8f, 30f, num - 16f, 20f), entity.Id.ToString(), alignright);
			if (PeSingleton<PeCreature>.Instance.mainPlayer != entity)
			{
				GUI.Label(new Rect(8f, 55f, num - 16f, 20f), entity.scenarioId.ToString(), alignright);
			}
			else
			{
				GUI.Label(new Rect(8f, 55f, num - 16f, 20f), PeCustomScene.Self.scenario.playerId.ToString(), alignright);
			}
		}
		else
		{
			GUI.Label(new Rect(5f, 5f, num - 10f, 20f), go.name, bold);
		}
		GUI.EndGroup();
	}
}
