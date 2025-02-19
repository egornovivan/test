using System;
using UnityEngine;
using WhiteCat;

public class ItemDraggingCreation : ItemDraggingArticle
{
	private CreationController controller;

	private Vector3[] locVecs;

	private float boundsHeight;

	private Vector3 boundsSize;

	private int detectCount;

	private float detectStep;

	private static Vector3[] vecs = new Vector3[8];

	private bool _valid;

	private float _lastOffsetY;

	private bool _inDungeonMsgTip;

	public override void OnDragOut()
	{
		DraggingDistance = 50f;
		controller = GetComponent<CreationController>();
		base.transform.rotation = Quaternion.identity;
		Bounds bounds = controller.bounds;
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		boundsHeight = min.y;
		boundsSize = max - min;
		locVecs = new Vector3[8];
		locVecs[0] = min;
		ref Vector3 reference = ref locVecs[1];
		reference = new Vector3(max.x, min.y, min.z);
		ref Vector3 reference2 = ref locVecs[2];
		reference2 = new Vector3(max.x, min.y, max.z);
		ref Vector3 reference3 = ref locVecs[3];
		reference3 = new Vector3(min.x, min.y, max.z);
		ref Vector3 reference4 = ref locVecs[4];
		reference4 = new Vector3(min.x, max.y, min.z);
		ref Vector3 reference5 = ref locVecs[5];
		reference5 = new Vector3(max.x, max.y, min.z);
		locVecs[6] = max;
		ref Vector3 reference6 = ref locVecs[7];
		reference6 = new Vector3(min.x, max.y, max.z);
		float num = Mathf.Clamp((max.z - min.z) * 0.5f, 1f, 32f);
		if (num > 5f)
		{
			detectStep = 0.5f;
			detectCount = Mathf.RoundToInt(num / detectStep);
		}
		else
		{
			detectCount = 10;
			detectStep = num / (float)detectCount;
		}
		controller.collidable = false;
	}

	public override bool OnDragging(Ray cameraRay)
	{
		if (RandomDungenMgrData.InDungeon)
		{
			if (!_inDungeonMsgTip)
			{
				PeTipMsg.Register(PELocalization.GetString(8000732), PeTipMsg.EMsgLevel.Warning);
				_inDungeonMsgTip = true;
			}
			return false;
		}
		if ((bool)RobotController.playerFollower && controller.category == EVCCategory.cgRobot)
		{
			_valid = false;
			mTooFar = true;
			return false;
		}
		if (Physics.Raycast(cameraRay, out fhitInfo, DraggingDistance, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore))
		{
			mTooFar = false;
			_valid = SetPos(fhitInfo.point);
			if (!base.rootGameObject.activeSelf)
			{
				base.rootGameObject.SetActive(value: true);
			}
			return _valid;
		}
		_valid = false;
		mTooFar = true;
		return false;
	}

	private new bool SetPos(Vector3 newPos)
	{
		Vector3 position = newPos;
		newPos.y = newPos.y - boundsHeight + 0.1f;
		base.transform.position = newPos;
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref vecs[i];
			reference = base.transform.TransformPoint(locVecs[i]);
		}
		Vector3 direction = vecs[3] - vecs[0];
		Vector3 direction2 = vecs[1] - vecs[0];
		_valid = false;
		for (int j = 0; j < detectCount; j++)
		{
			_valid = true;
			if (Physics.Linecast(vecs[0], vecs[6], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.Linecast(vecs[1], vecs[7], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.Linecast(vecs[5], vecs[3], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.Linecast(vecs[4], vecs[2], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.CapsuleCast(vecs[0], vecs[1], 0.01f, direction, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.CapsuleCast(vecs[0], vecs[4], 0.01f, direction, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.CapsuleCast(vecs[1], vecs[5], 0.01f, direction, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.CapsuleCast(vecs[0], vecs[4], 0.01f, direction2, boundsSize.x, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.CapsuleCast(vecs[3], vecs[7], 0.01f, direction2, boundsSize.x, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.CapsuleCast(vecs[4], vecs[5], 0.01f, direction, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.Linecast(vecs[6], vecs[0], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.Linecast(vecs[7], vecs[1], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.Linecast(vecs[3], vecs[5], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore) || Physics.Linecast(vecs[2], vecs[4], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore))
			{
				_valid = false;
				newPos.y += detectStep;
				for (int k = 0; k < 8; k++)
				{
					vecs[k].y += detectStep;
				}
				continue;
			}
			break;
		}
		if (_valid)
		{
			base.transform.position = newPos;
			_lastOffsetY = newPos.y - position.y;
		}
		else
		{
			position.y += _lastOffsetY;
			base.transform.position = position;
		}
		return _valid;
	}

	public override bool OnPutDown()
	{
		if (NetworkInterface.IsClient)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				IntVector3 intVector = new IntVector3(base.transform.position + 0.1f * Vector3.down);
				byte type = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z).Type;
				PlayerNetwork.mainPlayer.RequestDragOut(itemDragging.itemObj.instanceId, base.transform.position, base.transform.localScale, base.transform.rotation, type);
			}
		}
		else
		{
			PutDown(isCreation: true);
		}
		return false;
	}

	private void OnEnable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnGL));
	}

	private void OnDisable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnGL));
	}

	private void OnGL(Camera camera)
	{
		if (camera == Camera.main && locVecs != null)
		{
			PEVCConfig.instance.handleMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(base.transform.localToWorldMatrix);
			GL.Begin(1);
			GL.Color((!_valid) ? PEVCConfig.instance.dragInvalidLineColor : PEVCConfig.instance.dragValidLineColor);
			GL.Vertex(locVecs[0]);
			GL.Vertex(locVecs[1]);
			GL.Vertex(locVecs[1]);
			GL.Vertex(locVecs[2]);
			GL.Vertex(locVecs[2]);
			GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[0]);
			GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[0]);
			GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[1]);
			GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[2]);
			GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[7]);
			GL.End();
			GL.Begin(7);
			GL.Color((!_valid) ? PEVCConfig.instance.dragInvalidPlaneColor : PEVCConfig.instance.dragValidPlaneColor);
			GL.Vertex(locVecs[0]);
			GL.Vertex(locVecs[1]);
			GL.Vertex(locVecs[2]);
			GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[0]);
			GL.Vertex(locVecs[1]);
			GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[2]);
			GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[1]);
			GL.Vertex(locVecs[2]);
			GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[0]);
			GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[4]);
			GL.End();
			GL.PopMatrix();
		}
	}
}
