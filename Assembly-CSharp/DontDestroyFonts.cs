using System.Collections.Generic;
using UnityEngine;

public class DontDestroyFonts : MonoBehaviour
{
	public List<Font> m_DontDestroyFonts;

	private void Awake()
	{
		foreach (Font dontDestroyFont in m_DontDestroyFonts)
		{
			Object.DontDestroyOnLoad(dontDestroyFont);
		}
	}
}
