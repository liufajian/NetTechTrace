using Newtonsoft.Json.Linq;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OfficeLib.Converters.Template
{
    /// <summary>
    /// 
    /// </summary>
    partial class NpDocTemplateConverter
    {
        JsonVarDic _varDic;

        public NpDocTemplateConverter()
        {

        }

        public void Convert(string templatePath, string outputFilePath, JsonVarDic varDic)
        {
            if (templatePath == null)
            {
                throw new ArgumentNullException(nameof(templatePath));
            }

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("模板文件不存在:" + templatePath);
            }

            using (var rs = File.OpenRead(templatePath))
            {
                Convert(rs, outputFilePath, varDic);
            }
        }

        public void Convert(Stream templateStream, string outputFilePath, JsonVarDic varDic)
        {
            if (templateStream is null)
            {
                throw new ArgumentNullException(nameof(templateStream));
            }

            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                throw new ArgumentNullException(nameof(outputFilePath));
            }

            if (varDic == null || varDic.Count < 1)
            {
                using var outputStream = File.OpenWrite(outputFilePath);
                templateStream.CopyTo(outputStream);
                return;
            }

            _varDic = varDic;

            var doc = new XWPFDocument(templateStream);
            var root = NpDocTemplateNode.CreateRootSection();

            ParseDocument(doc, root);

            foreach (var section in root.Children.Reverse())
            {
                if (section.SectionType == NpDocSectionType.table)
                {
                    var table = (XWPFTable)doc.BodyElements[section.NodeData.BodyIndex];

                    HandleTable(table, section);
                }
                else if (section.SectionType == NpDocSectionType.mif)
                {
                    HandleBodyIf(doc, section);
                }
            }

            using (var stream = File.OpenWrite(outputFilePath))
            {
                doc.Write(stream);
            }
        }

        #region----Convert Methods----

        private void HandleBodyIf(XWPFDocument doc, NpDocTemplateNode mifSection)
        {
            var (jtoken, format) = GetPathValue(mifSection.NodeKey);

            var arr = JsonVarHelper.GetLoopArray(jtoken, format);

            var beginIndex = mifSection.NodeData.BodyIndex;
            var endIndex = mifSection.EndNode.NodeData.BodyIndex;

            if (arr == null)
            {
                var removeIndex = beginIndex;

                for (var i = beginIndex; i <= endIndex; i++)
                {
                    doc.RemoveBodyElement(removeIndex);
                }
            }
            else
            {
                var beginPara = mifSection.NodeData.Paragraph;
                beginPara.ReplaceText(mifSection.NodeText, string.Empty);

                var endPara = mifSection.EndNode.NodeData.Paragraph;
                endPara.ReplaceText(mifSection.EndNode.NodeText, string.Empty);

                if (mifSection.HasChild)
                {
                    foreach (var item in mifSection.Children)
                    {
                        if (item.NodeData.Paragraph != null)
                        {
                            (jtoken, format) = GetPathValue(item.NodeKey);
                            item.NodeData.Paragraph.ReplaceText(item.NodeText, jtoken?.FormatValue(format));
                        }
                        else //处理表格
                        {
                            var table = (XWPFTable)doc.BodyElements[item.NodeData.BodyIndex];

                            HandleTable(table, item);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(endPara.Text))
                {
                    doc.RemoveBodyElement(mifSection.EndNode.NodeData.BodyIndex);
                }

                if (string.IsNullOrWhiteSpace(beginPara.Text))
                {
                    doc.RemoveBodyElement(mifSection.NodeData.BodyIndex);
                }
            }
        }

        private void HandleTable(XWPFTable table, NpDocTemplateNode tableSection)
        {
            if (!tableSection.HasChild)
            {
                return;
            }

            Stack<NpDocTemplateNode> loopStack = null;

            foreach (var child in tableSection.Children)
            {
                if (child.SectionType == NpDocSectionType.loop)
                {
                    if (loopStack == null)
                    {
                        loopStack = new Stack<NpDocTemplateNode>();
                    }
                    loopStack.Push(child);
                }
                else
                {
                    var (jtoken, format) = GetPathValue(child.NodeKey);

                    child.NodeData.Paragraph.ReplaceText(child.NodeText, jtoken?.FormatValue(format));
                }
            }

            if (loopStack != null)
            {
                while (loopStack.Count > 0)
                {
                    var loopSection = loopStack.Pop();
                    var (jtoken, format) = GetPathValue(loopSection.NodeKey);
                    var loopValues = JsonVarHelper.GetLoopArray(jtoken, format);
                    new NpDocTableHandler(table).HandleLoop(loopSection, loopValues, GetPathValue);
                }
            }
        }

        private void CopyParagraph(XWPFParagraph sourcePara, XWPFParagraph targetPara)
        {
            var targetP = targetPara.GetCTP();
            var sourceP = sourcePara.GetCTP();

            targetP.pPr = sourceP.pPr;
            targetP.rsidR = sourceP.rsidR;
            targetP.rsidRPr = sourceP.rsidRPr;
            targetP.rsidRDefault = sourceP.rsidRDefault;
            targetP.rsidP = sourceP.rsidP;

            for (var r = 0; r < sourcePara.Runs.Count; r++)
            {
                var copyRun = sourcePara.Runs[r];

                var copyRunP = copyRun.GetCTR();

                var targetRun = targetPara.CreateRun();
                var targetRunP = targetRun.GetCTR();

                targetRunP.rPr = copyRunP.rPr;
                targetRunP.rsidRPr = copyRun.GetCTR().rsidRPr;
                targetRunP.rsidR = copyRun.GetCTR().rsidR;
                targetRun.SetText(copyRun.Text);
            }
        }

        /// <summary>
        /// 根据变量路径获取值
        /// </summary>
        private (JToken, string format) GetPathValue(string pathKey, JToken loopValue = null)
        {
            if (pathKey is null)
            {
                return (null, null);
            }

            if (pathKey == ".")
            {
                return (loopValue, null);
            }

            var index = pathKey.IndexOf('.');
            var findex = pathKey.LastIndexOf(':');

            string varKey = pathKey, format = null;

            if (findex > 0)
            {
                format = pathKey.Substring(findex + 1).Trim();
                varKey = pathKey.Substring(0, findex).TrimEnd();
                if (varKey == ".")
                {
                    return (loopValue, format);
                }
            }

            JToken jret;

            if (index < 0)
            {
                jret = loopValue is JObject jobj && jobj.TryGetValue(varKey, out var jn1) ? jn1 : _varDic.GetVar(varKey);
            }
            else if (index == 0)
            {
                jret = JsonVarHelper.GetPropertyValue(loopValue, varKey.AsSpan().Slice(1).TrimStart());
            }
            else
            {
                var key1 = varKey[..index];
                jret = loopValue is JObject jobj && jobj.TryGetValue(key1, out var jn2) ? jn2 : _varDic.GetVar(key1);
                jret = JsonVarHelper.GetPropertyValue(jret, varKey.AsSpan()[(index + 1)..]);
            }

            return (jret, format);
        }

        #endregion

        #region----Parse Methods----

        private void ParseDocument(XWPFDocument doc, NpDocTemplateNode rootSection)
        {
            var curSection = rootSection;

            //解析成树状结构再进行处理
            for (var i = 0; i < doc.BodyElements.Count; i++)
            {
                if (doc.BodyElements[i] is XWPFParagraph paragraph)
                {
                    ParseParagraph(ref curSection, new NpDocData { Paragraph = paragraph, BodyIndex = i });
                }
                else if (doc.BodyElements[i] is XWPFTable table)
                {
                    ParseTable(curSection, table, i);
                }
            }

            if (curSection != rootSection)
            {
                throw new TemplateConvertException("区块未关闭:" + curSection.NodeText);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ParseParagraph(ref NpDocTemplateNode curSection, NpDocData data)
        {
            var matches = Regex.Matches(data.Paragraph.Text, @"\{\{(.+?)}}");

            if (matches.Count < 1)
            {
                return;
            }

            foreach (Match m in matches)
            {
                var nodeKey = m.Groups[1].Value.Trim();

                if (nodeKey.StartsWith("#if "))
                {
                    nodeKey = nodeKey.Substring(4).TrimStart();

                    curSection = curSection.BeginSection(NpDocSectionType.mif, nodeKey, nodeText: m.Value, data: data);
                }
                else if (nodeKey.StartsWith("/if "))
                {
                    nodeKey = nodeKey.Substring(4).TrimStart();

                    curSection.EndSection(NpDocSectionType.mif, nodeKey, nodeText: m.Value, data: data);

                    curSection = curSection.Parent;
                }
                else if (nodeKey.StartsWith("#loop "))
                {
                    nodeKey = nodeKey.Substring(6).TrimStart();

                    curSection = curSection.BeginSection(NpDocSectionType.loop, nodeKey, nodeText: m.Value, data: data);
                }
                else if (nodeKey.StartsWith("/loop "))
                {
                    nodeKey = nodeKey.Substring(6).TrimStart();

                    curSection.EndSection(NpDocSectionType.loop, nodeKey, nodeText: m.Value, data: data);

                    curSection = curSection.Parent;
                }
                else if (curSection.SectionType != NpDocSectionType.none)
                {
                    curSection.AppendChildNode(nodeKey, nodeText: m.Value, data: data);
                }
                else
                {
                    //直接做了替换

                    var (jtoken, format) = GetPathValue(nodeKey);

                    data.Paragraph.ReplaceText(m.Value, jtoken?.FormatValue(format));
                }
            }
        }

        //不支持嵌套表格
        private void ParseTable(NpDocTemplateNode curSection, XWPFTable table, int bodyIndex)
        {
            var data = new NpDocData
            {
                BodyIndex = bodyIndex
            };

            var tableSection = curSection.BeginSection(NpDocSectionType.table, nodeKey: null, nodeText: null, data: data);

            curSection = tableSection;

            for (var rIndex = 0; rIndex < table.Rows.Count; rIndex++)
            {
                var cells = table.Rows[rIndex].GetTableCells();

                for (var cIndex = 0; cIndex < cells.Count; cIndex++)
                {
                    var paras = cells[cIndex].Paragraphs;

                    for (var pIndex = 0; pIndex < paras.Count; pIndex++)
                    {
                        data = new NpDocData
                        {
                            BodyIndex = pIndex,
                            Paragraph = paras[pIndex],
                            TableCellPos = new[] { rIndex, cIndex }
                        };
                        ParseParagraph(ref curSection, data);
                    }
                }
            }

            //检查是否区块被关闭了
            if (curSection != tableSection)
            {
                throw new TemplateConvertException("表格中的区块未关闭:" + curSection.NodeText);
            }
        }

        #endregion

        /// <summary>
        /// npoi文档表格处理
        /// </summary>
        struct NpDocTableHandler
        {
            readonly XWPFTable _table;

            public NpDocTableHandler(XWPFTable table)
            {
                _table = table;
            }

            /// <summary>
            /// 处理表格内的循环区块
            /// </summary>
            public void HandleLoop(NpDocTemplateNode loopSection, JArray loopValues, Func<string, JToken, (JToken, string)> GetPathValue)
            {
                if (loopValues == null)
                {
                    RemoveLoopSectionFull(loopSection);
                }
                else
                {
                    RemoveLoopSectionBegin(loopSection);
                    RemoveLoopSectionEnd(loopSection);

                    var beginRowIndex = loopSection.NodeData.RowIndex;
                    var endRowIndex = loopSection.EndNode.NodeData.RowIndex;
                    var rowCount = endRowIndex - beginRowIndex + 1;

                    if (loopValues.Count > 1)
                    {
                        var copyRows = new CT_Row[rowCount * (loopValues.Count - 1)];
                        var copyRowIndex = 0;

                        for (var i = 1; i < loopValues.Count; i++)
                        {
                            for (var rIndex = beginRowIndex; rIndex <= endRowIndex; rIndex++)
                            {
                                var row = _table.Rows[rIndex];
                                copyRows[copyRowIndex++] = row.GetCTRow().Copy();
                            }
                        }

                        for (var rIndex = copyRows.Length - 1; rIndex >= 0; rIndex--)
                        {
                            var copyRow = new XWPFTableRow(copyRows[rIndex], _table);

                            _table.AddRow(copyRow, endRowIndex + 1);
                        }
                    }

                    var rowIncrement = 0;

                    foreach (var loopValue in loopValues)
                    {
                        foreach (var child in loopSection.Children)
                        {
                            var row = _table.Rows[child.NodeData.RowIndex + rowIncrement];
                            var cell = row.GetCell(child.NodeData.CellIndex);
                            var para = cell.Paragraphs[child.NodeData.BodyIndex];
                            var (jtoken, format) = GetPathValue(child.NodeKey, loopValue);
                            para.ReplaceText(child.NodeText, jtoken.FormatValue(format) ?? string.Empty);
                        }

                        rowIncrement += rowCount;
                    }
                }
            }

            /// <summary>
            /// 移除循环开始标识
            /// </summary>
            private void RemoveLoopSectionBegin(NpDocTemplateNode loopSection)
            {
                var beginData = loopSection.NodeData;

                beginData.Paragraph.ReplaceText(loopSection.NodeText, string.Empty);

                if (!string.IsNullOrWhiteSpace(beginData.Paragraph.Text))
                {
                    return;
                }

                var beginRow = _table.GetRow(beginData.RowIndex);
                var beginCell = beginRow.GetCell(beginData.CellIndex);

                beginCell.RemoveParagraph(beginData.BodyIndex);

                if (beginCell.Paragraphs.Count > 0)
                {
                    //将同一单元格中后续节点的段落索引值前移一位
                    foreach (var child in loopSection.Children)
                    {
                        if (child.NodeData.TableCellPos == beginData.TableCellPos)
                        {
                            child.NodeData.BodyIndex--;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //如果结束标识和开始标识在同一个单元格中
                    if (beginData.TableCellPos == loopSection.EndNode.NodeData.TableCellPos)
                    {
                        loopSection.EndNode.NodeData.BodyIndex--;
                    }
                }
                else
                {
                    beginCell.SetText(string.Empty);
                }
            }

            /// <summary>
            /// 移除循环结束标识
            /// </summary>
            private void RemoveLoopSectionEnd(NpDocTemplateNode loopSection)
            {
                var endData = loopSection.EndNode.NodeData;

                endData.Paragraph.ReplaceText(loopSection.EndNode.NodeText, string.Empty);

                if (string.IsNullOrWhiteSpace(endData.Paragraph.Text))
                {
                    var endRow = _table.GetRow(endData.RowIndex);
                    var endCell = endRow.GetCell(endData.CellIndex);

                    endCell.RemoveParagraph(endData.BodyIndex);

                    if (endCell.Paragraphs.Count < 1)
                    {
                        endCell.SetText(string.Empty);
                    }

                    //结束节点后面已经被预先处理了
                }
            }

            /// <summary>
            /// 移除表格内的循环区块,如果循环首尾行有外部数据存在则保留外部数据
            /// </summary>
            private void RemoveLoopSectionFull(NpDocTemplateNode loopSection)
            {
                var beginData = loopSection.NodeData;
                var endData = loopSection.EndNode.NodeData;

                var beginRowIndex = beginData.RowIndex;
                var endRowIndex = endData.RowIndex;

                //清理区块起始标识所在的单元格
                var beginRow = _table.GetRow(beginRowIndex);
                var beginCell = beginRow.GetCell(beginData.CellIndex);
                while (beginCell.Paragraphs.Count > beginData.BodyIndex)
                {
                    beginCell.RemoveParagraph(beginData.BodyIndex);
                }
                beginCell.SetText(string.Empty);

                //清理区块结束标识所在的单元格
                var endRow = _table.GetRow(endRowIndex);
                if (beginData.TableCellPos != endData.TableCellPos)
                {
                    var endCell = endRow.GetCell(endData.CellIndex);
                    for (var i = 0; i <= endData.BodyIndex; i++)
                    {
                        endCell.RemoveParagraph(0);
                    }
                    endCell.SetText(string.Empty);
                }

                //删除首行判断
                var cells = beginRow.GetTableCells();
                if (!IsRangeEmpty(cells, 0, beginData.CellIndex))
                {
                    //如果首行不为空则设置循环开始标识后的单元格为空值
                    var endCellIndex = beginRowIndex == endRowIndex ? endData.CellIndex : cells.Count;
                    ClearRange(cells, beginData.CellIndex + 1, endCellIndex - 1);
                    beginRowIndex += 1;
                }

                //删除尾行判断
                cells = endRow.GetTableCells();
                if (!IsRangeEmpty(cells, endData.CellIndex, cells.Count - 1))
                {
                    //如果尾行不为空则设置循环结束标识前的单元格为空值
                    if (beginRowIndex <= endRowIndex)
                    {
                        var beginCellIndex = beginRowIndex == endRowIndex ? beginData.CellIndex : 0;
                        ClearRange(cells, beginCellIndex + 1, endData.CellIndex - 1);
                    }
                    endRowIndex -= 1;
                }

                //执行删除
                for (var rIndex = beginRowIndex; rIndex <= endRowIndex; rIndex++)
                {
                    _table.RemoveRow(beginRowIndex);
                }
            }

            /// <summary>
            /// 检查从开始索引位置到结束索引位置的单元格是否都是空的
            /// </summary>
            public static bool IsRangeEmpty(List<XWPFTableCell> cells, int beginCellIndex, int endCellIndex)
            {
                for (var i = beginCellIndex; i <= endCellIndex; i++)
                {
                    if (!string.IsNullOrWhiteSpace(cells[i].GetText()))
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// 将开始索引位置到结束索引位置的单元格都置为空
            /// </summary>
            public static void ClearRange(List<XWPFTableCell> cells, int beginCellIndex, int endCellIndex)
            {
                for (var i = beginCellIndex; i <= endCellIndex; i++)
                {
                    while (cells[i].Paragraphs.Count > 0)
                    {
                        cells[i].RemoveParagraph(0);
                    }
                    cells[i].SetText(string.Empty);
                }
            }
        }
    }
}
