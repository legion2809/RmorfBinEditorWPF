namespace RmorfBinEditorWPF
{
    public class RmorfBinHead
    {
        public ulong fileSize;
        public ulong key;
        public ulong animGroupCCount;

        public RmorfBinHead(ulong fsize, ulong k, ulong aGroupC)
        {
            fileSize = fsize;
            key = k;
            animGroupCCount = aGroupC;
        }
    }
}
