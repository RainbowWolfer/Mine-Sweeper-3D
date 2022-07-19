using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MineSweeper3D.GameUI {
	public class FpsCounter: MonoBehaviour {

		[SerializeField]
		private TextMeshProUGUI text;

		private void OnEnable() {
			StartCoroutine(LoopAsync());
		}

		private IEnumerator LoopAsync(){
			while(gameObject.activeSelf){
				yield return new WaitForSeconds(0.1f);
				int fps = (int)(1 / Time.deltaTime);
				text.text = $"{fps}";
			}
		}
	}
}
