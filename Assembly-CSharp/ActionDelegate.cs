using System;

internal class ActionDelegate
{
	private object _sender;

	private ActionEventHandler _actionEvent;

	private EventArgs _eventArgs;

	internal ActionDelegate(object sender, ActionEventHandler action, EventArgs args)
	{
		_sender = sender;
		_actionEvent = action;
		_eventArgs = args;
	}

	internal void OnAction()
	{
		if (_actionEvent != null)
		{
			_actionEvent(_sender, _eventArgs);
		}
	}
}
