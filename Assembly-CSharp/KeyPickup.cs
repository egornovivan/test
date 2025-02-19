using DunGen;
using Pathea;
using UnityEngine;

public class KeyPickup : MonoBehaviour, IKeyLock
{
	[HideInInspector]
	[SerializeField]
	private int keyID;

	[SerializeField]
	[HideInInspector]
	private KeyManager keyManager;

	public Key Key => keyManager.GetKeyByID(keyID);

	public void OnKeyAssigned(Key key, KeyManager keyManager)
	{
		keyID = key.ID;
		this.keyManager = keyManager;
	}

	private void OnTriggerEnter(Collider c)
	{
		MainPlayerCmpt componentInParent = c.GetComponentInParent<MainPlayerCmpt>();
		if (!(componentInParent == null))
		{
			RandomDungenMgr.Instance.PickUpKey(keyID);
			ScreenText.Log("Picked up {0} key", Key.Name);
			UnityUtil.Destroy(base.gameObject);
		}
	}
}
