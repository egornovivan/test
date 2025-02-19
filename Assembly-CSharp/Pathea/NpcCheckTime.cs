using System.Collections.Generic;

namespace Pathea;

public class NpcCheckTime
{
	private List<CheckSlot> _sleepSlots = new List<CheckSlot>();

	private List<CheckSlot> _eatSlots = new List<CheckSlot>();

	private List<CheckSlot> _chatSlots = new List<CheckSlot>();

	public void AddSlots(EScheduleType type)
	{
		NPCScheduleData.Item item = NPCScheduleData.Get((int)type);
		switch (type)
		{
		case EScheduleType.Chat:
			_chatSlots.Clear();
			_chatSlots.AddRange(item.Slots);
			break;
		case EScheduleType.Eat:
			_eatSlots.Clear();
			_eatSlots.AddRange(item.Slots);
			break;
		case EScheduleType.Sleep:
			_sleepSlots.Clear();
			_sleepSlots.AddRange(item.Slots);
			break;
		}
	}

	public void AddSleepSlots(float min, float max)
	{
		_sleepSlots.Add(new CheckSlot(min, max));
	}

	public void AddEatSlots(float min, float max)
	{
		_eatSlots.Add(new CheckSlot(min, max));
	}

	public void AddChatSlots(float min, float max)
	{
		_chatSlots.Add(new CheckSlot(min, max));
	}

	public void ClearSleepSlots()
	{
		_sleepSlots.Clear();
	}

	public void ClearEatSlots()
	{
		_eatSlots.Clear();
	}

	public void ClearChatSlots()
	{
		_chatSlots.Clear();
	}

	public bool IsSleepTimeSlot(float timeSlot)
	{
		return _sleepSlots.Count > 0 && _sleepSlots.Find((CheckSlot ret) => ret.InSlot(timeSlot)) != null;
	}

	public bool IsEatTimeSlot(float timeSlot)
	{
		return _eatSlots.Count > 0 && _eatSlots.Find((CheckSlot ret) => ret.InSlot(timeSlot)) != null;
	}

	public bool IsChatTimeSlot(float timeSlot)
	{
		return _chatSlots.Count > 0 && _chatSlots.Find((CheckSlot ret) => ret.InSlot(timeSlot)) != null;
	}
}
