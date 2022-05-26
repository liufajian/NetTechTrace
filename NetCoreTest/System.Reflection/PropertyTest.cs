namespace System.Reflection
{
    [TestClass]
    public class PropertyTest
    {
        [TestMethod("测试如何判断属性名是否在对象中")]
        public void Test1()
        {
            var t = typeof(MyClass);

            var m = t.GetMember("ASDFERT", BindingFlags.Instance | BindingFlags.Public).Where(n => n is PropertyInfo).FirstOrDefault();
            Assert.IsTrue(m == null);

            m = t.GetMember("AA", BindingFlags.Instance | BindingFlags.Public).Where(n => n is PropertyInfo).FirstOrDefault();
            Assert.IsTrue(m != null);

            m = t.GetMember("_STR", BindingFlags.Instance | BindingFlags.Public).Where(n => n is PropertyInfo).FirstOrDefault();
            Assert.IsTrue(m == null);

            m = t.GetMember("_STR", BindingFlags.Instance | BindingFlags.Public).Where(n => n is FieldInfo).FirstOrDefault();
            Assert.IsTrue(m != null);
        }
    }
}
