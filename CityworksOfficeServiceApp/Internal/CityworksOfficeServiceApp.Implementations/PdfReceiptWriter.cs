using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Text;

namespace CityworksOfficeServiceApp.Implementations;

public sealed class PdfReceiptWriter : IReceiptWriter
{
    private readonly IConverter converter;
    private readonly CaseModel pllCase;
    private readonly CaseReceiptDetailModel receiptDetail;

    internal PdfReceiptWriter(IConverter converter, CaseModel pllCase, CaseReceiptDetailModel receiptDetail)
    {
        this.converter = converter;
        this.pllCase = pllCase;
        this.receiptDetail = receiptDetail;
    }

    public byte[] Write()
    {
        var html = new StringBuilder();
        html.AppendLine("<html>");
        html.AppendLine($"<head><title>{pllCase.CaseNumber}</title></head>");
        html.AppendLine("</html>");
        html.AppendLine("<body>");
        AppendHtmlBody(html);
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        var globalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Grayscale,
            Orientation = Orientation.Landscape,
            PaperSize = PaperKind.A4
        };
        var objSettings = new ObjectSettings
        {
            HtmlContent = html.ToString()
        };
        var htmlToPdfDoc = new HtmlToPdfDocument
        {
            GlobalSettings = globalSettings
        };
        htmlToPdfDoc.Objects.Add(objSettings);
        var bytes = converter.Convert(htmlToPdfDoc);
        return bytes;
    }

    private void AppendHtmlBody(StringBuilder html)
    {
        html.AppendLine("<h3 style=\"text-align: center; padding: 0; margin: 0 50px 5px 50px;\">Greer CPW Receipt</h3>");
        html.AppendLine($"<div style=\"text-align: right; width: 800px; padding: 0; margin: 0 auto 10px auto;\">Receipt #: {receiptDetail.Receipt.ID}</div>");
        html.AppendLine($"<div style=\"text-align: right; width: 800px; padding: 0; margin: 0 auto 20px auto;\">Date: {DateTime.Now:M/d/yy}</div>");
        html.Append("<table style=\"margin: 0 auto 20px auto; width: 800px; border-collapse: collapse; border: 1px solid black;\">");
        AppendTableHeaders(html, new HtmlTableCell("Case Type"), new HtmlTableCell("Case Number"), new HtmlTableCell("Sub Type"));
        html.Append("<tbody>");
        AppendTableCells
        (
            html,
            new HtmlTableCell(pllCase.CaseType.Description),
            new HtmlTableCell(pllCase.CaseNumber),
            new HtmlTableCell(pllCase.SubTypeDescription)
        );
        html.Append("</tbody>");
        html.Append("</table>");
        html.Append("<table style=\"margin: 0 auto 20px auto; width: 800px; border-collapse: collapse; border: 1px solid black;\">");
        AppendTableHeaders(html, new HtmlTableCell("Payment Method"), new HtmlTableCell("Amount", textAlign: "right"));
        html.Append("<tbody>");
        foreach (var tenderType in receiptDetail.TenderTypes)
        {
            AppendTableCells
            (
                html,
                new HtmlTableCell(tenderType.Description),
                new HtmlTableCell($"{tenderType.Amount:C}", textAlign: "right")
            );
        }
        AppendTableCells
        (
            html,
            new HtmlTableCell("Sub Total:", textAlign: "right", fontWeight: "bold", Width: "500px"),
            new HtmlTableCell($"{receiptDetail.GetTotalAmountPaid():C}", textAlign: "right", fontWeight: "bold")
        );
        html.Append("</tbody>");
        html.Append("</table>");
        html.Append("<table style=\"margin: 0 auto 20px auto; width: 800px; border-collapse: collapse; border: 1px solid black;\">");
        AppendTableHeaders(html, new HtmlTableCell("Fee"), new HtmlTableCell("Amount", textAlign: "right"));
        html.Append("<tbody>");
        foreach (var fee in receiptDetail.Fees)
        {
            AppendTableCells
            (
                html,
                new HtmlTableCell(fee.Description),
                new HtmlTableCell($"{fee.AmountDue:C}", textAlign: "right")
            );
        }
        AppendTableCells
        (
            html,
            new HtmlTableCell("Sub Total:", textAlign: "right", fontWeight: "bold", Width: "500px"),
            new HtmlTableCell($"{receiptDetail.GetTotalAmountDue():C}", textAlign: "right", fontWeight: "bold")
        );
        html.Append("</tbody>");
        html.Append("</table>");
    }

    private void AppendTableHeaders(StringBuilder html, params HtmlTableCell[] headers)
    {
        html.Append("<tr>");
        foreach (var header in headers)
        {
            html.Append($"<th style=\"font-weight: bold; background-color: #ccc; text-align: {header.textAlign}; white-space: nowrap; padding: 20px 10px 20px 10px;\">{header.Data}</th>");
        }
        html.Append("</tr>");
    }

    private void AppendTableCells(StringBuilder html, params HtmlTableCell[] cells)
    {
        html.Append("<tr>");
        foreach (var cell in cells)
        {
            html.Append($"<td style=\"font-weight: {cell.fontWeight};padding: 0; width: {cell.Width}; text-overflow: clip; padding: 10px;\" valign=\"bottom\"><div style=\"width: {cell.Width}; text-overflow: clip; overflow: hidden; text-align: {cell.textAlign};\">{cell.Data}</div></td>");
        }
        html.Append("</tr>");
    }
}