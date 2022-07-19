using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MineSweeper3D.GameUI {
	[ExecuteInEditMode]
	public class CustomPanel: MonoBehaviour {
		public Selection row;
		public Selection column;
		public Selection mine;

		private void Update() {
			mine.min = 10;
			int max = row.Value * column.Value - 1;
			mine.max = max;
			if(mine.max >= max) {
				mine.max = max;
			}
		}
	}
}
