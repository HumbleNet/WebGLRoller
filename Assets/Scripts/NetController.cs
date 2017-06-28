using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using FlatBuffers;

public class NetController : MonoBehaviour {
	public Text peerIdText;
	public GameObject remotePlayerPrefab;
	public string peerServer;

	private float lastHelloCheck;


	const byte CHANNEL = 42;
	static HumbleNet.PeerId _myPeer = HumbleNet.PeerId.Invalid;

	static List<HumbleNet.PeerId> _pendingPeers = new List<HumbleNet.PeerId>();
	static Dictionary<HumbleNet.PeerId, GameObject> _remotePlayers = new Dictionary<HumbleNet.PeerId, GameObject>();

	// Use this for initialization
	void Start () {
		HumbleNetAPI.Init(peerServer);

		lastHelloCheck = Time.fixedTime;
	}

	#region Peer Management
	public static void AddConnectedPeer(HumbleNet.PeerId peer, GameObject obj) {
		_pendingPeers.Remove(peer);
		if (!_remotePlayers.ContainsKey(peer)) {
			_remotePlayers.Add(peer, obj);
		}
	}

	public static void ConnectToPeer( HumbleNet.PeerId peer ) {
		if (peer != _myPeer && !_pendingPeers.Contains(peer) && !_remotePlayers.ContainsKey(peer)) {
			_pendingPeers.Add(peer);
		}
	}
	
	public static void DropPeer( HumbleNet.PeerId peer ) {
		Console.WriteLine("Dropping peer: {0}", peer);
		// peer disconnected so we should remove them from the available peers.
		if (_remotePlayers.ContainsKey(peer)) {
			GameObject obj = _remotePlayers[peer];
			_remotePlayers.Remove(peer);
			Destroy(obj);
		}
		_pendingPeers.Remove(peer);
		HumbleNet.P2P.DisconnectPeer(peer);
	}

	#endregion

	#region Utilities
	private static byte[] getFlatBufferData(FlatBufferBuilder fbb)
	{
		byte[] buff;
		using (var ms = new MemoryStream(fbb.DataBuffer.Data, fbb.DataBuffer.Position, fbb.Offset))
		{
			buff = ms.ToArray();
		}
		return buff;
	}

	private static VectorOffset buildKnownPeers(FlatBufferBuilder fbb)
	{
		var elems = _pendingPeers.Count + _remotePlayers.Count;
		Networking.HelloPeer.StartPeersVector(fbb, elems);
		
		foreach(var peer in _pendingPeers) {
			fbb.AddUint((UInt32)peer);
		}
		foreach(var peer in _remotePlayers.Keys) {
			fbb.AddUint((UInt32)peer);
		}

		return fbb.EndVector();
	}
	#endregion

	#region Send Messages

	public static bool SendHello(HumbleNet.PeerId peer, bool is_response = false)
	{
		var fbb = new FlatBufferBuilder(16);
		var knownPeers = buildKnownPeers(fbb);
		var hello = Networking.HelloPeer.CreateHelloPeer(fbb, knownPeers, is_response);
		var msg = Networking.Message.CreateMessage(fbb, Networking.MessageSwitch.HelloPeer, hello.Value);
		Networking.Message.FinishMessageBuffer(fbb, msg);

		return HumbleNet.P2P.SendTo(getFlatBufferData(fbb), peer, HumbleNet.SendMode.SEND_RELIABLE, CHANNEL) >= 0;
	}

	public static void SendPosition(Vector3 position) {
		FlatBufferBuilder fbb = new FlatBufferBuilder(64);

		var pos = Networking.Position.CreatePosition(fbb, position.x, position.y, position.z);

		var msg = Networking.Message.CreateMessage(fbb, Networking.MessageSwitch.Position, pos.Value);
		Networking.Message.FinishMessageBuffer(fbb, msg);

		byte[] buff = getFlatBufferData(fbb);

		List<HumbleNet.PeerId> disconnected = null;

		foreach(HumbleNet.PeerId peer in _remotePlayers.Keys )
		{
			if( HumbleNet.P2P.SendTo(buff, peer, HumbleNet.SendMode.SEND_RELIABLE, CHANNEL) < 0) {
				if( disconnected == null )
					disconnected = new List<HumbleNet.PeerId>();
				disconnected.Add( peer );
			}
		}

		if( disconnected != null ) {
			foreach(var peer in disconnected ) {
				DropPeer( peer );
			}
		}
	}
	#endregion
	

	// Update is called once per frame
	void Update () {
		CheckPeer();

		if (!HumbleNetAPI.isConnected) {
			return;
		}

		bool done = false;

		while( !done )
		{
			byte[] buff;

			HumbleNet.PeerId peer;

			if (HumbleNet.P2P.RecvFrom(out buff, out peer, CHANNEL) > 0) {
				ByteBuffer bb = new ByteBuffer(buff);

				var msg = Networking.Message.GetRootAsMessage(bb);
				switch (msg.MessageType) {
				case Networking.MessageSwitch.HelloPeer: {
					var hello = (Networking.HelloPeer)msg.GetMessage(new Networking.HelloPeer());

					var position = new Vector3(0,0,0);
					GameObject obj = Instantiate(remotePlayerPrefab, position, Quaternion.identity) as GameObject;
					obj.SetActive(false);

					AddConnectedPeer(peer, obj);

					for (var i = 0; i < hello.PeersLength; ++i) {
						ConnectToPeer((HumbleNet.PeerId)hello.GetPeers(i));
					}

					if (!hello.IsResponse) {
						SendHello(peer, true);
					}
				}
					break;
				case Networking.MessageSwitch.Position: {
					var pos = (Networking.Position)msg.GetMessage(new Networking.Position());
					
					Vector3 position = new Vector3(pos.X, pos.Y, pos.Z);
					
					if (_remotePlayers.ContainsKey(peer)) {
						GameObject obj = _remotePlayers[peer];
						if (!obj.activeSelf) obj.SetActive(true);
						obj.transform.position = position;
					} else {
						Debug.LogFormat("Unexpected Peer Position data {0}", peer);
					}
				}
					break;
				default:
					Debug.LogFormat("Unknown network packet {0}", msg.MessageType);
					break;
				}
			} else if (peer.isValid()) {
				Console.WriteLine("Saw disconnected peer in receive! {0}", peer);
				DropPeer(peer);
			} else {
				done = true;
			}
		}

		float now = Time.fixedTime;

		List<HumbleNet.PeerId> toRemove = null;

		if ((now - lastHelloCheck) > 1) {
			lastHelloCheck = now;
			foreach(var pp in _pendingPeers) {
				if (!SendHello(pp)) {
					if (toRemove == null) {
						toRemove = new List<HumbleNet.PeerId>();
					}
					toRemove.Add(pp);
				}
			}

			if (toRemove != null) {
				foreach(var pp in toRemove) {
					DropPeer(pp);
				}
			}
		}
	}

	public void Connect(string peer) {
		Debug.LogFormat ("Connecting to peer {0}", peer);
		UInt32 peerId;
		if (UInt32.TryParse(peer, out peerId)) {
			Debug.LogFormat ("Connecting to peer (parsed) {0}", peerId);
			ConnectToPeer((HumbleNet.PeerId)peerId);
		}
	}
	
	void OnApplicationQuit() {
		HumbleNetAPI.Shutdown();
	}

	private void CheckPeer() {
		if (_myPeer.isInvalid()) {
			_myPeer = HumbleNetAPI.myPeer;
			if (_myPeer.isValid()) {
				peerIdText.text = string.Format("Peer ID: {0}", _myPeer);
			}
		}
	}
}
