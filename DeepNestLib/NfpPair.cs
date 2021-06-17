namespace DeepNestLib
{
    public class NfpPair
    {
        public NFP A;
        public NFP B;
        public NfpKey Key;
        public NFP nfp;

        public float ARotation;
        public float BRotation;

        public int Asource { get; internal set; }

        public int Bsource { get; internal set; }
    }
}
