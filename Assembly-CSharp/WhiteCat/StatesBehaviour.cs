namespace WhiteCat;

public class StatesBehaviour : BaseBehaviour
{
	private State _state;

	private float _stateTime;

	public State state
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != null && _state.onExit != null)
			{
				_state.onExit();
			}
			_stateTime = 0f;
			_state = value;
			if (_state != null && _state.onEnter != null)
			{
				_state.onEnter();
			}
		}
	}

	public float stateTime => _stateTime;

	protected void UpdateState(float deltaTime)
	{
		_stateTime += deltaTime;
		if (_state != null && _state.onUpdate != null)
		{
			_state.onUpdate(deltaTime);
		}
	}
}
