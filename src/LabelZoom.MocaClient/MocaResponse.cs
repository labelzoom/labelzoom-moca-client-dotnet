using System;
using System.Data;
using System.Globalization;
using System.Xml;

namespace LabelZoom.MocaClient
{
    public class MocaResponse
    {
        public bool IsError => StatusCode != 0;

        public int StatusCode { get; private set; }

        public string? StatusMessage { get; private set; }

        public DataTable? ResponseData { get; private set; }

        public static MocaResponse FromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return new MocaResponse()
            {
                StatusCode = int.Parse(doc.SelectSingleNode("/moca-response/status").InnerText),
                StatusMessage = doc.SelectSingleNode("/moca-response/message")?.InnerText,
                ResponseData = XmlToDataTable(doc.SelectSingleNode("/moca-response/moca-results")),
            };
        }

        private static DataTable? XmlToDataTable(XmlNode mocaResults)
        {
            if (mocaResults != null)
            {
                DataTable dt = new DataTable();
                XmlNode metadataNode = mocaResults.SelectSingleNode("metadata");
                foreach (XmlNode column in metadataNode.ChildNodes)
                {
                    string colnam = column.Attributes["name"].Value;
                    // Prevent duplicate columns by appending sequence number to the end
                    if (dt.Columns.Contains(colnam))
                    {
                        int i = 0;
                        while (dt.Columns.Contains($"{colnam}_{i}"))
                        {
                            i++;
                        }
                        colnam = $"{colnam}_{i}";
                    }
                    DataColumn newColumn = new DataColumn(colnam);

                    XmlAttribute nullable = column.Attributes["nullable"];
                    if (nullable != null && bool.Parse(nullable.Value))
                    {
                        newColumn.AllowDBNull = true;
                    }

                    string mocaType = column.Attributes["type"].Value;
                    switch (mocaType)
                    {
                        case "O":
                            newColumn.DataType = typeof(bool);
                            break;
                        case "I":
                        case "P":
                            newColumn.DataType = typeof(int);
                            break;
                        case "F":
                        case "X":
                            newColumn.DataType = typeof(double);
                            break;
                        case "D":
                            newColumn.DataType = typeof(DateTime);
                            break;
                        case "S":
                        case "Z":
                            newColumn.DataType = typeof(string);
                            break;
                        case "V":
                            newColumn.DataType = typeof(byte[]);
                            break;
                        case "R":
                            newColumn.DataType = typeof(DataTable);
                            break;
                        case "J":
                            newColumn.DataType = typeof(object);
                            break;
                    }

                    dt.Columns.Add(newColumn);
                }

                XmlNode dataNode = mocaResults.SelectSingleNode("data");
                foreach (XmlNode row in dataNode.ChildNodes)
                {
                    DataRow newRow = dt.NewRow();
                    int fieldNumber = 0;
                    foreach (XmlNode field in row.ChildNodes)
                    {
                        XmlAttribute isNull = field.Attributes["null"];
                        if (isNull != null && bool.Parse(isNull.Value))
                        {
                            newRow.SetField<string?>(fieldNumber, null);
                        }
                        else
                        {
                            Type columnType = dt.Columns[fieldNumber].DataType;
                            if (columnType == typeof(bool))
                            {
                                newRow.SetField(fieldNumber, "1".Equals(field.InnerText) ? true : false);
                            }
                            else if (columnType == typeof(double))
                            {
                                newRow.SetField(fieldNumber, double.Parse(field.InnerText));
                            }
                            else if (columnType == typeof(int))
                            {
                                newRow.SetField(fieldNumber, int.Parse(field.InnerText));
                            }
                            else if (columnType == typeof(DataTable))
                            {
                                newRow.SetField(fieldNumber, XmlToDataTable(field.SelectSingleNode("moca-results")));
                            }
                            else if (columnType == typeof(DateTime))
                            {
                                newRow.SetField(fieldNumber, DateTime.ParseExact(field.InnerText, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                newRow.SetField(fieldNumber, field.InnerText);
                            }
                        }
                        fieldNumber++;
                    }
                    dt.Rows.Add(newRow);
                }
                return dt;
            }
            return null;
        }
    }
}
