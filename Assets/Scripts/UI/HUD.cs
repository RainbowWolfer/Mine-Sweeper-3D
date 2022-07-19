using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MineSweeper3D.GameUI {
	public class HUD: MonoBehaviour {
		public RectTransform RectTransform { get; private set; }

		private bool activated;
		public bool Activated {
			get => activated;
			set {
				activated = value;
				this.gameObject.SetActive(value);
			}
		}


		private void Awake() {
			RectTransform = GetComponent<RectTransform>();
		}

	}
}
