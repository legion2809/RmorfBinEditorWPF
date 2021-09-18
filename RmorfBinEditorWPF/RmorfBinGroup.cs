using System.Collections.Generic;

namespace RmorfBinEditorWPF
{
    public class RmorfBinGroup
    {
        private uint morfCount;
        private uint animType;
        private uint animFrequency;
        private uint unknown3;
        private uint unknown4;
        private uint unknown5;
        private List<string> objNames;
        private byte nullb;

        public RmorfBinGroup(uint morfC, uint animT, uint animF, uint unk3, uint unk4, uint unk5, List<string> objN)
        {
            morfCount = morfC;
            animType = animT;
            animFrequency = animF;
            unknown3 = unk3;
            unknown4 = unk4;
            unknown5 = unk5;
            objNames = objN;
            nullb = 0x00;
        }
    }
}
