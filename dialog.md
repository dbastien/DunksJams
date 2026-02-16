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
- **The Takeaway**: Implement a `DialogueGraphWindow`. While NBDS uses manual **IMGUI** drawing (`OnGUI`, `DrawGridBackground`), we may prefer Unity's modern **GraphView API** for better performance and built-in zoom/pan.

### 2. Typed Variable Config & Persistence
Unlike our string-only variables, this system uses a `VariablesConfig` asset to define typed variables (Int, Bool, Float).
- **The Takeaway**: 
    - Create a `DialogVariables` ScriptableObject that defines "Global Variables" with default values.
    - Implement **Persistence**: NBDS uses `PlayerPrefs` (prefixed with `V_`) to save variable states between sessions.

### 3. Timeline Integration
A dedicated `DialogueTrack` and `DialogueClip` for Unity Timeline.
- **The Takeaway**: Build a Timeline track that can trigger `DialogManager.PlayEntry()` at specific points in a cinematic.

### 4. Text Processing Tags
Advanced tag replacement in strings (e.g., `{variableName}`, `[lua(math.random(1,10))]`).
- **The Takeaway**: Implement a `DialogTextProcessor` that runs regex on strings before they are sent to the UI. Support for `{var}` syntax is more standard.

### 5. Centralized External Functions
NBDS uses a `DialogExternalFunctionsHandler` to bind C# actions to string names.
- **The Takeaway**: Instead of just `UnityEvents` on nodes, use a delegate dictionary to bind code at runtime. This keeps the data asset clean and the code centralized.

### 6. Official Localization Integration
NBDS includes a `NodeGraphLocalizer` that attempts to interface with Unity's official `Localization` package.
- **The Takeaway**: Ensure our `Field` system can easily map to `LocalizedString` entries.

---

## Lessons from Conversa

### 1. Stack-Based Linear Nodes (Blocks)
Conversa uses a "Stack" node that contains multiple "Blocks" (lines of dialog).
- **The Takeaway**: In our future Graph Editor, allow grouping multiple linear dialog entries into a single visual "Stack" to reduce graph clutter.

### 2. Separated Data Flow (IValueNode)
Conversa cleanly separates **Execution Flow** (connecting nodes that *happen*) from **Data Flow** (connecting nodes that *provide values*).
- **The Takeaway**: Implement a port system where a `Message` field can be connected to a `String Concatenation` node or a `Global Variable` node, allowing dynamic text generation without complex scripting.

### 3. Attribute-Driven Ports
Ports are defined using a `[Slot]` attribute on C# properties.
- **The Takeaway**: Use Reflection in the Graph Editor to automatically generate ports based on attributes in the `DialogEntry` classes. This makes adding new node types (e.g., "Random Number", "Check Item") trivial.

### 4. "Sticky" Actor Context
Instead of assigning an actor to every single node, Conversa can look "up" the stack to find the last defined actor.
- **The Takeaway**: Implement an `ActorContext` that persists through a conversation until changed, reducing redundant data entry for designers.

---

## Lessons from NovaVertexCode/DialogGraphEngine

### 1. ScriptableObject-per-Node Folder Pattern
Unlike other systems that use one large asset, DGE treats a **Project Folder** as a conversation.
- **The Concept**: Each node (Text, Action, Random) is a separate `.asset` file saved within a specific directory.
- **The Takeaway**: While this creates file clutter, it's highly modular and avoids "gigantic asset" corruption risks. We could consider a hybrid where we use **Sub-Assets** inside one main ScriptableObject to get the best of both worlds.

### 2. Embedded GraphView UI
DGE embeds `TextField` elements directly into the node's `mainContainer` in the graph.
- **The Takeaway**: This is a massive UX win. Designers can type dialog directly into the graph without clicking a node and then moving their eyes/mouse to the Inspector. We should definitely implement this.

### 3. Placeholder Text System
It uses a custom placeholder system for UIElements `TextField`.
- **The Takeaway**: Implementing a `Label` that hides when the `TextField` has content makes the "Empty" state of the graph much clearer for designers (e.g., "Actor name...", "Text...").

### 4. Direct Asset "Ping" from Graph
Each node in the graph has a small button (📌) that calls `EditorGUIUtility.PingObject()` on the underlying asset.
- **The Takeaway**: This provides a quick "escape hatch" to find the raw data if the graph editor becomes limiting.

### 5. USS-Based Node Styling
DGE makes extensive use of `AddToClassList` and `.uss` files for node styling.
- **The Takeaway**: We should leverage USS (Unity Style Sheets) for our graph editor to keep the UI code clean and allow for easy "Themes" or color-coding (e.g., green for text, blue for logic).

---

## Roadmap

1.  **Phase 1**: Implement `DialogTextProcessor` for variable replacement.
2.  **Phase 2**: Add `SimStatus` tracking and persistence.
3.  **Phase 3**: Refactor `DialogEntry` to support **Ports/Slots** for data flow.
4.  **Phase 4**: Create a **GraphView** editor supporting **Stacks** and **Value Nodes**.
5.  **Phase 5**: Build Timeline integration clips.