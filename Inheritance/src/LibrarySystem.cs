namespace LibrarySystem;

public class Person
{
    private static readonly List<string> RegisteredPersonIds = new();
    public string PersonId { get; }
    public string FullName { get; }
    public string Phone { get; }
    protected Person(string id, string name, string phone)
    {
        FullName = Required(name, "Full name"); Phone = Required(phone, "Phone"); PersonId = UniqueId(id);
    }
    internal static string Required(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{label} cannot be empty.");
        return value;
    }
    private static string UniqueId(string id)
    {
        string value = Required(id, "Person ID");
        foreach (string registeredId in RegisteredPersonIds)
            if (registeredId == value) throw new ArgumentException($"Person ID '{value}' is already registered.");
        RegisteredPersonIds.Add(value);
        return value;
    }
}

public class Member : Person
{
    private readonly List<Loan> _loans = new();
    private readonly int _maximumLoans;
    private readonly decimal _discountPercent;
    private int _returnedLoanCount;
    public IReadOnlyList<Loan> Loans { get { return _loans.AsReadOnly(); } }
    public int MaximumLoans { get { return _maximumLoans; } }
    public decimal DiscountPercent { get { return _discountPercent; } }
    protected Member(string id, string name, string phone, int maximumLoans, decimal discountPercent) : base(id, name, phone)
    {
        _maximumLoans = maximumLoans; _discountPercent = discountPercent;
    }
    public Loan Borrow(string loanId, LibraryItem item, DateOnly borrowDate)
    {
        int active = 0;
        foreach (Loan loan in _loans) if (loan.Status == LoanStatus.Borrowed) active++;
        if (active >= _maximumLoans) throw new InvalidOperationException($"Member {PersonId} has reached the loan limit of {_maximumLoans}.");
        if (item.IsWithdrawn) throw new InvalidOperationException($"Item {item.CatalogNumber} is withdrawn and cannot be borrowed.");
        if (item.IsOnLoan) throw new InvalidOperationException($"Item {item.CatalogNumber} is already on loan.");
        Loan created = new(loanId, borrowDate, this, item);
        _loans.Add(created); item.MarkBorrowed(); return created;
    }
    internal void RecordReturnedLoan() { _returnedLoanCount++; }
    protected int ReturnedLoanCount { get { return _returnedLoanCount; } }
}

public sealed class StudentMember : Member
{
    public StudentMember(string id, string name, string phone) : base(id, name, phone, 3, 0m) { }
}
public sealed class PremiumMember : Member
{
    public int ReadingPoints { get { return ReturnedLoanCount * 5; } }
    public PremiumMember(string id, string name, string phone, decimal discountPercent) : base(id, name, phone, 10, ValidateDiscount(discountPercent)) { }
    private static decimal ValidateDiscount(decimal discount)
    {
        if (discount < 0m || discount > 100m) throw new ArgumentOutOfRangeException(nameof(discount), "Discount must be between 0 and 100 percent.");
        return discount;
    }
}

public class Staff : Person
{
    public DateOnly HireDate { get; }
    public decimal MonthlySalary { get; private set; }
    private readonly decimal _monthlyAllowance;
    protected Staff(string id, string name, string phone, DateOnly hireDate, decimal salary, decimal allowance = 0m) : base(id, name, phone)
    {
        if (salary <= 0m) throw new ArgumentOutOfRangeException(nameof(salary), "Salary must be positive.");
        HireDate = hireDate; MonthlySalary = salary; _monthlyAllowance = allowance;
    }
    public void GiveRaise(decimal percentage)
    {
        if (percentage <= 0m) throw new ArgumentOutOfRangeException(nameof(percentage), "Raise percentage must be positive.");
        MonthlySalary = Math.Round(MonthlySalary * (1m + percentage / 100m), 2);
    }
    public decimal MonthlyPay() { return MonthlySalary + _monthlyAllowance; }
}
public sealed class Librarian : Staff
{
    public Librarian(string id, string name, string phone, DateOnly hireDate, decimal salary) : base(id, name, phone, hireDate, salary) { }
    public void ProcessReturn(Loan loan, DateOnly returnDate) { loan.Return(returnDate); }
    public void MarkItemLost(Loan loan) { loan.MarkLost(); }
}
public sealed class Shelver : Staff
{
    public string Section { get; private set; }
    public Shelver(string id, string name, string phone, DateOnly hireDate, decimal salary, string section) : base(id, name, phone, hireDate, salary)
    {
        Section = Required(section, "Section");
    }
    public void Reassign(string section) { Section = Required(section, "Section"); }
}
public sealed class HeadLibrarian : Staff
{
    public HeadLibrarian(string id, string name, string phone, DateOnly hireDate, decimal salary) : base(id, name, phone, hireDate, salary, 400m) { }
    public void ChangeLateFee(LibraryItem item, decimal newBaseFee) { item.PriceBaseLateFee(newBaseFee); }
    public void Withdraw(LibraryItem item) { item.Withdraw(); }
    public void Restore(LibraryItem item) { item.Restore(); }
}

