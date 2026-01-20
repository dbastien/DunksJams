# DLog Enhancement Plan - UberLogger Integration

This document outlines potential enhancements to the DLog system based on ideas from the UberLogger system found in the `.working` folder. UberLogger provides a more sophisticated logging architecture that could significantly improve DLog's capabilities.

## Current DLog System Analysis

### Strengths
- ✅ Simple, Unity-integrated API
- ✅ Automatic file logging with timestamps
- ✅ Custom console window with hyperlink navigation
- ✅ Timing/profiling functionality
- ✅ Performance-conscious (early returns when disabled)
- ✅ Unity 6 compatibility

### Current Limitations
- ❌ No backend abstraction (hardcoded ConsoleSink/FileSink)
- ❌ No filtering system
- ❌ No channel/tag system for categorizing logs
- ❌ No runtime console window
- ❌ No regex search capabilities
- ❌ No log persistence/loading
- ❌ Limited stack trace manipulation
- ❌ No conditional compilation support

### Detailed Current System Analysis

**Location**: `Assets/Scripts/DLog.cs`

#### Parity Issues with Debug.Log

| Feature | Debug.Log | DLog | Status |
|---------|-----------|------|--------|
| Log(object) | ✓ | ✗ | Commented out line 47 |
| Log(object, Object) | ✓ | ✗ | Missing |
| LogWarning(object) | ✓ | ✗ | LogW is string only |
| LogWarning(object, Object) | ✓ | ✗ | Missing |
| LogError(object) | ✓ | ✗ | LogE is string only |
| LogError(object, Object) | ✓ | ✗ | Missing |
| LogFormat(string, params) | ✓ | ✗ | Missing |
| LogAssertion(object) | ✓ | ✗ | Missing |
| Assert(bool) | ✓ | ✗ | Missing |
| Break() | ✓ | ✗ | Missing |
| Conditional compilation | ✓ | ✗ | No #if UNITY_EDITOR support |

#### Specific Current Issues

##### ✅ Time() Performance Fixed
**Status**: Added early return when `IsLoggingEnabled` is false to avoid expensive timing operations.

##### Missing Features
- No `Log(object message)` overload (most common Unity pattern)
- No conditional logging by category/tag
- No remote logging
- No SettingsProvider integration

##### Performance Issues
- `Colorize()` allocates strings even when `IsColorEnabled = false` could be checked earlier

##### Unity Console Navigation Limitations
**Status**: Documented - these are Unity design limitations, not code bugs

- **Console navigation uses stack traces**: Double-clicking log entries navigates to the top of the stack trace (DLog.cs), not to hyperlinks or caller locations
- **Hyperlinks in main messages don't work**: Unity console only supports hyperlinks in expanded stack trace details, not in the main message text
- **No control over navigation behavior**: Unity's console has fixed navigation logic based on internal stack trace analysis
- **Navigation requires stack trace expansion**: Users must expand stack traces to access working hyperlinks

**Root Cause**: Unity's console navigation is hardcoded to use stack trace information, with logging framework code (DLog.cs) appearing at the top of traces.

**Comparison with Unity's Debug.Log**:
- **Unity Debug.Log**: Double-click navigates to source file (DLogTest.cs)
- **DLog**: Double-click navigates to logging framework (DLog.cs) ❌

**Workaround**: DLog provides readable file:line info in main messages and working hyperlinks in stack traces, which is superior to Unity's Debug.Log (no navigation info at all). Users must expand stack traces for proper navigation.

**Hyperlink Status**:
- ✅ **Stack trace hyperlinks only**: Work perfectly in expanded stack traces
- ❌ **Main message hyperlinks**: Impossible (Unity console limitation)
- ✅ **Main message caller info**: Shows file:line as readable text
- ✅ **File paths**: Correctly formatted for Unity's hyperlink system

##### ✅ Custom Console Window: IMPLEMENTED
**Status**: Completed - Custom console with working hyperlinks

**Location**: `Assets/Editor/DLogConsole.cs`

