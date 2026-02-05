# Stationpedia Search Function Optimization Report

**Date**: 2025-12-29
**Author**: Claude Sonnet 4.5
**Project**: Stationpedia Ascended Mod
**File**: `StationpediaAscended/mod/src/Harmony/SearchPatches.cs`

---

## Executive Summary

The current search enhancement implementation adds significant value by providing:
- Exact match prioritization
- Category-based grouping
- Filtering of junk items (wreckage, burnt, ruptured)
- Missing result injection

However, performance issues arise with high match counts due to:
1. **Multiple coroutine waits** checking for result stabilization (lines 147-160)
2. **Full page iteration** for every search (lines 277-298)
3. **Nested loops** in category lookup (lines 756-768)
4. **DOM manipulation overhead** creating headers and reordering (lines 781-842)

## Current Performance Bottlenecks

### 1. Result Stabilization Wait Loop (CRITICAL)
**Location**: `DelayedReorganize()` lines 147-160

**Current Code**:
```csharp
for (int i = 0; i < 10; i++)
{
    int currentCount = CountVisibleSearchResults(stationpedia.SearchContents);
    if (currentCount > 0 && currentCount == lastCount)
        break;
    lastCount = currentCount;
    yield return new WaitForSeconds(0.1f);  // Up to 1 second total delay!
}
```

**Problem**:
- Waits up to 1 second (10 × 0.1s) for results to stabilize
- **Every** search triggers this delay
- High match counts = longer vanilla search = longer wait

**Impact**: Major perceived lag, especially with broad searches

---

### 2. Full Page Scan for Missing Results
**Location**: `FindMissingMatches()` lines 270-299

**Current Code**:
```csharp
foreach (var page in Stationpedia.StationpediaPages)  // ALL pages every time!
{
    if (existingPageKeys.Contains(page.Key)) continue;
    if (ShouldHideFromSearch(page)) continue;

    string title = Regex.Replace(page.Title ?? "", "<[^>]+>", "").ToLowerInvariant().Trim();
    bool isExact = title == searchLower;
    bool isWholeWord = wholeWordRegex.IsMatch(title);

    if (isExact || isWholeWord)
        missingPages.Add(page);
}
```

**Problem**:
- Iterates through **ALL** StationpediaPages (~600-800+ items)
- Regex operations on every page title
- HashSet lookups for every page
- Runs on **every search**, even when not needed

**Impact**: O(n) complexity where n = total pages. Scales poorly.

---

### 3. Nested Category Lookup
**Location**: `GetPageCategory()` lines 756-768

**Current Code**:
```csharp
foreach (var listEntry in Stationpedia.DataHandler._listDictionary)
{
    foreach (var categoryEntry in listEntry.Value)
    {
        foreach (var insert in categoryEntry.Value)  // Triple nested!
        {
            if (insert.PageLink == key)
                return categoryEntry.Key;
        }
    }
}
```

**Problem**:
- Triple-nested loops
- Called for **every search result** during scoring (line 583)
- O(n×m×k) complexity

**Impact**: Significant when high match counts × many categories

---

### 4. UI Reordering and Header Creation
**Location**: `ReorderSearchUI()` lines 781-842, `AddCategoryHeader()` lines 844-947

**Problem**:
- Creates new GameObjects for each category header (lines 847-945)
- Sets sibling index for every single result item (lines 801, 811, 833)
- Multiple component additions per header (RectTransform, Image, TextMeshProUGUI, Button, LayoutElement)
- Forces layout rebuild (line 839)

**Impact**: Unity UI operations are expensive. More results = more lag.

---

## Recommended Optimizations

### Priority 1: Eliminate Stabilization Wait Loop (HIGH IMPACT)

**Current**:
```csharp
// Wait up to 1 second for results to stabilize
for (int i = 0; i < 10; i++) {
    yield return new WaitForSeconds(0.1f);
    if (count stable) break;
}
```

**Proposed Solution A - Event-Based Trigger**:
Hook into the vanilla search's completion event instead of polling:
```csharp
// Patch the vanilla search completion to trigger our reorganize directly
[HarmonyPostfix]
[HarmonyPatch(typeof(Stationpedia), "OnSearchComplete")] // If such method exists
public static void OnSearchComplete_Postfix(Stationpedia __instance)
{
    ReorganizeSearchResults(__instance, __instance.SearchField.text);
}
```

**Proposed Solution B - Single Frame Delay**:
If event hook isn't possible, reduce to single frame check:
```csharp
// Wait just one frame after search starts
yield return new WaitForEndOfFrame();
yield return new WaitForEndOfFrame(); // Two frames for safety
ReorganizeSearchResults(Stationpedia.Instance, searchText);
```

