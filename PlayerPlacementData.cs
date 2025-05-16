using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerPlacementData : INetworkSerializable, IEquatable<PlayerPlacementData>
{
    public ulong clientId;
    public int placement;

    public PlayerPlacementData(ulong clientId, int placement)
    {
        this.clientId = clientId;
        this.placement = placement;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref placement);
    }

    public bool Equals(PlayerPlacementData other)
    {
        return clientId == other.clientId && placement == other.placement;
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerPlacementData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(clientId, placement);
    }
}