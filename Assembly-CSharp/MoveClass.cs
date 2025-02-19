using UnityEngine;

public class MoveClass : MonoBehaviour
{
	private bool bMove;

	private void Start()
	{
	}

	private void Update()
	{
		if (bMove)
		{
			Vector3 position = base.gameObject.transform.position;
			base.gameObject.transform.position = new Vector3(position.x + 2f, position.y, position.z);
		}
	}

	public void StarMove()
	{
		Debug.Log("Btn On Click!");
		bMove = !bMove;
	}
}
