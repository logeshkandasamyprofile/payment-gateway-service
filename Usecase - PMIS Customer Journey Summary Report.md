# PMIS Customer Journey Summary Report

## Description

Build a customer journey summary report in the PGS Reporting Tool using PMIS logs.

This report enables PGS internal users to analyze the payment-method capture flow,
identify issues, and share data with stakeholders without running manual DB queries.

## User Roles

- Same role that permits viewing current PMIS logs.
- Access is limited to PGS internal team members.

## Requirement

Build a summary of customer journey report in the PGS Reporting Tool based on PMIS logs.

## Business Value

PGS internal users can easily:

- Analyze the payment-method capture flow.
- Identify issues in the journey.
- Share report data with stakeholders without manual DB querying.

## Acceptance Criteria

1. Create an overall summary view with aggregate counts across selected date range and merchant name.
2. Include this report as part of the PGS Reporting Tool.
3. Pull all report data from PMIS logs.
4. Provide drill-down capability to the currently existing detailed journey view.
5. Provide download options for both summary and detail views, excluding XML payloads.
6. Ensure efficient report loading performance for large data volumes.

## Summary View Filters

Provide input filters to generate a filtered summary view:

- Date range (mandatory)
- Merchant name or merchant ID or merchant app ID
- Order ID (optional)

### Merchant Mapping Rule

Merchant ID and merchant name filters must be correctly mapped to each other,
based on the designated mapping table.

## User Flow

1. User selects a valid date range.
2. User optionally applies merchant and order filters.
3. User clicks Next.
4. System displays the summary view with aggregated counts.
5. User can drill down to detail view.
6. User can download summary and detail outputs (excluding XML payloads).
