using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MineSweeper3D.GameUI {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Canvas))]
	public class UI: MonoBehaviour {
		public static UI Instance { get; private set; }
		public Animator anim;
		public FpsCounter fpsCounter;

		[Header("Canvas")]
		public Canvas canvas;

		[Header("HUDs")]
		public HUD entryHUD;
		public HUD inGame;
		public HUD outro;

		[Header("Buttons")]
		public ModeButton easy;
		public ModeButton normal;
		public ModeButton hard;
		public ModeButton custom;
		public ModeButton back;
		public ModeButton start;
		public ModeButton backToRestart;

		[Header("Side")]
		public SideTriangleButton quit;
		public SideTriangleButton credits;
		public SideTriangleButton sideBackButton;

		[Header("About")]
		public RectTransform aboutPanel;
		public ModeButton fpsButton;
		public ModeButton githubButton;

		[Header("Entry")]
		public CustomPanel customPanel;

		[Header("In Game")]
		public Text timeText;
		public Text mineText;

		[Header("Outro")]
		public Text title;
		public Text modeText;
		public Text minesSweptText;
		public Text timePassedText;
		public Text bestMines;
		public Text bestTime;
		private bool enableFPS;

		private bool EnableFPS {
			get => enableFPS;
			set {
				enableFPS = value;
				fpsCounter.gameObject.SetActive(enableFPS);
			}
		}

		private void Awake() {
			Instance = this;
			entryHUD.Activated = true;
			inGame.Activated = false;
			outro.Activated = false;
			easy.action = () => {
				Level.Instance.StartGame(9, 9, 10, "Easy");
			};
			normal.action = () => {
				Level.Instance.StartGame(16, 16, 40, "Normal");
			};
			hard.action = () => {
				Level.Instance.StartGame(30, 16, 99, "Hard");
			};
			custom.action = () => {
				OpenRightGroup();
			};
			back.action = () => {
				OpenLeftGroup();
			};
			start.action = () => {
				Level.Instance.StartGame(
					customPanel.row.Value,
					customPanel.column.Value,
					customPanel.mine.Value,
					customPanel.row.Value + "x" + customPanel.column.Value + " : " + customPanel.mine.Value);
			};
			backToRestart.action = () => {
				outro.Activated = false;
				inGame.Activated = false;
				entryHUD.Activated = true;
				Level.Instance.ClearGrids();
				CameraController.Instance.InitializeCameraPosition();
			};
			quit.action = () => {
				Application.Quit();
			};
			sideBackButton.action = () => {
				if(!Level.Instance.isGameStarted) {
					return;
				}
				Level.Instance.BackToMenu();
			};
			credits.action = () => {
				aboutPanel.gameObject.SetActive(!aboutPanel.gameObject.activeSelf);
			};
			fpsButton.action = () => {
				EnableFPS = !EnableFPS;
			};
			githubButton.action = () => {
				Application.OpenURL("https://github.com/RainbowWolfer/Mine-Sweeper-3D");
			};
		}

		private void Start() {

		}

		private void Update() {
			if(Level.Instance.isGameStarted) {
				quit.gameObject.SetActive(false);
				sideBackButton.gameObject.SetActive(true);
			} else {
				quit.gameObject.SetActive(true);
				sideBackButton.gameObject.SetActive(false);
			}
		}

		public void OpenLeftGroup() {
			anim.SetBool("Right", false);
		}
		public void OpenRightGroup() {
			anim.SetBool("Right", true);
		}
		public void SetScore(int mines, int allMines, int seconds, string mode) {
			minesSweptText.text = mines + "/" + allMines;
			timePassedText.text = Level.TransTimeSecondIntToString(seconds);
			string type = mode + " - " + mines + "/" + allMines;
			string time = timePassedText.text;
			try {
				Level.GetLastTypeAndTime(out string lastType, out string lastTime);
				bestMines.text = lastType;
				bestTime.text = lastTime;
			} catch(Exception) {
				bestMines.text = "???";
				bestTime.text = "???";
			}
			try {
				Level.WriteLastTypeAndTime(type, time);
			} catch(Exception) { }
			modeText.text = mode;
		}
		public void GameOver(bool win) {
			if(win) {
				title.text = "Congratulations";
				title.color = Color.red;
			} else {
				title.text = "Game Over";
				title.color = Color.black;
			}
		}
	}
}
