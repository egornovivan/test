using UnityEngine;

public interface IKeyJoyAccessor
{
	int FindInArray(KeySettingItem[] items, KeyCode key);

	void Set(KeySettingItem item, KeyCode keyToSet);

	KeyCode Get(KeySettingItem item);
}
