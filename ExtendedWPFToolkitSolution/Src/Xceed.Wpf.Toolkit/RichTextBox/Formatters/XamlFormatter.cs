/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using StdHelpers;

namespace Xceed.Wpf.Toolkit
{
    /// <summary>
    /// Formats the RichTextBox text as Xaml
    /// </summary>
    public class XamlFormatter : ITextFormatter
    {
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return ASCIIEncoding.Default.GetString(ms.ToArray());
            }
        }

        //public void SetText(System.Windows.Documents.FlowDocument document, string text)
        //{
        //    try
        //    {
        //        //if the text is null/empty clear the contents of the RTB. If you were to pass a null/empty string
        //        //to the TextRange.Load method an exception would occur.
        //        if (String.IsNullOrEmpty(text))
        //        {
        //            document.Blocks.Clear();
        //        }
        //        else
        //        {
        //            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);

        //            var sb = new StringBuilder();
        //            sb.Append(text);
        //            sb.Replace("&", "&amp;");

        //            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString())))
        //            {
        //                try
        //                {
        //                    tr.Load(ms, DataFormats.Xaml);
        //                }
        //                catch (System.Windows.Markup.XamlParseException ex)
        //                {
        //                    StdHelpers.StdHelpersLogger.WriteLog(ex.ToSummery());
        //                    tr.Load(ms, DataFormats.Text);
        //                }

        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw new InvalidDataException("Data provided is not in the correct Xaml format.");
        //    }
        //}

        public void SetText(FlowDocument document, string text)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    document.Blocks.Clear();
                    return;
                }

                TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);

                // Do NOT pre-escape (& -> &amp;) here; let the chosen DataFormat handle it.
                byte[] buffer = Encoding.UTF8.GetBytes(text);

                using (var ms = new MemoryStream(buffer, writable: false))
                {
                    try
                    {
                        // Only attempt XAML parsing if the payload actually looks like XAML.
                        if (LooksLikeXaml(text))
                        {
                            tr.Load(ms, DataFormats.Xaml);
                        }
                        else
                        {
                            // Treat as plain text (handles \r\n correctly).
                            tr.Load(ms, DataFormats.Text);
                        }
                    }
                    catch (XamlParseException ex)
                    {
                        // Your log call stays the same.
                        StdHelpers.StdHelpersLogger.WriteLog(ex.ToSummery());

                        // Critical: rewind before retrying with a different format.
                        ms.Position = 0;
                        tr.Load(ms, DataFormats.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                // Preserve original exception as InnerException for diagnostics.
                throw new InvalidDataException("Data provided is not in the correct Xaml format.", ex);
            }
        }

        private static bool LooksLikeXaml(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            foreach (var ch in s)
            {
                if (!char.IsWhiteSpace(ch))
                    return ch == '<'; // quick heuristic: XAML starts with a tag
            }
            return false;
        }
    }
}
