using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using OfficeLib.Converters.Format;

namespace ThirdPartyLibTest.OfficeLib
{
    [TestClass]
    public class TestLibreOfficeConverter
    {
        [TestMethod]
        public void TestDocToPdfConvert()
        {
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
                    LibreOfficeConverter.Convert(docList[index], "d:\\temp\\Test" + index + ".pdf");
                });
            }

            //var t2 = Task.Run(() =>
            //{
            //    var templatePath = Path.Combine(AppContext.BaseDirectory, "Resources\\OfficeLib\\test2.docx");
            //    LibreOfficeConverter.Convert(libreOfficeExePath, templatePath, "d:\\temp\\T112.pdf");
            //});

            Task.WaitAll(taskList);
        }

        [TestMethod]
        public void TestXlsxToPdfConvert()
        {
            Environment.SetEnvironmentVariable("LibreOffice_ExePath", @"D:\Program Files\LibreOffice\program\soffice.exe");
            Environment.SetEnvironmentVariable("LibreOffice_UserInstallation", "d:\\temp");

            var filePath = "d:\\111.xlsx";
            WriteToExcel(filePath);
            LibreOfficeConverter.Convert(filePath, "d:\\T112.pdf");
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

        private void WriteToExcel(string filePath)
        {
            //创建工作薄  
            IWorkbook wb;
            var extension = System.IO.Path.GetExtension(filePath);
            //根据指定的文件格式创建对应的类
            if (extension.Equals(".xls"))
            {
                wb = new HSSFWorkbook();
            }
            else
            {
                wb = new XSSFWorkbook();
            }

            var style1 = wb.CreateCellStyle();//样式
            style1.Alignment = HorizontalAlignment.Left;//文字水平对齐方式
            style1.VerticalAlignment = VerticalAlignment.Center;//文字垂直对齐方式
                                                                //设置边框
            style1.BorderBottom = BorderStyle.Thin;
            style1.BorderLeft = BorderStyle.Thin;
            style1.BorderRight = BorderStyle.Thin;
            style1.BorderTop = BorderStyle.Thin;
            style1.WrapText = true;//自动换行

            var style2 = wb.CreateCellStyle();//样式
            var font1 = wb.CreateFont();//字体
            font1.FontName = "楷体";
            font1.Color = HSSFColor.Red.Index;//字体颜色
            //font1.Boldweight = (short)FontBoldWeight.Normal;//字体加粗样式
            style2.SetFont(font1);//样式里的字体设置具体的字体样式
                                  //设置背景色
            style2.FillForegroundColor = HSSFColor.Yellow.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.FillBackgroundColor = HSSFColor.Yellow.Index;
            style2.Alignment = HorizontalAlignment.Left;//文字水平对齐方式
            style2.VerticalAlignment = VerticalAlignment.Center;//文字垂直对齐方式

            var dateStyle = wb.CreateCellStyle();//样式
            dateStyle.Alignment = HorizontalAlignment.Left;//文字水平对齐方式
            dateStyle.VerticalAlignment = VerticalAlignment.Center;//文字垂直对齐方式
                                                                   //设置数据显示格式
            var dataFormatCustom = wb.CreateDataFormat();
            dateStyle.DataFormat = dataFormatCustom.GetFormat("yyyy-MM-dd HH:mm:ss");

            //创建一个表单
            var sheet = wb.CreateSheet("Sheet0");
            //设置列宽
            int[] columnWidth = { 10, 10, 20, 10 };
            for (var i = 0; i < columnWidth.Length; i++)
            {
                //设置列宽度，256*字符数，因为单位是1/256个字符
                sheet.SetColumnWidth(i, 256 * columnWidth[i]);
            }

            //测试数据
            int rowCount = 3, columnCount = 4;
            object[,] data = {
                {"列0", "列1", "列2", "列3"},
                {"", 400, 5.2, 6.01},
                {"", true, "2014-07-02", DateTime.Now}
                //日期可以直接传字符串，NPOI会自动识别
                //如果是DateTime类型，则要设置CellStyle.DataFormat，否则会显示为数字
            };

            IRow row;
            NPOI.SS.UserModel.ICell cell;

            for (var i = 0; i < rowCount; i++)
            {
                row = sheet.CreateRow(i);//创建第i行
                for (var j = 0; j < columnCount; j++)
                {
                    cell = row.CreateCell(j);//创建第j列
                    cell.CellStyle = j % 2 == 0 ? style1 : style2;
                    //根据数据类型设置不同类型的cell
                    var obj = data[i, j];
                    SetCellValue(cell, data[i, j]);
                    //如果是日期，则设置日期显示的格式
                    if (obj.GetType() == typeof(DateTime))
                    {
                        cell.CellStyle = dateStyle;
                    }
                    //如果要根据内容自动调整列宽，需要先setCellValue再调用
                    //sheet.AutoSizeColumn(j);
                }
            }

            //合并单元格，如果要合并的单元格中都有数据，只会保留左上角的
            //CellRangeAddress(0, 2, 0, 0)，合并0-2行，0-0列的单元格
            var region = new CellRangeAddress(0, 2, 0, 0);
            sheet.AddMergedRegion(region);

            try
            {
                var fs = File.OpenWrite(filePath);
                wb.Write(fs);//向打开的这个Excel文件中写入表单并保存。  
                fs.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        //根据数据类型设置不同类型的cell
        public static void SetCellValue(NPOI.SS.UserModel.ICell cell, object obj)
        {
            if (obj.GetType() == typeof(int))
            {
                cell.SetCellValue((int)obj);
            }
            else if (obj.GetType() == typeof(double))
            {
                cell.SetCellValue((double)obj);
            }
            else if (obj.GetType() == typeof(IRichTextString))
            {
                cell.SetCellValue((IRichTextString)obj);
            }
            else if (obj.GetType() == typeof(string))
            {
                cell.SetCellValue(obj.ToString());
            }
            else if (obj.GetType() == typeof(DateTime))
            {
                cell.SetCellValue((DateTime)obj);
            }
            else if (obj.GetType() == typeof(bool))
            {
                cell.SetCellValue((bool)obj);
            }
            else
            {
                cell.SetCellValue(obj.ToString());
            }
        }
    }
}