**Current Features**:
- ✅ Intercept logs via `Application.logMessageReceived`
- ✅ Display with proper file hyperlinks in main messages
- ✅ Colored file:line references (cyan) in stack traces
- ✅ Double-click navigation on log entries and stack trace lines
- ✅ Right-click context menus for copying text
- ✅ Basic filtering by log type (Info/Warnings/Errors)
- ✅ Text search filtering
- ✅ Menu item: Window > DLog Console
- ✅ API: `DLog.OpenConsole()`

**Usage**: Use `DLog.OpenConsole()` or Window > DLog Console for proper hyperlink navigation that Unity's built-in console cannot provide.

**Navigation Methods**:
- **Double-click**: Click any log entry or stack trace line to navigate to the referenced file
- **Visual cues**: File:line references are colored cyan
- **Right-click**: Copy messages, full entries, or individual stack trace lines

**Hyperlink Status**:
- ✅ **Custom console hyperlinks**: Work perfectly in main messages and stack traces
- ✅ **Unity console hyperlinks**: Work in expanded stack traces only
- ❌ **Unity console main message hyperlinks**: Impossible (Unity limitation)

##### Console Feature Comparison

**Current DLog Console vs Console Pro**:

**✅ Already Implemented:**
- Click-to-navigate to source code (basic version)
- Filter by log type
- Search/filter by text

**🔄 Could Implement (from Console Pro):**
- **Regex search**: More powerful text filtering
- **Custom filter groups**: Color-coded categories beyond Info/Warning/Error
- **Source code preview**: Show surrounding code context for each log
- **Column display**: File name, namespace, GameObject, class columns
- **Ignore filters**: Hide unwanted log sources permanently
- **Variable watching**: Collapsible logs for changing values
- **Remote logging**: Logs from standalone/phone builds
- **Log file parsing**: Import Editor.log or other log files
- **Custom fonts/colors**: Enhanced visual customization
- **Export functionality**: Save logs to file with one click
- **Status bar override**: Custom status bar click behavior

**❌ Unity Integration Challenges:**
- **Stack trace source viewing**: Would require parsing .pdb files or IL code
- **Advanced stack filtering**: Complex interaction with Unity's logging system

**Priority Implementation Candidates:**
1. **Regex search** - Easy to add, very useful
2. **Custom filter groups** - Medium complexity, high value
3. **Column display** - Medium complexity, improves readability
4. **Source code preview** - High complexity, very valuable
5. **Ignore filters** - Easy to implement, reduces noise

**Effort Estimate for Priority Features**: 1-2 weeks additional development

---

## Integration Issues

### DLog Usage in Other Systems

#### DataPort.SetData Excessive Logging
**Location**: `Assets/Editor/GraphView/SerializedGraphNode.cs:69-73`
```csharp
public void SetData(T data)
{
    _data = data;
    DLog.Log($"{portName} received data: {data}");
}
```
**Problem**: Logs on every data set - noisy and potential performance issue.
**Solution**: Add conditional logging or rate limiting for data port updates.

### Debug.Log Usage Replacements Needed

The following files still use `Debug.Log` instead of `DLog.Log` and should be updated:

| File | Line(s) | Count | Notes |
|------|---------|-------|--------|
| Weapon.cs | 62, 175, 185 | 3 | Gameplay system |
| SaveSystem.cs | 30, 50 | 2 | Error logging |
| CardGameManager.cs | 79, 132 | 2 | Gameplay system |
| DataUtils.cs | 56, 60 | 2 | Utility system |
| ReflectionUtils.cs | 279 | 1 | Utility system |
| WaveManager.cs | 54 | 1 | Gameplay system |
| Screens.cs | 21, 22, 31 | 3 | UI system |
| Health.cs | 112 | 1 | Gameplay system |
| DevConsole.cs | 99 | 1 | Development tool |
| EventManager.cs | 103 | 1 | Core system |
| **Total** | | **17** | |

**Migration Strategy**:
1. Replace `Debug.Log()` with `DLog.Log()`
2. Replace `Debug.LogWarning()` with `DLog.LogW()`
3. Replace `Debug.LogError()` with `DLog.LogE()`
4. Test each replacement to ensure proper logging behavior
5. Remove any redundant context objects that DLog handles automatically

---

## UberLogger Architecture Overview

### Core Components

