using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ExerWorld;

public class Toggle : MonoBehaviour {
	private const int numToggles = 12;
	private const int xRooms = 4;
	private const int yRooms = 3;
	private bool[,,] switches = new bool[numToggles, xRooms, yRooms];
	private bool[,] rooms = new bool[xRooms, yRooms];
	private List<Vector2Int> path;
	private bool[] toggles = new bool[numToggles];
	private int levelsCompleted = 0;
	private bool levelFinished = false;
	private float startTime;

	public GameObject[] roomSprites;
	public Color roomDark;
	public GameObject vampire;
	public GameObject lady;
	public GameObject score;
	public int playTime = 20;
	public bool isRandom = false;

	// Use this for initialization
	void Start () {
		init ();
	}

	IEnumerator startGame() {
		yield return new WaitForSeconds (1);
		init ();
		//SceneManager.LoadScene (0);
	}

	private float getSpecialToggle (int toggle) {
		int row = toggle / 4 + 1;
		int col = toggle % 4 + 1;
		string s = "window_row_" + row + "_col_" + col;
		float f = ControlInput.GetCurrentValue (s);
		return f;
	}

	private void init() {
		this.switches = new bool[numToggles, xRooms, yRooms];
		this.rooms = new bool[xRooms, yRooms];
		this.rooms [0, 0] = true;
		this.rooms [3, 2] = true;

		this.toggles = new bool[numToggles];
		foreach (GameObject roomSprite in roomSprites) {
			roomSprite.GetComponent<SpriteRenderer> ().color = Color.white;
		}

		GameObject go = this.lady;
		this.lady = this.vampire;
		this.vampire = go;
		this.vampire.transform.position = getAnimationPos (new Vector2Int(0, 0));
		this.lady.transform.position = getAnimationPos (new Vector2Int(3, 2));
		this.vampire.GetComponent<Animator> ().SetBool ("walk", false);
		this.vampire.GetComponent<Animator> ().SetBool ("stairs", false);
		this.lady.GetComponent<Animator> ().SetBool ("vampire", false);
		if (isRandom) {
			HashSet<Vector2Int> duplicates = new HashSet<Vector2Int> ();
			int tries = 0;
			do {
				duplicates.Clear();
				switches = new bool[numToggles, xRooms, yRooms];
				for (int i = 0; i < numToggles; i++) {
					Vector2Int r1 = randomSwitch (i);
					duplicates.Add (r1);
					Vector2Int r2 = randomSwitch (i);
					while (r2.Equals(r1)) {
						r2 = randomSwitch (i);
					}
					duplicates.Add (r2);
					//randomSwitch (i);
					//randomSwitch (i);
				}
				tries++;
				//print("Duplicates: " + duplicates.Count);
			} while(duplicates.Count > (numToggles * 2) / 4);
			print ("Tries: " + tries);
		} else {
			switches [0, 1, 0] = true;
			switches [0, 2, 0] = true;

			switches [1, 3, 0] = true;
			switches [1, 3, 1] = true;
		}
		for (int toggle = 0; toggle < numToggles; toggle++) {
			float f = getSpecialToggle (toggle);
			if (f == 0.0) {
				this.toggles [toggle] = false;
			} else if (f > 0.6) {
				this.toggles [toggle] = true;
			} else if (f < 0.4) {
				this.toggles [toggle] = false;
			}
		}
		this.path = null;
		this.levelFinished = false;
		//this.score.SetActive (false);
		this.score.GetComponent<Text>().text = "Level " + (this.levelsCompleted + 1);
	}
	private Vector2Int randomSwitch(int toggle) {
		Vector2Int result = new Vector2Int ();
		result.x = Random.Range (0, xRooms);
		result.y = Random.Range (0, yRooms);
		if ((result.x == 0 && result.y == 0) || (result.x == 3 && result.y == 2))
			return randomSwitch (toggle);
		else
			switches [toggle, result.x, result.y] = true;
		return result;
	}

