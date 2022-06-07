using NPOI.XWPF.UserModel;

namespace ThirdPartyLibTest.NPOITests
{
    [TestClass]
    public class TestWordDocument
    {
        [TestMethod]
        public void TestRowCopy1()
        {
            var docPath = Path.Combine(AppContext.BaseDirectory, "Resources\\NPOI\\TestRowCopy.docx");

            using (var fileStream = File.OpenRead(docPath))
            {
                XWPFDocument document = new XWPFDocument(fileStream);
                XWPFTable table = document.Tables[0];
                XWPFTableRow row = table.Rows[0];

                XWPFTableRow copiedRow1 = new XWPFTableRow(row.GetCTRow().Copy(), table);
                copiedRow1.GetCell(4).SetText("biubiub");
                table.AddRow(copiedRow1, 1);

                XWPFTableRow copiedRow2 = new XWPFTableRow(row.GetCTRow().Copy(), table);
                copiedRow2.GetCell(4).SetText("gogogo");
                table.AddRow(copiedRow2, 2);

                var p1 = document.Paragraphs[0];
                Assert.IsTrue(p1.Text.Contains("fortest"));
                var copyp1 = document.CreateParagraph();
                //foreach (var run in p1.Runs)
                //{
                //    copyp1.AddRun(new XWPFRun(run.GetCTR().Copy(), (IRunBody)copyp1));
                //}
                //copyp1.CreateRun().SetText("hhhhh");
                CopyParagraph(p1, copyp1);
                copyp1.ReplaceText("fortest", "fortest-bbbb---");

                document.SetParagraph(copyp1, 0);
                document.SetParagraph(p1, 1);

                using (var outStream = File.OpenWrite("d:\\npoi_test.docx"))
                {
                    document.Write(outStream);
                }
            }
        }

        private void CopyParagraph(XWPFParagraph sourcePara, XWPFParagraph targetPara)
        {
            var targetP = targetPara.GetCTP();
            var sourceP = sourcePara.GetCTP();

            targetP.pPr = sourceP.pPr;
            targetP.rsidP = sourceP.rsidP;
            targetP.rsidR = sourceP.rsidR;
            targetP.rsidRPr = sourceP.rsidRPr;
            targetP.rsidRDefault = sourceP.rsidRDefault;
            
            for (int r = 0; r < sourcePara.Runs.Count; r++)
            {
                var copyRun = sourcePara.Runs[r];
                var copyRunP = copyRun.GetCTR();
                var targetRun = targetPara.CreateRun();
                var targetRunP = targetRun.GetCTR();

                targetRunP.rPr = copyRunP.rPr;
                targetRunP.rsidR = copyRunP.rsidR;
                targetRunP.rsidRPr = copyRunP.rsidRPr;
                targetRun.SetText(copyRun.Text);
            }
        }
    }
}
