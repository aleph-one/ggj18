using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggle : MonoBehaviour {
	private const int numToggles = 12;
	private const int xRooms = 4;
	private const int yRooms = 3;
	private bool[,,] switches = new bool[numToggles, xRooms, yRooms];
	private bool[,] rooms = new bool[xRooms, yRooms];

	public GameObject[] roomSprites;
	public Color roomDark;
	// Use this for initialization
	void Start () {
		switches [0, 1, 0] = true;
		switches [0, 2, 1] = true;
		switches [1, 2, 0] = true;
		switches [1, 2, 1] = true;
		switches [2, 1, 0] = true;
		switches [2, 3, 1] = true;

		rooms [0, 0] = true;
		rooms [3, 2] = true;


	}

	// Update is called once per frame
	void Update () {
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
				} else {
					roomSprites [yRoom * Toggle.xRooms + xRoom].GetComponent<SpriteRenderer> ().color = Color.white;
				}
			}
		}
		if (updated) {
			hasWon ();
		}
	}

	bool hasWon() {
		bool won = check (0, 0, new bool[Toggle.xRooms, Toggle.yRooms], new List<Vector2Int>());
		print (won);
		return won;
	}
	Vector2Int[] getNeighbors(int x, int y) {
		return new Vector2Int[] {new Vector2Int(x - 1, y), new Vector2Int(x, y - 1), new Vector2Int(x + 1, y), new Vector2Int(x, y + 1)};
	}
	bool check(int xRoom, int yRoom, bool[,] visited, List<Vector2Int> path) {
		visited [xRoom, yRoom] = true;
		if (xRoom == 3 && yRoom == 2) {//GOAL
			return true;
		}
		if (!this.rooms[xRoom, yRoom]) {
			return false;
		}
		bool valid = false;
		Vector2Int[] n = getNeighbors (xRoom, yRoom);
		for (int i = 0; i < n.GetLength(0); i++) {
			if (n [i].x >= 0 && n [i].x < Toggle.xRooms && n[i].y >= 0 && n[i].y < Toggle.yRooms && !visited [n [i].x, n[i].y]) {
				List<Vector2Int> np = new List<Vector2Int> (path.ToArray());
				np.Add (new Vector2Int(n[i].x, n[i].y));
				if (check (n [i].x, n[i].y, visited, np)) {
					valid = true;
				}
			}
		}
		return valid;
	}
}
