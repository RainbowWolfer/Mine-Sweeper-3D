using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MineSweeper3D.Editors {
	public class TestEdior: EditorWindow {
		[MenuItem("Window/Editor/TestEdior")]
		public static void ShowWindow() {
			GetWindow<TestEdior>("TestEdior");
		}

		public string font;

		private void OnGUI() {
			if(GUILayout.Button("Fix")) {
				if(Selection.activeGameObject == null) {
					return;
				}
				var selectedText = Selection.activeGameObject.GetComponent<Text>();
				var font = selectedText.font;
				
				var objs = Resources.FindObjectsOfTypeAll(typeof(Text));
				foreach(Text item in objs) {
					item.font = font;
				}
			}
		}

		private void Update() {
			Repaint();
		}
	}
}