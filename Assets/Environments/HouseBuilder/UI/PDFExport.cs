using HouseBuilder;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class PDFExport
{
    private class PdfReportPageEventHandler : PdfPageEventHelper
    {
        private Image m_ImageHeader;
        private string m_TextHeader;
        private string m_FooterSignature;
        private int m_MaxPageNumber;

        // We use a __single__ Image instance that's assigned __once__;
        // the image bytes added **ONCE** to the PDF file. If you create
        // separate Image instances in OnEndPage()/OnEndPage(), for example,
        // you'll end up with a much bigger file size.
        public Image ImageHeader
        {
            get { return m_ImageHeader; }
            set { m_ImageHeader = value; }
        }

        public string TextHeader
        {
            get { return m_TextHeader; }
            set { m_TextHeader = value; }
        }

        public string SignatureFooter
        {
            get { return m_FooterSignature; }
            set { m_FooterSignature = value; }
        }

        public int MaxPageNumber
        {
            get { return m_MaxPageNumber; }
            set { m_MaxPageNumber = value; }
        }
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            // cell height
            float cellHeight = MARGIN_TOP - MARGIN_HEADER_TOP;
            // PDF document size
            iTextSharp.text.Rectangle page = document.PageSize;

            // create two column table
            PdfPTable head = new PdfPTable(2);
            head.TotalWidth = page.Width - MARGIN_LEFT - MARGIN_RIGHT;

            // add the header text
            PdfPCell cell = new PdfPCell(new Phrase(m_TextHeader, LARGE_FONT_BOLD));
            cell.Border = PdfPCell.NO_BORDER;
            cell.VerticalAlignment = Element.ALIGN_BOTTOM;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.FixedHeight = cellHeight;
            head.AddCell(cell);

            // add image; PdfPCell() overload sizes image to fit cell
            cell = new PdfPCell(m_ImageHeader, true);
            cell.VerticalAlignment = Element.ALIGN_BOTTOM;
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell.FixedHeight = cellHeight;
            cell.Border = PdfPCell.NO_BORDER;
            head.AddCell(cell);

            // since the table header is implemented using a PdfPTable, we call
            // WriteSelectedRows(), which requires absolute positions!
            head.WriteSelectedRows(
                 0, -1,  // first/last row; -1 flags all write all rows
                MARGIN_LEFT,      // left offset
                                  // bottom - yPos of the table
                page.Height - cellHeight + head.TotalHeight - MARGIN_HEADER_TOP,
                writer.DirectContent
              );

            float fFooterCellHeight = MARGIN_BOTTOM - MARGIN_FOOTER_BOTTOM;

            PdfPTable footer = new PdfPTable(2);
            footer.TotalWidth = (page.Width * 0.5f) - MARGIN_LEFT - MARGIN_RIGHT;

            Phrase authBy = new Phrase("Authorised By:", LARGE_FONT);
            Phrase signatureName = new Phrase(m_FooterSignature, LARGE_FONT_BOLD);

            cell = new PdfPCell(authBy);
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell.FixedHeight = fFooterCellHeight;
            cell.Border = PdfPCell.NO_BORDER;
            footer.AddCell(cell);

            cell = new PdfPCell(signatureName);
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.FixedHeight = fFooterCellHeight;
            footer.AddCell(cell);

            // since the table header is implemented using a PdfPTable, we call
            // WriteSelectedRows(), which requires absolute positions!
            footer.WriteSelectedRows(
                 0, -1,  // first/last row; -1 flags all write all rows
                document.PageSize.Width * 0.5f,      // left offset
                                                     // bottom - yPos of the table
                MARGIN_FOOTER_BOTTOM + fFooterCellHeight,
                writer.DirectContent
              );
        }
    }

    private static readonly float[] LIST_ITEMS_COLUMN_WIDTHS = new float[]
{
        100f,//column 1 title
		100f,//column 2 count
		400f,//column 4 description
};

    private const float MARGIN_LEFT = 80f;
    private const float MARGIN_RIGHT = 80f;
    private const float MARGIN_TOP = 80f;
    private const float MARGIN_HEADER_TOP = 40f;
    private const float MARGIN_BOTTOM = 70f;
    private const float MARGIN_FOOTER_BOTTOM = 50f;

    private const iTextSharp.text.Font.FontFamily FONT_SYTLE = iTextSharp.text.Font.FontFamily.HELVETICA;

    public const string PDF_FILE_TYPE = ".pdf";
    private static readonly iTextSharp.text.Font DEFAULT_FONT = new iTextSharp.text.Font(FONT_SYTLE, 10f);
    private static readonly iTextSharp.text.Font DEFAULT_FONT_BOLD = new iTextSharp.text.Font(FONT_SYTLE, 10f, iTextSharp.text.Font.BOLD);
    private static readonly iTextSharp.text.Font LARGE_FONT = new iTextSharp.text.Font(FONT_SYTLE, 14f);
    private static readonly iTextSharp.text.Font LARGE_FONT_BOLD = new iTextSharp.text.Font(FONT_SYTLE, 14f, iTextSharp.text.Font.BOLD);

    public static bool WritePDF(List<HouseBuilder.ListItem> contents, string sFilePath)
    {
        try
        {
            string finalReportPath = sFilePath + "//" + DateTime.Now.ToString("yyyy-MM-dd")+ " Inventory Quotation";

            Texture2D logoTexture = Resources.Load("logosmall", typeof(Texture2D)) as Texture2D;
            byte[] logoBytes = logoTexture.EncodeToPNG();

            MemoryStream memoryStream = new MemoryStream();

            using (Document pdfReport = new Document())
            {
                pdfReport.SetPageSize(PageSize.A4);
                pdfReport.SetMargins(0f, 0f, MARGIN_TOP, MARGIN_BOTTOM);

                PdfWriter writer = PdfWriter.GetInstance(pdfReport, memoryStream);

                // instantiate the custom PdfPageEventHelper
                Image logo = Image.GetInstance(logoBytes);
                logo.ScalePercent(100f);
                PdfReportPageEventHandler pageEventHandler = new PdfReportPageEventHandler()
                {
                    ImageHeader = logo,
                    TextHeader = "Inventory Quotation",
                    SignatureFooter = "FirstName LastName"
                };
                writer.PageEvent = pageEventHandler;

                pdfReport.Open();

                // Add file meta data
                pdfReport.AddTitle("Music Tribe - Inventory Quotation");
                pdfReport.AddAuthor("Author placeholder");
                pdfReport.AddSubject("Subject placeholder");
                pdfReport.AddCreator("Creator Placeholder");

                // Contents
                Paragraph contentsTitle = new Paragraph("List of items", LARGE_FONT_BOLD);
                contentsTitle.Alignment = Element.ALIGN_CENTER;
                contentsTitle.SpacingAfter = 10f;

                PdfPTable userTable = CreateTableListOfItems(contents);

                userTable.SpacingAfter = 5f;

                pdfReport.Add(contentsTitle);
                pdfReport.Add(userTable);
            }

            PdfReader reader = new PdfReader(memoryStream.ToArray());
            using (FileStream fs = new FileStream(finalReportPath + PDF_FILE_TYPE, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (PdfStamper stamper = new PdfStamper(reader, fs))
                {
                    int pageCount = reader.NumberOfPages;
                    for (int i = 1; i <= pageCount; i++)
                    {
                        ColumnText.ShowTextAligned(stamper.GetOverContent(i), Element.ALIGN_LEFT, new Phrase(string.Format("Page {0} of {1}", i, pageCount),
                            DEFAULT_FONT_BOLD), MARGIN_LEFT, MARGIN_FOOTER_BOTTOM, 0);
                    }
                }
            }

            memoryStream.Flush();
            memoryStream.Close();

            Debug.Log("PDF report written: " + finalReportPath);
            return true;
        }
        catch (iTextSharp.text.DocumentException e)
        {
            Debug.LogError("Document Exception thrown: " + e.Message);
        }
        catch (iTextSharp.text.pdf.BadPdfFormatException e)
        {
            Debug.LogError("Bad PDF Format Exception thrown: " + e.Message);
        }
        catch (iTextSharp.text.pdf.PdfXConformanceException e)
        {
            Debug.LogError("PDF X Conformance Format Exception thrown: " + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.LogError("General Exception thrown: " + e.Message);
        }

        return false;
    }

    private static PdfPTable CreateTableListOfItems(List<HouseBuilder.ListItem> contents)
    {
        PdfPTable table = new PdfPTable(3);

        table.SetWidths(LIST_ITEMS_COLUMN_WIDTHS);

        //title/shortname of object
        AddCellToTable(ref table, "SHORT NAME", DEFAULT_FONT_BOLD);

        //count of objects
        AddCellToTable(ref table, "Qty", DEFAULT_FONT_BOLD);

        //full description
        AddCellToTable(ref table, "DESCRIPTION", DEFAULT_FONT_BOLD);

        //TODO: include duplicates to count items[i].m_Count.ToString()
        for (int i = 0; i < contents.Count; i++)
        {

            //Title/shortname
            AddCellToTable(ref table, contents[i].m_ItemData.CatalogueEntryRef.ShortName, DEFAULT_FONT, horizontalAlignment: PdfPCell.ALIGN_CENTER);

            //count
            AddCellToTable(ref table, "1", DEFAULT_FONT, horizontalAlignment: PdfPCell.ALIGN_CENTER);

            //description
            AddCellToTable(ref table, contents[i].m_ItemData.CatalogueEntryRef.FullName, DEFAULT_FONT, horizontalAlignment: PdfPCell.ALIGN_CENTER);
        }


        table.SpacingBefore = 5f;
        table.SpacingAfter = 5f;
        return table;
    }

    private static void AddCellToTable(ref PdfPTable table, string phrase, iTextSharp.text.Font font, int columnSpan = 1, float cellMinHeight = 28f,
        int verticalAlignment = PdfPCell.ALIGN_MIDDLE, int horizontalAlignment = PdfPCell.ALIGN_CENTER)
    {
        Phrase cellPhrase = new Phrase(phrase, font);
        PdfPCell cell = new PdfPCell(cellPhrase);
        cell.Colspan = columnSpan;
        cell.HorizontalAlignment = horizontalAlignment;
        cell.VerticalAlignment = verticalAlignment;
        cell.MinimumHeight = cellMinHeight;
        table.AddCell(cell);
    }
}