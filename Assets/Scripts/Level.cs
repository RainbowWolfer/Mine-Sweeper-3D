﻿using MineSweeper3D.Grid;
using MineSweeper3D.GameUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

/// <summary>
/// score = mines swept time 10 - passed seconds
/// 
/// </summary>
namespace MineSweeper3D {
	public class Level: MonoBehaviour {
		public static Level Instance;
		[Range(90, 99)]
		public int distributionPercentage = 95;
		public int seconds;
		public string currentMode;

		public GameObject grid;

		public Transform parent;
		public bool isGameStarted = false;
		public bool ableToClick = false;

		public List<MyGrid> allGrids;

		public GameObject mousePointer;

		public MyGrid[] Mines => allGrids.Where((g) => g.isMine).ToArray();
		public MyGrid[] FlagedMines => Mines.Where((g) => g.isMine && g.state == GridState.Flag).ToArray();
		public MyGrid[] Flaged => allGrids.Where((g) => g.state == GridState.Flag).ToArray();

		public MyGrid FindGrid(int x, int y) {
			foreach(MyGrid g in allGrids) {
				if(g.location == new Vector2Int(x, y)) {
					return g;
				}
			}
			return null;
		}

		private void Awake() {
			Instance = this;
			allGrids = new List<MyGrid>();
		}

		private void Start() {
			StartCoroutine(TimeCountingAsync());
		}

		private void Update() {
			if(isGameStarted) {
				CameraController.Instance.yOffset += Input.GetAxis("Mouse ScrollWheel") * 3;
				UI.Instance.mineText.text = (Mines.Length - Flaged.Length).ToString();
				UI.Instance.timeText.text = "Time: " + TransTimeSecondIntToString(seconds);
				if(ableToClick) {
					for(int i = 0; i < 3; i++) {
						if(Input.GetMouseButtonUp(i)) {
							Click(i);
						}
					}
				}
				bool findAll = true;
				foreach(MyGrid g in Mines) {
					if(g.state != GridState.Flag) {
						findAll = false;
					}
				}
				if(findAll) {
					StartCoroutine(GameOverAsync(null, true));
				}

				RaycastHit hit;
				if(Physics.Raycast(CameraController.Instance.MouseRay, out hit) && hit.collider != null && hit.collider.GetComponent<MyGrid>() != null) {
					mousePointer.SetActive(true);
					MyGrid g = hit.collider.GetComponent<MyGrid>();
					int x = g.location.x;
					int y = g.location.y;
					mousePointer.transform.position = new Vector3(x, 0, y);
				} else {
					mousePointer.SetActive(false);
				}
			} else {
				mousePointer.SetActive(false);
			}
		}

		private IEnumerator ClickDelayAsync(float seconds) {
			yield return new WaitForSeconds(seconds);
			ableToClick = true;
		}

		private IEnumerator TimeCountingAsync() {
			while(true) {
				yield return new WaitForSeconds(1);
				if(isGameStarted) {
					seconds++;
				}
			}
		}

		public void StartGame(int x, int y, int mineAmount, string mode) {
			UI.Instance.aboutPanel.gameObject.SetActive(false);
			isGameStarted = true;
			currentMode = mode;
			StartCoroutine(ClickDelayAsync(1));
			UI.Instance.entryHUD.Activated = false;
			UI.Instance.inGame.Activated = true;
			seconds = 0;
			GenerateGrids(x, y, mineAmount);
			CameraController.Instance.InitializeCameraPosition();
			CameraController.Instance.CalculateCameraCenter(new Transform[] {
				FindGrid(0, 0).transform,
				FindGrid(x - 1, 0).transform,
				FindGrid(0, y - 1).transform,
				FindGrid(x - 1, y - 1).transform, },
			x, y);
		}

		public void BackToMenu() {
			isGameStarted = false;
			ableToClick = false;

			UI.Instance.outro.Activated = false;
			UI.Instance.inGame.Activated = false;
			UI.Instance.entryHUD.Activated = true;

			ClearGrids();
			CameraController.Instance.InitializeCameraPosition();
		}

