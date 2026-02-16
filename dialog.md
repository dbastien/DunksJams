# Dialog System Analysis & Roadmap

This document outlines the architecture of our custom Dialog System and identifies key features to "borrow" from industry-standard plugins like **Pixel Crushers' Dialogue System** and **DialogNodeBasedSystem**.

## Current Implementation Highlights
- **Metadata-Driven Nodes**: Using `Field.cs` for extensible key-value pairs (inspired by Pixel Crushers).
- **Cinematic Sequencing**: A string-based command parser (`DialogSequencer.cs`) for audio, visuals, and timing.
- **Decoupled UI**: MVC-inspired `DialogUI.cs` that remains independent of conversation logic.
- **Basic Logic**: Variable-based branching using simple string comparisons.

---

## Further Lessons from Pixel Crushers

### 1. Localization via Fields
Instead of a separate localization system, Pixel Crushers often uses the Field list.
- **The Concept**: Add fields like `Dialogue Text fr`, `Dialogue Text es`.
- **The Takeaway**: Update our `DialogManager` to look for localized field keys based on a global language setting.

### 2. "SimStatus" (Conversation Memory)
Pixel Crushers tracks whether a node has been displayed (`WasDisplayed`), which is saved in the game state.
- **The Takeaway**: Add a `SimStatus` dictionary to `DialogManager` to allow conditions like `Dialog[5].SimStatus == "WasDisplayed"`.

### 3. Advanced Sequencer Commands
Pixel Crushers has a rich library of commands for cinematic control.
- **The Takeaway**: Expand `DialogSequencer.cs` with:
    - `Animation(animName, [target])`: Play legacy or Animator animations.
    - `Camera(angle, [target], [duration])`: Smoothly move the camera.
    - `Fade(in/out, [duration])`: Screen fades.
    - `LoadLevel(sceneName)`: Change scenes from dialog.
    - `TextInput(variableName)`: Open a prompt for player input.

### 4. Database Merging
Support for multiple small databases that can be loaded/unloaded at runtime.
- **The Takeaway**: Allow `DialogManager` to "mount" multiple `DialogConversation` assets into a master runtime lookup table.

### 5. Quest System Integration
Dialogue entries can directly update quest states (e.g., `Quest["FindGold"].State = "success"`).
- **The Takeaway**: Create a unified `GameVariable` system that both Dialog and Quests can read/write.

---

## Lessons from DialogNodeBasedSystem

### 1. Visual Node Graph Editor
This is the standout feature. Managing long conversations in the Inspector is difficult.
- **The Takeaway**: Implement a `DialogueGraphWindow` using Unity's **GraphView API** to visualize `DialogEntry` connections as nodes.

### 2. Typed Variable Config
Unlike our string-only variables, this system uses a `VariablesConfig` asset to define typed variables (Int, Bool, Float).
- **The Takeaway**: Create a `DialogVariables` ScriptableObject that defines "Global Variables" with default values.

### 3. Timeline Integration
A dedicated `DialogueTrack` and `DialogueClip` for Unity Timeline.
- **The Takeaway**: Build a Timeline track that can trigger `DialogManager.PlayEntry()` at specific points in a cinematic.

### 4. Text Processing Tags
Advanced tag replacement in strings (e.g., `[var=PlayerName]`, `[lua(math.random(1,10))]`).
- **The Takeaway**: Implement a `DialogTextProcessor` that runs regex on strings before they are sent to the UI.

### 5. External Function Nodes
Nodes that trigger specific C# events or delegates without using the Sequencer string.
- **The Takeaway**: Add an `Events` list to `DialogEntry` that designers can hook up via the Inspector (UnityEvents).

---

## Roadmap

1.  **Phase 1**: Implement `DialogTextProcessor` for variable replacement.
2.  **Phase 2**: Add `SimStatus` tracking and persistence.
3.  **Phase 3**: Create a basic **GraphView** editor for conversations.
4.  **Phase 4**: Build Timeline integration clips.