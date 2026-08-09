# Product

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

## Stack

Avalonia UI / .NET 8, selected explicitly by the user.

## Users

Windows or macOS users preparing a clean-system reinstall. They need a dependable checklist to restore their usual applications and license records.

## Product Purpose

「重灌清單」keeps a default post-reinstall application checklist, identifies software already present on the current machine, and preserves paid-software license notes for later restoration.

## Positioning

It combines a practical install-completion checklist with a portable, password-protected record, instead of treating either application lists or serials as separate documents.

## Operating Context

Used before a reinstall to export a record, and after a reinstall while applications are being installed one by one on Windows or macOS.

## Capabilities and Constraints

- The checklist is empty by default. Entries appear only when the user adds them or imports an encrypted backup.
- Users can add, edit, remove, filter, and mark applications as installed.
- Windows checks installed-program registry entries; macOS checks application bundles in `/Applications`.
- Export and import use a password-derived AES-256-GCM encrypted file. Each operation requires the user to enter and confirm the encryption password; the password is never stored in the export file.
- Before a backup is imported, local changes are written to a separate temporary record file. After import, changes are written to the imported record file. License fields should be treated as sensitive.

## Evidence on Hand

No external product assets or app catalogue have been supplied.

## Product Principles

- Make the next installation step obvious.
- Keep sensitive licence details deliberate and portable.
- Remain useful offline and on either desktop operating system.
- Never claim a package is installed without showing the match used.
