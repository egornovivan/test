using UnityEngine;

[RequireComponent(typeof(torch))]
public class PETorch : PeSword
{
	private torch mTorch;

	private torch torch
	{
		get
		{
			if (mTorch == null)
			{
				mTorch = GetComponent<torch>();
			}
			return mTorch;
		}
	}

	private void Update()
	{
		bool flag = false;
		flag = !(null != m_MotionMgr) || m_MotionMgr.GetMaskState(m_HandChangeAttr.m_HoldActionMask);
		torch.SetBurning(flag);
	}
}