	// Update is called once per frame
	void Update () {
		if (Time.realtimeSinceStartup - this.startTime > this.playTime) {//GAME OVER
			string msg = this.levelsCompleted + " levels finished in " + this.playTime + " seconds";
			if (this.levelsCompleted < 3) {
				msg += "\nThats pretty BAD!";
			} else if (this.levelsCompleted < 6) {
				msg += "\nThats quite OK!";
			} else {
				msg += "\nThats FANTASTIC!";
			}
			this.score.GetComponent<Text> ().text = msg;
			this.score.SetActive (true);
			if (Input.anyKey) {
				init ();
				this.levelsCompleted = 0;
				this.score.GetComponent<Text>().text = "Level " + (this.levelsCompleted + 1);
				this.startTime = Time.realtimeSinceStartup;
			}
		} else if (path == null) {// no path found yet
			bool updated = checkToggles ();
			if (updated) {
				path = checkPath (0, 0, new bool[Toggle.xRooms, Toggle.yRooms], new List<Vector2Int>());
			}
		} else if (path.Count == 0) {//reached GOAL
			AudioSource audio = GetComponent<AudioSource>();
			audio.Play();
			this.vampire.GetComponent<Animator> ().SetBool ("walk", false);
			this.vampire.GetComponent<Animator> ().SetBool ("stairs", false);
			this.lady.GetComponent<Animator> ().SetBool ("vampire", true);
			if (! this.levelFinished) {
				this.levelsCompleted++;
				StartCoroutine (startGame());
				this.levelFinished = true;
			}
		} else {//walking towards goal
			this.vampire.GetComponent<Animator> ().SetBool ("walk", true);
			float speed = 6 * Time.deltaTime;
			Vector3 nextPos = getAnimationPos (this.path[0]);
			this.vampire.transform.position = Vector3.MoveTowards (this.vampire.transform.position, nextPos, speed);
			if (this.vampire.transform.position.Equals (nextPos)) {
				if (this.path.Count > 1) {
					int deltaY = this.path [0].y - this.path [1].y;
					this.vampire.GetComponent<Animator> ().SetBool ("stairs", deltaY != 0);
				} else {
					this.vampire.GetComponent<Animator> ().SetBool ("walk", false);
				}
				if (this.path.Count > 0) {
					this.path.RemoveAt (0);
				}
			}
		}
	}
	Vector3 getAnimationPos(Vector2Int p) {
		return roomSprites [p.y * xRooms + p.x].transform.position;
	}
	bool checkToggles () {
		bool updated = false;
		for (int toggle = 0; toggle < numToggles; toggle++) {
			bool b = Input.GetButtonDown ("Toggle" + toggle);//F1-F12
			if (!b) {//our special controller
				float f = getSpecialToggle (toggle);
				if (f == 0.0) {
					this.toggles [toggle] = false;
				} else if (f > 0.6) {
					b = this.toggles [toggle] ^ true;
					this.toggles [toggle] = true;
				} else if (f < 0.4) {
					b = this.toggles [toggle] ^ false;
					this.toggles [toggle] = false;
				}
			}
			if (b) {
				updated = true;
				for (int xRoom = 0; xRoom < Toggle.xRooms; xRoom++) {
					for (int yRoom = 0; yRoom < Toggle.yRooms; yRoom++) {
						if (switches [toggle, xRoom, yRoom]) {
							rooms [xRoom, yRoom] = rooms [xRoom, yRoom] ^ true;
						}
					}
				}
			}
		}
		// update room sprites
		for (int xRoom = 0; xRoom < Toggle.xRooms; xRoom++) {
			for (int yRoom = 0; yRoom < Toggle.yRooms; yRoom++) {
				if (rooms [xRoom, yRoom]) {
					roomSprites [yRoom * Toggle.xRooms + xRoom].GetComponent<SpriteRenderer> ().color = this.roomDark;
				}
				else {
					roomSprites [yRoom * Toggle.xRooms + xRoom].GetComponent<SpriteRenderer> ().color = Color.white;
				}
			}
		}
		return updated;
	}

	Vector2Int[] getNeighbors(int x, int y) {
		return new Vector2Int[] {new Vector2Int(x + 1, y), new Vector2Int(x, y + 1), new Vector2Int(x - 1, y), new Vector2Int(x, y - 1)};
	}
	List<Vector2Int> checkPath(int xRoom, int yRoom, bool[,] visited, List<Vector2Int> path) {
		visited [xRoom, yRoom] = true;
		if (xRoom == 3 && yRoom == 2) {//GOAL
			return path;
		}
		if (!this.rooms[xRoom, yRoom]) {
			return null;
		}
		Vector2Int[] n = getNeighbors (xRoom, yRoom);
		for (int i = 0; i < n.GetLength(0); i++) {
			if (n [i].x >= 0 && n [i].x < Toggle.xRooms && n[i].y >= 0 && n[i].y < Toggle.yRooms && !visited [n [i].x, n[i].y]) {
				List<Vector2Int> np = new List<Vector2Int> (path.ToArray());
				np.Add (new Vector2Int(n[i].x, n[i].y));
				List<Vector2Int> p2 = checkPath (n [i].x, n [i].y, visited, np);
				if (p2 != null) {
					return p2;
				}
			}
		}
		return null;
	}
}
