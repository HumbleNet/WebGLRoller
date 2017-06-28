#define USE_THREAD

using System;
using System.Collections;

using UnityEngine;

#if ! UNITY_WEBGL
using System.Threading;
#endif


public class HumbleNetAPI {
	static private bool _isInitialized = false;
	static private bool _isConnected = false;
	static private HumbleNet.PeerId _myPeer;

#if ! UNITY_WEBGL && USE_THREAD
	internal class Poller {
		static Thread worker;
		static Poller instance;
		bool _running = true;

		public bool Running
		{
			set{ _running = value; }
		}

		static public void Init() {
			instance = new Poller();
			worker = new Thread( new ThreadStart( instance.Poll ) );
			worker.Start();
		}

		static public void Shutdown() {
			if (instance != null) {
				instance.Running = false;
				worker.Join();
			}
		}

		void Poll() {
			while( _running ) {
				// this call does NOT need to be lock/unlock'd, its internally threadsafe.
				HumbleNet.P2P.Wait(1000);
			}
		}
	}
#endif

	// Use this for initialization
	static public void Init(string server) {
		if (!_isInitialized) {
			_myPeer = HumbleNet.PeerId.Invalid;
			if (!HumbleNet.Init()) {
				throw new System.DllNotFoundException(string.Format("Could not locate HumbleNet! : {0}", HumbleNet.getError()));
			}
			HumbleNet.P2P.Initialize(server, "WebGLRoller", "<WebGLRoller secret>", null);
			HumbleNet.P2P.Wait(0);

#if ! UNITY_WEBGL && USE_THREAD
			Poller.Init();
#endif
			_isInitialized = true;
		}
	}

	static public void Shutdown() {
#if ! UNITY_WEBGL && USE_THREAD
		Poller.Shutdown();
#endif
		if (_isInitialized) {
			HumbleNet.Shutdown();
			_isInitialized = false;
			_isConnected = false;
			_myPeer = HumbleNet.PeerId.Invalid; 
		}
	}

	static public bool isInitialized {
		get { return _isInitialized; }
	}

	static public bool isConnected {
		get { return _isConnected; }
	}

	static public HumbleNet.PeerId myPeer
	{
		get {
			if (_myPeer.isInvalid() && _isInitialized) {
				_myPeer = HumbleNet.P2P.getMyPeerId();
				if (_myPeer.isValid()) { 
					_isConnected = true;
				}
			}
			return _myPeer;
		}
	}
}
