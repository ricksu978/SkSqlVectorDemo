using Microsoft.Extensions.VectorData;

public class Policy
{
    [VectorStoreKey] 
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [VectorStoreData(IsIndexed = true)] 
    public string Title { get; set; } = "";
    
    [VectorStoreData] 
    public string Content { get; set; } = "";

    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance)]
    public ReadOnlyMemory<float>? ContentEmbedding { get; set; }
}
