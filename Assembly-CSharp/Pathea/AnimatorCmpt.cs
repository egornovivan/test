using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class AnimatorCmpt : PeCmpt, IPeMsg
{
	private Animator m_Animator;

	private Dictionary<int, bool> m_Bools;

	private Dictionary<string, float> m_Floats;

	private Dictionary<string, int> m_Integers;

	private Dictionary<int, float> m_Layers;

	private float m_Speed = 1f;

	public Quaternion m_LastRot;

	public Vector3 m_LastMove;

	private HashSet<int> m_Parameters;

	private static Queue<Transform> toCheck = new Queue<Transform>(32);

	public Animator animator
	{
		get
		{
			return m_Animator;
		}
		set
		{
			if (m_Animator != value)
			{
				m_Animator = value;
				if (m_Animator != null)
				{
					InitParameters();
					InitAnimator();
				}
			}
		}
	}

	public float speed
	{
		get
		{
			return (!(null != animator)) ? m_Speed : animator.speed;
		}
		set
		{
			m_Speed = value;
			if (null != animator)
			{
				animator.speed = m_Speed;
			}
		}
	}

	public bool hasAnimator => null != m_Animator;

	public event Action<string> AnimEvtString;

	public bool ContainsParameter(string name)
	{
		return m_Parameters != null && m_Parameters.Contains(Animator.StringToHash(name));
	}

	public bool ContainsParameter(int paraHash)
	{
		return m_Parameters != null && m_Parameters.Contains(paraHash);
	}

	public void SetBool(string name, bool value)
	{
		if (!string.IsNullOrEmpty(name))
		{
			SetBool(Animator.StringToHash(name), value);
		}
	}

	public void SetBool(int name, bool value)
	{
		m_Bools[name] = value;
		if (m_Animator != null && ContainsParameter(name))
		{
			m_Animator.SetBool(name, value);
		}
	}

	public void SetFloat(string name, float value)
	{
		if (!string.IsNullOrEmpty(name))
		{
			m_Floats[name] = value;
			if (m_Animator != null && ContainsParameter(name))
			{
				m_Animator.SetFloat(name, value);
			}
		}
	}

	public void SetInteger(string name, int value)
	{
		if (!string.IsNullOrEmpty(name))
		{
			m_Integers[name] = value;
			if (m_Animator != null && ContainsParameter(name))
			{
				m_Animator.SetInteger(name, value);
			}
		}
	}

	public void SetTrigger(string name)
	{
		if (!string.IsNullOrEmpty(name) && m_Animator != null && ContainsParameter(name))
		{
			m_Animator.SetTrigger(name);
		}
	}

	public void ResetTrigger(string name)
	{
		if (!string.IsNullOrEmpty(name) && m_Animator != null && ContainsParameter(name))
		{
			m_Animator.ResetTrigger(name);
		}
	}

	public void SetLayerWeight(int layerIndex, float weight)
	{
		if (m_Animator == null || layerIndex < 0 || layerIndex >= m_Animator.layerCount)
		{
			return;
		}
		float value = -1f;
		if (!m_Layers.TryGetValue(layerIndex, out value) || Mathf.Approximately(value, weight))
		{
			m_Layers[layerIndex] = weight;
			if (m_Animator != null)
			{
				m_Animator.SetLayerWeight(layerIndex, weight);
			}
		}
	}

	public void SetLayerWeight(string layerName, float weight)
	{
		if (!(null == m_Animator))
		{
			SetLayerWeight(GetLayerIndex(layerName), weight);
		}
	}

	public bool GetBool(string name)
	{
		bool result = false;
		if (m_Animator != null && ContainsParameter(name))
		{
			result = m_Animator.GetBool(name);
		}
		return result;
	}

	public float GetFloat(string name)
	{
		float value = 0f;
		if (!m_Floats.TryGetValue(name, out value) && m_Animator != null && ContainsParameter(name))
		{
			value = m_Animator.GetFloat(name);
		}
		return value;
	}

	public int GetInteger(string name)
	{
		int value = 0;
		if (!m_Integers.TryGetValue(name, out value) && m_Animator != null && ContainsParameter(name))
		{
			value = m_Animator.GetInteger(name);
		}
		return value;
	}

	public float GetLayerWeight(int layerIndex)
	{
		float value = 0f;
		if (!m_Layers.TryGetValue(layerIndex, out value) && m_Animator != null)
		{
			value = m_Animator.GetLayerWeight(layerIndex);
		}
		return value;
	}

	public AnimatorStateInfo GetAnimatorStateInfo(int layerIndex)
	{
		if (m_Animator != null)
		{
			return m_Animator.GetCurrentAnimatorStateInfo(layerIndex);
		}
		return default(AnimatorStateInfo);
	}

	public bool IsAnimPlaying(string animName, int layer = -1)
	{
		if (null != m_Animator)
		{
			if (layer == -1)
			{
				for (int i = 0; i < m_Animator.layerCount; i++)
				{
					string layerName = m_Animator.GetLayerName(i);
					if (m_Animator.GetCurrentAnimatorStateInfo(i).IsName(layerName + "." + animName) || (m_Animator.IsInTransition(i) && m_Animator.GetNextAnimatorStateInfo(i).IsName(layerName + "." + animName)))
					{
						return true;
					}
				}
			}
			else if (layer >= 0 && layer < m_Animator.layerCount)
			{
				string layerName2 = m_Animator.GetLayerName(layer);
				return m_Animator.GetCurrentAnimatorStateInfo(layer).IsName(layerName2 + "." + animName) || (m_Animator.IsInTransition(layer) && m_Animator.GetNextAnimatorStateInfo(layer).IsName(layerName2 + "." + animName));
			}
		}
		return false;
	}

	public bool IsInTransition(int layer)
	{
		if (null != m_Animator && layer < m_Animator.layerCount)
		{
			return m_Animator.IsInTransition(layer);
		}
		return false;
	}

	public int GetLayerCount()
	{
		if (null != m_Animator)
		{
			return m_Animator.layerCount;
		}
		return 0;
	}

	public int GetLayerIndex(string layerName)
	{
		if (null != m_Animator)
		{
			return m_Animator.GetLayerIndex(layerName);
		}
		return -1;
	}

	public string GetLayerName(int layerIndex)
	{
		if (null != m_Animator)
		{
			return m_Animator.GetLayerName(layerIndex);
		}
		return string.Empty;
	}

	private void InitParameters()
	{
		AnimatorControllerParameter[] parameters = m_Animator.parameters;
		int num = parameters.Length;
		m_Parameters = new HashSet<int>();
		for (int i = 0; i < num; i++)
		{
			m_Parameters.Add(Animator.StringToHash(parameters[i].name));
		}
	}

	private void InitAnimator()
	{
		for (int i = 0; i < m_Animator.layerCount; i++)
		{
			if (!m_Layers.ContainsKey(i))
			{
				m_Layers.Add(i, m_Animator.GetLayerWeight(i));
			}
			else
			{
				m_Animator.SetLayerWeight(i, m_Layers[i]);
			}
		}
		foreach (KeyValuePair<int, bool> @bool in m_Bools)
		{
			if (ContainsParameter(@bool.Key))
			{
				m_Animator.SetBool(@bool.Key, @bool.Value);
			}
		}
		foreach (KeyValuePair<string, float> @float in m_Floats)
		{
			if (ContainsParameter(@float.Key))
			{
				m_Animator.SetFloat(@float.Key, @float.Value);
			}
		}
		foreach (KeyValuePair<string, int> integer in m_Integers)
		{
			if (ContainsParameter(integer.Key))
			{
				m_Animator.SetInteger(integer.Key, integer.Value);
			}
		}
	}

	public void AnimEvent(string para)
	{
		if (this.AnimEvtString != null)
		{
			this.AnimEvtString(para);
		}
	}

	public override void Awake()
	{
		base.Awake();
		m_Bools = new Dictionary<int, bool>();
		m_Floats = new Dictionary<string, float>();
		m_Integers = new Dictionary<string, int>();
		m_Layers = new Dictionary<int, float>();
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		if (msg == EMsg.View_Model_Build && base.isActiveAndEnabled)
		{
			StartCoroutine(FindAnimator(args[0] as GameObject, 0.5f));
		}
	}

	private IEnumerator FindAnimator(GameObject obj, float delayTime)
	{
		if (delayTime > 0f)
		{
			yield return new WaitForSeconds(delayTime);
		}
		if (null == obj)
		{
			yield break;
		}
		toCheck.Clear();
		toCheck.Enqueue(obj.transform);
		while (toCheck.Count > 0)
		{
			Transform tran = toCheck.Dequeue();
			animator = tran.GetComponent<Animator>();
			if (animator != null)
			{
				break;
			}
			foreach (Transform t in tran)
			{
				toCheck.Enqueue(t);
			}
		}
		speed = m_Speed;
	}
}