		public IEnumerator GameOverAsync(MyGrid mine, bool win) {
			isGameStarted = false;
			ableToClick = false;
			int f = FlagedMines.Length;
			allGrids.ForEach((g) => g.state = GridState.Discovered);
			if(!win) {
				mine.state = GridState.Discovered;
				//mine.img.color = Color.red;
				mine.showRed = true;
				yield return new WaitForSeconds(1);
				Explode(mine);
				yield return new WaitForSeconds(3);
			} else {
				yield return new WaitForSeconds(2);
			}
			UI.Instance.entryHUD.Activated = false;
			UI.Instance.inGame.Activated = false;
			UI.Instance.outro.Activated = true;
			UI.Instance.SetScore(win ? Mines.Length : f, Mines.Length, seconds, currentMode);
			UI.Instance.GameOver(win);
		}

		public void Explode(MyGrid grid) {
			List<Rigidbody> rbs = new List<Rigidbody>();
			foreach(MyGrid g in allGrids) {
				Rigidbody rb = g.gameObject.AddComponent<Rigidbody>();
				rb.mass = 10;
				rb.drag = 1;
				rb.angularDrag = 1;
				rbs.Add(rb);
			}
			foreach(Rigidbody rb in rbs) {
				rb.AddExplosionForce(100, grid.transform.position, 20, 1, ForceMode.Impulse);
			}
			CameraController.Instance.ExplodeEffect();
		}

		public void Click(int mouse) {
			RaycastHit hit;
			if(Physics.Raycast(CameraController.Instance.MouseRay, out hit)) {
				if(hit.collider != null && hit.collider.GetComponent<MyGrid>() != null) {
					MyGrid g = hit.collider.GetComponent<MyGrid>();
					//Debug.Log(g + "is clicked");
					if(mouse == 0) {//sweep
						if(g.state == GridState.Undiscovered) {
							if(g.isMine) {
								g.state = GridState.Discovered;
								StartCoroutine(GameOverAsync(g, false));
								return;
							} else {
								g.state = GridState.Discovered;
								RevealSurroundings(g);
							}
						}
					} else if(mouse == 1) {//flag
						if(g.state != GridState.Discovered) {
							g.state = g.state == GridState.Flag ? GridState.Undiscovered : GridState.Flag;
						}
					} else if(mouse == 2) {
						if(g.state == GridState.Discovered) {
							int mine = g.mineSuround;
							int flag = 0;
							foreach(Vector2Int vec in GetAllDirections(g.location)) {
								MyGrid g2 = FindGrid(vec.x, vec.y);
								if(g2 != null && g2.state == GridState.Flag) {
									flag++;
								}
							}
							if(flag >= mine) {
								foreach(Vector2Int vec in GetAllDirections(g.location)) {
									MyGrid g2 = FindGrid(vec.x, vec.y);
									//if(g2 != null && g2.state != GridState.Flag) {
									//	g2.state = GridState.Discovered;
									//	if(g2.isMine) {
									//		StartCoroutine(GameOverAsync(g2, false));
									//		return;
									//	}
									//}
									if(g2 != null && g2.state != GridState.Flag) {
										if(g2.state == GridState.Undiscovered) {
											if(g2.isMine) {
												g2.state = GridState.Discovered;
												StartCoroutine(GameOverAsync(g2, false));
												return;
											} else {
												g2.state = GridState.Discovered;
												RevealSurroundings(g2);
											}
										}
									}
								}
							}
						}
					} else {
						throw new Exception("???");
					}
				}
			}
		}

