using System.IO;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("PLAY SPEECH")]
public class PlaySpeechAction : Action
{
	private OBJECT obj;

	private string text;

	private float time;

	private float _curTime;

	private bool _closeUIWnd;

	private bool _started;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		text = Utility.ToText(base.missionVars, base.parameters["text"]);
		time = Utility.ToSingle(base.missionVars, base.parameters["time"]);
	}

	public override bool Logic()
	{
		if (GameUI.Instance != null && GameUI.Instance.mNPCSpeech != null)
		{
			if (!_started)
			{
				_started = true;
				if (GameUI.Instance.mNPCSpeech.speechInterpreter.SetObject(obj))
				{
					GameUI.Instance.mNPCSpeech.speechInterpreter.SetSpeechContent(text);
					GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick += OnSpeechUIClick;
					GameUI.Instance.mNPCSpeech.Show();
					if (GameUI.Instance.mNpcDialog.isShow)
					{
						GameUI.Instance.mNpcDialog.Hide();
					}
					GameUI.Instance.mNpcDialog.allowShow = false;
					return false;
				}
				Debug.LogWarning("Get Object scenario id[" + obj.Id + "] error");
				GameUI.Instance.mNpcDialog.allowShow = true;
				return true;
			}
			if (time > 0.0001f || time < -0.0001f)
			{
				_curTime += Time.deltaTime;
				if (_closeUIWnd || _curTime > time)
				{
					GameUI.Instance.mNPCSpeech.Hide();
					GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick -= OnSpeechUIClick;
					GameUI.Instance.mNpcDialog.allowShow = true;
					return true;
				}
				return false;
			}
			if (_closeUIWnd)
			{
				GameUI.Instance.mNPCSpeech.Hide();
				GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick -= OnSpeechUIClick;
				GameUI.Instance.mNpcDialog.allowShow = true;
				return true;
			}
			return false;
		}
		return true;
	}

	public override void RestoreState(BinaryReader r)
	{
		_curTime = r.ReadSingle();
	}

	public override void StoreState(BinaryWriter w)
	{
		w.Write(_curTime);
	}

	private void OnSpeechUIClick()
	{
		_closeUIWnd = true;
		GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick -= OnSpeechUIClick;
	}
}
