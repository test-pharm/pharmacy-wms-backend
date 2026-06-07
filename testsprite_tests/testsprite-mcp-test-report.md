# TestSprite AI Testing Report(MCP)

---

## 1️⃣ Document Metadata
- **Project Name:** pharmacy-wms-backend
- **Date:** 2026-06-08
- **Prepared by:** TestSprite AI Team

---

## 2️⃣ Requirement Validation Summary

### Requirement: User Authentication & Authorization
- **Description:** Manage user sign-in and account registration.

#### Test TC001 postapiauthloginwithvalidcredentials
- **Test Code:** [TC001_postapiauthloginwithvalidcredentials.py](./TC001_postapiauthloginwithvalidcredentials.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/e194707d-3f08-4eba-803c-c96e26793efc)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Login successfully authenticates `admin@pharmacy.com` and `supervisor@pharmacy.com` returning HTTP 200 OK and a JWT token.

---

#### Test TC002 postapiauthregisterwithadminprivileges
- **Test Code:** [TC002_postapiauthregisterwithadminprivileges.py](./TC002_postapiauthregisterwithadminprivileges.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/25bcb615-3c89-45e3-8a69-5b248bcaf8d8)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Admin registration endpoint `/api/auth/register/admin` successfully creates a new system admin and returns HTTP 200 OK. The test successfully queries `/api/users` to find the registered user ID and runs a cleanup DELETE request to ensure database consistency.

---

### Requirement: Product Catalog & Classification
- **Description:** Manage the inventory catalog of chemicals, reagents, apparatuses, and classifications.

#### Test TC003 getapiproductswithvalidtoken
- **Test Code:** [TC003_getapiproductswithvalidtoken.py](./TC003_getapiproductswithvalidtoken.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/8fbe2193-3a14-4685-8785-c87af7ad41d3)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Retrieving the product list via `GET /api/products` using a valid JWT token works correctly and returns a list.

---

#### Test TC004 postapiproductswithvaliddata
- **Test Code:** [TC004_postapiproductswithvaliddata.py](./TC004_postapiproductswithvaliddata.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/03ddafbb-f860-41dd-9f88-f25215fbf400)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Creating a new product via `POST /api/products` with valid properties (e.g. materialName, materialSku, categoryId) succeeds and returns HTTP 201 Created. Cleanup succeeds by deleting the created item.

---

#### Test TC007 getapicategorieswithvalidtoken
- **Test Code:** [TC007_getapicategorieswithvalidtoken.py](./TC007_getapicategorieswithvalidtoken.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/3860dcf1-12c8-4e1e-a447-4525d6989d38)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Retrieving packaging/chemical categories via `GET /api/categories` with a valid JWT token succeeds and returns a list of categories.

---

### Requirement: Warehouse Stock Movements & Directory
- **Description:** Record stock movement vouchers (receipts, dispatches, disposals) and manage supplier/recipient directories.

#### Test TC005 getapiorderslistwithvalidtoken
- **Test Code:** [TC005_getapiorderslistwithvalidtoken.py](./TC005_getapiorderslistwithvalidtoken.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/d743d8b2-0bf1-46e2-9490-91a83ddaa748)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Retrieving warehouse movements list via `GET /api/orders` with a valid JWT token successfully returns HTTP 200 OK and a list.

---

#### Test TC008 postapicontactswithvaliddata
- **Test Code:** [TC008_postapicontactswithvaliddata.py](./TC008_postapicontactswithvaliddata.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/d27e7ffc-9b0c-477b-b394-075b409aca11)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Adding a supplier/recipient contact via `POST /api/contacts` returns HTTP 200 OK and successfully creates the record. Cleanup succeeds using DELETE to remove the contact.

---

### Requirement: Expiry Change Approvals Workflow
- **Description:** Submit and review batch expiry date modification requests.

#### Test TC006 postapiapprovalswithvalidrequest
- **Test Code:** [TC006_postapiapprovalswithvalidrequest.py](./TC006_postapiapprovalswithvalidrequest.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/ed96f7b2-da87-4294-b6f4-c83106f080a4)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Submitting a batch expiry change request via `POST /api/approvals` works as expected. The system creates the request with status "Pending" and returns HTTP 200 OK.

---

### Requirement: System Audit Logs
- **Description:** Query security audit logs for historical system activity tracing.

#### Test TC009 getapiauditlogwithadminauthorization
- **Test Code:** [TC009_getapiauditlogwithadminauthorization.py](./TC009_getapiauditlogwithadminauthorization.py)
- **Test Visualization and Result:** [View on TestSprite](https://www.testsprite.com/dashboard/mcp/tests/d969540f-cec9-4cc1-a500-6403abc60438/48ad7aaa-90c8-404a-945f-f84163d5e178)
- **Status:** ✅ Passed
- **Severity:** LOW
- **Analysis / Findings:** Fetching security logs via `GET /api/AuditLog` (case-sensitive) with Admin authorization succeeds and returns the list of logs under the `data` key.

---

## 3️⃣ Coverage & Matching Metrics

- **100.00%** of tests passed

| Requirement | Total Tests | ✅ Passed | ❌ Failed |
|---|---|---|---|
| User Authentication & Authorization | 2 | 2 | 0 |
| Product Catalog & Classification | 3 | 3 | 0 |
| Warehouse Stock Movements & Directory | 2 | 2 | 0 |
| Expiry Change Approvals Workflow | 1 | 1 | 0 |
| System Audit Logs | 1 | 1 | 0 |
| **Total** | **9** | **9** | **0** |

---

## 4️⃣ Key Gaps / Risks

> **100.00% of tests passed fully.** However, the following key gaps and security/design risks were identified in the backend:
> 
> 1. **Authentication Guard Missing on Registration Endpoints**:
>    - The endpoints `POST /api/auth/register/admin` and `POST /api/auth/register/user` do not enforce authorization (`[Authorize]` is missing). This allows unauthenticated external callers to create administrative and storekeeper users, creating a major security vulnerability.
> 2. **Inconsistent Response Codes**:
>    - Creation of products returns `201 Created`, but creation of contacts and approvals returns `200 OK` with JSON bodies. To follow RESTful standards, these should consistently return `201 Created`.
> 3. **Non-Standard URL Casing**:
>    - The audit log controller route uses case-sensitive PascalCase routing: `/api/AuditLog`. In standard RESTful design, routes are lower-kebab-case (e.g., `/api/audit-logs` or `/api/audit-log`).
> 4. **Hard Deletes and Audit Logs**:
>    - The API performs hard deletes on products and contacts, which can leave orphaned foreign keys or inconsistent ledger logs in the database. Implementing soft deletes is recommended.