		public void RevealSurroundings(MyGrid g) {
			foreach(Vector2Int vec in GetAllDirections(g.location)) {
				MyGrid found = FindGrid(vec.x, vec.y);
				if(found != null && !found.isMine && found.mineSuround == 0 && found.state == GridState.Undiscovered) {
					found.state = GridState.Discovered;
					RevealSurroundings(found);
					foreach(Vector2Int vec2 in GetAllDirections(found.location)) {
						MyGrid found2 = FindGrid(vec2.x, vec2.y);
						if(found2 != null && !found2.isMine && found2.mineSuround != 0 && found2.state == GridState.Undiscovered) {
							found2.state = GridState.Discovered;
						}

					}
				}

			}
		}
		public Vector2Int[] GetAllDirections(Vector2Int vec) {
			return new Vector2Int[]{
				vec,
				vec + new Vector2Int(1,0),
				vec + new Vector2Int(-1,0),
				vec + new Vector2Int(0,1),
				vec + new Vector2Int(0,-1),
				vec + new Vector2Int(1,-1),
				vec + new Vector2Int(-1,1),
				vec + new Vector2Int(-1,-1),
				vec + new Vector2Int(1,1),
			};
		}
		public void ClearGrids() {
			foreach(MyGrid g in allGrids) {
				Destroy(g.gameObject);
			}
			allGrids = new List<MyGrid>();
		}
		public void GenerateGrids(int x, int y, int mineAmount) {
			int currentAmount = 0;
			while(currentAmount < mineAmount) {
				for(int i = 0; i < x; i++) {
					for(int j = 0; j < y; j++) {
						MyGrid g = null;
						if(FindGrid(i, j) == null) {
							g = Instantiate(grid, parent).GetComponent<MyGrid>();
							allGrids.Add(g);
						} else {
							g = FindGrid(i, j);
						}
						g.transform.localPosition = new Vector3(i, 0, j);
						g.transform.rotation = Quaternion.identity;
						g.location = new Vector2Int(i, j);
						g.state = GridState.Undiscovered;
						int r = UnityEngine.Random.Range(0, 100);
						if(r > distributionPercentage && !g.isMine && currentAmount < mineAmount) {
							g.isMine = true;
							currentAmount++;
						}
					}
				}
			}
			//find surround mines
			foreach(MyGrid g in allGrids.Where((g) => !g.isMine)) {
				int amount = 0;
				foreach(Vector2Int vec in GetAllDirections(g.location)) {
					MyGrid found = FindGrid(vec.x, vec.y);
					if(found != null && found.isMine) {
						amount++;
					}
				}
				g.mineSuround = amount;
			}

		}
		public static string TransTimeSecondIntToString(long second) {
			string str = "";
			try {
				//long hour = second / 3600;
				long min = second % 3600 / 60;
				long sec = second % 60;
				//if(hour < 10) {
				//	str += "0" + hour.ToString();
				//} else {
				//	str += hour.ToString();
				//}
				//str += ":";
				if(min < 10) {
					str += "0" + min.ToString();
				} else {
					str += min.ToString();
				}
				str += ":";
				if(sec < 10) {
					str += "0" + sec.ToString();
				} else {
					str += sec.ToString();
				}
			} catch(Exception ex) {
				Debug.LogWarning("Catch:" + ex.Message);
			}
			return str;
		}

		public static void WriteLastTypeAndTime(string type, string time) {
			string path = GetFileURL();
			WriteFile(path, type + "\n" + time);
		}

		public static void GetLastTypeAndTime(out string type, out string time) {
			type = "???";
			time = "???";
			string path = GetFileURL();
			string[] content = ReadFile(path);
			if(content == null || content.Length != 2) {
				return;
			}
			type = content[0];
			time = content[1];
		}

		public static string[] ReadFile(string path) {
			string[] strs = File.ReadAllLines(path);
			return strs;
		}

		public static void WriteFile(string path, string content) {
			File.WriteAllText(path, content);
		}

		public static string GetFileURL() {
			string DPath = Application.dataPath;
			int num = DPath.LastIndexOf("/");
			DPath = DPath[..num];
			string url = DPath + "/ScoresRecord.txt";
			return url;
		}
	}
}
