namespace UnitTestProject1.MRIMClient
{
    internal class Mrim_packet_header
    {
        private long cS_MAGIC;
        private long pROTO_VERSION;
        private int seq;
        private long mRIM_CS_SMS;
        private int v1;
        private int v2;
        private int v3;
        private int v4;
        private int v5;
        private int v6;
        private int v7;

        public Mrim_packet_header(long cS_MAGIC, long pROTO_VERSION, int seq, long mRIM_CS_SMS, int v1, int v2, int v3, int v4, int v5, int v6, int v7)
        {
            this.cS_MAGIC = cS_MAGIC;
            this.pROTO_VERSION = pROTO_VERSION;
            this.seq = seq;
            this.mRIM_CS_SMS = mRIM_CS_SMS;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
            this.v5 = v5;
            this.v6 = v6;
            this.v7 = v7;
        }
    }
}