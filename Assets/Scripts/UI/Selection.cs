using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MineSweeper3D.GameUI {
	[ExecuteInEditMode]
	public class Selection: MonoBehaviour {
		public string content;
		public Text text;
		public Text number;
		public Slider slider;
		public int min = 9;
		public int max = 32;
		public int Value => (int)slider.value;

		private void Awake() {

		}

		private void Update() {
			text.text = content;
			slider.value = (int)slider.value;
			slider.minValue = min;
			slider.maxValue = max;
			number.text = Value.ToString();
		}
	}
}
