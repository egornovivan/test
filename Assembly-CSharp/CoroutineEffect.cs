using System.Collections;
using UnityEngine;

public class CoroutineEffect : IEnumerator
{
	public bool stop;

	private IEnumerator enumerator;

	private Coroutine coroutine;

	public object Current => enumerator.Current;

	public CoroutineEffect(MonoBehaviour behaviour, IEnumerator enumerator)
	{
		stop = false;
		this.enumerator = enumerator;
		coroutine = behaviour.StartCoroutine(this);
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
