# Abyss Survivor Design System

## 1. Atmosphere & Identity

Abyss Survivor feels like a compact dark-fantasy command deck for repeated dungeon runs. The signature is a portrait 390x844 frame with deep violet-black surfaces, restrained gold-orange rewards, and clear stacked panels that make text choices, combat logs, and run resources readable on a phone.

## 2. Color

### Palette

| Role | Token | Light | Dark | Usage |
|------|-------|-------|------|-------|
| Surface/primary | `AbyssDesignTokens.Background` | #0d0d14 | #0d0d14 | Root mobile background |
| Surface/secondary | `AbyssDesignTokens.Panel` | #1a1a29 | #1a1a29 | Panels, cards, screen bodies |
| Surface/elevated | `AbyssDesignTokens.MutedPanel` | #12121f | #12121f | Navigation rows, secondary fields |
| Text/primary | `AbyssDesignTokens.TextPrimary` | #f2efff | #f2efff | Titles and readable body copy |
| Text/secondary | `AbyssDesignTokens.TextMuted` | #a99dc9 | #a99dc9 | Captions, metadata, disabled copy |
| Accent/primary | `AbyssDesignTokens.PurpleAccent` | #8c40ff | #8c40ff | Primary actions, selected screen |
| Accent/reward | `AbyssDesignTokens.OrangeAccent` | #e6731a | #e6731a | Rewards, gold, dangerous choices |

### Rules
- Purple is for navigation and primary action only.
- Orange is for rewards, treasure, and warning/danger emphasis.
- Surface depth comes from tonal shifts, not heavy shadows.

## 3. Typography

### Scale

| Level | Size | Weight | Line Height | Tracking | Usage |
|-------|------|--------|-------------|----------|-------|
| Display | 42px | 800 | 1.05 | 0 | `ABYSS SURVIVOR` title |
| H1 | 28px | 700 | 1.15 | 0 | Screen title |
| H2 | 20px | 700 | 1.25 | 0 | Panel title |
| Body | 15px | 400 | 1.45 | 0 | Default text |
| Body/sm | 13px | 400 | 1.35 | 0 | Metadata and logs |
| Caption | 11px | 600 | 1.2 | 0 | Badges and small labels |

### Font Stack
- Primary: TextMesh Pro default mobile-safe sans, falling back to Unity built-in sans.
- Mono: not used.
- Serif: not used.

### Rules
- Body text must stay at 13px or larger.
- Display text appears only on the title screen.
- Korean and English labels may coexist, but no text should wrap outside a button.

## 4. Spacing & Layout

### Base Unit

All spacing derives from a base of 4px.

| Token | Value | Usage |
|-------|-------|-------|
| `space-2` | 8px | Button inner rhythm, list gaps |
| `space-3` | 12px | Compact panel padding |
| `space-4` | 16px | Standard panel padding |
| `space-6` | 24px | Major vertical gaps |
| `space-8` | 32px | Title to controls |

### Grid
- Reference size: 390x844 portrait.
- Canvas scaler reference resolution: 390x844.
- Page margin: 18px horizontal.
- Screen root: full canvas, one visible screen at a time.

### Rules
- Cards and panels use 8px radius or less.
- No nested cards; use rows and tonal sections inside panels.
- Fixed-format controls use stable heights so labels do not shift layout.

## 5. Components

### Mobile Screen Root
- **Structure**: root panel, header, content stack, action stack, bottom navigation.
- **Variants**: title, town, dungeon, combat, overlay/result.
- **Spacing**: `space-4` panel padding, `space-3` row gap.
- **States**: visible, hidden, selected navigation.
- **Accessibility**: minimum 44px button height.
- **Motion**: instant for MVP; future transitions use opacity/transform only.

### Action Button
- **Structure**: Unity `Button` with panel image and TMP label.
- **Variants**: primary purple, reward orange, secondary muted.
- **Spacing**: 48px fixed height, 14px horizontal padding.
- **States**: default, highlighted, pressed, disabled through Button colors.
- **Accessibility**: high contrast text, explicit label.
- **Motion**: Unity button color transition only.

## 6. Motion & Interaction

### Timing

| Type | Duration | Easing | Usage |
|------|----------|--------|-------|
| Micro | 100ms | ease-out | Button press color transition |
| Standard | 200ms | ease-in-out | Future screen fade |

### Rules
- UI changes must not resize the screen root.
- Button state feedback is color-based in this MVP.
- No layout-property animation.

## 7. Depth & Surface

### Strategy

Tonal-shift. The root background, panels, and secondary rows use darker/lighter violet-black tones with thin accent strips where needed. Shadows are avoided for the MVP because mobile text readability matters more than decorative elevation.
