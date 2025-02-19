using System.Collections.Generic;
using UnityEngine;

public class UITreeGrid : MonoBehaviour
{
	[SerializeField]
	private bool m_RepositionNow;

	public bool m_DrawLine;

	public GameObject m_HorzLine;

	public GameObject m_VertLine;

	public float m_IconSize = 32f;

	private List<GameObject> m_Lines = new List<GameObject>();

	public Transform m_Content;

	public float m_LineHeight = 60f;

	public float m_GridWidth = 80f;

	public List<UITreeGrid> m_Children = new List<UITreeGrid>();

	private int m_Expand;

	public float TotalWidth => (float)m_Expand * m_GridWidth;

	public Vector3 ContentPosition => base.transform.localPosition + m_Content.localPosition;

	private void Update()
	{
		if (m_RepositionNow)
		{
			m_RepositionNow = false;
			Reposition();
		}
	}

	public void Reposition()
	{
		foreach (UITreeGrid child in m_Children)
		{
			child.Reposition();
		}
		m_Expand = 0;
		foreach (UITreeGrid child2 in m_Children)
		{
			m_Expand += child2.m_Expand;
		}
		m_Expand = Mathf.Max(m_Expand, 1);
		int num = 0;
		foreach (UITreeGrid child3 in m_Children)
		{
			Vector3 localPosition = child3.transform.localPosition;
			localPosition.x = (float)num * m_GridWidth;
			child3.transform.localPosition = localPosition;
			num += child3.m_Expand;
		}
		if (m_Content != null)
		{
			Vector3 zero = Vector3.zero;
			int num2 = m_Children.Count - 1;
			if (num2 >= 0)
			{
				zero.x = (m_Children[0].ContentPosition.x + m_Children[num2].ContentPosition.x) * 0.5f;
			}
			m_Content.localPosition = zero;
		}
		Vector3 localPosition2 = base.transform.localPosition;
		localPosition2.y = 0f - m_LineHeight;
		base.transform.localPosition = localPosition2;
		if (m_DrawLine)
		{
			CreateLine();
		}
	}

	private void ClearLines()
	{
		foreach (GameObject line in m_Lines)
		{
			Object.Destroy(line);
		}
		m_Lines.Clear();
	}

	private void DrawUILine(bool horz, Vector3 begin, Vector3 end)
	{
		GameObject gameObject = ((!horz) ? m_VertLine : m_HorzLine);
		if (gameObject != null)
		{
			GameObject gameObject2 = Object.Instantiate(gameObject);
			gameObject2.transform.parent = base.transform;
			gameObject2.transform.localPosition = begin;
			gameObject2.transform.localRotation = Quaternion.identity;
			Vector3 localScale = end - begin;
			if (horz)
			{
				localScale.y = 2f;
			}
			else
			{
				localScale.x = 2f;
			}
			localScale.z = 1f;
			gameObject2.transform.localScale = localScale;
			m_Lines.Add(gameObject2);
		}
	}

	private void CreateLine()
	{
		ClearLines();
		int num = m_Children.Count - 1;
		if (num < 0)
		{
			return;
		}
		DrawUILine(horz: false, m_Content.localPosition + Vector3.down * m_LineHeight * 0.5f, m_Content.localPosition + Vector3.down * (m_IconSize * 0.5f + 1f));
		DrawUILine(horz: true, m_Children[0].ContentPosition + Vector3.up * m_LineHeight * 0.5f, m_Children[num].ContentPosition + Vector3.up * m_LineHeight * 0.5f);
		foreach (UITreeGrid child in m_Children)
		{
			DrawUILine(horz: false, child.ContentPosition + Vector3.up * m_IconSize * 0.5f, child.ContentPosition + Vector3.up * (m_LineHeight * 0.5f + 1f));
		}
	}
}