#### 1. ILogger Interface
```csharp
public interface ILogger
{
    void Log(LogInfo logInfo);
}
```
**Benefits**: Allows multiple logging backends (console, file, network, analytics, etc.)

#### 2. IFilter Interface
```csharp
public interface IFilter
{
    bool ApplyFilter(string channel, UnityEngine.Object source, LogSeverity severity, object message, params object[] par);
}
```
**Benefits**: Runtime filtering of log messages before they reach backends.

#### 3. LogInfo Structure
Rich logging information including:
- Source Unity Object
- Channel/Category
- Severity level
- Full callstack with file/line info
- Timestamps (absolute + relative)
- Formatted messages

#### 4. Attributes for Control
```csharp
[StackTraceIgnore]     // Exclude methods from stack traces
[LogUnityOnly]         // Prevent UberLogger from handling certain logs
```
**Benefits**: Fine-grained control over logging behavior.

---

## Proposed DLog Enhancements

### Phase 1: Core Architecture Improvements

#### 1. Backend Abstraction
**Current**: Hardcoded sinks in DLog class
**Proposed**: ILogger interface with pluggable backends

```csharp
public interface IDLogSink
{
    void Log(LogType logType, string message, Object context, DLogInfo info);
}

public class DLogInfo
{
    public string Channel;
    public List<DLogStackFrame> Callstack;
    public DLogStackFrame OriginatingSourceLocation;
    public double RelativeTimeStamp;
    public DateTime AbsoluteTimeStamp;
    // ... additional metadata
}
```

**Benefits**:
- Multiple simultaneous sinks (console + file + network)
- Easy to add custom backends
- Better separation of concerns

#### 2. Channel System
**Current**: No categorization
**Proposed**: Channel-based logging like UberLogger

```csharp
public static void Log(string channel, string msg, Object ctx = null)
// Or with fluent API
DLog.Channel("Network").Log("Connection established");
DLog.Channel("AI").Warning("Pathfinding failed");
```

**Benefits**:
- Filter logs by subsystem (UI, Audio, Network, etc.)
- Different log levels per channel
- Better organization in large projects

#### 3. Filtering System
**Current**: All or nothing (IsLoggingEnabled)
**Proposed**: IFilter interface for conditional logging

```csharp
public interface IDLogFilter
{
    bool ShouldLog(string channel, LogType type, string message, Object context);
}

// Example filters
public class ChannelFilter : IDLogFilter { /* Filter by channel */ }
public class SourceFilter : IDLogFilter { /* Filter by GameObject/script */ }
public class PerformanceFilter : IDLogFilter { /* Sample logging for performance */ }
```

**Benefits**:
- Runtime log filtering without code changes
- Conditional logging based on build type/channel
- Performance filtering (sample every Nth log)

### Phase 2: Enhanced Features

#### 4. Runtime Console Window
**Current**: Editor-only console
**Proposed**: In-game console like UberLoggerAppWindow

**Features to port**:
- Toggle button in corner of screen
- Full log viewing in builds
- Channel filtering in runtime
- Error highlighting
- Configurable UI skin

**Benefits**:
- Debug builds without attaching debugger
- User-reported issue investigation
- Development testing on device

#### 5. Advanced Stack Trace Features
**Current**: Basic stack trace display
**Proposed**: UberLogger's advanced stack trace manipulation

**Features**:
- `[DLogIgnore]` attribute to hide internal methods
- Source code preview in console (show surrounding lines)
- Better stack frame formatting
- Clickable stack navigation

#### 6. Regex Search and Filtering
**Current**: No search capabilities
**Proposed**: Regex-based message filtering

```csharp
// In console window
FilterRegex = "Player.*died";  // Show only player death messages
FilterRegex = "Exception|Error"; // Show only errors/exceptions
```

**Benefits**:
- Quickly find specific log patterns
- Filter out noise during debugging
- Complex search patterns

### Phase 3: Advanced Capabilities

#### 7. Log Persistence and Loading
**Current**: Logs only in memory during session
**Proposed**: Save/load log sessions

```csharp
DLog.SaveSession("debug_session.json");
DLog.LoadSession("debug_session.json");
```

