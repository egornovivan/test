using UnityEngine;

public class CampBuildEventMgr : MonoBehaviour
{
	public GameObject[] objects;

	private bool mInit;

	private bool HasShow;

	private TimeSlot[] Times;

	private int index = -1;

	private void Awake()
	{
		Init();
	}

	private void Start()
	{
	}

	private void Init()
	{
		TimeSlot timeSlot = new TimeSlot(12f, 12.5f);
		TimeSlot timeSlot2 = new TimeSlot(19f, 19.5f);
		Times = new TimeSlot[2];
		Times[0] = timeSlot;
		Times[1] = timeSlot2;
		mInit = true;
	}

	private void CalculateTimes()
	{
		for (int i = 0; i < Times.Length; i++)
		{
			if (!HasShow && Times[i].InTheRange((float)GameTime.Timer.HourInDay))
			{
				objects[0].SetActive(value: true);
				index = i;
				HasShow = true;
			}
		}
		if (HasShow && index >= 0 && !Times[index].InTheRange((float)GameTime.Timer.HourInDay))
		{
			objects[0].SetActive(value: false);
			HasShow = false;
			index = -1;
		}
	}

	private void Update()
	{
		if (!mInit)
		{
			Init();
		}
		if (mInit)
		{
			CalculateTimes();
		}
	}
}
