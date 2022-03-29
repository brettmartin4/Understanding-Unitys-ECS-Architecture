using Unity.Entities;

[GenerateAuthoringComponent]
public struct AircraftData : IComponentData
{
    public float x;
    public float y;
    public float z;
}
