using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggle : MonoBehaviour {
	private const int numToggles = 12;
	private const int xRooms = 4;
	private const int yRooms = 3;
	private bool[,,] switches = new bool[numToggles, xRooms, yRooms];
	private bool[,] rooms = new bool[xRooms, yRooms];
	private List<Vector3> path;
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
		switches [2, 3, 1] = true;

		switches [3, 2, 2] = true;
		switches [3, 1, 2] = true;
		switches [3, 3, 0] = true;

		switches [4, 2, 1] = true;
		switches [4, 1, 0] = true;
		switches [4, 2, 2] = true;

		rooms [0, 0] = true;
		rooms [3, 2] = true;


	}

	// Update is called once per frame
	void Update () {
		if (path == null) {
			bool updated = checkToggles ();
			if (updated) {
				path = hasWon ();
			}
		} else {
			this.vampire.GetComponent<Animator> ().SetBool ("walk", true);
			float speed = 3 * Time.deltaTime;
			this.vampire.transform.position = Vector3.MoveTowards (this.vampire.transform.position, this.path [0], speed);
			if (this.vampire.transform.position.Equals (this.path [0])) {
				if (this.path.Count > 1) {
					int deltaY = this.path [0].y - this.path [1].y;
					this.vampire.GetComponent<Animator> ().SetBool ("stairs", deltaY != 0);
					this.path.RemoveAt (0);
				} else {
					this.vampire.GetComponent<Animator> ().SetBool ("walk", false);
				}
			}
		}
	}

	bool checkToggles () {
		bool updated = false;
		for (int toggle = 0; toggle < numToggles; toggle++) {
			bool b = Input.GetButtonDown ("Toggle" + toggle);
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

	List<Vector3> hasWon() {
		List<Vector2Int> path = check (0, 0, new bool[Toggle.xRooms, Toggle.yRooms], new List<Vector2Int>());
		if (path != null) {
			string s = "";
			List<Vector3> animPath = new List<Vector3> ();
			foreach (Vector2Int p in path) {
				s += (p.x + ":"+ p.y + "|");
				animPath.Add (roomSprites [p.y * xRooms + p.x].transform.position);
			}
			print (s);
			return animPath;
		}
		
		return null;
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
