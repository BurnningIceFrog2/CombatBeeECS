using Unity.Entities;

public struct PropertiesBlobReferenceComp : ISharedComponentData
{
    public BlobAssetReference<PropertiesBlob> PropertyBlob;
}
