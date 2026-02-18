# Dialog System Analysis & Roadmap

This document outlines the architecture of our custom Dialog System, consolidating features and patterns from
industry-standard plugins like **Pixel Crushers' Dialogue System**, **DialogNodeBasedSystem (NBDS)**, **Conversa**, and
**DialogGraphEngine**.

## Current Implementation Highlights

- **Metadata-Driven Nodes**: Using `Field.cs` for extensible key-value pairs (inspired by Pixel Crushers).
- **Cinematic Sequencing**: A string-based command parser (`DialogSequencer.cs`) for audio, visuals, and timing.
- **Decoupled UI**: MVC-inspired `DialogUI.cs` that remains independent of conversation logic.
- **Variable Processing**: `DialogTextProcessor` with Regex-based tag replacement (e.g., `[var=Gold]`). (✓)

## Consolidated Feature Requirements

### 1. Visual Node Graph Editor

* **Architecture**: Built using Unity's modern **GraphView API** for better performance and built-in zoom/pan.
* **Organization**: Support for **Stacks** (grouping multiple linear entries) to reduce graph clutter.
* **UX**: **Embedded UI** (TextFields directly in nodes) for inline editing, with **Placeholder Text** to maintain
  clarity.
* **Styling**: Use **USS (Unity Style Sheets)** for themeable, color-coded nodes (logic vs. dialogue).
* **Integration**: Direct **Asset "Ping"** buttons on nodes to quickly locate the underlying data.

### 2. Logic & Variable System

* **Variable Storage**: A centralized `VariablesConfig` defining typed global variables (Int, Bool, Float).
* **Conversation Memory**: **SimStatus** tracking to record if a node has been displayed.
* **Persistence**: Automatic saving of variable states between sessions (prefixed keys).
* **Quest Integration**: entries can directly check/update quest states (e.g., `Quest["FindGold"].State`).
* **Scalability**: **Database Merging** to allow small, level-specific databases to reference a "Master" database.

### 3. Sequencing & Cinematic Control

* **Advanced Sequencer**: String-based parser supporting **Blocking Actions** (e.g., waiting for screen fade).
* **Execution**: Complex commands as temporary **MonoBehaviour instances** for robust `Update` loop support.
* **Flow**: **Data vs. Execution Flow** separation, where value ports provide dynamic data to message fields.
* **Context**: **"Sticky" Actor Context** to automatically inherit speakers from preceding nodes.
* **Timeline**: Dedicated **Dialogue Tracks and Clips** for driving dialogue via Unity Timeline.

### 4. Text Processing & Localization

* **Localization**: **Field-based localization** where translations are stored as metadata (`Text_fr`, `Text_es`).
* **Tags**: Standardized `{variableName}` syntax for dynamic replacement.
* **Styling**: **Emphasis Slots** (e.g., `[em1]`) that look up global style definitions (Color, Bold, Italic).
* **Typewriter**: Tokenization-based parser that handles rich text tags and supports embedded **Pause Tags**.

### 5. Advanced UI & UX Patterns

* **Bark System**: Support for transient lines with **Priority Handling** and **History Randomization**.
* **World-Space Bubbles**: Character-anchored panels with **Viewport Clamping** to keep them on screen.
* **Backlog/History**: A scrollable UI overlay buffering previous lines and player choices for review.

---

## Source-Specific Insights

### Pixel Crushers (The Industry Standard)

* **Field Pattern**: Treat nodes as collections of key-value pairs to ensure future-proof extensibility.
* **DialogueActor**: Centralized component on NPCs for portrait, name, and UI prefab overrides.

### Conversa (Logic & Ports)

* **Attribute-Driven Ports**: Use `[Slot]` attributes on C# properties to automatically generate ports via Reflection.

### NovaVertexCode/DialogGraphEngine (UX Focus)

* **Sub-Asset Pattern**: Consider using Sub-Assets for nodes to keep them modular but contained in one file.

### DialogNodeBasedSystem (Legacy & Persistence)

* **External Functions**: Use a delegate dictionary to bind C# actions to dialogue names at runtime.

### DialogGraphSystem (History Focus)

* **JSON Portability**: Provide tools for exporting/importing conversation data for external authoring.

---

## Roadmap

1. **Phase 1**: Implement `DialogTextProcessor` for variable replacement. (✓)
2. **Phase 2**: Add **SimStatus** tracking and basic variable persistence.
3. **Phase 3**: Implement the **History Backlog** system and **Pause Integration**.
4. **Phase 4**: Add **Bark System** and **World-Space Bubbles** with head-anchoring.
5. **Phase 5**: Refactor `DialogEntry` to support **Field-based localization** and **Attribute-driven ports**.
6. **Phase 6**: Create the **GraphView Editor** with **Stacks**, **Value Nodes**, and **Embedded UI**.
7. **Phase 7**: Build **Timeline Integration** and **Quest System hooks**.