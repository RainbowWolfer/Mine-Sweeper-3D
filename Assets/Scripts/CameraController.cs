using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MineSweeper3D {
	public class CameraController: MonoBehaviour {
		public static CameraController Instance;
		public Camera main;
		public PostProcessVolume volume;
		private ChromaticAberration chromaticAberration;

		public Ray MouseRay => main.ScreenPointToRay(Input.mousePosition);

		public Vector3 pos;
		public float yOffset = 0;

		private void Awake() {
			Instance = this;
			pos = new Vector3(0, 10, 2);
			volume.profile.TryGetSettings(out chromaticAberration);
		}

		private void Update() {
			transform.position = Vector3.Lerp(transform.position, pos + new Vector3(0, yOffset, 0), Time.deltaTime * 5);
			yOffset = Mathf.Clamp(yOffset, -pos.y / 2, 5);
		}

		public void InitializeCameraPosition() {
			pos = new Vector3(0, 10, 2);
			yOffset = 0;
		}
		public void CalculateCameraCenter(Transform[] boundsTargets, int x, int y) {
			Bounds bounds = new Bounds();
			foreach(Transform t in boundsTargets) {
				bounds.Encapsulate(t.position);
			}
			pos.x = bounds.center.x;
			pos.z = bounds.center.z;
			float d = Mathf.Sqrt((float)x * x + y * (float)y);
			pos.y = d * 0.7f;
		}
		public void ExplodeEffect() {
			StartCoroutine(ExplodeAsync());
		}
		private IEnumerator ExplodeAsync() {
			chromaticAberration.enabled.value = true;
			while(chromaticAberration.intensity < 0.95f) {
				chromaticAberration.intensity.value = Mathf.Lerp(
					chromaticAberration.intensity,
					1f,
					Time.deltaTime * 6
				);
				yield return null;
			}
			while(chromaticAberration.intensity > 0.1f) {
				chromaticAberration.intensity.value = Mathf.Lerp(
					chromaticAberration.intensity,
					0f,
					Time.deltaTime * 6
				);
				yield return null;
			}
			chromaticAberration.enabled.value = false;
			yield return null;
		}
	}
}