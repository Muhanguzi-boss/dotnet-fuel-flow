# SRS Requirements Coverage

Source: `Petrol_Station_Management_System_Requirements_Muhanguzi_Boss.docx`. The lab scope in the assignment brief is authoritative where it differs from the SRS.

## Functional requirements
| ID | Requirement | Status | Where |
|----|-------------|--------|-------|
| FR-01 | Login / logout | Covered | LoginForm, AuthController (BCrypt) |
| FR-02 | Role-based access | Covered | Session permission map, menu visibility, controller `Need()` checks |
| FR-03 | User administration | Covered | UsersForm, UserController (Admin only) |
| FR-04 | Fuel products | Covered | ProductsForm |
| FR-05 | Tanks | Covered | TanksForm (capacity, current stock, threshold) |
| FR-06 | Pumps / nozzles | Covered | PumpsForm (linked to tank/product) |
| FR-07 | Suppliers | Covered | SuppliersForm, delivery history grid |
| FR-08 | Record delivery | Covered | DeliveriesForm, DeliveryController |
| FR-09 | Stock update on delivery | Covered | One DB transaction; capacity + product/tank checks |
| FR-10 | Stock adjustment | Partial | Tank stock editable by Manager/Admin and audited (old -> new); no separate approval workflow |
| FR-11 | View stock balances | Covered | Dashboard (low stock in red), Stock levels report |
| FR-12 | Pump readings | Covered | ShiftsForm (opening/closing per pump and attendant) |
| FR-13 | Calculate pump sales | Covered | litres = closing - opening; value = litres x price at close time |
| FR-14 | Record fuel sale | Covered | Sales created per pump when a shift closes; Sales view |
| FR-15 | Fuel prices | Covered | PricesForm; history in `fuel_prices`; sales store the price used |
| FR-16 | Validate readings | Covered | closing >= opening enforced (controller + DB CHECK); no correction process |
| FR-17 | Payment recording | Covered | Cash, Mobile Money, Card, Credit with reference |
| FR-18 | Split payments | Out of scope | Several payments can be recorded per shift, but no split-allocation per sale |
| FR-19 | Customer management | Covered | CustomersForm (credit limit, balance) |
| FR-20 | Credit sales | Covered | Credit payment raises balance; rejected above limit |
| FR-21 | Credit payment | Covered | Receive Credit Payment screen |
| FR-22 | Expenses | Covered | ExpensesForm (no attachment) |
| FR-23 | Shift management | Covered | Open/close shift, one open shift at a time |
| FR-24 | Shift reconciliation | Covered | ReconciliationForm: expected vs paid, shortage/overage |
| FR-25 | Dashboard | Covered | MainForm |
| FR-26 | Sales reports | Partial | By product / pump / attendant with date range; not by payment method |
| FR-27 | Stock movement report | Partial | Stock level report only (no opening/closing movement breakdown) |
| FR-28 | Delivery / supplier reports | Covered | Deliveries by supplier (date range) + delivery history grid |
| FR-29 | Expense and cash reports | Covered | Expenses by category, shift reconciliation report |
| FR-30 | Export and print | Out of scope | No PDF/Excel export |
| FR-31 | Search and filtering | Partial | Date-range, user and action filters; no free-text search |
| FR-32 | Audit trail | Covered | `audit_logs`; Audit Log viewer (Admin/Manager) |
| FR-33 | Transaction correction | Covered | Void with reason, Manager/Admin only, rows kept, effects reversed, audited |
| FR-34 | Notifications | Out of scope | Only the red low-stock highlight on the dashboard |
| FR-35 | Backup and restore | Out of scope | No UI; use `pg_dump` / `pg_restore` manually |

## Non-functional requirements
| ID | Status | Notes |
|----|--------|-------|
| NFR-01 Usability | Covered | Consistent forms, friendly validation messages |
| NFR-02 Performance | Covered | Simple indexed queries; not formally measured |
| NFR-03 Security | Covered | BCrypt salted hashes, role checks, parameterised SQL |
| NFR-04 Data integrity | Covered | Input validation, CHECK/FK constraints, transactions for multi-table changes |
| NFR-05 Availability | Not addressed | Single desktop client + local PostgreSQL |
| NFR-06 Reliability | Partial | Controllers return friendly errors; no idempotency tokens against duplicate retries |
| NFR-07 Auditability | Covered | User + timestamp on audited actions and on transactions |
| NFR-08 Maintainability | Covered | MVC, repositories, documented configuration |
| NFR-09 Compatibility | N/A | SRS mentions browsers; lab requires a WinForms desktop app |
| NFR-10 Backup and recovery | Out of scope | Manual `pg_dump` |
| NFR-11 Privacy | Covered | Only operational data stored; role-restricted screens |
| NFR-12 Scalability | Covered | Pumps, tanks, products, users are data, not code |
