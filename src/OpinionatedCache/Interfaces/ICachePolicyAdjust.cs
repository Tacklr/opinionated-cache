namespace OpinionatedCache.API
{
    public interface ICachePolicyAdjust
    {
        ICachePolicy Adjust(ICachePolicy policy);
    }
}
