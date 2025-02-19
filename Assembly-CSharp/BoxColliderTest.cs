using UnityEngine;

public class BoxColliderTest : MonoBehaviour
{
	[SerializeField]
	private BoxCollider mBgCollider;

	[SerializeField]
	private BoxCollider mTopCollider;

	private bool isCover;

	private Bounds bounds;

	private void Start()
	{
		bounds = mBgCollider.bounds;
	}

	private void Update()
	{
	}
}
