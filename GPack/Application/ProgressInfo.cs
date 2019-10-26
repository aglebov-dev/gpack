namespace GPack
{
    public struct ProgressInfo
    {
        public long Bytes { get; internal set; }
        public ProgressInfo(long processedBytes)
        {
            Bytes = processedBytes;
        }
    }
}
