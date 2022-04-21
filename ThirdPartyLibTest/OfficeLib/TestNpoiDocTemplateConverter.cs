using NPOI.XWPF.UserModel;
using OfficeLib;
using OfficeLib.Json;

namespace ThirdPartyLibTest.OfficeLib
{
    [TestClass]
    public class TestNpoiDocTemplateConverter
    {
        [TestMethod("测试坏的文档模板")]
        public void TestBadTemplate()
        {
            string templatePath;

            var targetPath = "111.docx";

            var converter = new NpoiDocTemplateConverter();

            try
            {
                templatePath = CreateBadTemplate("bad1");

                converter.Convert(templatePath, targetPath);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "区块未关闭:#loop1");
            }

            try
            {
                templatePath = CreateBadTemplate("bad2");

                converter.Convert(templatePath, targetPath);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "区块未关闭:#loop2");
            }

            try
            {
                templatePath = CreateBadTemplate("bad3");

                converter.Convert(templatePath, targetPath);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "没有找到要关闭的区块:/loop2");
            }

            try
            {
                templatePath = CreateBadTemplate("bad4");

                converter.Convert(templatePath, targetPath);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "执行'/loop1'之前请先关闭'#loop2'的区块");
            }

            try
            {
                templatePath = CreateBadTemplate("bad-table-close1");

                converter.Convert(templatePath, targetPath);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "不能在表格中关闭表格外的区块:#loop2");
            }
        }

        private string CreateBadTemplate(string badKey)
        {
            XWPFDocument doc = new XWPFDocument();
            XWPFParagraph para = doc.CreateParagraph();
            XWPFRun r0 = para.CreateRun();
            r0.SetText("{{#loop1}}");

            if (badKey == "bad2")
            {
                para = doc.CreateParagraph();
                r0 = para.CreateRun();
                r0.SetText("{{#loop2}}");
            }

            if (badKey == "bad3")
            {
                para = doc.CreateParagraph();
                r0 = para.CreateRun();
                r0.SetText("{{/loop1}}");

                para = doc.CreateParagraph();
                r0 = para.CreateRun();
                r0.SetText("{{/loop2}}");
            }

            if (badKey == "bad4")
            {
                para = doc.CreateParagraph();
                r0 = para.CreateRun();
                r0.SetText("{{#loop2}}");

                para = doc.CreateParagraph();
                r0 = para.CreateRun();
                r0.SetText("{{/loop1}}");
            }

            if (badKey == "bad-table-close1")
            {
                para = doc.CreateParagraph();
                r0 = para.CreateRun();
                r0.SetText("{{#loop2}}");

                XWPFTable table = doc.CreateTable(3, 3);

                XWPFTableCell c1 = table.GetRow(0).GetCell(0);
                XWPFParagraph p1 = c1.AddParagraph();
                XWPFRun r1 = p1.CreateRun();
                r1.SetText("{{#loop3}}{{A1}}{{/loop3}}");

                table.GetRow(1).GetCell(1).SetText("{{/loop2}}");
            }

            var tempFilePath = Path.GetTempFileName();

            using (FileStream fs = new FileStream(tempFilePath, FileMode.Create))
            {
                doc.Write(fs);
            }

            return tempFilePath;
        }

        [TestMethod("测试坏的文档模板")]
        public void TestTemplate1()
        {
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Resources\\OfficeLib\\test1.docx");

            var targetPath = Path.Combine("d:\\", "test1.docx");

            var converter = new NpoiDocTemplateConverter();

            converter.AddVariable("BeginIf1", true);
            var jn = new JsonObject
            {
                { "A1", "测试A1" },
                { "A2", "测试A2" },
                { "B1", "测试BB1" },
                { "B2", "测试BB2" },
                { "T11", "测试T11" },
            };

            converter.AddVariable("Test", jn);

            converter.Convert(templatePath, targetPath);
        }
    }
}
