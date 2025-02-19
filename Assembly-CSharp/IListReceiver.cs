using UnityEngine;

public interface IListReceiver
{
	void SetContent(int index, GameObject go);

	void ClearContent(GameObject go);
}
