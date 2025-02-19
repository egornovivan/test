using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class CampFoodTimer : MonoBehaviour
{
	private List<CheckSlot> timeSlots;

	private bool HasShow;

	public GameObject foodObj;

	private int index;

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (foodObj == null || timeSlots == null || timeSlots.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < timeSlots.Count; i++)
		{
			if (!HasShow && timeSlots[i].InSlot((float)GameTime.Timer.HourInDay))
			{
				foodObj.SetActive(value: true);
				index = i;
				HasShow = true;
			}
		}
		if (HasShow && index >= 0 && !timeSlots[index].InSlot((float)GameTime.Timer.HourInDay))
		{
			foodObj.SetActive(value: false);
			HasShow = false;
			index = -1;
		}
	}

	public void SetSlots(List<CheckSlot> slots)
	{
		if (timeSlots == null)
		{
			timeSlots = new List<CheckSlot>();
		}
		timeSlots.Clear();
		timeSlots.AddRange(slots);
	}
}
