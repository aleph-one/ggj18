using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Globalization;
using ExerWorld.SimpleJSON;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
using System.Diagnostics;
#endif

/* This client is designed to work with the exerlinker, part of the exer.world framework */
using System.IO;


namespace ExerWorld {
	public class ExerWorldPlugin : MonoBehaviour
	{
		#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		[DllImport("user32.dll")]
		internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		public enum User32 {
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_NORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11,
			SW_MAX = 11
		}
		#endif


		public static ExerWorldPlugin it;
		public bool killSelfWhenThereIsOtherOwn = true;


		public string gameID;

		// connection handling
		public string	linker_ip = "127.0.0.1";
	    public int linker_tcp_port = 20666;
	    public int linker_http_api_port = 5000; // TODO: use in some way. fallback ? data transmission upon event ?
		public float cooldownConnectionTry = 5; // in seconds

		private static Thread 		clientThread;
		private static Thread 		connectThread;
		private static TcpClient 	tcpclient;
		// private static bool 		receivedAnyData = false;

		private static int UPDATE_EVERY = 20; // ms

		// generic dicts descriping the incoming data
		// the key of these dicts are always the values name
		private Dictionary<string, float> inValues = new Dictionary<string, float> ();
		// private Dictionary<string, float> inTimestamps 	= new Dictionary<string, float>();;

		// definition of in values
		// NOTE: when this list is updated, the controller who sends the values has to be notified too
		public string[] inNames; // TODO: this is more like a debug list. gets filled by what is there anyways

	    // debug
        //TODO: these flags are used very inconsistently, don't rely on them
		public bool enableLog = false;
		public bool enableLogSometimes = false;
		public bool debugShowData = true;


		public class CreateQueueEntry {
			public ControlInput ciParent;
			public string multiId;
			public string name;
			public float initVal;
		};

		List<CreateQueueEntry> createQueue = new List<CreateQueueEntry>();

		public bool autoBootLinker = false; // TODO / NOTE: only on windows

		#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		private Process procLinker;
        //TODO: might be able to get the info from the procLinker directly
        private bool hasLinkerStarted = false;
		#endif

		private System.IntPtr windowHandle = IntPtr.Zero;

		// Use this for initialization
		void Start ()
		{
			#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			windowHandle = GetActiveWindow();
			#endif

			if (killSelfWhenThereIsOtherOwn) {
				if (it == this.gameObject) {
				} else if (it == null) {
					it = this;
				} else {
					Destroy (this.gameObject); // kill yourself!
					EWUnityExtensions.Log ("kill myself");
					return; 
				}
			} else {
				it = this;
			}

			if (inNames == null || inNames.Length == 0) {
				inNames = new string[ControlInput.allControlsList.Count];

				ControlInput[] cis = this.GetComponents<ControlInput> ();
				for (int i=0; i < cis.Length; i++) {
					ControlInput ci  = cis[i];

					if (ci is ControlInputMulti) {
						continue;
					}
					inNames [i] = ci.controlName;

				}
			}

			DontDestroyOnLoad(transform.gameObject);	
            //This is not necessary as it auto connects in the update method
			//StartClientThread ();

			if (autoBootLinker) {
				AutoBootLinker ();
			}

		}


		// TODO ! self register app . at least give base information
		public void SendAppSpecs() {

			// TODO/NOTE: example specs as json:
			/*
			{
				\"id\": \"ancientrobot"
				, \"name\": "ancientrobot"
				, "description": "Ancient Robot"
				, "icon": "icon.png"
				, "dir": ""
				, "version": "0.0.1",

				"inputs": [
					{
						"name": "playerMulti_btn1"
						,"isMulti" : true
					}
					, {
						"name": "playerMulti_btn2"
						,"isMulti" : true
					}
					, {
						"name": "playerMulti_xRel"
						,"isMulti" : true
					}
					, {
						"name": "playerMulti_yRel"
						,"isMulti" : true
					}
				],

				"outputs": [

				],

				"settings": [

				]
			}
			*/


		}


