using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Sound")]
public class UIButtonSound : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease
	}

	public float volume = 1f;

	public float pitch = 1f;

	public Trigger trigger;

	[Header("NGUI Mode Use")]
	public AudioClip audioClip;

	[Header("PE Mode Use")]
	public int AudioID = 422;

	[Header("Change Model")]
	public bool UsePeMode = true;

	private void OnHover(bool isOver)
	{
		if (base.enabled && ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut)))
		{
			PlaySound();
		}
	}

	private void OnPress(bool isPressed)
	{
		if (base.enabled && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
		{
			PlaySound();
		}
	}

	private void OnClick()
	{
		if (base.enabled && trigger == Trigger.OnClick)
		{
			PlaySound();
		}
	}

	private void PlaySound()
	{
		if (UsePeMode)
		{
			if (null != AudioManager.instance)
			{
				AudioManager.instance.Create(Vector3.zero, AudioID);
			}
		}
		else
		{
			NGUITools.PlaySound(audioClip, volume, pitch);
		}
	}
}
