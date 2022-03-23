using Unity.Entities;

public struct PropertiesBlobReferenceComp : IComponentData
{
    public BlobAssetReference<PropertiesBlob> PropertyBlob;
}
