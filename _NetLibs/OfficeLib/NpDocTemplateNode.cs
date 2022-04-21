using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;

namespace OfficeLib
{
    public partial class NpoiDocTemplateConverter
    {
        enum MySectionType { none, root, table, mif, loop }

        class NpData
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
        }

        class NpDocTemplateNode
        {
            List<NpDocTemplateNode> _children;

            private NpDocTemplateNode(MySectionType sectionType)
            {
                SectionType = sectionType;
                NodeData = new NpData();
            }

            public NpDocTemplateNode(MySectionType sectionType, string nodeKey, string nodeText, NpData data)
            {
                NodeKey = nodeKey;
                NodeText = nodeText;

                SectionType = sectionType;

                NodeData = data ?? throw new ArgumentNullException(nameof(data));
            }

            /// <summary>
            /// 区块类型
            /// </summary>
            public MySectionType SectionType { get; }

            /// <summary>
            /// 节点Key
            /// </summary>
            public string NodeKey { get; }

            /// <summary>
            /// 节点文本
            /// </summary>
            public string NodeText { get; }

            /// <summary>
            /// NPOI数据
            /// </summary>
            public NpData NodeData { get; }

            /// <summary>
            /// 闭合节点
            /// </summary>
            public NpDocTemplateNode EndNode { get; private set; }

            /// <summary>
            /// 父节点
            /// </summary>
            public NpDocTemplateNode Parent { get; private set; }

            /// <summary>
            /// 是否有子节点存在
            /// </summary>
            public bool HasChild => _children != null && _children.Count > 0;

            /// <summary>
            /// 子节点集合
            /// </summary>
            public IEnumerable<NpDocTemplateNode> Children => _children;

            /// <summary>
            /// 
            /// </summary>
            public bool RemoveChild(NpDocTemplateNode child)
            {
                return HasChild && _children.Remove(child);
            }

            private NpDocTemplateNode AppendChild(MySectionType sectionType, string nodeKey, string nodeText, NpData data)
            {
                var child = new NpDocTemplateNode(sectionType, nodeKey, nodeText, data)
                {
                    Parent = this
                };

                if (_children == null)
                {
                    _children = new List<NpDocTemplateNode>();
                }

                _children.Add(child);

                return child;
            }

            /// <summary>
            /// 开始区块
            /// </summary>
            public NpDocTemplateNode BeginSection(MySectionType openType, string nodeKey, string nodeText, NpData data)
            {
                if (openType == MySectionType.mif)
                {
                    if (data?.TableCellPos != null)
                    {
                        throw new TemplateConvertException("暂时不支持在表格中的使用if:" + nodeKey);
                    }
                }
                else if (openType == MySectionType.loop)
                {
                    //检查附加循环子节点

                    if (data?.TableCellPos == null)
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
                        for (var i = _children.Count - 1; i >= 0; i--)
                        {
                            if (_children[i].NodeData.RowIndex != data.RowIndex)
                            {
                                break;
                            }

                            if (_children[i].SectionType == MySectionType.loop)
                            {
                                throw new TemplateConvertException($"表格中同一行内不支持多个循环:{_children[i].EndNode.NodeText},{nodeText}");
                            }
                        }
                    }
                }
                else if (openType != MySectionType.table)
                {
                    throw new InvalidOperationException();
                }

                return AppendChild(openType, nodeKey, nodeText, data);
            }

            /// <summary>
            /// 结束区块
            /// </summary>
            public NpDocTemplateNode EndSection(MySectionType sectionType, string nodeKey, string nodeText, NpData data)
            {
                if (SectionType == MySectionType.none)
                {
                    throw new InvalidOperationException();
                }

                if (sectionType == MySectionType.mif)
                {
                    if (data?.TableCellPos != null)
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

                    return EndNode = new NpDocTemplateNode(MySectionType.none, nodeKey, nodeText, data);
                }

                if (sectionType == MySectionType.loop)
                {
                    if (data?.TableCellPos == null)
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

                    return EndNode = new NpDocTemplateNode(MySectionType.none, nodeKey, nodeText, data);
                }

                if (sectionType == MySectionType.table)
                {

                }

                return this;
            }

            /// <summary>
            /// 附加子节点
            /// </summary>
            public NpDocTemplateNode AppendChildNode(string nodeKey, string nodeText, NpData data)
            {
                if (SectionType == MySectionType.none)
                {
                    throw new InvalidOperationException();
                }

                return AppendChild(MySectionType.none, nodeKey, nodeText, data);
            }

            /// <summary>
            /// 
            /// </summary>
            public static NpDocTemplateNode CreateRootSection()
            {
                return new NpDocTemplateNode(MySectionType.root);
            }

            public override string ToString()
            {
                return NodeText;
            }
        }
    }
}
