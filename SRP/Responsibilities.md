SRP Responsibilities Review

| Original class | Responsibilities found | Refactored boundary |
|---|---|---|
| `WardBoard` | Bed/patient state, acuity scoring, pager alerts, handoff wording, census CSV | `WardBoard` owns assignments and latest scores; `AcuityScorer`, `WardPager`, `WardHandoffFormatter`, and `WardCensusCsvExporter` own scoring, alert log, handoff text, and export. |
| `CheckoutBasket` | Basket lines/totals, coupon parsing, gift-wrap fee, gift-message copy, payment authorization | `CheckoutBasket` owns lines and totals; `CouponDiscountCalculator`, `GiftWrapPricing`, `GiftMessageFormatter`, and `PaymentAuthorizationStub` own the separate pricing and output rules. |
| `SupportTicket` | Ticket identity/messages, priority heuristics, SLA timing, public reply, escalation text | `SupportTicket` owns ticket state; `TicketPriorityClassifier`, `SupportSlaPolicy`, `PublicReplyFormatter`, and `EscalationBlurbFormatter` own rules and communication formats. |
| `LoanDesk` | Application facts/risk, eligibility, compliance checklist, decision letter, underwriter CSV | `LoanDesk` owns application facts and eligibility; `LoanDocumentPolicy`, `LoanDecisionLetterFormatter`, and `UnderwriterCsvExporter` own checklist and output concerns. |
| `CourseEnrollmentDesk` | Seat/waitlist state, waitlist promotion, welcome markdown, tuition invoice formatting | `CourseEnrollmentDesk` owns enrollment state; `WelcomePacketFormatter` and `TuitionInvoiceFormatter` own customer and finance output. |
| `KitchenTicket` | Order-item storage, allergen rules, ready-time estimate, thermal ticket formatting, expo routing | `KitchenTicket` owns ticket items; `AllergenDetector`, `KitchenEtaEstimator`, `ThermalTicketFormatter`, and `ExpoLanePolicy` own food-safety, operations, printer, and routing rules. |
| `SubscriptionBilling` | Subscription/proration state, invoice numbering, failed-payment policy, dunning email, ledger export | `SubscriptionBilling` owns subscription facts and proration; `InvoiceNumberGenerator`, `DunningPolicy`, `DunningEmailFormatter`, and `SubscriptionLedgerExporter` own numbering and outputs. |
| `WarehousePickList` | Pick-line storage/allocation, walking order, picker instructions, WMS XML | `WarehousePickList` owns requested lines; `StockAllocationPolicy`, `WarehouseRoutePlanner`, `PickerScriptFormatter`, and `WmsXmlBatchSerializer` own allocation, routing, and output. |
| `GradeBook` | Score storage/averaging, letter bands, honor-roll policy, transcript, CSV | `GradeBook` owns scores and average; `LetterGradePolicy`, `HonorRollPolicy`, `TranscriptFormatter`, and `GradeCsvExporter` own policy and presentation. |
| `AppointmentDesk` | Booking state, opening-hours rules, slot search, ICS serialization, SMS copy | `AppointmentDesk` owns booking and slot rules; `IcsCalendarFormatter` and `AppointmentSmsFormatter` own the calendar and messaging formats. |

The original entry points remain as thin delegators where needed so the supplied runner continues to exercise meaningful behavior. Each extracted class has one focused reason to change, and the lab uses no custom interfaces.
