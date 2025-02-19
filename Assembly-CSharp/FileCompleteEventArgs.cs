using System;

public class FileCompleteEventArgs : EventArgs
{
	private bool _success;

	public bool Success => _success;

	public FileCompleteEventArgs(bool success)
	{
		_success = success;
	}
}
