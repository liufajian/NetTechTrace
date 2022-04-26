using NPOI.XWPF.UserModel;
using OfficeLib.Converters.Format;

namespace ThirdPartyLibTest.OfficeLib
{
    [TestClass]
    public class TestLibreOfficeConverter
    {
        [TestMethod]
        public void TestConvert()
        {
            const string libreOfficeExePath = @"D:\Program Files\LibreOffice\program\soffice.exe";

            //Environment.SetEnvironmentVariable("LibreOffice_UserInstallation", "d:\\temp");

            var maxIndex = 1;

            var taskList = new Task[maxIndex];
            var docList = new string[maxIndex];

            for (var i = 0; i < maxIndex; i++)
            {
                docList[i] = CreateDocument("d:\\temp\\ttt");
            }

            for (var i = 0; i < maxIndex; i++)
            {
                var index = i;

                taskList[index] = Task.Run(() =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(docList[index]);
                    LibreOfficeConverter.Convert(libreOfficeExePath, docList[index], "d:\\temp\\Test" + index + ".pdf");
                });
            }

            //var t2 = Task.Run(() =>
            //{
            //    var templatePath = Path.Combine(AppContext.BaseDirectory, "Resources\\OfficeLib\\test2.docx");
            //    LibreOfficeConverter.Convert(libreOfficeExePath, templatePath, "d:\\temp\\T112.pdf");
            //});

            Task.WaitAll(taskList);
        }

        private string CreateDocument(string outputDir)
        {
            var doc = new XWPFDocument();

            var para = doc.CreateParagraph();
            var r0 = para.CreateRun();
            r0.SetText(Path.GetTempFileName());

            para = doc.CreateParagraph();
            r0 = para.CreateRun();
            r0.SetText(Path.GetTempFileName());

            var table = doc.CreateTable(3, 3);

            var c1 = table.GetRow(0).GetCell(0);
            var p1 = c1.AddParagraph();
            var r1 = p1.CreateRun();
            r1.SetText(Path.GetTempFileName());

            table.GetRow(1).GetCell(1).SetText(Path.GetTempFileName());

            var tempFilePath = Path.Combine(outputDir, Guid.NewGuid().ToString("N") + ".docx");

            using (var fs = new FileStream(tempFilePath, FileMode.Create))
            {
                doc.Write(fs);
            }

            return tempFilePath;
        }
    }
}