		public void AutoBootLinker() {
			string pathToExerWorldLinker = "";
			string exectuableName = "";

			#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			exectuableName = "ExerWorldLinker.exe";
			#endif

			pathToExerWorldLinker = "";

			if (Application.platform == RuntimePlatform.OSXPlayer) {
				pathToExerWorldLinker = "" + Application.dataPath + "/../../";
			} else {
				pathToExerWorldLinker = "" + Application.dataPath + "/../";
			}

			pathToExerWorldLinker += "ExerWorldLinker/";

			string pathtoExerWorldLinkerWithExecutable = pathToExerWorldLinker + exectuableName;
			EWUnityExtensions.Log ("Try to start ExerWorldLinker here:" + "'" + pathToExerWorldLinker + "'" );

            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			try {
				
				procLinker = new Process();
				procLinker.StartInfo.FileName = pathtoExerWorldLinkerWithExecutable;
				procLinker.StartInfo.WorkingDirectory = pathToExerWorldLinker;
				
				// procLinker.StartInfo.Arguments = ...

                // Stop the process from opening a new window
				// TODO / FIXME nice to have but seems to block sth inside of exer.world's linker
				// procLinker.StartInfo.RedirectStandardOutput = true;
				// procLinker.StartInfo.UseShellExecute = false;
				// procLinker.StartInfo.CreateNoWindow = true;

				procLinker.Start();
                hasLinkerStarted = true;

                // GainFocus();
                StartCoroutine(GainFocusDelayed(0.1f));

			} catch (Exception e) {
				EWUnityExtensions.LogWarning ("Could not start ExerWorld's Linker from " + pathToExerWorldLinker + "\n" + e);
			}
            #endif

        }


		void GainFocus() {
			#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

			UnityEngine.Debug.Log("Try to regain focus..." + " current Window " + windowHandle);
			if (windowHandle != IntPtr.Zero)
			{
				SetForegroundWindow(windowHandle);
				int windowOption = (int) User32.SW_MAXIMIZE;
				UnityEngine.Debug.Log("<color=orange>GainFocus with window option " + windowOption + "</color>");
				ShowWindow(windowHandle, windowOption);
			}
			#endif

		}

		IEnumerator GainFocusDelayed (float delay) {

			yield return new WaitForSeconds (delay);
			GainFocus ();

		}


        // start read thread, it will read the data from the connection, if available
        void StartClientThread ()
		{
            //No need to get data if there is no connection 
            if (IsConnectionLost())
            {
                //no connection, no data
                EWUnityExtensions.LogWarning("##### ExerWorldPlugin StartClientThread, Unable to start client thread because no connection was established beforehand");
                return;
            }

			if (clientThread != null) {
				clientThread.Abort ();
			}
			clientThread = new Thread (HandleStreamLoop);
			clientThread.Start ();
		}
		
		private void ConnectToServer ()
		{
			try {
				// be sure that there is no old client hanging around
				if (tcpclient != null) {
					tcpclient.Close ();
				}

	            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(linker_ip), linker_tcp_port);
				
				tcpclient = new TcpClient ();
				tcpclient.NoDelay = true;
				tcpclient.Connect (endpoint);
				EWUnityExtensions.Log ("##### ExerWorldPlugin ConnectToServer, tcpclient.Connected ? " + tcpclient.Connected);

                //No need to start checking the stream if no connection was established
                if (tcpclient.Connected){
                    StartClientThread();
                }

	        } catch (Exception e) {
				EWUnityExtensions.Log ("##### ExerWorldPlugin ConnectToServer, Could not connect!!!! " + e);
			}

			connectThread.Abort ();
		}

		public bool IsConnectionLost ()
		{
			return tcpclient == null || tcpclient.Connected == false;
		}

		// Update is called once per frame
		private float timeLastConnectionTry = -1000;
		void Update ()
		{
			float curTime = Time.realtimeSinceStartup;

			// check if ControlInputs have to be created (needed for multiinputs which can dynamically grow)

			foreach (CreateQueueEntry entry in createQueue) {
				ControlInput parent = entry.ciParent;
				string multiId = entry.multiId;
				string multiFullName = entry.name;

				if (ControlInputMulti.allMultiControls.ContainsKey (multiFullName) == false) {
					EWUnityExtensions.Log ("Create new multi val controlinput child for '" + multiFullName + "'");
					ControlInputMulti ciChild = this.gameObject.AddComponent<ControlInputMulti> ();
					ciChild.multiId = multiId;
					ciChild.RegisterMulti (multiFullName);
					ciChild.multiParent = parent;
					ControlInput.MultiInputEntry multiEntry = new ControlInput.MultiInputEntry();
					multiEntry.multiId = multiId;
					multiEntry.name = multiFullName;
					multiEntry.controlInput = ciChild;
					parent.multiInputs.Add (multiEntry);
					ciChild.SetCurrentValue (entry.initVal);
				} else {
					EWUnityExtensions.LogWarning ("'" + multiFullName + "' already created!");
				}
			}
			createQueue.Clear ();


			// TODO: restart client if closed... look for server (rendevous/zeroconf ? )
			if (IsConnectionLost () && (curTime - timeLastConnectionTry) > cooldownConnectionTry) {
				timeLastConnectionTry = curTime;
				EWUnityExtensions.Log ("Try to connect to server '" + linker_ip + ":" + linker_tcp_port + "'");
				connectThread = new Thread (ConnectToServer);
				connectThread.Start ();

			}	

			foreach (var kV in inValues) {
				EWUnityExtensions.Log ("in Values => " + kV.Key + " -> " + kV.Value); 
			}


		}
		
