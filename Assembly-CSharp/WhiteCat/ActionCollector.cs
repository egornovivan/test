using UnityEngine;
using UnityEngine.Events;

namespace WhiteCat;

public class ActionCollector : MonoBehaviour
{
	[SerializeField]
	private UnityEvent _action = new UnityEvent();

	public event UnityAction action
	{
		add
		{
			_action.AddListener(value);
		}
		remove
		{
			_action.RemoveListener(value);
		}
	}

	public void InvokeActions()
	{
		_action.Invoke();
	}
}
