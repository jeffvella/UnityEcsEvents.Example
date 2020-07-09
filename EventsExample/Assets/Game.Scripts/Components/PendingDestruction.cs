using System;
using Unity.Entities;

public struct PendingDestruction : IComponentData
{
    public DateTime DestroyTime;
    public DateTime QueuedTime;
}
