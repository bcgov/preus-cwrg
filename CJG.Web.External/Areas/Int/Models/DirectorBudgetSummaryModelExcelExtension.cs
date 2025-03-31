using System.IO;
using ClosedXML.Excel;

namespace CJG.Web.External.Areas.Int.Models
{
	public static class DirectorBudgetSummaryModelExcelExtension
	{
		public static byte[] GetExcelContent(this DirectorBudgetSummaryModel model)
		{
			using (var stream = new MemoryStream())
			{
				var wb = new XLWorkbook();
				var ws = wb.AddWorksheet();

				ws.ShowRowColHeaders = true;
				ws.Name = "CWRG Directors Report";

				var columnOffset = 1;
				var rowOffset = 1;

				ws.Cell(rowOffset, columnOffset).SetValue($"CWRG Directors Report - {model.FiscalYear}");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Font.FontSize = 12;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.AshGrey;
				
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var budgetTitle = budget.GroupingStreams;
					ws.Cell(rowOffset, columnOffset).SetValue(budgetTitle);
					ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
					ws.Cell(rowOffset, columnOffset).Style.Font.FontSize = 12;
					ws.Cell(rowOffset, columnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.AshGrey;
				}

				rowOffset = 2;
				columnOffset = 1;

				ws.Cell(rowOffset, columnOffset).SetValue("Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.MintGreen;
				ws.Cell(rowOffset, columnOffset).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;

					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.MintGreen;
					FormatMoneyCell(xlCell, budget.Budget);
					xlCell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
				}

				rowOffset = 3;
				foreach (var budgetRow in model.OpeningBudgetRows)
				{
					var rowTitle = !string.IsNullOrWhiteSpace(budgetRow.Name) ? budgetRow.Name : "Adjustment";
					ws.Cell(rowOffset, 1).SetValue(rowTitle);
					ws.Cell(rowOffset, 1).Style.Fill.BackgroundColor = XLColor.MistyRose;

					var budgetEntryColumnOffset = 2;
					foreach (var budgetEntry in budgetRow.DirectorBudgetEntries)
					{
						var xlCell = ws.Cell(rowOffset, budgetEntryColumnOffset);
						xlCell.Style.Fill.BackgroundColor = XLColor.MistyRose;
						FormatMoneyCell(xlCell, budgetEntry.Budget);
						budgetEntryColumnOffset++;
					}

					rowOffset++;
				}

				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Adjusted Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.MintGreen;
				ws.Cell(rowOffset, columnOffset).Style.Border.BottomBorder = XLBorderStyleValues.Thick;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.MintGreen;
					FormatMoneyCell(xlCell, budget.DirectorsReportAdjustedBudget);
					xlCell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Committed (Schedule A)");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					FormatMoneyCell(xlCell, budget.DirectorsReportCommittedScheduleA);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Claims Processed");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportClaimsProcessed);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Unclaimed");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportUnclaimed);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Receivables (total set up in current FY)");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportReceivables);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Slippage (Sched A - Claims Processed - Unclaimed)");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportSlippage);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("YTD Actuals (Claims Processed - Receivables)");
				ws.Cell(rowOffset, columnOffset).Style.Border.BottomBorder = XLBorderStyleValues.Thick;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					FormatMoneyCell(xlCell, budget.DirectorsReportYtdActual);
					xlCell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Available Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.MintGreen;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.MintGreen;
					FormatMoneyCell(xlCell, budget.DirectorsReportAvailableBudget);
				}

				rowOffset++;
				foreach (var budgetRow in model.ClosingBudgetRows)
				{
					var rowTitle = !string.IsNullOrWhiteSpace(budgetRow.Name) ? budgetRow.Name : "Adjustment";
					ws.Cell(rowOffset, 1).SetValue(rowTitle);
					ws.Cell(rowOffset, 1).Style.Fill.BackgroundColor = XLColor.MistyRose;

					var budgetEntryColumnOffset = 2;
					foreach (var budgetEntry in budgetRow.DirectorBudgetEntries)
					{
						var xlCell = ws.Cell(rowOffset, budgetEntryColumnOffset);
						xlCell.Style.Fill.BackgroundColor = XLColor.MistyRose;
						FormatMoneyCell(xlCell, budgetEntry.Budget);
						budgetEntryColumnOffset++;
					}

					rowOffset++;
				}

				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Total Remaining Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.LightGreen;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
					FormatMoneyCell(xlCell, budget.DirectorsReportRemainingBudget);
				}

				foreach (var cell in ws.CellsUsed())
				{
					var useThickBorder = cell.Style.Border.BottomBorder == XLBorderStyleValues.Thick;
					cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

					if (useThickBorder)
						cell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
				}

				ws.ColumnsUsed().AdjustToContents(20d, 60d);
				ws.RowsUsed().CellsUsed().Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
				ws.RowsUsed().AdjustToContents();

				wb.SaveAs(stream);

				return stream.ToArray();
			}
		}

		private static void FormatMoneyCell(IXLCell cell, decimal? budgetAmount)
		{
			if (budgetAmount.HasValue)
				cell.SetValue(budgetAmount.Value);
			cell.Style.NumberFormat.Format = "#,##0.00";
			cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
			cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
		}

		private static void FormatMoneyCell(IXLCell cell, decimal budgetAmount)
		{
			cell.SetValue(budgetAmount);
			cell.Style.NumberFormat.Format = "#,##0.00";
			cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
			cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
		}
	}
}