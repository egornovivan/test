using UnityEngine;

namespace AnimFollow;

public class ToggleSlomo_AF : MonoBehaviour
{
	public float slomoOnKeyN = 0.3f;

	private bool slomo;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.N) && !slomo)
		{
			Time.timeScale = slomoOnKeyN;
			slomo = true;
		}
		else if (slomo && Input.GetKeyDown(KeyCode.N))
		{
			Time.timeScale = 1f;
			slomo = false;
		}
	}
}
