# Pharmacy Warehouse Management System (WMS) - Requirements Document

## 1. Project Overview
The Pharmacy Warehouse Management System (WMS) is a digital platform designed for the Faculty of Pharmacy at October 6 University (O6U). The system manages the inventory of laboratory chemicals, reagents, and apparatuses. It replaces manual paper-based logs to eliminate human errors, track material expiration dates, enforce automated safety rules, and maintain secure audit logs.

## 2. Core Functional Requirements

### 2.1 User Authentication & Authorization
- **Roles**: Support two roles: Admin (Warehouse Manager) and Storekeeper (Warehouse Staff).
- **Authentication**: Secure login using JWT tokens and PBKDF2 password hashing.
- **Deactivation Guard**: Deactivated users must be blocked from accessing the system.
- **Access Control Matrix**:
  - Admin: CRUD on users, CRUD on products, direct database audits, override approvals.
  - Storekeeper: Submit stock movements (receipt, dispatch, disposal), request expiry changes.

### 2.2 Product Catalog & Batches
- **Catalog CRUD**: Admin creates and manages chemical products with dynamic low-stock thresholds.
- **Batch tracking**: Support tracking multiple batches of the same chemical product with different quantities and expiration dates (1:N mapping).

### 2.3 FEFO (First Expire First Out) Rotation
- **Dispatch Rule**: When chemicals are dispatched (issued to a laboratory), the system must automatically allocate stock from the earliest-expiring batch first.
- **Deduction Engine**: Deduct recursively from sorted batches until the requested quantity is fulfilled. Reject if total available stock is insufficient.

### 2.4 Expiry Change Approvals Workflow
- **Submission**: Storekeepers request expiry modifications on specific batches, providing the new date and a justification reason.
- **Approval Gateway**: Requests remain in a "Pending" state. Admin reviews requests and either Approves (updating batch date) or Rejects.

### 2.5 Offline Resilience & Synchronization
- **Local Cache**: Cache catalogs and pending operations locally using Hive key-value database.
- **Connection Heartbeats**: Monitor active connectivity using ping protocols.
- **Background Sync Queue**: Automatically replay offline-logged transactions to the ASP.NET Core API sequentially when connectivity recovers.

### 2.6 Bilingual PDF Report Generator
- **Bilingual Support**: Dynamic Arabic RTL and English LTR layout rendering.
- **Vouchers**: Generate 5 official university receipt, issue, and disposal PDF vouchers with required signature blocks in Arabic (using Cairo font rendering).

## 3. Infrastructure & DevOps
- **Docker Compose**: Containerized execution environment.
- **Backups**: Daily compressed Postgres dump uploads to AWS S3.
- **Monitoring**: Performance and error observability via Prometheus and Grafana dashboards.
