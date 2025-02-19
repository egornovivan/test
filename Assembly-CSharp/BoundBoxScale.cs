using UnityEngine;

public class BoundBoxScale : MonoBehaviour
{
	public GameObject xyfar;

	public GameObject xynear;

	public GameObject yzfar;

	public GameObject yznear;

	public void SetRestrictionRange(int maxX, int maxY, int maxZ)
	{
		float num = maxX;
		float num2 = maxY;
		xyfar.transform.localScale = new Vector3((num - 2f) / 10f, 0f, num2 / 10f);
		xynear.transform.localScale = new Vector3((num - 2f) / 10f, 0f, num2 / 10f);
		yzfar.transform.localScale = new Vector3((num - 2f) / 10f, 0f, num2 / 10f);
		yznear.transform.localScale = new Vector3((num - 2f) / 10f, 0f, num2 / 10f);
		xyfar.transform.localPosition = new Vector3(num / 2f, num2 / 2f, num - 1f);
		xynear.transform.localPosition = new Vector3(num / 2f, num2 / 2f, 0.5f);
		yzfar.transform.localPosition = new Vector3(num - 1f, num2 / 2f, num / 2f);
		yznear.transform.localPosition = new Vector3(0.5f, num2 / 2f, num / 2f);
	}
}