**Benefits**:
- Analyze logs across sessions
- Share logs between team members
- Post-mortem analysis of crashes

#### 8. Remote Logging
**Current**: Local only
**Proposed**: Network logging backend

```csharp
public class RemoteSink : IDLogSink
{
    void Log(LogType type, string msg, Object ctx, DLogInfo info)
    {
        SendToRemoteServer(info);
    }
}
```

**Benefits**:
- Log aggregation from multiple devices
- Real-time monitoring
- Cloud-based log analysis

#### 9. Analytics Integration
**Proposed**: Backend that feeds into analytics systems

```csharp
public class AnalyticsSink : IDLogSink
{
    void Log(LogType type, string msg, Object ctx, DLogInfo info)
    {
        if (type == LogType.Error)
            Analytics.LogError(info.Channel, msg, info.Callstack);
    }
}
```

**Benefits**:
- Automatic error reporting
- Usage analytics from logs
- Performance monitoring

### Phase 4: Developer Experience

#### 10. Settings Provider Integration
**Current**: TODO comment mentions SettingsProvider
**Proposed**: Unity Settings window integration

Add to `Project Settings > DLog`:
- Enable/disable channels globally
- Set log levels per channel
- Configure file logging paths
- Set performance sampling rates
- Configure remote endpoints

#### 11. Conditional Compilation Support
**Current**: No #if support
**Proposed**: Build-time log stripping

```csharp
#if DLOG_NETWORK_LOGS
DLog.Channel("Network").Log("Connection details...");
#endif
```

**Benefits**:
- Zero runtime cost for disabled logs
- Different log levels per build type
- Automatic log stripping in release builds

#### 12. Visual Enhancements
**From UberLogger**:
- Color-coded channels
- Icons for different log types
- Collapsible log groups
- Log count badges
- Better typography and spacing

---

## Implementation Strategy

### Incremental Approach
1. **Phase 1**: Core architecture (backends, channels, basic filtering)
2. **Phase 2**: UI enhancements (runtime console, regex search)
3. **Phase 3**: Advanced features (persistence, remote logging)
4. **Phase 4**: Polish and integrations

### Backward Compatibility
- Keep existing DLog.Log() API working
- Make new features opt-in
- Provide migration path from current system

### Testing Strategy
- Unit tests for new interfaces
- Integration tests with multiple backends
- Performance benchmarks
- Compatibility tests with existing code

---

## Comparison: DLog vs UberLogger

| Feature | Current DLog | UberLogger | Enhancement Priority |
|---------|-------------|------------|---------------------|
| Backend Abstraction | ❌ | ✅ | **HIGH** |
| Channel System | ❌ | ✅ | **HIGH** |
| Filtering | ❌ | ✅ | **MEDIUM** |
| Runtime Console | ❌ | ✅ | **MEDIUM** |
| Regex Search | ❌ | ✅ | **LOW** |
| Log Persistence | ❌ | ❌ | **LOW** |
| Remote Logging | ❌ | ❌ | **LOW** |
| Stack Manipulation | ❌ | ✅ | **MEDIUM** |
| Source Preview | ❌ | ✅ | **LOW** |
| Settings Integration | ❌ | ❌ | **MEDIUM** |

---

## Migration Path

### For Existing Users
```csharp
// Current code continues to work
DLog.Log("Hello world");

// New features are opt-in
DLog.Channel("UI").Log("Button clicked");
DLog.WithFilter(myFilter).Log("Conditional log");
```

### For New Projects
```csharp
// Modern API
var logger = DLog.Channel("Gameplay")
    .WithMinLevel(LogType.Warning)
    .WithFilter(gameplayFilter);

logger.Info("Player spawned");
logger.Error("Invalid state");
```

---

## Success Metrics

- **Maintainability**: Easier to extend and modify
- **Performance**: Better filtering reduces overhead
- **Developer Experience**: Faster debugging with advanced features
- **Scalability**: Handles larger projects with channel organization
- **Compatibility**: No breaking changes for existing users

This enhancement plan would transform DLog from a simple logging utility into a comprehensive logging framework comparable to professional logging systems, while maintaining its Unity-specific advantages and simplicity.