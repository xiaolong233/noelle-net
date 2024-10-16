namespace NoelleNet.Core;

public class NoelleGuidGenerator : IGuidGenerator
{
    public Guid Generate()
    {
        return Guid.NewGuid();
    }
}
