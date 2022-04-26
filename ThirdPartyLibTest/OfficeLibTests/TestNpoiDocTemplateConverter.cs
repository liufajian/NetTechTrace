using Newtonsoft.Json.Linq;
using NPOI.XWPF.UserModel;
using OfficeLib.Converters;

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
            var varDic = new JsonVarDic();
            varDic.SetVar("a1", "11");

            var converter = new GenericConverter();

            try
            {
                templatePath = CreateBadTemplate("bad1");

                converter.DocTemplateConvert(templatePath, targetPath, varDic);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "区块未关闭:#loop1");
            }

            try
            {
                templatePath = CreateBadTemplate("bad2");

                converter.DocTemplateConvert(templatePath, targetPath, varDic);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "区块未关闭:#loop2");
            }

            try
            {
                templatePath = CreateBadTemplate("bad3");

                converter.DocTemplateConvert(templatePath, targetPath, varDic);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "没有找到要关闭的区块:/loop2");
            }

            try
            {
                templatePath = CreateBadTemplate("bad4");

                converter.DocTemplateConvert(templatePath, targetPath, varDic);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "执行'/loop1'之前请先关闭'#loop2'的区块");
            }

            try
            {
                templatePath = CreateBadTemplate("bad-table-close1");

                converter.DocTemplateConvert(templatePath, targetPath, varDic);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "不能在表格中关闭表格外的区块:#loop2");
            }
        }

        private string CreateBadTemplate(string badKey)
        {
            var doc = new XWPFDocument();
            var para = doc.CreateParagraph();
            var r0 = para.CreateRun();
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

                var table = doc.CreateTable(3, 3);

                var c1 = table.GetRow(0).GetCell(0);
                var p1 = c1.AddParagraph();
                var r1 = p1.CreateRun();
                r1.SetText("{{#loop3}}{{A1}}{{/loop3}}");

                table.GetRow(1).GetCell(1).SetText("{{/loop2}}");
            }

            var tempFilePath = Path.GetTempFileName();

            using (var fs = new FileStream(tempFilePath, FileMode.Create))
            {
                doc.Write(fs);
            }

            return tempFilePath;
        }

        [TestMethod("测试坏的文档模板")]
        public void TestTemplate1()
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Resources\\OfficeLib\\test1.docx");

            var targetPath = Path.Combine("d:\\", "test1.docx");

            var converter = new GenericConverter();

            var varDic = new JsonVarDic();

            varDic.SetVar("BeginIf1", true);

            var loop2 = new JObject {
                {"A1","myA1/2" },
                {"A2","myA2/2" },
                {"A3","myA3/2" },
                {"A4","myA4/2" },
                {"A5","myA5/2" },
            };

            var loop22 = new JArray() { loop2, loop2 };

            var loop4 = new JObject {
                {"A1","myA1" },
                {"A2","myA2" },
                {"A3","myA3" },
                {"A4","myA4" },
                {"A5","myA5" },
            };

            var loop44 = new JArray() { loop4, loop4 };

            var loop5 = new JArray();
            for (var i = 0; i < 6; i++)
            {
                loop5.Add(DateTime.Now.AddDays(i));
            }

            var testObj = new JObject
            {
                { "A1", "测试A1" },
                { "A2", "测试A2" },
                { "B1", "测试BB1" },
                { "B2", "测试BB2" },
                { "T11", "测试T11" },
                { "loop4", loop4 },
            };

            varDic.SetVar("Test", testObj);
            varDic.SetVar("loop2", loop22);
            varDic.SetVar("loop4", loop44);
            varDic.SetVar("loop5", loop5);

            converter.DocTemplateConvert(templatePath, targetPath, varDic);
        }
    }
}
