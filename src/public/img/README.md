# Asset Pack — drop point

This folder maps 1:1 to the Morning Routine asset-pack reference sketch.
Drop individual PNG (or SVG) exports here using the exact filenames below
and the app will pick them up. Until a file exists, the code falls back to
the corresponding emoji so nothing 404s in dev.

Suggested dimensions: 256×256 PNG with transparent background for icons,
1024×512 for the journey landscape, 1024×1024 for character art.

## avatars/
Used for the markers on each child's journey path and in the Settings
picker.

- `avatars/rocket.png`
- `avatars/dinosaur.png`
- `avatars/unicorn.png`
- `avatars/car.png`
- `avatars/dog.png`
- `avatars/bicycle.png`

## characters/
The kids themselves (header avatar circle, completion celebration art).

- `characters/kai.png`
- `characters/braxton.png`
- `characters/high-five.png`

## tasks/
Illustrated task-card icons. Each child can pick any of these per task in
Settings; add more by adding more files here.

- `tasks/breakfast.png`
- `tasks/teeth.png`
- `tasks/dressed.png`
- `tasks/bag.png`
- `tasks/bed.png`
- `tasks/shoes.png`

## environments/
Endpoints and middle of the journey landscape.

- `environments/home.png`
- `environments/path.png`
- `environments/school.png`

## journey/
Optional single composite landscape for the tablet "shared path" view.

- `journey/landscape.png`

## awards/
For the future Rewards tab.

- `awards/star.png`
- `awards/trophy.png`
- `awards/medal.png`
- `awards/streak-badge.png`
- `awards/flame.png`

## themes/
Background variants for the journey scene.

- `themes/space.png`
- `themes/dinosaur.png`
- `themes/school.png`
- `themes/city.png`

## decorative/
Sky and accent elements layered into illustrations.

- `decorative/cloud.png`
- `decorative/sun.png`
- `decorative/star-small.png`
- `decorative/sparkle.png`
- `decorative/confetti.png`
- `decorative/bush.png`

---

## Design tokens reproduced in code

**Colour palette** (used in [src/RoutineTracker/SettingsView.fs](../../RoutineTracker/SettingsView.fs)):
`#2563eb` blue · `#16a34a` green · `#facc15` yellow · `#f97316` orange ·
`#9333ea` purple · `#ec4899` pink · `#06b6d4` sky · `#64748b` gray ·
`#1e293b` navy.

**Fonts** (loaded in [src/index.html](../../index.html), bound in
[tailwind.config.js](../../../tailwind.config.js)):
- Header: **Fredoka** (`font-display`, falls back to Nunito).
- Body: **Nunito** (default `font-sans`).

When a file in this folder is added, wire it into the relevant component
by replacing the emoji fallback with an `<img>` tag pointed at e.g.
`/img/avatars/rocket.png`.
