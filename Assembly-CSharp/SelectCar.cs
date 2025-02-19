using System;
using UnityEngine;
using WhiteCat;

public class SelectCar : SingletonBehaviour<SelectCar>
{
	private Transform[] trans;

	private PathDriver[] drivers;

	private TweenMaterialColor[] tweens;

	private int index = 1;

	private TweenInterpolator interpolator;

	private RaycastHit hitInfo;

	public static Transform carTransform => SingletonBehaviour<SelectCar>.instance.trans[SingletonBehaviour<SelectCar>.instance.index];

	public static PathDriver carDriver => SingletonBehaviour<SelectCar>.instance.drivers[SingletonBehaviour<SelectCar>.instance.index];

	public static void HideUnselected()
	{
		for (int i = 0; i < 3; i++)
		{
			if (SingletonBehaviour<SelectCar>.instance.index != i)
			{
				SingletonBehaviour<SelectCar>.instance.drivers[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void ShowAll()
	{
		for (int i = 0; i < 3; i++)
		{
			drivers[i].gameObject.SetActive(value: true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		trans = new Transform[3];
		drivers = new PathDriver[3];
		tweens = new TweenMaterialColor[3];
		for (int i = 0; i < 3; i++)
		{
			trans[i] = base.transform.GetChild(i).GetChild(0);
			drivers[i] = trans[i].parent.GetComponent<PathDriver>();
			tweens[i] = trans[i].GetComponentInChildren<TweenMaterialColor>();
		}
		interpolator = GetComponent<TweenInterpolator>();
	}

	private void OnEnable()
	{
		interpolator.normalizedTime = 0f;
		interpolator.isPlaying = true;
		tweens[index].OnRecord();
		tweens[index].enabled = true;
	}

	private void OnDisable()
	{
		tweens[index].enabled = false;
		tweens[index].OnRestore();
		interpolator.isPlaying = false;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
		{
			tweens[index].enabled = false;
			tweens[index].OnRestore();
			index = Array.IndexOf(trans, hitInfo.transform);
			interpolator.normalizedTime = 0f;
			tweens[index].OnRecord();
			tweens[index].enabled = true;
		}
	}
}
