using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class HitReactionCharacter : MonoBehaviour
{
	private string colliderName;

	[SerializeField]
	private string mixingAnim;

	[SerializeField]
	private Transform recursiveMixingTransform;

	[SerializeField]
	public Camera cam;

	[SerializeField]
	public HitReaction hitReaction;

	[SerializeField]
	public float hitForce = 1f;

	protected virtual void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(ray, out hitInfo, 100f))
			{
				hitReaction.Hit(hitInfo.collider, ray.direction * hitForce, hitInfo.point);
				colliderName = hitInfo.collider.name;
			}
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("LMB to shoot the Dummy, RMB to rotate the camera.");
		if (colliderName != string.Empty)
		{
			GUILayout.Label("Last Bone Hit: " + colliderName);
		}
	}

	private void Start()
	{
		if (mixingAnim != string.Empty)
		{
			GetComponent<Animation>()[mixingAnim].layer = 1;
			GetComponent<Animation>()[mixingAnim].AddMixingTransform(recursiveMixingTransform, recursive: true);
			GetComponent<Animation>().Play(mixingAnim);
		}
	}
}
