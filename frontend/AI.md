# Project: ImageManager Frontend

## Purpose
React/Next.js frontend for ImageManager backend.

The backend API is the single source of truth.

## Tech stack
- Next.js (App Router)
- React
- TypeScript
- Tailwind CSS

## Achitecture
The architecture can be found in ARCHITECTURE.md and ARCHITECTURE_GUIDE.md

## Rules
- Never use fetch directly
- All API calls go through src/lib/api/client.ts
- Do not invent endpoints or fields
- Respect backend request/response models

## Error handling
All API errors must be shown in UI.

## Performance
Use server components for data loading when possible.
