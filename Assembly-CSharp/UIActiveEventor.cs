using UnityEngine;
using UnityEngine.Events;

public class UIActiveEventor : MonoBehaviour
{
	[SerializeField]
	private UnityEvent OnActive;

	private void OnEnable()
	{
		OnActive.Invoke();
	}
}
