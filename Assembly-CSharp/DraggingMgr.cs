using Pathea;
using PeEvent;
using UnityEngine;

public class DraggingMgr : MonoLikeSingleton<DraggingMgr>
{
	public interface IDragable
	{
		void OnDragOut();

		bool OnDragging(Ray cameraRay);

		bool OnCheckPutDown();

		void OnPutDown();

		void OnCancel();

		void OnRotate();
	}

	public class EventArg : PeEvent.EventArg
	{
		public IDragable dragable;
	}

	private Event<EventArg> mEventor = new Event<EventArg>();

	private IDragable mDragable;

	private bool mPutDownEnable;

	private Vector3 mLastMousePos;

	private Vector3 mLastCameraPos;

	public Event<EventArg> eventor => mEventor;

	public IDragable Dragable => mDragable;

	private void Clear()
	{
		mDragable = null;
		mPutDownEnable = false;
	}

	public bool IsDragging()
	{
		return mDragable != null;
	}

	public bool Begin(IDragable dragable)
	{
		if (dragable == null)
		{
			return false;
		}
		mDragable = dragable;
		mDragable.OnDragOut();
		return true;
	}

	public bool End()
	{
		if (mDragable != null)
		{
			if (mDragable.OnCheckPutDown() && mPutDownEnable)
			{
				eventor.Dispatch(new EventArg
				{
					dragable = mDragable
				}, this);
				mDragable.OnPutDown();
				Clear();
				return true;
			}
			Cancel();
			return false;
		}
		Clear();
		return false;
	}

	public void Cancel()
	{
		if (mDragable != null)
		{
			mDragable.OnCancel();
		}
		Clear();
	}

	public void Rotate()
	{
		if (mDragable != null)
		{
			mDragable.OnRotate();
		}
	}

	public void UpdateRay()
	{
		if (mDragable != null && !mDragable.Equals(null) && (mLastMousePos != Input.mousePosition || Vector3.SqrMagnitude(mLastCameraPos - Camera.main.transform.position) > 1f))
		{
			mLastMousePos = Input.mousePosition;
			mLastCameraPos = Camera.main.transform.position;
			mPutDownEnable = mDragable.OnDragging(PeCamera.mouseRay);
		}
	}
}
