using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct StructPlayer : INetworkSerializable
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 vel;


    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref pos);
        s.SerializeValue(ref rot);
        s.SerializeValue(ref vel);
    }
}
