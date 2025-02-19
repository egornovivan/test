using System.Collections;
using UnityEngine;

namespace SkillAsset;

public class CoroutineStoppable : IEnumerator
{
	public bool stop;

	private IEnumerator enumerator;

	private MonoBehaviour behaviour;

	private Coroutine coroutine;

	public object Current => enumerator.Current;

	public CoroutineStoppable(MonoBehaviour behaviour, IEnumerator enumerator)
	{
		stop = false;
		this.behaviour = behaviour;
		this.enumerator = enumerator;
		if (behaviour != null && behaviour.gameObject.activeSelf)
		{
			coroutine = behaviour.StartCoroutine(this);
		}
	}

	public bool MoveNext()
	{
		return !stop && enumerator.MoveNext();
	}

	public void Reset()
	{
		enumerator.Reset();
	}
}