**Expected Gain**: 0.5-0.9 second reduction in response time

---

### Priority 2: Cache Page Title Index (HIGH IMPACT)

**Current**: Full scan of all pages every search

**Proposed Solution - Lazy-Loaded Index**:
```csharp
private static Dictionary<string, List<StationpediaPage>> _pageTitleIndex = null;
private static Dictionary<string, List<StationpediaPage>> _pageWordIndex = null;

private static void BuildPageIndexes()
{
    if (_pageTitleIndex != null) return; // Already built

    _pageTitleIndex = new Dictionary<string, List<StationpediaPage>>();
    _pageWordIndex = new Dictionary<string, List<StationpediaPage>>();

    foreach (var page in Stationpedia.StationpediaPages)
    {
        if (ShouldHideFromSearch(page)) continue;

        string title = CleanTitle(page.Title).ToLowerInvariant();

        // Index by full title
        if (!_pageTitleIndex.ContainsKey(title))
            _pageTitleIndex[title] = new List<StationpediaPage>();
        _pageTitleIndex[title].Add(page);

        // Index by individual words
        string[] words = title.Split(' ');
        foreach (string word in words)
        {
            if (!_pageWordIndex.ContainsKey(word))
                _pageWordIndex[word] = new List<StationpediaPage>();
            _pageWordIndex[word].Add(page);
        }
    }
}

private static List<StationpediaPage> FindMissingMatches_Optimized(string searchText, HashSet<string> existingKeys)
{
    BuildPageIndexes(); // Lazy init

    string searchLower = searchText.ToLowerInvariant().Trim();
    var missingPages = new List<StationpediaPage>();

    // O(1) lookup for exact matches
    if (_pageTitleIndex.TryGetValue(searchLower, out var exactPages))
    {
        foreach (var page in exactPages)
        {
            if (!existingKeys.Contains(page.Key))
                missingPages.Add(page);
        }
    }

    // O(1) lookup for whole word matches
    if (_pageWordIndex.TryGetValue(searchLower, out var wordPages))
    {
        foreach (var page in wordPages)
        {
            if (!existingKeys.Contains(page.Key))
                missingPages.Add(page);
        }
    }

    return missingPages;
}
```

**Expected Gain**: O(n) → O(1) lookup. 100-500ms reduction with large page sets.

---

### Priority 3: Cache Category Lookups (MEDIUM IMPACT)

**Current**: Triple-nested loop for each result

**Proposed Solution - Category Cache**:
```csharp
private static Dictionary<string, string> _categoryCache = new Dictionary<string, string>();

private static string GetPageCategory_Cached(StationpediaPage page)
{
    if (page == null) return "Other";

    string key = page.Key ?? "";

    // Check cache first
    if (_categoryCache.TryGetValue(key, out string cachedCategory))
        return cachedCategory;

    // Compute category (existing logic)
    string category = ComputeCategory(page);

    // Cache for next time
    _categoryCache[key] = category;

    return category;
}

// Call this on mod init or first search
private static void PreloadCategoryCache()
{
    foreach (var page in Stationpedia.StationpediaPages)
    {
        GetPageCategory_Cached(page); // Populate cache
    }
}
```

**Expected Gain**: O(n×m×k) → O(1) lookup after first use. 50-200ms reduction.

---

### Priority 4: Optimize UI Reordering (MEDIUM IMPACT)

**Current**: Creates headers on every search, sets sibling index for all results

**Proposed Solution A - Object Pooling**:
```csharp
private static Queue<GameObject> _headerPool = new Queue<GameObject>();

private static GameObject GetOrCreateHeader(RectTransform parent, string text, Color color)
{
    GameObject headerGO;

    if (_headerPool.Count > 0)
    {
        headerGO = _headerPool.Dequeue();
        headerGO.SetActive(true);

        // Update text and color
        var textComponent = headerGO.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
        }
    }
    else
    {
        headerGO = CreateNewHeader(parent, text, color);
    }

    return headerGO;
}

private static void CleanupCategoryHeaders_Pooled()
{
    foreach (var header in _searchCategoryHeaders)
    {
        if (header != null)
        {
            header.SetActive(false);
            _headerPool.Enqueue(header);
        }
    }
    _searchCategoryHeaders.Clear();
}
```