		protected void HandleStream ()
		{
			// EWUnityExtensions.Log("handleStream called");
			NetworkStream stream = null;
			if (tcpclient != null && tcpclient.Connected) {
				stream = tcpclient.GetStream ();

				// reading
				if (stream != null && stream.DataAvailable) {
					int dataAvailable = tcpclient.Available;
					byte[] data = new byte[dataAvailable];
					
					int bytesRead = 0;
					try {
						bytesRead = stream.Read (data, 0, data.Length);
					} catch {
					}
					
					if (bytesRead < data.Length) {
						byte[] lastData = data;
						data = new byte[bytesRead];
						Array.ConstrainedCopy (lastData, 0, data, 0, bytesRead); // TODO / NOTE replace array with something unity like, so we can remove 'using system'
					}

					string data_str = ByteArrayToString (data);
					string[] lines = data_str.Split (new string[] { "\r\n", "\n" }, StringSplitOptions.None);
	                if (debugShowData)
	                {
	                    // EWUnityExtensions.Log("##### ExerWorldPlugin nr of lines " + lines.Length + " dataStr: " + data_str);
	                }

					string[] usefulLines;

					// NOTE: there should be at least 2 lines when splitted by newlines 
					if (lines.Length < 2) {
						// not enough
						// return;
					} else { 
						// there are enough so use them
						// take the last line, before the empty last one
						usefulLines = new string[]{lines [lines.Length - 2]}; 
										
						// TODO: ugly but works
						// ExerWorldPlugin.receivedAnyData = true; // TODO: remove / do different
						
						try {
							this.UpdateValues (usefulLines);
						} catch (SystemException e) {
							EWUnityExtensions.Log ("ExerWorldPlugin something went terribly wrong while updateValues .... " + e);
						}
					}



				} else {
					EWUnityExtensions.Log ("##### ExerWorldPlugin # readstream, no data available on stream");
				}

				////////////////////// writing

				if (stream != null) {
					// TODO: actually this info does not always have to be sent ... only when it changes ...
					/* // NOTE: old approach
					string data = "#send# ";


					// EWUnityExtensions.Log ("##### ExerWorldPlugin # inNames " + this.inNames.ToString() + " size: " + this.inNames.Length); 
					for (int i = 0; i < this.inNames.Length; i++) {
						string inName = this.inNames [i];
						// EWUnityExtensions.Log ("inName is " + inName );
						data = data + inName + " ";
					}
					*/

					string data = "#setName# " + this.gameID;

					// EWUnityExtensions.Log ("##### ExerWorldPlugin # data to send: '" + data + "'");

					data += "\n"; // NOTE: this is important, because the master server will read till the new line delimiter
		
					byte[] byteData = Encoding.ASCII.GetBytes (data);

					stream.Write (byteData, 0, byteData.Length);
					stream.Flush ();

					
				} else {
					EWUnityExtensions.Log ("##### ExerWorldPlugin # handleStream, writing fail ....");
				}

				
			} else {
				EWUnityExtensions.LogSometimes ("##### ExerWorldPlugin readStream: tcpclient not available");
			}



		}
	    
		protected void HandleStreamLoop ()
		{
			EWUnityExtensions.Log ("##### ExerWorldPlugin handleStreamLoop starting called");
			while (true) {
				HandleStream ();
				// EWUnityExtensions.Log ("sleep!");
				Thread.Sleep (UPDATE_EVERY);
			}
		}

