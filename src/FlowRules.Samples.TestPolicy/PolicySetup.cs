using FlowRules.Engine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace FlowRules.Samples.TestPolicy;

public static class PolicySetup
{
    public static Policy<MortgageApplication> GetPolicy()
    {
        Lookups? lookup = GetLookups();

        Rule<MortgageApplication> validMortgageTypeRule = new(
            "MA001",
            "KnownMortgageType",
            "Checks the mortgage type",
            (r) => $"The {nameof(r.MortgageType)} [{r.MortgageType}] is not known.",
            async (request, token) =>
            {
                await Task.Delay(100, token);
                ColumnResolver mortgageType = lookup["Default"][request.MortgageType];
                return mortgageType != null;
            });

        Rule<MortgageApplication> ageLimitRule = new(
            "MA002",
            "MinAgeCheck",
            "Minimum age of the applicant",
            (r) => $"The {nameof(r.ApplicantAge)} [{r.ApplicantAge}] is too young.",
            async (request, token) =>
            {
                await Task.Delay(50, token);                
                int minAgeForMortgage = lookup["Default"][request.MortgageType]["MinApplicantAge"].As<int>();
                return request.ApplicantAge >= minAgeForMortgage;
            });

        Rule<MortgageApplication> minLoanAmountRule = new(
            "MA003",
            "MinLoanAmount",
            "minimum loan amount check",
            (r) => $"The {nameof(r.LoanAmount)} [{r.LoanAmount}] is too small.",
            async (request, token) =>
            {
                await Task.Delay(20, token);
                int minLoanAmount = lookup["Default"][request.MortgageType]["MinLoan"].As<int>();
                return request.LoanAmount >= minLoanAmount;
            });

        Rule<MortgageApplication> maxLoanAmountRule = new(
            "MA004",
            "MaxLoanAmount",
            "Maximum loan amount check",
            (r) => $"The {nameof(r.LoanAmount)} [{r.LoanAmount}] is too large.",
            async (request, token) =>
            {
                await Task.Delay(20, token);               
                int maxLoanAmount = lookup["Default"][request.MortgageType]["MaxLoan"].As<int>();
                return request.LoanAmount <= maxLoanAmount;
            });

        Rule<MortgageApplication> lendersCanServiceLoanBasedOnLTVRule = new(
            "MA005",
            "LTV",
            "Loan-To-Value Ratio must be above the minimum threshold for lenders to satisfy loan serviceability requirements",
            (r) => {
                double ltv = (r.LoanAmount - r.PrincipalAmount) / (double)r.LoanAmount * 100;
                double minLTV = lookup["Default"][r.MortgageType]["MinLTV"].As<double>();

                return $"The LTV ratio [{ltv}] is above the minimum threshold for the high-ltv loans [{minLTV}]. " +
                       $"Either increase the principal {r.PrincipalAmount} or lower the loan amount {r.LoanAmount}";
            },
            async (request, token) =>
            {
                await Task.Delay(20, token);
                double minLTV = lookup["Default"][request.MortgageType]["MinLTV"].As<double>();
                double ltv = (request.LoanAmount - request.PrincipalAmount) / (double)request.LoanAmount * 100;                               
                return ltv <= minLTV;
            });

        Rule<MortgageApplication> applicantCanSatisfyMonthlyLoanCommitmentsRule = new(
            "MA006",
            "DSR",
            "Debt-To-Service Ratio must be below threshold so applicant can handle monthly loan commitments",
            (r) => {
                double interestRate = lookup["Default"][r.MortgageType]["InterestRateDSCR"].As<double>();
                int minDSR = lookup["Default"][r.MortgageType]["MinDSCR"].As<int>();
                double monthlyRepayment = monthlyRepayment = Financial.Pmt(interestRate, r.LoanTerm * 12, -r.LoanAmount);
                double monthlyOutgoings = r.MonthlyHouseholdExpenses + monthlyRepayment + (0.02 * r.LoanAmount) / 12.0;                
                double dscr = (monthlyOutgoings - r.MonthlyLivingExpenses) / (r.GrossIncome / 12);               

                return $"The DSCR ratio [{dscr}] is above the minimum threshold [{minDSR}]. " +
                       $"Either increase applicant monthly salary or reduce applicant monthly expenditures";
            },
            async (request, token) =>
            {
                await Task.Delay(20, token);
                double interestRate = lookup["Default"][request.MortgageType]["InterestRateDSCR"].As<double>();
                int minDSR = lookup["Default"][request.MortgageType]["MinDSCR"].As<int>();
                double monthlyRepayment = monthlyRepayment = Financial.Pmt(interestRate, request.LoanTerm * 12, -request.LoanAmount); 
                double monthlyOutgoings = request.MonthlyHouseholdExpenses + monthlyRepayment + (0.02 * request.LoanAmount) / 12.0;                
                double dsr = (monthlyOutgoings - request.MonthlyLivingExpenses) / (request.GrossIncome / 12);                
                return dsr <= minDSR;
            });

        Policy<MortgageApplication> policy =
            new(
                "P001",
                "LoanPolicy",
                "Simple loan policy",
                [
                    validMortgageTypeRule,
                    ageLimitRule,
                    minLoanAmountRule,
                    maxLoanAmountRule,
                    lendersCanServiceLoanBasedOnLTVRule,
                    applicantCanSatisfyMonthlyLoanCommitmentsRule
                ]);

        return policy;
    }

    private static Lookups GetLookups()
    {
        Lookups lookups = new(
            new List<(string page, string row, string column, object value)>
            {
                // First Time Buyer rules
                ("Default", "FTB", "MinLoan", 100_000),
                ("Default", "FTB", "MaxLoan", 420_000), 
                ("Default", "FTB", "MinApplicantAge", 25),                
                ("Default", "FTB", "MinLTV", 95.0),                
                ("Default", "FTB", "MinDSCR", 50),
                ("Default", "FTB", "InterestRateDSCR", 0.95),
                // Buy to Let rules
                ("Default", "BTL", "MinLoan", 200_000),
                ("Default", "BTL", "MaxLoan", 2_000_000),
                ("Default", "BTL", "MinApplicantAge", 30),    
                ("Default", "BTL", "MinLTV", 75.0),
                ("Default", "BTL", "MinDSCR", 50),
                ("Default", "BTL", "InterestRateDSCR", 0.95),
            }
        );

        return lookups;
    }
}
