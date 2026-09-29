using SrpLab;

Console.WriteLine("SrpLab — refactored responsibility smoke checks");
Console.WriteLine("=================================================");

var ward = new WardBoard();
ward.AssignBed(1, "p-88", heartRate: 130, spo2: 89);
Console.WriteLine(ward.BuildHandoffNote(1));
Console.WriteLine(string.Join(" | ", ward.DrainPagerLog()));

var basket = new CheckoutBasket();
basket.AddLine("SKU-1", 40m, 2);
basket.ApplyCouponText("SAVE10");
basket.EnableGiftWrap();
Console.WriteLine($"basket total={basket.GrandTotal()} auth={basket.AuthorizePaymentStub("4242")}");

var ticket = new SupportTicket("T-1", "cannot login", "prod is down for me", DateTimeOffset.UtcNow);
Console.WriteLine(ticket.DraftPublicReply("Nora"));

var loan = new LoanDesk(60_000m, 640, 4, hasCollateral: false);
Console.WriteLine(loan.DecisionLetter("Omar"));

var course = new CourseEnrollmentDesk("SEF-101", capacity: 1, tuition: 3000m);
Console.WriteLine(course.Register("a@mail.com"));
Console.WriteLine(course.Register("b@mail.com"));
Console.WriteLine(course.WelcomePacketMarkdown("b@mail.com", "Bea"));

var kitchen = new KitchenTicket();
kitchen.AddItem("Pasta", new[] { "wheat", "milk" }, 12);
Console.WriteLine(kitchen.RenderThermalTicket(42));

var sub = new SubscriptionBilling("c-9", 99m, new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1));
sub.RegisterFailedPayment();
Console.WriteLine(sub.DunningEmail("Sara", new DateOnly(2026, 9, 20)));

var pick = new WarehousePickList();
pick.AddNeed("BOLT", "A", 3, 10, 7);
pick.AddNeed("NUT", "B", 1, 5, 5);
Console.WriteLine(pick.PickerScript());

var grades = new GradeBook();
grades.Record("s1", 92);
grades.Record("s1", 88);
Console.WriteLine(grades.TranscriptPlain("s1", "Ali"));

var appt = new AppointmentDesk(new TimeOnly(9, 0), new TimeOnly(17, 0), 30);
var slot = appt.FindNextSlot(DateTimeOffset.Parse("2026-09-21T08:00:00Z"), 48);
if (slot is null) throw new InvalidOperationException("no slot");
appt.TryBook(slot.Value);
Console.WriteLine(appt.SmsReminder(slot.Value, "0100"));

Console.WriteLine("Done. Now split responsibilities — without breaking behavior.");
