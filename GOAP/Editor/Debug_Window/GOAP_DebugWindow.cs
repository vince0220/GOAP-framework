using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using GOAP;

namespace GOAP.Debug{
	public class GOAP_DebugWindow : EditorWindow {
		// styles
		private GUIStyle nodeDefaultStyle;
		private GUIStyle nodeActiveStyle;
		private GUIStyle nodeActionStyle;
		private GUIStyle nodeActionStyleInPlan;

		private Vector2 offset;
		private Vector2 drag;
		private Vector2 totalDrag;

		private GOAP_Agent CurrentAgent;
		private List<GOAP_EditorNode> nodes = new List<GOAP_EditorNode>();

		[MenuItem("Window/Goap Debugger")]
		private static void OpenWindow()
		{
			GOAP_DebugWindow window = GetWindow<GOAP_DebugWindow>();
			window.titleContent = new GUIContent("Goap Debugger");			
		}

		private void OnEnable(){
			nodeDefaultStyle = new GUIStyle();
			nodeDefaultStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
			nodeDefaultStyle.border = new RectOffset(12, 12, 12, 12);
			nodeDefaultStyle.richText = true;
			nodeDefaultStyle.alignment = TextAnchor.MiddleCenter;
			nodeDefaultStyle.normal.textColor = Color.white;

			nodeActiveStyle = new GUIStyle();
			nodeActiveStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
			nodeActiveStyle.border = new RectOffset(12, 12, 12, 12);
			nodeActiveStyle.richText = true;
			nodeActiveStyle.alignment = TextAnchor.MiddleCenter;
			nodeActiveStyle.normal.textColor = Color.white;

			nodeActionStyle = new GUIStyle();
			nodeActionStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D;
			nodeActionStyle.border = new RectOffset(12, 12, 12, 12);
			nodeActionStyle.richText = true;
			nodeActionStyle.alignment = TextAnchor.MiddleCenter;
			nodeActionStyle.normal.textColor = Color.white;

			nodeActionStyleInPlan = new GUIStyle();
			nodeActionStyleInPlan.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
			nodeActionStyleInPlan.border = new RectOffset(12, 12, 12, 12);
			nodeActionStyleInPlan.richText = true;
			nodeActionStyleInPlan.alignment = TextAnchor.MiddleCenter;
			nodeActionStyleInPlan.normal.textColor = Color.white;
		}

		private void OnGUI(){
			if (Selection.activeGameObject != null) {
				CurrentAgent = Selection.activeGameObject.GetComponent<GOAP_Agent> ();
			}


			// draw grid
			DrawGrid (20, 0.2f, Color.gray);
			DrawGrid (100, 0.4f, Color.gray);
			ProcessEvents (Event.current); // drag event
			ProcessNodeEvents(Event.current);

			// draw nodes
			if (CurrentAgent == null) {
				Repaint ();
				return;
			}

			// draw nodes
			UpdateGOAPNodes (CurrentAgent);

			Repaint ();
		}

		private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
		{
			int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
			int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

			Handles.BeginGUI();
			Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

			offset += drag * 0.5f;
			Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

			for (int i = 0; i < widthDivs; i++)
				Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);

			for (int j = 0; j < heightDivs; j++)
				Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);

