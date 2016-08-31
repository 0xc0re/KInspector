using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using NPOI.XWPF.UserModel;

namespace Kentico.KInspector.Modules
{
    public static class ExportDocxExtensions
    {
        public static XWPFRun CreateRun(this XWPFParagraph paragraph, params string[] data)
        {
            if (paragraph == null)
            {
                throw new ArgumentNullException(nameof(paragraph));
            }

            var run = paragraph.CreateRun();
            run.SetText(string.Join(" ", data));

            return run;
        }

        public static XWPFParagraph CreateParagraph(this XWPFDocument document, params string[] data)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var par = document.CreateParagraph();

            foreach (string val in data)
            {
                par.CreateRun(val);
            }

            return par;
        }

        public static XWPFTableRow FillRow(this XWPFTableRow row, params string[] data)
        {
            return row.FillRow(data as IEnumerable<string>);
        }

        public static XWPFTableRow FillRow(this XWPFTableRow row, params object[] data)
        {
            return row.FillRow(data as IEnumerable<object>);
        }

        public static XWPFTableRow FillRow<T>(this XWPFTableRow row, IEnumerable<T> data)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            int ii = 0;
            foreach (var val in data)
            {
                var cell = row.GetCell(ii) ?? row.AddNewTableCell();
                cell.SetText(Convert.ToString(val));

                ii++;
            }

            return row;
        }

        public static XWPFTable FillRows(this XWPFTable table, DataTable data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Insert table headers
            table.GetRow(0).FillRow(data.Columns.Cast<DataColumn>().Select(column => column.ColumnName));

            // Insert row data
            int ii = 1;
            foreach (DataRow dataRow in data.Rows)
            {
                var row = table.GetRow(ii) ?? table.CreateRow();
                row.FillRow(dataRow.ItemArray);

                ii++;
            }

            return table;
        }

        public static XWPFTable FillTable<T>(this XWPFTable table, IEnumerable<T> data)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            int ii = 0;
            foreach (var val in data)
            {
                // Get or create table row
                var row = table.GetRow(ii) ?? table.CreateRow();

                row.FillRow(Convert.ToString(val));

                ii++;
            }

            return table;
        }

        #region ReplaceText

        public static XWPFDocument ReplaceText(this XWPFDocument document, string oldValue, string newValue)
        {
            document.Paragraphs
                .Concat(document.FooterList.SelectMany(footer => footer.Paragraphs))
                .Concat(document.HeaderList.SelectMany(header => header.Paragraphs))
                .Where(paragraph => paragraph.Text.Contains(oldValue))
                .ToList()
                .ForEach(paragraph => paragraph.ReplaceText(oldValue, newValue));

            document.Tables
                .Concat(document.FooterList.SelectMany(footer => footer.Tables))
                .Concat(document.HeaderList.SelectMany(header => header.Tables))
                .Where(table => table.Text.Contains(oldValue))
                .ToList()
                .ForEach(table => table.ReplaceText(oldValue, newValue));

            return document;
        }

        public static XWPFTable ReplaceText(this XWPFTable table, string oldValue, string newValue)
        {
            table.Rows.ForEach(row => row.ReplaceText(oldValue, newValue));

            return table;
        }

        public static XWPFTableRow ReplaceText(this XWPFTableRow row, string oldValue, string newValue)
        {
            row.GetTableCells()
                .SelectMany(cell => cell.Paragraphs)
                .Where(paragraph => paragraph.Text.Contains(oldValue))
                .ToList()
                .ForEach(paragraph => paragraph.ReplaceText(oldValue, newValue));

            return row;
        }

        #endregion
    }
}