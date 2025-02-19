using System.Collections;
using UnityEngine;

namespace SkillSystem;

public class CoroutineStoppable : IEnumerator
{
	public bool stop;

	private IEnumerator enumerator;

	public object Current => enumerator.Current;

	public CoroutineStoppable(MonoBehaviour behaviour, IEnumerator enumerator)
	{
		stop = false;
		this.enumerator = enumerator;
		if (behaviour != null && behaviour.gameObject.activeSelf)
		{
			behaviour.StartCoroutine(this);
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
