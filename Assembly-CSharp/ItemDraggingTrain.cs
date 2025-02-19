using Pathea;
using UnityEngine;

public class ItemDraggingTrain : ItemDraggingBase
{
	public override bool OnDragging(Ray cameraRay)
	{
		PEStationCtrl station = GetStation();
		if (null != station && !station.isJoint && PERailwayCtrl.CheckRoute(station.point))
		{
			base.rootGameObject.transform.position = station.station.mJointPoint.position;
			base.rootGameObject.transform.rotation = station.station.mJointPoint.rotation;
			return true;
		}
		base.OnDragging(cameraRay);
		return false;
	}

	public override bool OnPutDown()
	{
		PEStationCtrl station = GetStation();
		if (null != station)
		{
			PeSingleton<RailwayOperate>.Instance.RequestAutoCreateRoute(station.point.id, base.itemObjectId);
		}
		return base.OnPutDown();
	}

	private PEStationCtrl GetStation()
	{
		PEStationCtrl result = null;
		if (PeSingleton<MousePicker>.Instance.curPickObj != null && !PeSingleton<MousePicker>.Instance.curPickObj.Equals(null))
		{
			result = PeSingleton<MousePicker>.Instance.curPickObj as PEStationCtrl;
		}
		return result;
	}
}
