using UnityEngine;

public class ItemScript_Door : ItemScript_State
{
	public int mOpenSoundID;

	public int mCloseSoundID;

	public bool IsOpen => mSubState > 0;

	public void OpenDoor()
	{
		SetState(1);
	}

	public void ShutDoor()
	{
		SetState(0);
	}

	protected override void ApplyState(int state)
	{
		base.ApplyState(state);
		if (IsOpen)
		{
			base.gameObject.GetComponent<Animation>().CrossFade("Open");
			AudioManager.instance.Create(base.transform.position, mOpenSoundID);
		}
		else
		{
			base.gameObject.GetComponent<Animation>().CrossFade("Close");
			AudioManager.instance.Create(base.transform.position, mCloseSoundID);
		}
	}
}