**Proposed Solution B - Batch SetSiblingIndex**:
```csharp
// Instead of calling SetSiblingIndex repeatedly, batch the operations
private static void ReorderSearchUI_Batched(...)
{
    // Build complete order list first
    var orderedTransforms = new List<Transform>();

    // Add headers and results to list in desired order
    // (existing grouping logic)

    // Set all indices in one pass
    for (int i = 0; i < orderedTransforms.Count; i++)
    {
        orderedTransforms[i].SetSiblingIndex(i);
    }

    // Single layout rebuild at the end
    LayoutRebuilder.ForceRebuildLayoutImmediate(searchContents);
}
```

**Expected Gain**: 20-100ms reduction in UI update time

---

### Priority 5: Debounce onValueChanged (LOW IMPACT)

**Current**: Triggers reorganize 0.8s after each keystroke

**Proposed Optimization**:
```csharp
private static void OnSearchValueChanged(string searchText)
{
    if (string.IsNullOrEmpty(searchText))
    {
        CleanupCategoryHeaders();
        return;
    }

    // Only reorganize for 3+ characters AND after typing has stopped
    if (searchText.Length >= 3)
    {
        // Existing debounce logic is already good
        // Consider increasing delay to 1.0s to reduce triggers during typing
        _reorganizeCoroutine = StationpediaAscendedMod.Instance.StartCoroutine(
            DelayedReorganize(searchText, 1.0f)); // Increased from 0.8s
    }
}
```

**Expected Gain**: Reduces unnecessary reorganizations during typing. Minor CPU savings.

---

## Implementation Priority

| Priority | Optimization | Complexity | Est. Time | Impact |
|----------|--------------|------------|-----------|--------|
| **P1** | Eliminate wait loop | Low | 30 min | High (0.5-0.9s) |
| **P2** | Cache page title index | Medium | 2-3 hrs | High (100-500ms) |
| **P3** | Cache category lookups | Low | 1 hr | Medium (50-200ms) |
| **P4** | Object pooling for headers | Medium | 2 hrs | Medium (20-100ms) |
| **P5** | Batch UI operations | Low | 1 hr | Low (10-50ms) |

**Total Expected Improvement**: 680ms - 1.65 seconds faster search

---

## Additional Recommendations

### 1. Add Performance Metrics
```csharp
private static void ReorganizeSearchResults(...)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    // ... existing logic ...

    stopwatch.Stop();
    ConsoleWindow.Print($"[Search Performance] Reorganized {items.Count} results in {stopwatch.ElapsedMilliseconds}ms");
}
```

### 2. Consider Pagination for Large Result Sets
If searches regularly return 100+ results, consider:
- Show top 50 results initially
- "Load More" button for additional results
- Lazy rendering of category groups

### 3. Optimize Regex Usage
Replace repeated `Regex.Replace(title, "<[^>]+>", "")` calls with cached cleaned titles:
```csharp
private static Dictionary<string, string> _cleanedTitles = new Dictionary<string, string>();

private static string CleanTitle(string title)
{
    if (string.IsNullOrEmpty(title)) return "";
    if (_cleanedTitles.TryGetValue(title, out string cleaned))
        return cleaned;

    cleaned = Regex.Replace(title, "<[^>]+>", "").Trim();
    _cleanedTitles[title] = cleaned;
    return cleaned;
}
```

---

## Testing Recommendations

### Before Optimization Baseline
1. Search for "iron" (high match count)
2. Measure: Time from keypress to reorganized results
3. Record: Console logs, frame time

### After Each Optimization
1. Re-run same searches
2. Compare metrics
3. Verify no regressions in functionality

### Test Cases
- **High volume**: "iron", "steel", "cable" (50+ results)
- **Low volume**: "corn", "motherdroid" (1-10 results)
- **Exact match**: "Ice (Oxite)" (should appear first)
- **Partial**: "tool" (should group by category)

---

## Conclusion

The search enhancements add significant value but can be optimized for better performance. Implementing P1 and P2 alone would provide 70-80% of the potential gains with relatively low effort.

**Recommended Next Steps**:
1. Implement P1 (eliminate wait loop) - Quick win
2. Add performance metrics to measure impact
3. Implement P2 (page index) - Largest algorithmic improvement
4. Monitor and iterate based on real-world usage data

---

## Code References

| Component | File | Lines |
|-----------|------|-------|
| Stabilization wait | SearchPatches.cs | 141-163 |
| Missing matches scan | SearchPatches.cs | 270-299 |
| Category lookup | SearchPatches.cs | 737-779 |
| UI reordering | SearchPatches.cs | 781-842 |
| Header creation | SearchPatches.cs | 844-947 |
