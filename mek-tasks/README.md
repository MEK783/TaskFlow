
# MEK Tasks – React + Tailwind project skeleton

A starter project that matches the UI/UX you specified:
- Responsive 3-column board (To do, In Progress, Finished) with vertical borders on large screens; single-tab on small screens
- Navbar with site logo + GitHub/LinkedIn under the title; right side order: Theme toggle, Invite (auth), Help, Login/Logout (green when Log in, red when Log out)
- Round buttons across the interface
- Light mode uses off-white background (`#FAFAF7`) and pastel accents; Dark mode uses a black diagonal gradient and neon accents
- Watermark (bottom-right) rotated 15°; place your monochrome logo (with motifs) at `public/watermark.svg` to override the placeholder
- Tasks are collapsible, editable (title + rich text), with subtasks; only one subtask expanded at a time
- Drag-and-drop library is set in `package.json` (`@dnd-kit/*`), wire in as needed

> **Note:** This skeleton wires state, layout and styling. Authentication, persistence (API/DB), and real drag-and-drop reordering between columns can be added on top.

## Getting started

```bash
npm install
npm run dev
```

## Replace assets
- **Site logo**: `src/assets/app-logo.svg` (web app logo, not your personal one)
- **Watermark**: Put your monochrome logo *with motifs* in `public/watermark.svg` (or `watermark.png`) – the app references `/watermark.svg`.

## Theming
Tailwind config defines `mek` (neon) and `meklight` (pastel) palettes. Dark mode is toggled by a class on `<html>` and persisted in `localStorage`.

## Routes
- `/login` – Login landing page
- `/register` – New user (username, password, referral code)
- `/app` – Main board

## Where to extend
- **Drag & Drop**: connect `@dnd-kit` in `TaskColumn` to enable dragging between lists and reordering.
- **Persistence**: replace Zustand-only state with API calls; keep optimistic updates.
- **Auth**: swap the demo login for real auth (e.g., JWT/OAuth) and gate routes with guards.
- **Invites**: connect to backend to issue and track claim status.
- **Accessibility**: ensure focus states and keyboard DnD fallbacks.

## License
MIT
