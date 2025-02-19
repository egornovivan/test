using System;
using UnityEngine;

public class UISetGoPos_yToInt : MonoBehaviour
{
	public GameObject mGo;

	private SpringPanel mSp;

	private int frame;

	private void Start()
	{
		mSp = mGo.GetComponent<SpringPanel>();
	}

	private void Update()
	{
		frame++;
		if (mGo != null && frame % 10 == 0 && mSp != null && !mSp.enabled)
		{
			Vector3 localPosition = mGo.transform.localPosition;
			int value = Convert.ToInt32(localPosition.y);
			mGo.transform.localPosition = new Vector3(localPosition.x, Convert.ToSingle(value), localPosition.z);
			frame = 0;
		}
	}
}
