using AutoMapper;

namespace ThirdPartyLibTest.AutoMapperTests
{
    [TestClass]
    public class TestMapDictionary
    {
        [TestMethod]
        public void Test1()
        {
            var mapcfg = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<string, bool>().ForAllMembers(cc =>
                {
                    cc.Condition(s => !string.IsNullOrEmpty(s));
                });

                cfg.CreateMap<string, bool>().ConvertUsing(s => s == "true");
            });

            var dic = new Dictionary<string, string> {
                {"AA","11" },
                {"BB","" },
                {"BBT","" },
                //{"EE","1" }
            };

            var poco = new Mapper(mapcfg).Map<MyPoco>(dic);

            Assert.IsTrue(poco.AA == "11" && !poco.BB && poco.BBT && poco.EE == MyEnum.n1);

            dic = new Dictionary<string, string> {
                {"AA","33" },
                {"BB","true" },
                {"BBT","ffff" },
                {"EE","2" }
            };

            new Mapper(mapcfg).Map(dic, poco);

            Assert.IsTrue(poco.AA == "33" && poco.BB &&!poco.BBT && poco.EE == MyEnum.n2);
        }

        class MapStringToBoolConvert : IValueConverter<string, bool>
        {
            public bool Convert(string sourceMember, ResolutionContext context)
            {
                return sourceMember == "true";
            }
        }
    }
}
