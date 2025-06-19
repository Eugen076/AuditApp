using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    public class CreditController : Controller
    {
        [HttpGet]
        public IActionResult Index(string creditType = "Nevoi personale")
        {
            decimal minAmount = creditType == "Imobiliar-Ipotecar" ? 7000 : 5000;

            return View(new CreditViewModel
            {
                LoanAmount = minAmount,
                LoanTermYears = 1,
                LoanTermMonths = 12,
                CreditType = creditType,
                Currency = "LEI"
            });
        }

        [HttpPost]
        public IActionResult Calculate(CreditViewModel model)
        {
            if (ModelState.IsValid)
            {
                decimal annualInterestRate = model.CreditType == "Imobiliar-Ipotecar" ? 0.055m : 0.085m;
                int loanTermMonths = model.LoanTermYears > 0 ? model.LoanTermYears * 12 : model.LoanTermMonths;
                model.LoanTermMonths = loanTermMonths;
                model.MonthlyPayment = CalculateMonthlyPayment(model.LoanAmount, loanTermMonths, annualInterestRate);
                Console.WriteLine($"LoanAmount: {model.LoanAmount}, LoanTermMonths: {model.LoanTermMonths}, MonthlyPayment: {model.MonthlyPayment}");
            }
            return View("Index", model);
        }

        private decimal CalculateMonthlyPayment(decimal loanAmount, int loanTermMonths, decimal annualInterestRate)
        {
            if (loanTermMonths <= 0 || annualInterestRate < 0 || loanAmount <= 0)
                return 0;

            decimal monthlyInterestRate = annualInterestRate / 12;
            decimal factor = (decimal)Math.Pow(1 + (double)monthlyInterestRate, loanTermMonths);
            if (factor - 1 == 0) return loanAmount / loanTermMonths;
            decimal monthlyPayment = (loanAmount * monthlyInterestRate * factor) / (factor - 1);
            return Math.Round(monthlyPayment, 2);
        }

        [HttpGet]
        public IActionResult DetailedCalculation(decimal loanAmount, int loanTermMonths, decimal monthlyPayment, string creditType)
        {

            monthlyPayment = monthlyPayment / 100;

            decimal annualInterestRate = creditType == "Imobiliar-Ipotecar" ? 0.055m : 0.085m;
            decimal totalAmountPaid = monthlyPayment * loanTermMonths;
            decimal totalInterest = totalAmountPaid - loanAmount;

            var model = new DetailedCalculationViewModel
            {
                LoanAmount = loanAmount,
                LoanTermMonths = loanTermMonths,
                MonthlyPayment = Math.Round(monthlyPayment, 2),
                CreditType = creditType,
                TotalInterest = Math.Round(totalInterest, 2),
                TotalAmountPaid = Math.Round(totalAmountPaid, 2),

            };

            return View(model);
        }
        [HttpGet]
        public IActionResult BackToSimulator(decimal loanAmount, int loanTermMonths, decimal monthlyPayment, string creditType)
        {
            var model = new CreditViewModel
            {
                LoanAmount = loanAmount,
                LoanTermMonths = loanTermMonths,
                CreditType = creditType,
                MonthlyPayment = monthlyPayment,
                Currency = "LEI"
            };

            return View("Index", model);
        }


    }
}