public class LibraryItem
{
    public string CatalogNumber { get; }
    public string Title { get; }
    public int LoanPeriodDays { get; }
    public decimal BaseLateFee { get; private set; }
    private readonly decimal _feeMultiplier;
    private static readonly List<string> RegisteredCatalogNumbers = new();
    public bool IsWithdrawn { get; private set; }
    public bool IsOnLoan { get; private set; }
    protected LibraryItem(string catalogNumber, string title, int loanPeriodDays, decimal baseLateFee, decimal feeMultiplier) 
    {
        CatalogNumber = Person.Required(catalogNumber, "Catalog number"); Title = Person.Required(title, "Title");
        if (baseLateFee <= 0m) throw new ArgumentOutOfRangeException(nameof(baseLateFee), "Base late fee must be positive.");
        foreach (string registeredNumber in RegisteredCatalogNumbers)
            if (registeredNumber == CatalogNumber) throw new ArgumentException($"Catalog number '{CatalogNumber}' is already registered.");
        RegisteredCatalogNumbers.Add(CatalogNumber);
        LoanPeriodDays = loanPeriodDays; BaseLateFee = baseLateFee; _feeMultiplier = feeMultiplier;
    }
    public decimal DailyLateFee() { return BaseLateFee * _feeMultiplier; }
    internal void PriceBaseLateFee(decimal fee) { if (fee <= 0m) throw new ArgumentOutOfRangeException(nameof(fee), "Late fee must be positive."); BaseLateFee = fee; }
    internal void Withdraw() { IsWithdrawn = true; }
    internal void Restore() { IsWithdrawn = false; }
    internal void MarkBorrowed() { IsOnLoan = true; }
    internal void MarkReturned() { IsOnLoan = false; }
}
public sealed class Book : LibraryItem { public Book(string no, string title, decimal fee) : base(no, title, 21, fee, 1m) { } }
public sealed class DVD : LibraryItem { public DVD(string no, string title, decimal fee) : base(no, title, 7, fee, 2m) { } }
public sealed class Magazine : LibraryItem { public Magazine(string no, string title, decimal fee) : base(no, title, 3, fee, 0.5m) { } }

public enum LoanStatus { Borrowed, Returned, Lost }
public sealed class Loan
{
    private static readonly List<string> RegisteredLoanIds = new();
    public string LoanId { get; }
    public DateOnly BorrowDate { get; }
    public Member Member { get; }
    public LibraryItem Item { get; }
    public DateOnly DueDate { get { return BorrowDate.AddDays(Item.LoanPeriodDays); } }
    public DateOnly? ReturnDate { get; private set; }
    public LoanStatus Status { get; private set; }
    public decimal LateFee
    {
        get
        {
            if (ReturnDate is null || ReturnDate.Value <= DueDate) return 0m;
            int lateDays = ReturnDate.Value.DayNumber - DueDate.DayNumber;
            decimal gross = lateDays * Item.DailyLateFee();
            return Math.Round(gross * (1m - Member.DiscountPercent / 100m), 2);
        }
    }
    internal Loan(string loanId, DateOnly borrowDate, Member member, LibraryItem item)
    {
        LoanId = Person.Required(loanId, "Loan ID");
        foreach (string registeredId in RegisteredLoanIds)
            if (registeredId == LoanId) throw new ArgumentException($"Loan ID '{LoanId}' is already registered.");
        RegisteredLoanIds.Add(LoanId);
        BorrowDate = borrowDate; Member = member; Item = item; Status = LoanStatus.Borrowed;
    }
    public void Return(DateOnly returnDate)
    {
        if (Status != LoanStatus.Borrowed) throw new InvalidOperationException("Only a borrowed loan can be returned.");
        if (returnDate < BorrowDate) throw new ArgumentException("Return date cannot be earlier than the borrow date.");
        ReturnDate = returnDate; Status = LoanStatus.Returned; Item.MarkReturned(); Member.RecordReturnedLoan();
    }
    public void MarkLost()
    {
        if (Status != LoanStatus.Borrowed) throw new InvalidOperationException("Only a borrowed loan can be marked as lost.");
        Status = LoanStatus.Lost;
    }
}
