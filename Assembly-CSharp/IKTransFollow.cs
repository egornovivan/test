using UnityEngine;

[ExecuteInEditMode]
public class IKTransFollow : MonoBehaviour
{
	[SerializeField]
	[SetProperty("FollowTarget")]
	private Transform m_FollowTarget;

	public Transform FollowTarget
	{
		get
		{
			return m_FollowTarget;
		}
		set
		{
			m_FollowTarget = value;
			SyncInfo();
		}
	}

	[ContextMenu("ExecSyncInfo")]
	public void SyncInfo()
	{
		if (null != m_FollowTarget)
		{
			base.transform.position = m_FollowTarget.position;
			base.transform.rotation = m_FollowTarget.rotation;
		}
		else
		{
			base.transform.position = Vector3.zero;
			base.transform.rotation = Quaternion.identity;
		}
	}

	private void Update()
	{
		SyncInfo();
	}

	private void LateUpdate()
	{
		SyncInfo();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
	}
}
