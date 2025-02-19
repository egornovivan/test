using System;
using UnityEngine;

public class TestWeaponEffect : MonoBehaviour
{
	[Serializable]
	public class Attack
	{
		public string attackAnim;

		public GameObject maleEffect;

		public GameObject femaleEffect;
	}

	[SerializeField]
	private Animator maleAnim;

	[SerializeField]
	private Animator femaleAnim;

	[SerializeField]
	private Attack[] m_Attacks;

	private void Start()
	{
	}

	private void Update()
	{
		bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		for (int i = 0; i < m_Attacks.Length; i++)
		{
			if (Input.GetKeyDown((KeyCode)(48 + i - (flag ? 10 : 0))))
			{
				Debug.LogError("AttackStart");
				if (null != maleAnim)
				{
					maleAnim.SetTrigger(m_Attacks[i].attackAnim);
				}
				if (null != m_Attacks[i].maleEffect)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(m_Attacks[i].maleEffect);
					EffectLateupdateHelper effectLateupdateHelper = gameObject.AddComponent<EffectLateupdateHelper>();
					effectLateupdateHelper.Init(maleAnim.transform);
				}
				if (null != femaleAnim)
				{
					femaleAnim.SetTrigger(m_Attacks[i].attackAnim);
				}
				if (null != m_Attacks[i].maleEffect)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(m_Attacks[i].femaleEffect);
					EffectLateupdateHelper effectLateupdateHelper2 = gameObject2.AddComponent<EffectLateupdateHelper>();
					effectLateupdateHelper2.Init(femaleAnim.transform);
				}
			}
		}
	}
}
