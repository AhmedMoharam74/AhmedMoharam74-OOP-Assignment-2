using LibrarySystem;

// These examples must NOT compile:
// Person plainPerson = new Person("P0", "Name", "0100");
// Member plainMember = new Member("M0", "Name", "0100", 5, 0m);
// Staff plainStaff = new Staff("S0", "Name", "0100", DateOnly.Today, 1000m);
// LibraryItem plainItem = new LibraryItem("I0", "Title", 1, 1m, 1m);
// student.FullName = "Changed";
// student.Loans.Add(existingLoan);
// book.IsOnLoan = true;

StudentMember student = new("S-1", "Salma Student", "01000000001");
PremiumMember premium = new("P-1", "Pia Premium", "01000000002", 20m);
Book book = new("B-1", "Clean Code", 2m);
DVD dvd = new("D-1", "Patterns", 4m);
Magazine magazine = new("M-1", "Design Weekly", 1m);

book.Withdraw();
try { student.Borrow("L-1", book, new DateOnly(2026, 9, 1)); }
catch (Exception error) { Console.WriteLine($"Rejected withdrawn item: {error.Message}"); }
book.Restore();
Loan first = student.Borrow("L-1", book, new DateOnly(2026, 9, 1));
try { premium.Borrow("L-2", book, new DateOnly(2026, 9, 2)); }
catch (Exception error) { Console.WriteLine($"Rejected duplicate loan: {error.Message}"); }

StudentMember limitStudent = new("S-2", "Laila Student", "01000000006");
limitStudent.Borrow("S-L1", new Book("S-B1", "Student book 1", 1m), new DateOnly(2026, 9, 1));
limitStudent.Borrow("S-L2", new Book("S-B2", "Student book 2", 1m), new DateOnly(2026, 9, 1));
limitStudent.Borrow("S-L3", new Book("S-B3", "Student book 3", 1m), new DateOnly(2026, 9, 1));
try { limitStudent.Borrow("S-L4", new Book("S-B4", "Extra", 1m), new DateOnly(2026, 9, 1)); }
catch (Exception error) { Console.WriteLine($"Rejected fourth student loan: {error.Message}"); }

List<Staff> staff = new()
{
    new Librarian("E-1", "Lina Librarian", "01000000003", new DateOnly(2020, 1, 1), 12000m),
    new HeadLibrarian("E-2", "Hadi Head", "01000000004", new DateOnly(2015, 1, 1), 20000m),
    new Shelver("E-3", "Sami Shelver", "01000000005", new DateOnly(2022, 1, 1), 9000m, "Fiction")
};
foreach (Staff employee in staff) Console.WriteLine($"{employee.FullName}: monthly pay {employee.MonthlyPay():0.00}");

List<LibraryItem> items = new() { book, dvd, magazine };
foreach (LibraryItem item in items) Console.WriteLine($"{item.Title}: loan period {item.LoanPeriodDays} days, daily late fee {item.DailyLateFee():0.00}");

Loan premiumLoan = premium.Borrow("P-L1", dvd, new DateOnly(2026, 9, 1));
premiumLoan.Return(new DateOnly(2026, 9, 13));
Console.WriteLine($"Due date: {premiumLoan.DueDate:yyyy-MM-dd}; late fee: {premiumLoan.LateFee:0.00}; reading points: {premium.ReadingPoints}");
try { premiumLoan.Return(new DateOnly(2026, 9, 14)); }
catch (Exception error) { Console.WriteLine($"Rejected second return: {error.Message}"); }
try { premiumLoan.MarkLost(); }
catch (Exception error) { Console.WriteLine($"Rejected returned loan as lost: {error.Message}"); }
try { staff[0].GiveRaise(0m); }
catch (Exception error) { Console.WriteLine($"Rejected zero raise: {error.Message}"); }
try { ((HeadLibrarian)staff[1]).ChangeLateFee(items[0], 0m); }
catch (Exception error) { Console.WriteLine($"Rejected zero late fee: {error.Message}"); }
try { Loan invalidDateLoan = premium.Borrow("P-L2", magazine, new DateOnly(2026, 9, 10)); invalidDateLoan.Return(new DateOnly(2026, 9, 9)); }
catch (Exception error) { Console.WriteLine($"Rejected invalid return date: {error.Message}"); }
