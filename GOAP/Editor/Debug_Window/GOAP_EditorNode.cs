using System;
using UnityEditor;
using UnityEngine;

namespace GOAP.Debug{
	public struct GOAP_EditorNode
	{
		public delegate void GOAP_NodeEvent(GOAP_EditorNode node, Event e);
		public GOAP_NodeEvent OnEvent;

		public Rect Rect;
		public string Title;
		public bool IsSelected;

		public GUIStyle Style;
		public GUIStyle DefaultNodeStyle;

		public GOAP_EditorNode(string title, Vector2 position, float width, float height, GUIStyle nodeStyle, bool isSelected = false, GOAP_NodeEvent onEvent = null)
		{
			Rect = new Rect(position.x, position.y, width, height);
			Style = nodeStyle;
			DefaultNodeStyle = nodeStyle;

			Title = title;
			IsSelected = isSelected;
			OnEvent = onEvent;
		}

		public void Drag(Vector2 delta)
		{
			Rect.position += delta;
		}

		public void Draw()
		{
			GUI.Box(Rect, Title, Style);
		}

		public bool ProcessEvents(Event e)
		{
			if (OnEvent != null)
			{
				switch (e.type)
				{
				case EventType.mouseDown:
					if (e.button == 0)
					{
						if (e.isMouse && e.button == 0 && this.Rect.Contains (e.mousePosition)) {
							OnEvent (this, e);
						}
					}
					break;
				}
			
				//OnEvent(this, e);
			}
			return false;
		}
	}
}