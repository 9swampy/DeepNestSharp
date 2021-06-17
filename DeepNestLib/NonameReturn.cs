namespace DeepNestLib
{
    public class NonameReturn
    {
        public NfpKey key;
        public NFP[] nfp;

        public NFP[] value
        {
            get
            {
                return this.nfp;
            }
        }

        public NonameReturn(NfpKey key, NFP[] nfp)
        {
            this.key = key;
            this.nfp = nfp;
        }
    }
}
