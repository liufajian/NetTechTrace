namespace ThirdPartyLibTest
{
    internal class MyPoco
    {
        public string AA { get; set; }

        public bool BB { get; set; }

        public bool BBT { get; set; } = true;

        public MyEnum EE { get; set; }
    }

    enum MyEnum { n0, n1, n2 }
}
