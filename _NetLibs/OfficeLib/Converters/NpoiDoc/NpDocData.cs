using NPOI.XWPF.UserModel;

namespace OfficeLib.NpoiModule
{
    class NpDocData
    {
        /// <summary>
        /// body或cell中的索引位置
        /// </summary>
        public int BodyIndex { get; set; }

        /// <summary>
        /// body或cell中的段落
        /// </summary>
        public XWPFParagraph Paragraph { get; set; }

        /// <summary>
        /// 表格单元格位置,rowIndex + cellIndex
        /// </summary>
        public int[] TableCellPos { get; set; }

        public int RowIndex => TableCellPos[0];

        public int CellIndex => TableCellPos[1];

        public override string ToString()
        {
            return TableCellPos != null ? $"row:{RowIndex},cell:{CellIndex}" : $"body index:{BodyIndex}";
        }
    }
}
