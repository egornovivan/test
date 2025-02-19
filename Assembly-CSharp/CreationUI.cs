using UnityEngine;

public class CreationUI : MonoBehaviour
{
	private static CreationUI s_Instance;

	public Transform m_MissileLockerGroup;

	public MissileLockerUI m_MissileLockerPrefab;

	public static CreationUI Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		s_Instance = null;
	}

	private void Update()
	{
	}

	public MissileLockerUI MissileLockObject(Transform target)
	{
		MissileLockerUI missileLockerUI = Object.Instantiate(m_MissileLockerPrefab);
		missileLockerUI.transform.parent = m_MissileLockerGroup;
		missileLockerUI.transform.localScale = Vector3.one;
		missileLockerUI.UpdatePos();
		missileLockerUI.m_TargetObject = target;
		missileLockerUI.FadeIn();
		return missileLockerUI;
	}
}
