using UnityEngine;

public class WorldMapBG : MonoBehaviour
{
	public GameObject mMsgtarget;

	private void OnDrag(Vector2 delta)
	{
		if (Input.GetMouseButton(0))
		{
			mMsgtarget.SendMessage("OnMapDrag", delta, SendMessageOptions.DontRequireReceiver);
		}
	}
}
