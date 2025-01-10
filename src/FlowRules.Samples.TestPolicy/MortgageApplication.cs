namespace FlowRules.Samples.TestPolicy;

public record MortgageApplication(
    int ApplicantAge, 
    string MortgageType, 
    double LoanAmount, 
    double PrincipalAmount,
    double GrossIncome, 
    double MonthlyLivingExpenses, 
    double MonthlyHouseholdExpenses, 
    int LoanTerm = 25);
