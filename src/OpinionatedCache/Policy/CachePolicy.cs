namespace OpinionatedCache.Policy
{
    public class CachePolicy
    {
        public static readonly int Unused = -1;
        public static readonly int Infinite = -2;

        public int AbsoluteSeconds { get; set; }
        public int SlidingSeconds { get; set; }
        public int RefillCount { get; set; }

        public CachePolicy()
        {
            AbsoluteSeconds = CachePolicy.Unused;
            SlidingSeconds = CachePolicy.Unused;
            RefillCount = CachePolicy.Unused;
        }

        public virtual CachePolicy Clone()
        {
            return new CachePolicy
                {
                    AbsoluteSeconds = this.AbsoluteSeconds,
                    SlidingSeconds = this.SlidingSeconds,
                    RefillCount = this.RefillCount
                };
        }

        public static CachePolicy Sliding(int seconds)
        {
            return new CachePolicy { SlidingSeconds = seconds };
        }

        public static CachePolicy Absolute(int seconds)
        {
            return new CachePolicy { AbsoluteSeconds = seconds };
        }
    }
}
