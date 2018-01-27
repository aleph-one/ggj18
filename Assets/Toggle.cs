using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExerWorld;

public class Toggle : MonoBehaviour {
	private const int numToggles = 12;
	private const int xRooms = 4;
	private const int yRooms = 3;
	private bool[,,] switches = new bool[numToggles, xRooms, yRooms];
	private bool[,] rooms = new bool[xRooms, yRooms];
	private List<Vector2Int> path;
	private bool[] toggles = new bool[numToggles];
	public GameObject[] roomSprites;
	public Color roomDark;
	public GameObject vampire;

	// Use this for initialization
	void Start () {
		switches [0, 1, 0] = true;
		switches [0, 2, 1] = true;

		switches [1, 2, 0] = true;
		switches [1, 2, 1] = true;

		switches [2, 1, 0] = true;
		switches [2, 3, 0] = true;

		switches [3, 3, 1] = true;
		switches [3, 2, 1] = true;

		switches [4, 2, 2] = true;
		switches [4, 3, 1] = true;

		rooms [0, 0] = true;
		rooms [3, 2] = true;


	}

	// Update is called once per frame
	void Update () {
		if (Time.realtimeSinceStartup < 2)
			return;
		if (path == null) {
			bool updated = checkToggles ();
			if (updated) {
				path = check (0, 0, new bool[Toggle.xRooms, Toggle.yRooms], new List<Vector2Int>());
			}
		} else if (path.Count == 0) {
			//reached GOAL
			this.vampire.GetComponent<Animator> ().SetBool ("walk", false);
			this.vampire.GetComponent<Animator> ().SetBool ("stairs", false);
		} else {
			this.vampire.GetComponent<Animator> ().SetBool ("walk", true);
			float speed = 3 * Time.deltaTime;
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
			bool b = Input.GetButtonDown ("Toggle" + toggle);
			int row = toggle / 4 + 1;
			int col = toggle % 4 + 1;
			string s = "window_row_" + row + "_col_" + col;
			float f = ControlInput.GetCurrentValue (s);
			//bool b = f > 0.5;
			if (f == 0.0) {
				this.toggles [toggle] = false;
			} else if (f > 0.7) {
				//b = true;
				b = this.toggles [toggle] ^ true;
				this.toggles [toggle] = true;
			} else if (f < 0.3) {
				//b = true;
				b = this.toggles [toggle] ^ false;
				this.toggles [toggle] = false;
			}
			//this.toggles [toggle] = b;
			if (b) {
				//print ("Toggle: " + toggle);
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
		return new Vector2Int[] {new Vector2Int(x - 1, y), new Vector2Int(x, y - 1), new Vector2Int(x + 1, y), new Vector2Int(x, y + 1)};
	}
	List<Vector2Int> check(int xRoom, int yRoom, bool[,] visited, List<Vector2Int> path) {
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
				List<Vector2Int> p2 = check (n [i].x, n [i].y, visited, np);
				if (p2 != null) {
					return p2;
				}
			}
		}
		return null;
	}
}
