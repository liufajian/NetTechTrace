﻿using NPOI.XWPF.UserModel;
using OfficeLib.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace OfficeLib
{
    /// <summary>
    /// 
    /// </summary>
    public partial class NpoiDocTemplateConverter : TemplateConverter
    {
        public NpoiDocTemplateConverter()
        {

        }

        public void Convert(string templatePath, string outputFilePath)
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
                Convert(rs, outputFilePath);
            }
        }

        public void Convert(Stream templateStream, string outputFilePath)
        {
            if (templateStream is null)
            {
                throw new ArgumentNullException(nameof(templateStream));
            }

            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                throw new ArgumentNullException(nameof(outputFilePath));
            }

            var doc = new XWPFDocument(templateStream);
            var root = NpDocTemplateNode.CreateRootSection();

            ParseDocument(doc, root);

            var bodyIndexIncrement = 0;

            foreach (var section in root.Children)
            {
                if (section.SectionType == MySectionType.table)
                {
                    var table = (XWPFTable)doc.BodyElements[section.NodeData.BodyIndex + bodyIndexIncrement];

                    HandleTable(table, section);
                }
                else if (section.SectionType == MySectionType.mif)
                {
                    HandleBodyIf(doc, section, ref bodyIndexIncrement);
                }
            }

            using (var stream = File.OpenWrite(outputFilePath))
            {
                doc.Write(stream);
            }
        }

        #region----Convert Methods----

        private void HandleBodyIf(XWPFDocument doc, NpDocTemplateNode mifSection, ref int bodyIndexIncrement)
        {
            var value = GetPathValue(mifSection.NodeKey);

            var arr = JsonHelper.GetLoopArray(value);

            var beginIndex = mifSection.NodeData.BodyIndex;
            var endIndex = mifSection.EndNode.NodeData.BodyIndex;

            if (arr == null)
            {
                var removeIndex = beginIndex + bodyIndexIncrement;

                for (var i = beginIndex; i <= endIndex; i++)
                {
                    doc.RemoveBodyElement(removeIndex);
                }

                bodyIndexIncrement -= endIndex - beginIndex;
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
                            var jval = GetPathValue(item.NodeKey);
                            item.NodeData.Paragraph.ReplaceText(item.NodeText, jval?.ToString());
                        }
                        else //处理表格
                        {
                            var table = (XWPFTable)doc.BodyElements[item.NodeData.BodyIndex + bodyIndexIncrement];

                            HandleTable(table, item);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(beginPara.Text))
                {
                    doc.RemoveBodyElement(mifSection.NodeData.BodyIndex + bodyIndexIncrement);
                    bodyIndexIncrement--;
                }

                if (string.IsNullOrWhiteSpace(endPara.Text))
                {
                    doc.RemoveBodyElement(mifSection.EndNode.NodeData.BodyIndex + bodyIndexIncrement);
                    bodyIndexIncrement--;
                }
            }
        }

        private void HandleTable(XWPFTable table, NpDocTemplateNode tableSection)
        {
            if (!tableSection.HasChild)
            {
                return;
            }

            var rowIncrement = 0;

            NpDocTemplateNode loopSection = null;

            foreach (var child in tableSection.Children)
            {
                if (loopSection != null)
                {
                    if (loopSection.EndNode.NodeData.RowIndex != child.NodeData.RowIndex)
                    {
                        HandleTableLoop(table, loopSection, ref rowIncrement);

                        loopSection = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(child.SectionType != MySectionType.loop);
                    }
                }

                if (child.SectionType == MySectionType.loop)
                {
                    loopSection = child; //碰到循环后,先将循环结束标识后位于同一行的单元格处理掉
                }
                else
                {
                    var jval = GetPathValue(child.NodeKey);

                    child.NodeData.Paragraph.ReplaceText(child.NodeText, jval?.ToString());
                }
            }

            if (loopSection != null)
            {
                HandleTableLoop(table, loopSection, ref rowIncrement);
            }
        }

        private void HandleTableLoop(XWPFTable table, NpDocTemplateNode loopSection, ref int rowIncrement)
        {
            var beginData = loopSection.NodeData;
            var endData = loopSection.EndNode.NodeData;

            var beginRowIndex = beginData.RowIndex;
            var endRowIndex = endData.RowIndex;

            var beginPara = beginData.Paragraph;
            beginPara.ReplaceText(loopSection.NodeText, string.Empty);

            var endPara = endData.Paragraph;
            endPara.ReplaceText(loopSection.EndNode.NodeText, string.Empty);

            var jval = GetPathValue(loopSection.NodeKey);
            var loopArray = JsonHelper.GetLoopArray(jval);

            if (loopArray == null)
            {
                beginRowIndex += rowIncrement;
                endRowIndex += rowIncrement;

                //清理区块起始标识所在的单元格
                var beginRow = table.GetRow(beginRowIndex);
                var beginCell = beginRow.GetCell(beginData.CellIndex);
                while (beginCell.Paragraphs.Count > beginData.BodyIndex)
                {
                    beginCell.RemoveParagraph(beginData.BodyIndex);
                }
                beginCell.SetText(string.Empty);

                //清理区块结束标识所在的单元格
                var endRow = table.GetRow(endRowIndex);
                var endCell = endRow.GetCell(endData.CellIndex);
                for (var i = 0; i <= endData.BodyIndex; i++)
                {
                    endCell.RemoveParagraph(0);
                }
                endCell.SetText(string.Empty);

                //删除首行判断
                var cells = beginRow.GetTableCells();
                if (!isRangeEmpty(cells, 0, beginData.CellIndex))
                {
                    //如果首行不为空则设置循环开始标识后的单元格为空值
                    var endCellIndex = beginRowIndex == endRowIndex ? endData.CellIndex : cells.Count;
                    clearRange(cells, beginData.CellIndex + 1, endCellIndex - 1);
                    beginRowIndex += 1;
                }

                //删除尾行判断
                cells = endRow.GetTableCells();
                if (!isRangeEmpty(cells, endData.CellIndex, cells.Count - 1))
                {
                    //如果尾行不为空则设置循环结束标识前的单元格为空值
                    if (beginRowIndex <= endRowIndex)
                    {
                        var beginCellIndex = beginRowIndex == endRowIndex ? beginData.CellIndex : 0;
                        clearRange(cells, beginCellIndex + 1, endData.CellIndex - 1);
                    }
                    endRowIndex -= 1;
                }

                //执行删除
                for (var rIndex = beginRowIndex; rIndex <= endRowIndex; rIndex++)
                {
                    table.RemoveRow(beginRowIndex);

                    rowIncrement--;
                }
            }
            else
            {
                foreach (var loopValue in loopArray)
                {
                    var rowIncrement2 = rowIncrement;

                    for (var rIndex = beginRowIndex; rIndex <= endRowIndex; rIndex++)
                    {
                        var row = table.Rows[rowIncrement + rIndex];
                        var copiedRow = new XWPFTableRow(row.GetCTRow().Copy(), table);
                        table.AddRow(copiedRow, rowIncrement2 + rIndex);
                        rowIncrement++;
                    }

                    foreach (var child in loopSection.Children)
                    {
                        var row = table.Rows[child.NodeData.RowIndex + rowIncrement2];
                        var cell = row.GetCell(child.NodeData.CellIndex);
                        var para = cell.Paragraphs[child.NodeData.BodyIndex];
                        var jstr = GetPathValue(child.NodeKey, loopValue)?.ToString();
                        para.ReplaceText(child.NodeText, jstr ?? string.Empty);
                    }
                }
            }

            bool isRangeEmpty(List<XWPFTableCell> cells, int beginCellIndex, int endCellIndex)
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

            void clearRange(List<XWPFTableCell> cells, int beginCellIndex, int endCellIndex)
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
        private JsonValue GetPathValue(string pathKey, JsonValue loopValue = null)
        {
            if (pathKey is null)
            {
                return null;
            }

            if (pathKey == ".")
            {
                return loopValue;
            }

            var index = pathKey.IndexOf('.');

            if (index < 0)
            {
                return loopValue is JsonObject jobj1 && jobj1.TryGetValue(pathKey, out var jn1) ? jn1 : base.GetVarValue(pathKey);
            }

            if (index == 0)
            {
                return JsonHelper.GetPropertyValue(loopValue, pathKey.AsSpan().Slice(1).TrimStart());
            }

            var key1 = pathKey[..index];
            var jval = loopValue is JsonObject jobj2 && jobj2.TryGetValue(key1, out var jn2) ? jn2 : base.GetVarValue(key1);
            return JsonHelper.GetPropertyValue(jval, pathKey.AsSpan()[(index + 1)..]);
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
                    ParseParagraph(ref curSection, new NpData { Paragraph = paragraph, BodyIndex = i });
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
        private void ParseParagraph(ref NpDocTemplateNode curSection, NpData data)
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

                    curSection = curSection.BeginSection(MySectionType.mif, nodeKey, nodeText: m.Value, data: data);
                }
                else if (nodeKey.StartsWith("/if "))
                {
                    nodeKey = nodeKey.Substring(4).TrimStart();

                    curSection.EndSection(MySectionType.mif, nodeKey, nodeText: m.Value, data: data);

                    curSection = curSection.Parent;
                }
                else if (nodeKey.StartsWith("#loop "))
                {
                    nodeKey = nodeKey.Substring(6).TrimStart();

                    curSection = curSection.BeginSection(MySectionType.loop, nodeKey, nodeText: m.Value, data: data);
                }
                else if (nodeKey.StartsWith("/loop "))
                {
                    nodeKey = nodeKey.Substring(6).TrimStart();

                    curSection.EndSection(MySectionType.loop, nodeKey, nodeText: m.Value, data: data);

                    curSection = curSection.Parent;
                }
                else if (curSection.SectionType != MySectionType.none)
                {
                    curSection.AppendChildNode(nodeKey, nodeText: m.Value, data: data);
                }
                else
                {
                    //直接做了替换

                    var jval = GetPathValue(nodeKey);

                    data.Paragraph.ReplaceText(m.Value, jval?.ToString());
                }
            }
        }

        //不支持嵌套表格
        private void ParseTable(NpDocTemplateNode curSection, XWPFTable table, int bodyIndex)
        {
            var data = new NpData
            {
                BodyIndex = bodyIndex
            };

            var tableSection = curSection.BeginSection(MySectionType.table, nodeKey: null, nodeText: null, data: data);

            curSection = tableSection;

            for (var rIndex = 0; rIndex < table.Rows.Count; rIndex++)
            {
                var cells = table.Rows[rIndex].GetTableCells();

                for (var cIndex = 0; cIndex < cells.Count; cIndex++)
                {
                    var paras = cells[cIndex].Paragraphs;

                    for (var pIndex = 0; pIndex < paras.Count; pIndex++)
                    {
                        data = new NpData
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
    }
}
