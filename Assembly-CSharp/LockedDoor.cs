using DunGen;
using Pathea;
using UnityEngine;

public class LockedDoor : MonoBehaviour, IKeyLock
{
	public float OpenDuration = 1f;

	public Vector3 OpenPositionOffset = new Vector3(0f, -3f, 0f);

	[SerializeField]
	[HideInInspector]
	private int keyID;

	[SerializeField]
	[HideInInspector]
	private KeyManager keyManager;

	private Vector3 initialPosition;

	private float openTime;

	private bool isOpening;

	private Door door;

	public Key Key => keyManager.GetKeyByID(keyID);

	public bool IsOpen => door.IsOpen;

	private void Start()
	{
		door = GetComponent<Door>();
	}

	public void OnKeyAssigned(Key key, KeyManager keyManager)
	{
		keyID = key.ID;
		this.keyManager = keyManager;
	}

	private void OnTriggerEnter(Collider c)
	{
		if (isOpening)
		{
			return;
		}
		MainPlayerCmpt componentInParent = c.GetComponentInParent<MainPlayerCmpt>();
		if (!(componentInParent == null))
		{
			if (RandomDungenMgr.Instance.HasKey(keyID))
			{
				ScreenText.Log("Opened {0} door", Key.Name);
				RandomDungenMgr.Instance.RemoveKey(keyID);
				Open();
			}
			else
			{
				ScreenText.Log("{0} key required", Key.Name);
			}
		}
	}

	private void Update()
	{
		if (isOpening)
		{
			openTime += Time.deltaTime;
			if (openTime >= OpenDuration)
			{
				openTime = OpenDuration;
				isOpening = false;
			}
			base.transform.position = Vector3.Lerp(initialPosition, initialPosition + OpenPositionOffset, openTime / OpenDuration);
		}
	}

	public void Open()
	{
		if (!isOpening)
		{
			isOpening = true;
			initialPosition = base.transform.position;
			openTime = 0f;
			door.IsOpen = true;
		}
	}
}
