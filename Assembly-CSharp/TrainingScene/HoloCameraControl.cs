using System.Collections.Generic;
using UnityEngine;

namespace TrainingScene;

public class HoloCameraControl : MonoBehaviour
{
	private static HoloCameraControl s_instance;

	private Camera holocmr;

	private Camera maincmr;

	[HideInInspector]
	public RenderTexture textureBase;

	public List<GameObject> renderObjs1 = new List<GameObject>();

	public List<MeshRenderer> renderObjs2 = new List<MeshRenderer>();

	public List<MeshRenderer> renderObjs3 = new List<MeshRenderer>();

	public static HoloCameraControl Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		holocmr = (Object.Instantiate(Resources.Load("TrainingHoloCamera")) as GameObject).GetComponent<Camera>();
		maincmr = Camera.main;
		Transform transform = holocmr.transform;
		transform.parent = maincmr.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		holocmr.targetTexture = new RenderTexture(Screen.width, Screen.height, 16);
		textureBase = holocmr.targetTexture;
	}

	private void Update()
	{
		holocmr.fieldOfView = maincmr.fieldOfView;
		if (0 < renderObjs1.Count)
		{
			foreach (GameObject item in renderObjs1)
			{
				item.SetActive(value: true);
			}
			holocmr.Render();
			{
				foreach (GameObject item2 in renderObjs1)
				{
					item2.SetActive(value: false);
				}
				return;
			}
		}
		if (0 >= renderObjs2.Count)
		{
			return;
		}
		foreach (MeshRenderer item3 in renderObjs2)
		{
			item3.enabled = true;
		}
		if (0 < renderObjs3.Count)
		{
			foreach (MeshRenderer item4 in renderObjs3)
			{
				item4.enabled = true;
			}
		}
		holocmr.Render();
		foreach (MeshRenderer item5 in renderObjs2)
		{
			item5.enabled = false;
		}
		if (0 >= renderObjs3.Count)
		{
			return;
		}
		foreach (MeshRenderer item6 in renderObjs3)
		{
			item6.enabled = false;
		}
	}
}