		private void UpdateValues (string[] lines)
		{
	        // EWUnityExtensions.Log("Update values: " + lines[0]);

			// TODO: actually only one line is needed ...
			// for (int i=0; i < lines.Length ; i++) {
			string line = lines [0];

			if (debugShowData) {
				EWUnityExtensions.Log ("Line to parse is '" + line + "'");
			}

			JSONNode rootNode = JSON.Parse(line);
			// EWUnityExtensions.Log ("input interpreted as Json has values # " + rootNode.Count);
			JSONNode values = (JSONNode) rootNode ["values"];
			if (values != null) {
				// EWUnityExtensions.Log ("=> values has => # " + values.Count);
			}
				

			for (int j = 0; j < inNames.Length; j++) {
				string inValName = inNames [j];

				JSONNode valueArray = values [j];

				// TODO: parse the values and split them appropriately. inputs can be look:  
				//			[[], [], [], [], [0.990325079762794], [-0.2852937250435257]]
				// 		OR	[[0.990325079762794,0.123], [], [], [], [], [-0.2852937250435257]]
				// 		OR	[[], [], [], [], [], []]
				//		OR	[[0.123], [0.123], [0.123], [0.123], [0.123], [0.123]]


				bool foundFitting = false;
	            if (this.debugShowData)
	            {
					// EWUnityExtensions.Log("# controlInputs to update: " + ControlInput.allControlsList.Count);
	            }

				foreach (ControlInput ci in ControlInput.allControlsList) {
					if (ci.controlName == inValName) {

						float avg_val = 0;

						for (int ik = 0; ik < valueArray.Count; ik++) {
							JSONNode val = valueArray [ik];

							bool isJsonClass = val is JSONClass;
							bool isJsonData = val is SimpleJSON.JSONData;


							if (debugShowData)
								EWUnityExtensions.Log ("val has count # " + val.Count + " and is json class ? " + isJsonClass);
							if (isJsonClass && ci.isMultiInput) {
								JSONClass valDict = (JSONClass)val;
								if (debugShowData)
									EWUnityExtensions.Log ("valDict has children: #" + valDict.Count);
								foreach (string key in valDict.GetKeys ()) {
									float childVal = float.Parse (valDict [key]);
									if (debugShowData)
										EWUnityExtensions.Log ("=> found key " + key + " with value " + childVal);
									string multiFullName = ControlInputMulti.MakeMultiValueName (ci.controlName, key);
									ControlInputMulti ciChild = null;
									if (ControlInputMulti.allMultiControls.ContainsKey (multiFullName) == false) {
										CreateQueueEntry entry = new CreateQueueEntry ();
										entry.ciParent = ci;
										entry.multiId = key;
										entry.name = multiFullName;
										entry.initVal = childVal;
										createQueue.Add (entry);
									} else {
										ciChild = ControlInputMulti.allMultiControls [multiFullName];
										ciChild.SetCurrentValue (childVal);
									}
								}
							} else if (isJsonData) {
								avg_val += val.AsFloat;
							} else {
								EWUnityExtensions.LogWarning ("Unknown json type" + val.ToString() + " " + val.GetType () + ", isMulti ? " + ci.isMultiInput);
							}
						}

						ci.SetCurrentValue(avg_val);

						foundFitting = true;
						break;
					}
				}


				if (foundFitting) {
	                // EWUnityExtensions.Log("Found fitting control!");
				} else {
	                // EWUnityExtensions.Log("Found NOOOO fitting control!");
				}

			}
		}

		private void CloseClientThread ()
		{
			EWUnityExtensions.Log ("##### ExerWorldPlugin close tcp connection ");
			if (clientThread != null) {
				try {
					if (tcpclient != null) {
						tcpclient.GetStream ().Dispose ();
					}
				} catch (Exception) {
				}

				try {
					tcpclient.Close ();
				} catch (Exception) {
				}

				try {
					clientThread.Abort ();
				} catch {
					EWUnityExtensions.LogWarning ("##### ExerWorldPlugin ExerLinker: client close Error");
				}

			} else {
				EWUnityExtensions.LogWarning ("##### ExerWorldPlugin ExerLinker: clientThread was null");
			}
		}
		
		private String ByteArrayToString (byte[] ba)
		{
			return System.Text.Encoding.Default.GetString (ba);
		}
		
		void OnApplicationQuit ()
		{
			EWUnityExtensions.LogWarning ("##### ExerWorldPlugin OnApplicationQuit called");
			CloseClientThread ();
		}


		// static methods
		public static float ApplyDefaultCalibration (string inputName, float _value)
		{

			float outValue = _value;
			// fingers
			if (inputName.Contains ("Thumb") || inputName.Contains ("Index") || inputName.Contains ("Middle")) {
				float min = 275;
				float max = 537;
				outValue = ClampNormByRange (outValue, min, max);
			} // else if (    ) {		} 

			return outValue;
		}

		public static float ClampNormByRange (float _value, float min, float max)
		{
			float range = max - min;
			return (Mathf.Clamp (_value - min, 0, max - min)) / range;
		}
			
		void OnDestroy() {
			DestroyExerWorldLinker();
		}

		void DestroyExerWorldLinker() {

			EWUnityExtensions.Log("Try to kill autobooted ExerWorldLinker");

            //TODO: this needs to be extended for other platforms as well
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (hasLinkerStarted)
            {
                procLinker.Kill();
            }
			#endif

		}
	}



}