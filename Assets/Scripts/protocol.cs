// automatically generated, do not modify

namespace Networking
{

using System;
using FlatBuffers;

public enum MessageSwitch : byte
{
 NONE = 0,
 HelloPeer = 1,
 Position = 2,
};

public sealed class HelloPeer : Table {
  public static HelloPeer GetRootAsHelloPeer(ByteBuffer _bb) { return GetRootAsHelloPeer(_bb, new HelloPeer()); }
  public static HelloPeer GetRootAsHelloPeer(ByteBuffer _bb, HelloPeer obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public HelloPeer __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint GetPeers(int j) { int o = __offset(4); return o != 0 ? bb.GetUint(__vector(o) + j * 4) : (uint)0; }
  public int PeersLength { get { int o = __offset(4); return o != 0 ? __vector_len(o) : 0; } }
  public ArraySegment<byte>? GetPeersBytes() { return __vector_as_arraysegment(4); }
  public bool IsResponse { get { int o = __offset(6); return o != 0 ? 0!=bb.Get(o + bb_pos) : (bool)false; } }

  public static Offset<HelloPeer> CreateHelloPeer(FlatBufferBuilder builder,
      VectorOffset peersOffset = default(VectorOffset),
      bool is_response = false) {
    builder.StartObject(2);
    HelloPeer.AddPeers(builder, peersOffset);
    HelloPeer.AddIsResponse(builder, is_response);
    return HelloPeer.EndHelloPeer(builder);
  }

  public static void StartHelloPeer(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddPeers(FlatBufferBuilder builder, VectorOffset peersOffset) { builder.AddOffset(0, peersOffset.Value, 0); }
  public static VectorOffset CreatePeersVector(FlatBufferBuilder builder, uint[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddUint(data[i]); return builder.EndVector(); }
  public static void StartPeersVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddIsResponse(FlatBufferBuilder builder, bool isResponse) { builder.AddBool(1, isResponse, false); }
  public static Offset<HelloPeer> EndHelloPeer(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<HelloPeer>(o);
  }
};

public sealed class Position : Table {
  public static Position GetRootAsPosition(ByteBuffer _bb) { return GetRootAsPosition(_bb, new Position()); }
  public static Position GetRootAsPosition(ByteBuffer _bb, Position obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Position __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public float X { get { int o = __offset(4); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float Y { get { int o = __offset(6); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float Z { get { int o = __offset(8); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }

  public static Offset<Position> CreatePosition(FlatBufferBuilder builder,
      float x = 0,
      float y = 0,
      float z = 0) {
    builder.StartObject(3);
    Position.AddZ(builder, z);
    Position.AddY(builder, y);
    Position.AddX(builder, x);
    return Position.EndPosition(builder);
  }

  public static void StartPosition(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddX(FlatBufferBuilder builder, float x) { builder.AddFloat(0, x, 0); }
  public static void AddY(FlatBufferBuilder builder, float y) { builder.AddFloat(1, y, 0); }
  public static void AddZ(FlatBufferBuilder builder, float z) { builder.AddFloat(2, z, 0); }
  public static Offset<Position> EndPosition(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Position>(o);
  }
};

public sealed class Message : Table {
  public static Message GetRootAsMessage(ByteBuffer _bb) { return GetRootAsMessage(_bb, new Message()); }
  public static Message GetRootAsMessage(ByteBuffer _bb, Message obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Message __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public MessageSwitch MessageType { get { int o = __offset(4); return o != 0 ? (MessageSwitch)bb.Get(o + bb_pos) : MessageSwitch.NONE; } }
  public TTable GetMessage<TTable>(TTable obj) where TTable : Table { int o = __offset(6); return o != 0 ? __union(obj, o) : null; }

  public static Offset<Message> CreateMessage(FlatBufferBuilder builder,
      MessageSwitch message_type = MessageSwitch.NONE,
      int messageOffset = 0) {
    builder.StartObject(2);
    Message.AddMessage(builder, messageOffset);
    Message.AddMessageType(builder, message_type);
    return Message.EndMessage(builder);
  }

  public static void StartMessage(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddMessageType(FlatBufferBuilder builder, MessageSwitch messageType) { builder.AddByte(0, (byte)messageType, 0); }
  public static void AddMessage(FlatBufferBuilder builder, int messageOffset) { builder.AddOffset(1, messageOffset, 0); }
  public static Offset<Message> EndMessage(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    builder.Required(o, 6);  // message
    return new Offset<Message>(o);
  }
  public static void FinishMessageBuffer(FlatBufferBuilder builder, Offset<Message> offset) { builder.Finish(offset.Value); }
};


}
