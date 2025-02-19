using UnityEngine;

namespace AnimFollow;

[RequireComponent(typeof(AudioSource))]
public class ShootStuff_AF : MonoBehaviour
{
	public Camera theCamera;

	private Rect guiBox = new Rect(5f, 5f, 160f, 120f);

	public Texture crosshairTexture;

	private RaycastHit raycastHit;

	public float bulletForce = 8000f;

	private bool userNeedsToFixStuff;

	private void Awake()
	{
		if (!theCamera)
		{
			Debug.LogWarning("You need to assign a camera to the ShootStuff script on " + base.name);
			userNeedsToFixStuff = true;
		}
		else if (!crosshairTexture)
		{
			Debug.LogWarning("You need to assign crosshairTexture in the ShootStuff script on " + base.name);
			userNeedsToFixStuff = true;
		}
		else
		{
			Cursor.visible = false;
		}
		if (GetComponent<AudioSource>().clip == null)
		{
			Debug.LogWarning("Assign audio clip to audiosource on " + base.name + "\n");
		}
	}

	private void Update()
	{
		if (userNeedsToFixStuff)
		{
			return;
		}
		if (Input.GetMouseButton(1) && !guiBox.Contains(new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y)))
		{
			theCamera.fieldOfView = 30f;
		}
		else
		{
			theCamera.fieldOfView = 60f;
		}
		if (Input.GetMouseButtonDown(0) && !guiBox.Contains(new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y)))
		{
			GetComponent<AudioSource>().Play();
			Ray ray = theCamera.ScreenPointToRay(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			if (Physics.Raycast(ray, out raycastHit, 100f))
			{
				BulletHitInfo_AF bulletHitInfo_AF = new BulletHitInfo_AF();
				bulletHitInfo_AF.hitTransform = raycastHit.transform;
				bulletHitInfo_AF.bulletForce = (raycastHit.point - base.transform.position).normalized * bulletForce;
				bulletHitInfo_AF.hitNormal = raycastHit.normal;
				bulletHitInfo_AF.hitPoint = raycastHit.point;
				raycastHit.transform.root.SendMessage("HitByBullet", bulletHitInfo_AF, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void OnGUI()
	{
		if (!userNeedsToFixStuff)
		{
			GUI.DrawTexture(new Rect(Input.mousePosition.x - 20f, (float)Screen.height - Input.mousePosition.y - 20f, 40f, 40f), crosshairTexture, ScaleMode.ScaleToFit, alphaBlend: true);
			GUI.Box(guiBox, "Fire = Left mouse\nB = Launch ball\nN = Slow motion\nZoom = Right mouse\n\nBullet force");
			bulletForce = GUI.HorizontalSlider(new Rect(10f, 105f, 150f, 15f), bulletForce, 1000f, 20000f);
		}
	}
}
