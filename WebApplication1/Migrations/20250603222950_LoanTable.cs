using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class LoanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "Loans",
                type: "decimal(5,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LoanTermMonths",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LoanType",
                table: "Loans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPayment",
                table: "Loans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Loans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountPaid",
                table: "Loans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LoanTermMonths",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LoanType",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "MonthlyPayment",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "TotalAmountPaid",
                table: "Loans");
        }
    }
}
