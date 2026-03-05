using Spectre.Console;

internal static class FileExplorer
{
    // ── Key constants ──────────────────────────────────────────────────────────
    private const string KeyExit      = "__EXIT__";
    private const string KeyUp        = "__UP__";
    private const string KeyNewFile   = "__NEW_FILE__";
    private const string KeyNewFolder = "__NEW_FOLDER__";
    private const string DirPrefix    = "DIR::";
    private const string FilePrefix   = "FILE::";

    private static string _root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    // ── Entry point ────────────────────────────────────────────────────────────
    public static void Run()
    {
        _root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        while (true)
        {
            AnsiConsole.Clear();
            RenderHeader();

            var choices = BuildChoices();

            string selection;
            try
            {
                selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Navigate with [cyan]↑↓[/] · [cyan]Enter[/] to open · type to filter[/]")
                        .PageSize(24)
                        .MoreChoicesText("[grey](Scroll to reveal more entries)[/]")
                        .EnableSearch()
                        .SearchPlaceholderText("[grey]🔍 Filter entries...[/]")
                        .HighlightStyle(Style.Parse("bold cyan"))
                        .UseConverter(FormatEntry)
                        .AddChoices(choices));
            }
            catch (Exception)
            {
                // Ctrl+C or terminal resize edge cases
                break;
            }

            AnsiConsole.Clear();

            switch (selection)
            {
                case KeyExit:
                    return;

                case KeyUp:
                    NavigateUp();
                    break;

                case KeyNewFile:
                    CreateEntry(isFolder: false);
                    break;

                case KeyNewFolder:
                    CreateEntry(isFolder: true);
                    break;

                default:
                    if (selection.StartsWith(DirPrefix))
                        HandleFolderSelection(selection[DirPrefix.Length..]);
                    else if (selection.StartsWith(FilePrefix))
                        HandleFileSelection(selection[FilePrefix.Length..]);
                    break;
            }
        }
    }

    // ── Navigation ─────────────────────────────────────────────────────────────
    private static void NavigateUp()
    {
        var parent = Directory.GetParent(_root);
        if (parent is not null)
            _root = parent.FullName;
        else
            AnsiConsole.MarkupLine("[grey]Already at the filesystem root.[/]");
    }

    private static void HandleFolderSelection(string folderName)
    {
        var fullPath = Path.Combine(_root, folderName);

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold]📁 {Markup.Escape(folderName)}[/]")
                .HighlightStyle(Style.Parse("bold cyan"))
                .AddChoices("Open", "Rename", "Delete", "Cancel"));

        switch (action)
        {
            case "Open":
                _root = fullPath;
                break;
            case "Rename":
                RenameEntry(fullPath, isFolder: true);
                break;
            case "Delete":
                DeleteEntry(fullPath, isFolder: true);
                break;
        }
    }

    private static void HandleFileSelection(string fileName)
    {
        var fullPath = Path.Combine(_root, fileName);
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var isText = ext is ".txt" or ".md" or ".json" or ".xml" or ".csv" or ".yaml"
                         or ".yml" or ".log" or ".cs" or ".js" or ".ts" or ".html"
                         or ".css" or ".sh" or ".py" or ".toml" or ".ini" or ".cfg" or ".env" or "";

        var choices = new List<string>();
        if (isText) choices.Add("View Content");
        choices.Add("Rename");
        choices.Add("Delete");
        choices.Add("Cancel");

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold]📄 {Markup.Escape(fileName)}[/]  [grey dim]{FormatSize(fullPath)}[/]")
                .HighlightStyle(Style.Parse("bold cyan"))
                .AddChoices(choices));

        switch (action)
        {
            case "View Content":
                ViewFile(fullPath);
                break;
            case "Rename":
                RenameEntry(fullPath, isFolder: false);
                break;
            case "Delete":
                DeleteEntry(fullPath, isFolder: false);
                break;
        }
    }

    // ── Choice building ────────────────────────────────────────────────────────
    private static List<string> BuildChoices()
    {
        var list = new List<string>
        {
            KeyUp,
            KeyNewFile,
            KeyNewFolder,
            KeyExit,
        };

        // Directories first, sorted case-insensitively
        try
        {
            foreach (var dir in Directory.GetDirectories(_root)
                                         .Select(Path.GetFileName)
                                         .Where(n => n is not null)
                                         .Order(StringComparer.OrdinalIgnoreCase))
                list.Add(DirPrefix + dir);
        }
        catch { /* permission denied */ }

        // Files next, sorted
        try
        {
            foreach (var file in Directory.GetFiles(_root)
                                          .Select(Path.GetFileName)
                                          .Where(n => n is not null)
                                          .Order(StringComparer.OrdinalIgnoreCase))
                list.Add(FilePrefix + file);
        }
        catch { /* permission denied */ }

        return list;
    }

    // ── Display converter (used by SelectionPrompt.UseConverter) ───────────────
    private static string FormatEntry(string key) => key switch
    {
        KeyUp        => "[grey] ↑  ..  Go Up[/]",
        KeyNewFile   => "[bold green] +  New File[/]",
        KeyNewFolder => "[bold blue] +  New Folder[/]",
        KeyExit      => "[bold red] ✕  Exit Explorer[/]",
        _ when key.StartsWith(DirPrefix)  => $"[blue] 📁  {Markup.Escape(key[DirPrefix.Length..])}[/]",
        _ when key.StartsWith(FilePrefix) => $"[white] 📄  {Markup.Escape(key[FilePrefix.Length..])}[/]",
        _ => key,
    };

    // ── Header ─────────────────────────────────────────────────────────────────
    private static void RenderHeader()
    {
        // Breadcrumb path segments
        var parts = _root.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        var breadcrumb = string.Join(
            $"[grey] {Path.DirectorySeparatorChar} [/]",
            parts.Select(p => $"[white]{Markup.Escape(p)}[/]"));

        // Entry counts
        int dirs = 0, files = 0;
        try { dirs  = Directory.GetDirectories(_root).Length; } catch { }
        try { files = Directory.GetFiles(_root).Length;       } catch { }

        var stats = $"[grey]{dirs} folder{(dirs == 1 ? "" : "s")}, {files} file{(files == 1 ? "" : "s")}[/]";

        var content = new Rows(
            new Markup($" {breadcrumb}"),
            new Markup($" {stats}"));

        AnsiConsole.Write(
            new Panel(content)
                .Header("[aqua bold] 🗂  File Explorer [/]", Justify.Center)
                .Border(BoxBorder.Rounded)
                .Expand());

        AnsiConsole.WriteLine();
    }

    // ── File viewer ────────────────────────────────────────────────────────────
    private static void ViewFile(string path)
    {
        AnsiConsole.Clear();

        try
        {
            const int maxLines = 60;
            var allLines = File.ReadLines(path).Take(maxLines + 1).ToList();
            var truncated = allLines.Count > maxLines;
            if (truncated) allLines.RemoveAt(allLines.Count - 1);

            var content = string.Join("\n", allLines.Select(l => Markup.Escape(l)));
            if (truncated) content += "\n[grey]… (truncated — showing first 60 lines)[/]";

            AnsiConsole.Write(
                new Panel(new Markup(content))
                    .Header($"[aqua bold] {Markup.Escape(Path.GetFileName(path))} [/]", Justify.Center)
                    .Border(BoxBorder.Rounded)
                    .Expand());
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not read file:[/] {Markup.Escape(ex.Message)}");
        }

        Pause();
    }

    // ── Rename ─────────────────────────────────────────────────────────────────
    private static void RenameEntry(string path, bool isFolder)
    {
        var currentName = Path.GetFileName(path) ?? path;

        var newName = AnsiConsole.Prompt(
            new TextPrompt<string>("New name:")
                .DefaultValue(currentName)
                .PromptStyle("cyan")
                .Validate(name =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                        return ValidationResult.Error("Name cannot be empty.");
                    if (name.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
                        return ValidationResult.Error("Name contains invalid characters.");
                    return ValidationResult.Success();
                }));

        if (newName == currentName) return;

        try
        {
            var dir     = Path.GetDirectoryName(path)!;
            var newPath = Path.Combine(dir, newName);

            if (isFolder) Directory.Move(path, newPath);
            else          File.Move(path, newPath);

            AnsiConsole.MarkupLine($"[green]✓ Renamed to[/] [bold]{Markup.Escape(newName)}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }

        Pause();
    }

    // ── Delete ─────────────────────────────────────────────────────────────────
    private static void DeleteEntry(string path, bool isFolder)
    {
        var name = Path.GetFileName(path) ?? path;

        AnsiConsole.MarkupLine($"[yellow]This will permanently delete[/] [bold]{Markup.Escape(name)}[/].");
        if (isFolder) AnsiConsole.MarkupLine("[yellow]All contents inside the folder will also be removed.[/]");

        var confirmed = AnsiConsole.Confirm("[red]Are you sure?[/]", defaultValue: false);
        if (!confirmed) return;

        try
        {
            if (isFolder) Directory.Delete(path, recursive: true);
            else          File.Delete(path);

            AnsiConsole.MarkupLine($"[green]✓ Deleted[/] [bold]{Markup.Escape(name)}[/].");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }

        Pause();
    }

    // ── Create new file or folder ──────────────────────────────────────────────
    private static void CreateEntry(bool isFolder)
    {
        AnsiConsole.Clear();
        RenderHeader();

        var label = isFolder ? "folder" : "file";
        var icon  = isFolder ? "📁" : "📄";

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>($"New {label} name {icon}:")
                .PromptStyle("cyan")
                .Validate(n =>
                {
                    if (string.IsNullOrWhiteSpace(n))
                        return ValidationResult.Error("Name cannot be empty.");
                    if (n.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
                        return ValidationResult.Error("Name contains invalid characters.");
                    return ValidationResult.Success();
                }));

        var target = Path.Combine(_root, name);

        try
        {
            if (isFolder)
            {
                Directory.CreateDirectory(target);
                AnsiConsole.MarkupLine($"[green]✓ Folder created:[/] [bold]{Markup.Escape(name)}[/]");
            }
            else
            {
                File.WriteAllText(target, string.Empty);
                AnsiConsole.MarkupLine($"[green]✓ File created:[/] [bold]{Markup.Escape(name)}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }

        Pause();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private static string FormatSize(string path)
    {
        try
        {
            var bytes = new FileInfo(path).Length;
            return bytes switch
            {
                < 1024          => $"{bytes} B",
                < 1024 * 1024   => $"{bytes / 1024.0:F1} KB",
                < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
                _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB",
            };
        }
        catch { return ""; }
    }

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }
}
