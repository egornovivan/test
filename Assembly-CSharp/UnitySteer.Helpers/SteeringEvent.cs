namespace UnitySteer.Helpers;

public class SteeringEvent<T>
{
	private Steering _sender;

	private string _action;

	private T _parameter;

	public string Action
	{
		get
		{
			return _action;
		}
		set
		{
			_action = value;
		}
	}

	public T Parameter
	{
		get
		{
			return _parameter;
		}
		set
		{
			_parameter = value;
		}
	}

	public Steering Sender
	{
		get
		{
			return _sender;
		}
		set
		{
			_sender = value;
		}
	}

	public SteeringEvent(Steering sender, string action, T parameter)
	{
		_sender = sender;
		_action = action;
		_parameter = parameter;
	}
}
