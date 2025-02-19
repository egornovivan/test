using FMOD;
using FMOD.Studio;
using UnityEngine;

public class FMODAudioListener : MonoBehaviour
{
	private static FMODAudioListener sListener;

	private Rigidbody cachedRigidBody;

	public static FMODAudioListener listener => sListener;

	private void OnEnable()
	{
		if (listener != this && listener != null)
		{
			Object.Destroy(this);
			return;
		}
		Application.runInBackground = true;
		sListener = this;
		if (FMODAudioSystem.system != null)
		{
			FMODAudioSystem.system.setNumListeners(1);
			Update3DAttributes();
		}
	}

	private void OnDisable()
	{
		if (!FMODAudioSystem.isShutDown)
		{
			if (sListener == this)
			{
				sListener = null;
			}
			if (FMODAudioSystem.system != null)
			{
				FMODAudioSystem.system.setNumListeners(0);
			}
		}
	}

	private void Update()
	{
		Update3DAttributes();
	}

	private void Update3DAttributes()
	{
		if (FMODAudioSystem.system != null)
		{
			int numlisteners = 0;
			FMODAudioSystem.system.getNumListeners(out numlisteners);
			if (numlisteners == 1)
			{
				FMOD.Studio.ATTRIBUTES_3D attributes = UnityUtil.to3DAttributes(base.gameObject, cachedRigidBody);
				ERRCHECK(FMODAudioSystem.system.setListenerAttributes(0, attributes));
			}
		}
	}

	private void ERRCHECK(RESULT result)
	{
		UnityUtil.ERRCHECK(result);
	}
}