			Handles.color = Color.white;
			Handles.EndGUI();
		}

		public float Round(float value, int digits)
		{
			float mult = Mathf.Pow(10.0f, (float)digits);
			return Mathf.Round(value * mult) / mult;
		}

		private void UpdateGOAPNodes(GOAP_Agent agent){
			nodes.Clear ();

			#region Render goals
			var width = 300f;
			var height = 70f;

			var nodePosition = new Vector2 (0f,0f);
			var nodeYMiddle = new Vector2 (0f,height * 0.5f);
			var Green = "#8BC34A";
			var Red = "#F44336";


			var Goals = agent.InitialGoals;
			GOAP_EditorNode? previousNode = null;

			foreach (GOAP_Goal goal in Goals) {
				var curHeight = height;
				var text = string.Format("<b>GOAL: </b> <i>{0}</i>\n", goal);
				foreach (var keyValue in goal.Goal.Conditions)
				{
					text += string.Format("<b>'{0}'</b> = <i>'{1}'</i>\n", keyValue.Key, keyValue.Value.Value);
				}

				// is possible
				curHeight+= 13;

				text += string.Format("<b>Priority: {0}</b>\n",Round(goal.Priority(),2));


				text += string.Format("<color={0}><b>Possible</b>: {1}</color>",(goal.IsGoalPossible())?Green:Red,goal.IsGoalPossible());

				curHeight+= 13;

				var style = (goal == agent.CurrentGoal) ? nodeActiveStyle : nodeDefaultStyle;

				var newNode = DrawGenericNode (text,width,curHeight,style,ref nodePosition,false);


				if (previousNode != null) {
					
				}
				previousNode = newNode;
			}

			#endregion

			#region ACtions
			previousNode = null;
			nodePosition = new Vector2 (0f,nodePosition.y + height + 30);
			height = 90f;
			var maxHeight = height;

			var Actions = agent.Actions;

			foreach (var action in Actions) {
				var curHeight = height;
				var text = string.Format("<b>Action: </b> <i>{0}</i>\n", action);
				text += "<b>Preconditions:</b>\n";

				var Preconditions = action.PreConditions;

				var style = (agent.Plan != null && agent.ActionPlan.Contains(action))?nodeActionStyleInPlan:nodeActionStyle;
				if (agent.Plan != null && agent.Plan.Length > 0 && agent.Plan[0].Action == action) {
					style = nodeActiveStyle;
				}

				foreach (var pair in Preconditions.Conditions) {
					curHeight += 13;
					var color = Green;
					if (Preconditions.Conditions.ContainsKey(pair.Key))
					{
						color = Red;
					}
					text += string.Format("<color={2}>'<b>{0}</b>' = '<i>{1}</i>'</color>\n", pair.Key, pair.Value.Value, color);
				}

				text += "<b>Effects:</b>\n";

				foreach (var effectPair in action.Effects.Conditions) {
					curHeight += 13;
					text += string.Format("'<b>{0}</b>' = '<i>{1}</i>'\n", effectPair.Key, effectPair.Value.Value);
				}

				curHeight += 13;
				text += "<b>Cost: "+action.Cost+"</b>\n";

				curHeight += 13;

				var proceduralCheck = (agent.CurrentGoal != null)?action.CheckProceduralPrecondition(agent.CurrentGoal.Goal):false;
				var UsableCheck = (agent.CurrentGoal != null)?action.CheckProceduralUsablity(agent.CurrentGoal.Goal):false;
				var IsDone = (agent.CurrentGoal != null)?action.IsDone (agent.CurrentGoal.Goal):false;
				text += string.Format("<color={0}><b>Procedural Conditions</b>: {1}</color>\n", proceduralCheck ? Green : Red, proceduralCheck);
				text += string.Format("<color={0}><b>Usability</b>: {1}</color>\n", UsableCheck ? Green : Red, UsableCheck);
				text += string.Format("<color={0}><b>Is Done</b>: {1}</color>\n", IsDone ? Green : Red, IsDone);
				maxHeight = Mathf.Max(maxHeight, curHeight);
				var newNode = DrawGenericNode(text, width, curHeight, style, ref nodePosition);
				var nodeMiddleY = new Vector2(0f, curHeight * 0.5f);

				previousNode = newNode;
			}

			#endregion
			#region General info
			previousNode = null;
			nodePosition = new Vector2 (-300f,0);
			height = 50f;
			maxHeight = height;

			var globalText = "<b>General Info: </b>\n";
			globalText += string.Format("Current state: "+CurrentAgent.StateMachine.State+"\n");
			height += 13;

			globalText += string.Format("Memory: "+"\n");
			height += 13;

			foreach(KeyValuePair<string,GOAP_State.PriorityValue> item in CurrentAgent.Memory.State.Conditions){
				globalText += string.Format("{0} : {1}\n",item.Key,item.Value.Value);
				height += 13;
			}

			DrawGenericNode(globalText, width, height, nodeActionStyle, ref nodePosition);
			#endregion

			DrawNodes ();
		}
		private void ProcessEvents(Event e)
		{
			drag = Vector2.zero;

			switch (e.type)
			{
			case EventType.MouseDrag:
				if (e.button == 0)
				{
					OnDrag(e.delta);
				}
				break;
			}
		}
		private void ProcessNodeEvents(Event e)
		{
			if (nodes == null) return;
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				bool guiChanged = nodes[i].ProcessEvents(e);

				if (guiChanged)
				{
					GUI.changed = true;
				}
			}
		}
		private void OnDrag(Vector2 delta)
		{
			totalDrag += delta;
			drag = delta;

			GUI.changed = true;
		}

		GOAP_EditorNode DrawGenericNode(string title, float width, float height, GUIStyle style, ref Vector2 nodePosition, bool isSelected = false, GOAP_EditorNode.GOAP_NodeEvent onEvent = null)
		{
			var node = new GOAP_EditorNode(title, nodePosition + totalDrag, width, height, style, isSelected, onEvent);
			nodePosition += new Vector2(width, 0f);
			nodes.Add (node);
			return node;
		}

		private void DrawNodes(){
			foreach (GOAP_EditorNode node in nodes) {
				node.Draw ();
			}
		}
	}
}
