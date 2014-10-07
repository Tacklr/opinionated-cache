namespace OpinionatedCache.API
{
    public interface ICachePolicy
    {
        int AbsoluteSeconds { get; set; }
        ICachePolicy Clone();
        int RefillCount { get; set; }
        int SlidingSeconds { get; set; }
    }
}
