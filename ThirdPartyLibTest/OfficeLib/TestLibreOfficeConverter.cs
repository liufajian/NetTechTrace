using OfficeLib.Converts;

namespace ThirdPartyLibTest.OfficeLib
{
    [TestClass]
    public class TestLibreOfficeConverter
    {
        [TestMethod]
        public void TestConvert()
        {
            var t1 = Task.Run(() =>
            {
                var templatePath = Path.Combine(AppContext.BaseDirectory, "Resources\\OfficeLib\\test11.docx");
                DocxToPdfConverter.ConvertByLibreOffice(@"D:\Program Files\LibreOffice\program\soffice.exe", templatePath, "d:\\");
            });

            //var t2 = Task.Run(() =>
            //{
            //    var templatePath = Path.Combine(AppContext.BaseDirectory, "Resources\\OfficeLib\\test1.docx");
            //    DocxToPdfConverter.ConvertByLibreOffice(@"D:\Program Files\LibreOffice\program\soffice.exe", templatePath, "d:\\");
            //});

            //Task.WaitAll(t1, t2);
            Task.WaitAll(t1);
        }
    }
}
