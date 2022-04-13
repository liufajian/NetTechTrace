using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace OfficeLib
{
    /// <summary>
    /// 
    /// </summary>
    public class NpoiDocTemplateConverter : TemplateConverter
    {
        readonly JsonNode _loopValue;

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
            var root = TemplateNode.CreateRootSection();

            ParseDocument(doc, root);

            foreach (var section in root.Children)
            {
                if (section.SectionType == MySectionType.mif)
                {
                    HandleIf(doc, section);
                }
            }
        }

        #region----Convert Methods----

        private void HandleIf(XWPFDocument doc, TemplateNode sectionIf)
        {
            var value = GetJsonNode(sectionIf.NodeKey);

            var arr = JsonNodeHelper.GetLoopArray(value);

            if (arr == null)
            {
            }
        }

        private void HandleTableLoop(XWPFTable table, TemplateNode sectionLoop, ref int rowIncrement)
        {
            //System.Diagnostics.Debug.Assert(sectionLoop.NodeType == MyNodeType.loop);

            //var loopArray = GetLoopArray(sectionLoop.NodeKey);

            //var closeNode = sectionLoop.LastChild();

            //if (RemoveTableSection(table, sectionLoop, ref rowIncrement))
            //{

            //}

            //if (sectionLoop.TableInfo.Cell == closeNode.TableInfo.Cell)
            //{
            //    var curNode = openNode;

            //    foreach (var loopValue in loopArray)
            //    {
            //        _sectionManager.CurrentValue = loopValue;

            //        while (curNode != null)
            //        {
            //            if (curNode.Paragraph != null)
            //            {
            //                var newp = openNode.TableCell.AddParagraph();

            //                CopyParagraph(curNode.Paragraph, newp);

            //                var str = GetString(curNode.NodeKey);

            //                newp.ReplaceText(curNode.NodeText, str);
            //            }
            //            curNode = curNode.Next;
            //        }
            //    }
            //}
            //else
            //{
            //    var firstIndex = openNode.RowIndex + rowIncrement;

            //    var curNode = openNode;

            //    while (curNode != null)
            //    {
            //        curNode = curNode.Next;
            //    }

            //    foreach (var loopValue in loopArray)
            //    {
            //        _sectionManager.CurrentValue = loopValue;


            //        //XWPFTableRow copiedRow1 = new XWPFTableRow(row.GetCTRow().Copy(), table);
            //        //copiedRow1.GetCell(4).SetText("biubiub");
            //        //table.AddRow(copiedRow1, 1);
            //    }
            //}
        }

        private bool RemoveTableSection(XWPFTable table, TemplateNode node, ref int rowIncrement)
        {
            //node.Paragraph.ReplaceText(node.NodeText, string.Empty);

            //if (node.TableInfo.Row.GetTableCells().All(n => string.IsNullOrWhiteSpace(n.GetText())))
            //{
            //    table.RemoveRow(node.TableInfo.RowIndex + rowIncrement);
            //    rowIncrement--;
            //    return true;
            //}

            return false;
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

            for (int r = 0; r < sourcePara.Runs.Count; r++)
            {
                XWPFRun copyRun = sourcePara.Runs[r];

                var copyRunP = copyRun.GetCTR();

                var targetRun = targetPara.CreateRun();
                var targetRunP = targetRun.GetCTR();

                targetRunP.rPr = copyRunP.rPr;
                targetRunP.rsidRPr = copyRun.GetCTR().rsidRPr;
                targetRunP.rsidR = copyRun.GetCTR().rsidR;
                targetRun.SetText(copyRun.Text);
            }
        }

        protected override JsonNode GetJsonNode(string pathKey)
        {
            if (pathKey is null)
            {
                return null;
            }

            if (pathKey == ".")
            {
                return _loopValue;
            }

            var index = pathKey.IndexOf('.');

            if (index < 0)
            {
                return _loopValue is JsonObject jobj1 && jobj1.TryGetPropertyValue(pathKey, out var jn1) ? jn1 : base.GetJsonNode(pathKey);
            }

            if (index == 0)
            {
                return JsonNodeHelper.GetValue(_loopValue, pathKey.AsSpan().Slice(1).TrimStart());
            }

            var key1 = pathKey[..index];
            var jnode = _loopValue is JsonObject jobj2 && jobj2.TryGetPropertyValue(key1, out var jn2) ? jn2 : base.GetJsonNode(key1);
            return JsonNodeHelper.GetValue(jnode, pathKey.AsSpan()[(index + 1)..]);
        }

        #endregion

        #region----Parse Methods----

        private void ParseDocument(XWPFDocument doc, TemplateNode rootSection)
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
        private void ParseParagraph(ref TemplateNode curSection, NpData data)
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

                    curSection = curSection.OpenSection(MySectionType.mif, nodeKey, nodeText: m.Value, data: data);
                }
                else if (nodeKey.StartsWith("/if "))
                {
                    nodeKey = nodeKey.Substring(4).TrimStart();

                    curSection.CloseSection(MySectionType.mif, nodeKey, nodeText: m.Value, data: data);

                    curSection = curSection.Parent;
                }
                else if (nodeKey.StartsWith("#loop "))
                {
                    nodeKey = nodeKey.Substring(6).TrimStart();

                    curSection = curSection.OpenSection(MySectionType.loop, nodeKey, nodeText: m.Value, data: data);
                }
                else if (nodeKey.StartsWith("/loop "))
                {
                    nodeKey = nodeKey.Substring(6).TrimStart();

                    curSection.CloseSection(MySectionType.loop, nodeKey, nodeText: m.Value, data: data);

                    curSection = curSection.Parent;
                }
                else if (curSection.SectionType != MySectionType.none)
                {
                    curSection.AppendChild(nodeKey, nodeText: m.Value, data: data);
                }
                else
                {
                    //直接做了替换

                    var replacement = JsonNodeHelper.GetString(GetJsonNode(nodeKey));

                    data.Paragraph.ReplaceText(m.Value, replacement);
                }
            }
        }

        //不支持嵌套表格
        private void ParseTable(TemplateNode curSection, XWPFTable table, int bodyIndex)
        {
            var data = new NpData
            {
                BodyIndex = bodyIndex,
                Table = new NpTableWrap(table)
            };

            var tableSection = curSection.OpenSection(MySectionType.table, nodeKey: null, nodeText: null, data: data);

            curSection = tableSection;

            for (var rIndex = 0; rIndex < table.Rows.Count; rIndex++)
            {
                var cells = table.Rows[rIndex].GetTableCells();

                foreach (var cell in cells)
                {
                    var paras = cell.Paragraphs.ToArray();

                    for (var pIndex = 0; pIndex < cell.Paragraphs.Count; pIndex++)
                    {
                        data = new NpData
                        {
                            BodyIndex = pIndex,
                            Paragraph = cell.Paragraphs[pIndex],
                            Table = new NpTableWrap(table)
                            {
                                Cell = cell,
                                RowIndex = rIndex,
                                Row = table.Rows[rIndex]
                            }
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

        #region----Inner Class----

        class NpTableWrap
        {
            public NpTableWrap(XWPFTable table)
            {
                Table = table ?? throw new ArgumentNullException(nameof(table));
            }

            public XWPFTable Table { get; }

            public XWPFTableRow Row { get; set; }

            public int RowIndex { get; set; }

            public XWPFTableCell Cell { get; set; }
        }

        class NpData
        {
            public int BodyIndex { get; set; }

            public XWPFParagraph Paragraph { get; set; }

            public NpTableWrap Table { get; set; }
        }

        enum MySectionType { none, root, table, mif, loop }

        enum MyNodeType { none, closeIf, closeLoop }

        class TemplateNode
        {
            List<TemplateNode> _children;

            private TemplateNode(MySectionType sectionType)
            {
                SectionType = sectionType;
            }

            public TemplateNode(MySectionType sectionType, MyNodeType nodeType, string nodeKey, string nodeText, NpData data)
            {
                NodeKey = nodeKey;
                NodeText = nodeText;
                NodeType = nodeType;

                SectionType = sectionType;

                NodeData = data ?? throw new ArgumentNullException(nameof(data));
            }

            /// <summary>
            /// 区块类型
            /// </summary>
            public MySectionType SectionType { get; }

            /// <summary>
            /// 节点类型
            /// </summary>
            public MyNodeType NodeType { get; }

            /// <summary>
            /// 
            /// </summary>
            public string NodeKey { get; }

            /// <summary>
            /// 
            /// </summary>
            public string NodeText { get; }

            /// <summary>
            /// NPOI数据
            /// </summary>
            public NpData NodeData { get; }

            /// <summary>
            /// 父节点
            /// </summary>
            public TemplateNode Parent { get; private set; }

            /// <summary>
            /// 是否有子节点存在
            /// </summary>
            public bool HasChild => _children != null && _children.Count > 0;

            /// <summary>
            /// 子节点集合
            /// </summary>
            public IEnumerable<TemplateNode> Children => _children;

            /// <summary>
            /// 最后的子节点
            /// </summary>
            public TemplateNode LastChild()
            {
                return HasChild ? _children[^1] : null;
            }

            /// <summary>
            /// 
            /// </summary>
            public bool RemoveChild(TemplateNode child)
            {
                return HasChild && _children.Remove(child);
            }

            private TemplateNode AppendChild(MySectionType sectionType, MyNodeType nodeType, string nodeKey, string nodeText, NpData data)
            {
                var child = new TemplateNode(sectionType, nodeType, nodeKey, nodeText, data)
                {
                    Parent = this
                };

                if (_children == null)
                {
                    _children = new List<TemplateNode>();
                }

                _children.Add(child);

                return child;
            }

            /// <summary>
            /// 
            /// </summary>
            public TemplateNode OpenSection(MySectionType openType, string nodeKey, string nodeText, NpData data)
            {
                if (openType == MySectionType.mif)
                {
                    if (data?.Table != null)
                    {
                        throw new TemplateConvertException("暂时不支持在表格中的使用if:" + nodeKey);
                    }
                }
                else if (openType == MySectionType.loop)
                {
                    //检查附加循环子节点

                    if (data?.Table == null)
                    {
                        throw new TemplateConvertException("暂时仅支持表格中的循环:" + nodeText);
                    }

                    if (SectionType != MySectionType.table)
                    {
                        throw new TemplateConvertException("表格中不支持嵌套循环:" + nodeText);
                    }

                    //检查是否存在表中同一行的循环

                    if (_children != null)
                    {
                        for (var i = _children.Count - 1; i >= 0; i++)
                        {
                            if (_children[i].NodeData.Table.RowIndex != NodeData.Table.RowIndex)
                            {
                                break;
                            }

                            if (_children[i].SectionType == MySectionType.loop && !_children[i].IsLoopInSameCell())
                            {
                                throw new TemplateConvertException($"表格中同一行内不支持多个跨单元格循环:{_children[i].LastChild().NodeText},{nodeText}");
                            }
                        }
                    }
                }
                else if (openType != MySectionType.table)
                {
                    throw new InvalidOperationException();
                }

                return AppendChild(openType, MyNodeType.none, nodeKey, nodeText, data);
            }

            public TemplateNode CloseSection(MySectionType closeType, string nodeKey, string nodeText, NpData data)
            {
                if (SectionType == MySectionType.none)
                {
                    throw new InvalidOperationException();
                }

                if (closeType == MySectionType.mif)
                {
                    if (data?.Table != null)
                    {
                        throw new TemplateConvertException("暂时不支持在表格中使用:" + nodeText);
                    }

                    if (SectionType != MySectionType.mif)
                    {
                        throw new TemplateConvertException("没有找到要关闭的区块:" + nodeText);
                    }

                    if (NodeKey != nodeKey)
                    {
                        throw new TemplateConvertException($"执行 {nodeText} 之前请先关闭 {NodeText}");
                    }

                    return AppendChild(MySectionType.none, MyNodeType.closeIf, nodeKey, nodeText, data);
                }

                if (closeType == MySectionType.loop)
                {
                    if (data?.Table == null)
                    {
                        throw new TemplateConvertException("暂时仅支持表格中的循环:" + nodeText);
                    }

                    if (SectionType != MySectionType.loop)
                    {
                        throw new TemplateConvertException("没有找到要关闭的区块:" + nodeText);
                    }

                    if (NodeKey != nodeKey)
                    {
                        throw new TemplateConvertException($"执行 {nodeText} 之前请先关闭 {NodeText}");
                    }

                    return AppendChild(MySectionType.none, MyNodeType.closeLoop, nodeKey, nodeText, data);
                }

                if (closeType == MySectionType.table)
                {

                }

                return this;
            }

            /// <summary>
            /// 附加子节点
            /// </summary>
            public TemplateNode AppendChild(string nodeKey, string nodeText, NpData data)
            {
                if (SectionType == MySectionType.none)
                {
                    throw new InvalidOperationException();
                }

                return AppendChild(MySectionType.none, MyNodeType.none, nodeKey, nodeText, data);
            }

            //循环是否在同一个单元格中开始并关闭
            private bool IsLoopInSameCell()
            {
                if (SectionType == MySectionType.loop)
                {
                    var last = LastChild();

                    if (last != null)
                    {
                        return last.NodeType == MyNodeType.closeLoop && NodeData.Table.Cell == last.NodeData.Table.Cell;
                    }
                }

                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            public static TemplateNode CreateRootSection()
            {
                return new TemplateNode(MySectionType.root);
            }
        }

        #endregion
    }
}
