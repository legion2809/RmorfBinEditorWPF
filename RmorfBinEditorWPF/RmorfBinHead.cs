using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmorfBinEditorWPF
{
    public class RmorfBinHead
    {
        private ulong fileSize;
        private ulong key;
        private ulong animGroupCCount;

        public RmorfBinHead(ulong fsize, ulong k, ulong aGroupC)
        {
            fileSize = fsize;
            key = k;
            animGroupCCount = aGroupC;
        }
    }
}
