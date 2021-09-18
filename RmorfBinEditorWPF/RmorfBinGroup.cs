using System.Collections.Generic;

namespace RmorfBinEditorWPF
{
    public class RmorfBinGroup
    {
        public uint morfCount;
        public uint animType;
        public uint animFrequency;
        public uint unknown3;
        public uint unknown4;
        public uint unknown5;
        public List<string> objNames;
        public byte nullb;

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
