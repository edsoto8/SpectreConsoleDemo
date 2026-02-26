using Spectre.Console;

internal static class DeviceExplorer
{
    public static void Run()
    {
        var devices = DeviceCsvRepository.LoadDevices();

        if (devices.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No devices were loaded from sample-data/devices.csv.[/]");
            return;
        }

        var activeFilters = new List<DeviceFilter>();
        var topCount = 20;
        var currentResults = GetDefaultTop(devices, topCount);
        var totalMatchCount = devices.Count;
        string? currentSortColumn = null;
        var sortDescending = false;

        while (true)
        {
            AnsiConsole.Clear();
            ShowStatusBar(activeFilters, currentSortColumn, sortDescending, currentResults.Count, totalMatchCount, devices.Count);
            AnsiConsole.WriteLine();
            ShowTable(currentResults);
            AnsiConsole.WriteLine();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Choose an action[/]")
                    .AddChoices("🖥️ Select device", "🔍 Filter data", "🔀 Sort data", "📊 Show statistics", "✏️ Manage devices", "🎯 Select Top Count", "🚪 Exit")
                    .EnableSearch()
                    .SearchPlaceholderText("[grey]Type to search actions...[/]")
                    .HighlightStyle(Style.Parse("cyan")));

            if (action == "🚪 Exit")
            {
                return;
            }

            if (action == "🎯 Select Top Count")
            {
                topCount = AnsiConsole.Prompt(
                    new TextPrompt<int>("Show top [green]how many[/] devices?")
                        .DefaultValue(topCount)
                        .Validate(n => n is >= 1 and <= 500
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Enter a value between 1 and 500.")));
                activeFilters.Clear();
                currentResults = GetDefaultTop(devices, topCount);
                totalMatchCount = devices.Count;
                currentSortColumn = null;
                sortDescending = false;
                continue;
            }

            if (action == "🖥️ Select device")
            {
                SelectAndShowDevice(currentResults, devices);
                continue;
            }

            if (action == "📊 Show statistics")
            {
                AnsiConsole.Clear();
                ShowSummary(currentResults, devices.Count);
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                System.Console.ReadKey(intercept: true);
                continue;
            }

            if (action == "✏️ Manage devices")
            {
                var dataChanged = ManageDevices(devices);
                if (dataChanged)
                {
                    activeFilters.Clear();
                    currentResults = GetDefaultTop(devices, topCount);
                    totalMatchCount = devices.Count;
                    currentSortColumn = null;
                    sortDescending = false;
                }
                continue;
            }

            if (action == "🔍 Filter data")
            {
                var (filtered, filteredTotal) = BuildFilteredResults(devices, activeFilters, topCount);
                if (filtered.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No devices matched your filters. Resetting view.[/]");
                    Thread.Sleep(1200);
                    activeFilters.Clear();
                    currentResults = GetDefaultTop(devices, topCount);
                    totalMatchCount = devices.Count;
                }
                else
                {
                    currentResults = filtered;
                    totalMatchCount = filteredTotal;
                }
                continue;
            }

            // Sort data
            var (sortedResults, chosenColumn, chosenDesc) = BuildSortedResults(currentResults, topCount);
            currentResults = sortedResults;
            currentSortColumn = chosenColumn;
            sortDescending = chosenDesc;
        }
    }

    // ── Filter ─────────────────────────────────────────────────────────────────

    private static (List<Device> Results, int TotalMatches) BuildFilteredResults(
        IReadOnlyList<Device> devices, List<DeviceFilter> activeFilters, int topCount)
    {
        var filterAction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Filter options[/]")
                .AddChoices("Add one filter", "Clear filters", "Back")
                .EnableSearch()
                .SearchPlaceholderText("[grey]Type to search...[/]")
                .HighlightStyle(Style.Parse("cyan")));

        if (filterAction == "Clear filters")
        {
            activeFilters.Clear();
            AnsiConsole.MarkupLine("[green]All filters were cleared.[/]");
            Thread.Sleep(700);
            return (GetDefaultTop(devices, topCount), devices.Count);
        }

        if (filterAction == "Back")
        {
            if (activeFilters.Count == 0)
                return (GetDefaultTop(devices, topCount), devices.Count);
            var backFiltered = ApplyActiveFilters(devices, activeFilters).ToList();
            return (backFiltered.Take(topCount).ToList(), backFiltered.Count);
        }

        activeFilters.Add(PromptForSingleFilter(devices));
        var filtered = ApplyActiveFilters(devices, activeFilters).ToList();
        return (filtered.Take(topCount).ToList(), filtered.Count);
    }

    private static DeviceFilter PromptForSingleFilter(IReadOnlyList<Device> devices)
    {
        var column = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a column to filter")
                .PageSize(12)
                .AddChoices("Terminal ID", "Location", "Address", "Type", "Manufacturer", "Model", "Customer ID", "Region", "Status", "Software Version"));

        return column switch
        {
            "Terminal ID"      => BuildTextContainsFilter("Terminal ID",      d => d.TerminalID),
            "Location"         => BuildTextContainsFilter("Location",         d => d.Location),
            "Address"          => BuildTextContainsFilter("Address",          d => d.Address),
            "Software Version" => BuildTextContainsFilter("Software Version", d => d.SoftwareVersion),
            "Type"             => BuildValueSelectionFilter("Type",         devices.Select(d => d.Type),         d => d.Type),
            "Manufacturer"     => BuildValueSelectionFilter("Manufacturer", devices.Select(d => d.Manufacturer), d => d.Manufacturer),
            "Model"            => BuildValueSelectionFilter("Model",        devices.Select(d => d.Model),        d => d.Model),
            "Customer ID"      => BuildValueSelectionFilter("Customer ID",  devices.Select(d => d.CustomerID),   d => d.CustomerID),
            "Region"           => BuildValueSelectionFilter("Region",       devices.Select(d => d.Region),       d => d.Region),
            "Status"           => BuildValueSelectionFilter("Status",       devices.Select(d => d.Status),       d => d.Status),
            _                  => BuildTextContainsFilter("Location", d => d.Location)
        };
    }

    private static DeviceFilter BuildTextContainsFilter(string column, Func<Device, string> selector)
    {
        var value = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter text for [green]{column}[/] contains")
                .Validate(input => string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Error("Value cannot be empty.")
                    : ValidationResult.Success()));

        return new DeviceFilter(column, "contains", value,
            d => selector(d).Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static DeviceFilter BuildValueSelectionFilter(string column, IEnumerable<string> values, Func<Device, string> selector)
    {
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Select [green]{column}[/] value")
                .PageSize(12)
                .AddChoices(values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToList()));

        return new DeviceFilter(column, "equals", selected,
            d => selector(d).Equals(selected, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<Device> ApplyActiveFilters(IEnumerable<Device> devices, IReadOnlyCollection<DeviceFilter> activeFilters)
    {
        return activeFilters.Count == 0
            ? devices
            : devices.Where(d => activeFilters.All(f => f.Predicate(d)));
    }

    // ── Sort ───────────────────────────────────────────────────────────────────

    private static (List<Device> Results, string SortColumn, bool Descending) BuildSortedResults(
        IReadOnlyList<Device> devices, int topCount)
    {
        if (devices.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No devices to sort.[/]");
            Thread.Sleep(800);
            return ([], string.Empty, false);
        }

        var sortColumn = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Sort by")
                .AddChoices("Terminal ID", "Location", "Type", "Manufacturer", "Customer ID", "Region", "Status", "Last Service")
                .HighlightStyle(Style.Parse("cyan")));

        var sortDirection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Sort direction")
                .AddChoices("Ascending", "Descending")
                .HighlightStyle(Style.Parse("cyan")));

        var descending = sortDirection == "Descending";
        return (ApplySort(devices, sortColumn, descending).Take(topCount).ToList(), sortColumn, descending);
    }

    private static IEnumerable<Device> ApplySort(IEnumerable<Device> devices, string column, bool descending)
    {
        return (column, descending) switch
        {
            ("Terminal ID",   false) => devices.OrderBy(d => d.TerminalID),
            ("Terminal ID",   true)  => devices.OrderByDescending(d => d.TerminalID),
            ("Location",      false) => devices.OrderBy(d => d.Location),
            ("Location",      true)  => devices.OrderByDescending(d => d.Location),
            ("Type",          false) => devices.OrderBy(d => d.Type),
            ("Type",          true)  => devices.OrderByDescending(d => d.Type),
            ("Manufacturer",  false) => devices.OrderBy(d => d.Manufacturer),
            ("Manufacturer",  true)  => devices.OrderByDescending(d => d.Manufacturer),
            ("Customer ID",   false) => devices.OrderBy(d => d.CustomerID),
            ("Customer ID",   true)  => devices.OrderByDescending(d => d.CustomerID),
            ("Region",        false) => devices.OrderBy(d => d.Region),
            ("Region",        true)  => devices.OrderByDescending(d => d.Region),
            ("Status",        false) => devices.OrderBy(d => d.Status),
            ("Status",        true)  => devices.OrderByDescending(d => d.Status),
            ("Last Service",  false) => devices.OrderBy(d => d.LastService),
            ("Last Service",  true)  => devices.OrderByDescending(d => d.LastService),
            _                        => devices.OrderBy(d => d.TerminalID)
        };
    }

    private static List<Device> GetDefaultTop(IEnumerable<Device> devices, int count)
    {
        return devices
            .OrderBy(d => d.TerminalID)
            .Take(count)
            .ToList();
    }

    // ── Select / Detail ────────────────────────────────────────────────────────

    private static void SelectAndShowDevice(IReadOnlyList<Device> currentResults, IReadOnlyList<Device> allDevices)
    {
        if (allDevices.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No devices available.[/]");
            Thread.Sleep(700);
            return;
        }

        var selected = PromptToPickDevice(allDevices, "[bold]Select a device[/]");
        AnsiConsole.WriteLine();
        DisplayDeviceDetails(selected);
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        System.Console.ReadKey(intercept: true);
    }

    private static Device PromptToPickDevice(IReadOnlyList<Device> devices, string title)
    {
        var options = devices
            .Select(d => $"{d.TerminalID} - {Markup.Escape(d.Location)} ({Markup.Escape(d.Type)})")
            .ToList();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(20)
                .AddChoices(options)
                .EnableSearch()
                .SearchPlaceholderText("[grey]Type Terminal ID or location to jump...[/]")
                .HighlightStyle(Style.Parse("cyan")));

        return devices[options.IndexOf(selected)];
    }

    private static void DisplayDeviceDetails(Device d)
    {
        var panel = new Panel(
            new Markup($"""
                [yellow]Terminal ID:[/]       [cyan bold]{Markup.Escape(d.TerminalID)}[/]
                [yellow]Location:[/]          {Markup.Escape(d.Location)}
                [yellow]Address:[/]           {Markup.Escape(d.Address)}
                [yellow]Type:[/]              {ColorizeType(d.Type)}
                [yellow]Manufacturer:[/]      {Markup.Escape(d.Manufacturer)}
                [yellow]Model:[/]             {Markup.Escape(d.Model)}
                [yellow]Customer ID:[/]       {Markup.Escape(d.CustomerID)}
                [yellow]Region:[/]            {Markup.Escape(d.Region)}
                [yellow]Status:[/]            {ColorizeStatus(d.Status)}
                [yellow]Software Version:[/]  {Markup.Escape(d.SoftwareVersion)}
                [yellow]Last Service:[/]      {Markup.Escape(d.LastService)}
                """))
            .Header("[bold blue]Device Details[/]")
            .BorderColor(Color.Cyan1)
            .Padding(1, 1);

        AnsiConsole.Write(panel);
    }

    // ── CRUD ───────────────────────────────────────────────────────────────────

    private static bool ManageDevices(List<Device> devices)
    {
        var subAction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Manage Devices[/]")
                .AddChoices("➕ Add device", "✏️ Update device", "🗑️ Delete device", "↩ Back")
                .HighlightStyle(Style.Parse("cyan")));

        return subAction switch
        {
            "➕ Add device"    => AddDevice(devices),
            "✏️ Update device" => UpdateDevice(devices),
            "🗑️ Delete device" => DeleteDevice(devices),
            _                  => false
        };
    }

    private static bool AddDevice(List<Device> devices)
    {
        AnsiConsole.Write(new Rule("[bold green]Add New Device[/]").RuleStyle("green dim"));
        AnsiConsole.WriteLine();

        var terminalId = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Terminal ID:[/]")
                .Validate(s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return ValidationResult.Error("Required.");
                    if (devices.Any(d => d.TerminalID.Equals(s, StringComparison.OrdinalIgnoreCase)))
                        return ValidationResult.Error("A device with that Terminal ID already exists.");
                    return ValidationResult.Success();
                }));

        var location = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Location:[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s) ? ValidationResult.Success() : ValidationResult.Error("Required.")));

        var address = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Address[/] [grey](avoid commas):[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s) ? ValidationResult.Success() : ValidationResult.Error("Required.")));

        var type = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Type:[/]")
                .AddChoices("ATM", "Smart Safe", "Kiosk", "CDM", "Recycler")
                .HighlightStyle(Style.Parse("cyan")));

        var manufacturer = PromptPickOrCustom("Manufacturer", devices.Select(d => d.Manufacturer), string.Empty);
        var model        = PromptPickOrCustom("Model",        devices.Select(d => d.Model),        string.Empty);
        var customerId   = PromptPickOrCustom("Customer ID",  devices.Select(d => d.CustomerID),   string.Empty);
        var region       = PromptPickOrCustom("Region",       devices.Select(d => d.Region),       string.Empty);

        var status = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Status:[/]")
                .AddChoices("Active", "Offline", "Maintenance", "Decommissioned")
                .HighlightStyle(Style.Parse("cyan")));

        var softwareVersion = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Software Version:[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s) ? ValidationResult.Success() : ValidationResult.Error("Required.")));

        var lastService = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Last Service[/] [grey](YYYY-MM-DD):[/]")
                .DefaultValue(DateTime.Today.ToString("yyyy-MM-dd")));

        var newDevice = new Device(terminalId, location, address, type, manufacturer, model,
                                   customerId, region, status, softwareVersion, lastService);

        devices.Add(newDevice);
        DeviceCsvRepository.SaveDevices(devices);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Device [bold]{Markup.Escape(terminalId)}[/] added.[/]");
        Thread.Sleep(1000);
        return true;
    }

    private static bool UpdateDevice(List<Device> devices)
    {
        if (devices.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No devices to update.[/]");
            Thread.Sleep(700);
            return false;
        }

        var device = PromptToPickDevice(devices, "[bold]Select device to update[/]");

        AnsiConsole.Write(new Rule($"[bold cyan]Updating: {Markup.Escape(device.TerminalID)}[/]").RuleStyle("cyan dim"));
        AnsiConsole.WriteLine();

        var field = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which field to update?")
                .PageSize(12)
                .AddChoices("Location", "Address", "Type", "Manufacturer", "Model", "Customer ID", "Region", "Status", "Software Version", "Last Service")
                .HighlightStyle(Style.Parse("cyan")));

        Device updated = field switch
        {
            "Location"         => device with { Location        = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Location:[/]").DefaultValue(device.Location)) },
            "Address"          => device with { Address         = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Address:[/]").DefaultValue(device.Address)) },
            "Type"             => device with { Type            = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[yellow]Type:[/]").AddChoices("ATM", "Smart Safe", "Kiosk", "CDM", "Recycler").HighlightStyle(Style.Parse("cyan"))) },
            "Manufacturer"     => device with { Manufacturer    = PromptPickOrCustom("Manufacturer", devices.Select(d => d.Manufacturer), device.Manufacturer) },
            "Model"            => device with { Model           = PromptPickOrCustom("Model",        devices.Select(d => d.Model),        device.Model) },
            "Customer ID"      => device with { CustomerID      = PromptPickOrCustom("Customer ID",  devices.Select(d => d.CustomerID),   device.CustomerID) },
            "Region"           => device with { Region          = PromptPickOrCustom("Region",       devices.Select(d => d.Region),       device.Region) },
            "Status"           => device with { Status          = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[yellow]Status:[/]").AddChoices("Active", "Offline", "Maintenance", "Decommissioned").HighlightStyle(Style.Parse("cyan"))) },
            "Software Version" => device with { SoftwareVersion = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Software Version:[/]").DefaultValue(device.SoftwareVersion)) },
            "Last Service"     => device with { LastService     = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Last Service (YYYY-MM-DD):[/]").DefaultValue(device.LastService)) },
            _                  => device
        };

        var index = devices.IndexOf(device);
        devices[index] = updated;
        DeviceCsvRepository.SaveDevices(devices);

        AnsiConsole.MarkupLine($"[green]{Markup.Escape(field)} updated.[/]");
        Thread.Sleep(800);
        return true;
    }

    private static bool DeleteDevice(List<Device> devices)
    {
        if (devices.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No devices to delete.[/]");
            Thread.Sleep(700);
            return false;
        }

        var device = PromptToPickDevice(devices, "[bold red]Select device to delete[/]");

        AnsiConsole.WriteLine();
        DisplayDeviceDetails(device);
        AnsiConsole.WriteLine();

        var confirmed = AnsiConsole.Prompt(
            new ConfirmationPrompt($"[red]Delete [bold]{Markup.Escape(device.TerminalID)}[/]? This cannot be undone.[/]"));

        if (!confirmed)
        {
            AnsiConsole.MarkupLine("[grey]Delete cancelled.[/]");
            Thread.Sleep(600);
            return false;
        }

        devices.Remove(device);
        DeviceCsvRepository.SaveDevices(devices);

        AnsiConsole.MarkupLine($"[red]{Markup.Escape(device.TerminalID)} deleted.[/]");
        Thread.Sleep(800);
        return true;
    }

    private static string PromptPickOrCustom(string label, IEnumerable<string> existingValues, string defaultValue)
    {
        var options = existingValues
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v)
            .Append("✏️ Custom...")
            .ToList();

        var pick = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{label}:[/]")
                .PageSize(14)
                .AddChoices(options)
                .HighlightStyle(Style.Parse("cyan")));

        return pick == "✏️ Custom..."
            ? AnsiConsole.Prompt(
                new TextPrompt<string>($"[yellow]Enter {label}:[/]")
                    .DefaultValue(string.IsNullOrWhiteSpace(defaultValue) ? "" : defaultValue))
            : pick;
    }

    // ── Status bar ─────────────────────────────────────────────────────────────

    private static void ShowStatusBar(
        IReadOnlyCollection<DeviceFilter> activeFilters,
        string? sortColumn,
        bool sortDescending,
        int displayedCount,
        int matchedCount,
        int totalInDataset)
    {
        var parts = new List<string>();

        if (activeFilters.Count == 0)
            parts.Add("[grey]Filter:[/] [dim]none[/]");
        else
        {
            var filterText = string.Join(", ", activeFilters.Select(
                f => $"[yellow]{Markup.Escape(f.Column)}[/] [grey]{Markup.Escape(f.Operator)}[/] [cyan]'{Markup.Escape(f.Value)}'[/]"));
            parts.Add($"[grey]Filter:[/] {filterText}");
        }

        if (string.IsNullOrEmpty(sortColumn))
            parts.Add("[grey]Sorted by:[/] [dim]Terminal ID (↑ default)[/]");
        else
        {
            var arrow = sortDescending ? "↓" : "↑";
            var dir   = sortDescending ? "Descending" : "Ascending";
            parts.Add($"[grey]Sorted by:[/] [cyan]{Markup.Escape(sortColumn)}[/] [grey]({arrow} {dir})[/]");
        }

        var showingColor = displayedCount < matchedCount ? "yellow" : "green";
        parts.Add($"[grey]Showing[/] [{showingColor}]{displayedCount}[/] [grey]of[/] [cyan]{matchedCount}[/] [grey]matched[/] [dim]({totalInDataset} total)[/]");

        AnsiConsole.MarkupLine(" " + string.Join("  [grey]|[/]  ", parts));
    }

    // ── Table ──────────────────────────────────────────────────────────────────

    private static void ShowTable(IReadOnlyList<Device> devices)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Terminal ID")
            .AddColumn("Location")
            .AddColumn("Type")
            .AddColumn("Manufacturer")
            .AddColumn("Model")
            .AddColumn("Customer ID")
            .AddColumn("Region")
            .AddColumn("Status")
            .AddColumn("Software Ver")
            .AddColumn("Last Service");

        foreach (var d in devices)
        {
            table.AddRow(
                $"[bold]{Markup.Escape(d.TerminalID)}[/]",
                Markup.Escape(d.Location),
                ColorizeType(d.Type),
                Markup.Escape(d.Manufacturer),
                Markup.Escape(d.Model),
                Markup.Escape(d.CustomerID),
                Markup.Escape(d.Region),
                ColorizeStatus(d.Status),
                Markup.Escape(d.SoftwareVersion),
                Markup.Escape(d.LastService));
        }

        AnsiConsole.Write(table);
    }

    // ── Statistics ─────────────────────────────────────────────────────────────

    private static void ShowSummary(IReadOnlyCollection<Device> devices, int totalInDataset)
    {
        var activeCount        = devices.Count(d => d.Status.Equals("Active",          StringComparison.OrdinalIgnoreCase));
        var offlineCount       = devices.Count(d => d.Status.Equals("Offline",         StringComparison.OrdinalIgnoreCase));
        var maintenanceCount   = devices.Count(d => d.Status.Equals("Maintenance",     StringComparison.OrdinalIgnoreCase));
        var decommissionedCount= devices.Count(d => d.Status.Equals("Decommissioned",  StringComparison.OrdinalIgnoreCase));

        var atmCount           = devices.Count(d => d.Type.Equals("ATM",         StringComparison.OrdinalIgnoreCase));
        var safeCount          = devices.Count(d => d.Type.Equals("Smart Safe",  StringComparison.OrdinalIgnoreCase));
        var kioskCount         = devices.Count(d => d.Type.Equals("Kiosk",       StringComparison.OrdinalIgnoreCase));
        var cdmCount           = devices.Count(d => d.Type.Equals("CDM",         StringComparison.OrdinalIgnoreCase));
        var recyclerCount      = devices.Count(d => d.Type.Equals("Recycler",    StringComparison.OrdinalIgnoreCase));

        var byRegion = devices
            .GroupBy(d => d.Region)
            .Select(g => (Region: g.Key, Count: g.Count()))
            .OrderByDescending(g => g.Count)
            .ToList();

        var byCustomer = devices
            .GroupBy(d => d.CustomerID)
            .Select(g => (Customer: g.Key, Count: g.Count(),
                Types: string.Join(" / ", g.Select(d => d.Type).Distinct().OrderBy(t => t))))
            .OrderByDescending(g => g.Count)
            .ToList();

        var byManufacturer = devices
            .GroupBy(d => d.Manufacturer)
            .Select(g => (Manufacturer: g.Key, Count: g.Count()))
            .OrderByDescending(g => g.Count)
            .ToList();

        // ── Header ──────────────────────────────────────────────────────────
        AnsiConsole.Write(new Rule("[bold yellow]Device Profile Statistics[/]").RuleStyle("yellow dim"));
        AnsiConsole.WriteLine();

        // ── Row 1: overview / status / type panels ───────────────────────────
        var statGrid = new Grid().Expand();
        statGrid.AddColumn().AddColumn().AddColumn();

        var overviewGrid = new Grid();
        overviewGrid.AddColumn().AddColumn();
        overviewGrid.AddRow("[bold]Showing[/]",          $"[cyan bold]{devices.Count}[/] [grey]of {totalInDataset} total[/]");
        overviewGrid.AddRow("[bold]Customers[/]",        $"[cyan bold]{byCustomer.Count}[/]");
        overviewGrid.AddRow("[bold]Manufacturers[/]",    $"[cyan bold]{byManufacturer.Count}[/]");
        overviewGrid.AddRow("[bold]Regions[/]",          $"[cyan bold]{byRegion.Count}[/]");

        var statusGrid = new Grid();
        statusGrid.AddColumn().AddColumn();
        statusGrid.AddRow("[green bold]Active[/]",           $"[green]{activeCount}[/]");
        statusGrid.AddRow("[red bold]Offline[/]",            $"[red]{offlineCount}[/]");
        statusGrid.AddRow("[yellow bold]Maintenance[/]",     $"[yellow]{maintenanceCount}[/]");
        statusGrid.AddRow("[grey bold]Decommissioned[/]",    $"[grey]{decommissionedCount}[/]");

        var typeGrid = new Grid();
        typeGrid.AddColumn().AddColumn();
        typeGrid.AddRow("[cyan bold]ATM[/]",         $"[cyan]{atmCount}[/]");
        typeGrid.AddRow("[blue bold]Smart Safe[/]",  $"[blue]{safeCount}[/]");
        typeGrid.AddRow("[magenta bold]Kiosk[/]",    $"[magenta]{kioskCount}[/]");
        typeGrid.AddRow("[yellow bold]CDM[/]",       $"[yellow]{cdmCount}[/]");
        typeGrid.AddRow("[green bold]Recycler[/]",   $"[green]{recyclerCount}[/]");

        statGrid.AddRow(
            new Panel(overviewGrid).Header("[bold]Overview[/]").Expand().BorderColor(Color.Cyan1),
            new Panel(statusGrid).Header("[bold]Status[/]").Expand().BorderColor(Color.Green),
            new Panel(typeGrid).Header("[bold]Device Types[/]").Expand().BorderColor(Color.Blue));

        AnsiConsole.Write(statGrid);
        AnsiConsole.WriteLine();

        // ── Bar chart: by Type ───────────────────────────────────────────────
        var typeChart = new BarChart()
            .Width(60)
            .Label("[bold]Devices by Type[/]")
            .CenterLabel()
            .AddItem("ATM",        atmCount,      Color.Cyan1)
            .AddItem("Smart Safe", safeCount,     Color.Blue)
            .AddItem("Kiosk",      kioskCount,    Color.MediumPurple)
            .AddItem("CDM",        cdmCount,      Color.Yellow)
            .AddItem("Recycler",   recyclerCount, Color.Green);

        AnsiConsole.Write(new Panel(typeChart).Header("[bold]Type Distribution[/]").Expand().BorderColor(Color.Blue));
        AnsiConsole.WriteLine();

        // ── Bar chart: by Region ─────────────────────────────────────────────
        var regionChart = new BarChart()
            .Width(60)
            .Label("[bold]Devices by Region[/]")
            .CenterLabel();
        var regionColors = new[] { Color.CornflowerBlue, Color.MediumPurple, Color.Teal, Color.Orange1, Color.HotPink };
        for (var i = 0; i < byRegion.Count; i++)
            regionChart.AddItem(byRegion[i].Region, byRegion[i].Count, regionColors[i % regionColors.Length]);

        AnsiConsole.Write(new Panel(regionChart).Header("[bold]Regional Distribution[/]").Expand().BorderColor(Color.CornflowerBlue));
        AnsiConsole.WriteLine();

        // ── Bar chart: by Status ─────────────────────────────────────────────
        var statusChart = new BarChart()
            .Width(60)
            .Label("[bold]Devices by Status[/]")
            .CenterLabel()
            .AddItem("Active",          activeCount,         Color.Green)
            .AddItem("Offline",         offlineCount,        Color.Red)
            .AddItem("Maintenance",     maintenanceCount,    Color.Yellow)
            .AddItem("Decommissioned",  decommissionedCount, Color.Grey);

        AnsiConsole.Write(new Panel(statusChart).Header("[bold]Status Distribution[/]").Expand().BorderColor(Color.Green));
        AnsiConsole.WriteLine();

        // ── Table: by Customer ───────────────────────────────────────────────
        var custTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Gold1)
            .AddColumn(new TableColumn("[bold]Customer ID[/]").Centered())
            .AddColumn(new TableColumn("[bold]Device Count[/]").Centered())
            .AddColumn("[bold]Device Types Used[/]");

        foreach (var (customer, count, types) in byCustomer)
            custTable.AddRow(Markup.Escape(customer), $"[cyan]{count}[/]", Markup.Escape(types));

        AnsiConsole.Write(new Panel(custTable).Header("[bold gold1]Devices by Customer[/]").Expand().BorderColor(Color.Gold1));
        AnsiConsole.WriteLine();

        // ── Table: by Manufacturer ───────────────────────────────────────────
        var mfgTable = new Table()
            .Border(TableBorder.Simple)
            .BorderColor(Color.CornflowerBlue)
            .AddColumn("[bold]Manufacturer[/]")
            .AddColumn(new TableColumn("[bold]Count[/]").Centered());

        foreach (var (mfg, count) in byManufacturer)
            mfgTable.AddRow(Markup.Escape(mfg), $"[cyan]{count}[/]");

        AnsiConsole.Write(new Panel(mfgTable).Header("[bold]Manufacturer Breakdown[/]").Expand().BorderColor(Color.CornflowerBlue));
        AnsiConsole.WriteLine();
    }

    // ── Colorizers ─────────────────────────────────────────────────────────────

    private static string ColorizeType(string type)
    {
        var e = Markup.Escape(type);
        return type.ToLowerInvariant() switch
        {
            "atm"         => $"[cyan]{e}[/]",
            "smart safe"  => $"[blue]{e}[/]",
            "kiosk"       => $"[magenta]{e}[/]",
            "cdm"         => $"[yellow]{e}[/]",
            "recycler"    => $"[green]{e}[/]",
            _             => e
        };
    }

    private static string ColorizeStatus(string status)
    {
        var e = Markup.Escape(status);
        return status.ToLowerInvariant() switch
        {
            "active"         => $"[green]{e}[/]",
            "offline"        => $"[red]{e}[/]",
            "maintenance"    => $"[yellow]{e}[/]",
            "decommissioned" => $"[grey]{e}[/]",
            _                => e
        };
    }
}

// ── Repository ─────────────────────────────────────────────────────────────────

internal static class DeviceCsvRepository
{
    public static List<Device> LoadDevices()
    {
        var path = ResolveCsvPath();
        if (path is null || !File.Exists(path))
            return [];

        var lines = File.ReadAllLines(path);
        return lines
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseDevice)
            .ToList();
    }

    public static void SaveDevices(List<Device> devices)
    {
        var path = ResolveCsvPath();
        if (path is null)
        {
            AnsiConsole.MarkupLine("[red]Could not resolve CSV path — changes were not saved.[/]");
            return;
        }

        var lines = new List<string>(devices.Count + 1)
        {
            "TerminalID,Location,Address,Type,Manufacturer,Model,CustomerID,Region,Status,SoftwareVersion,LastService"
        };

        lines.AddRange(devices.Select(d =>
            $"{d.TerminalID},{d.Location},{d.Address},{d.Type},{d.Manufacturer},{d.Model},{d.CustomerID},{d.Region},{d.Status},{d.SoftwareVersion},{d.LastService}"));

        File.WriteAllLines(path, lines);
    }

    private static string? ResolveCsvPath()
    {
        var current = Path.Combine(Directory.GetCurrentDirectory(), "sample-data", "devices.csv");
        if (File.Exists(current)) return current;

        var candidate = AppContext.BaseDirectory;
        for (var i = 0; i < 6; i++)
        {
            var filePath = Path.Combine(candidate, "sample-data", "devices.csv");
            if (File.Exists(filePath)) return filePath;
            var parent = Directory.GetParent(candidate);
            if (parent is null) break;
            candidate = parent.FullName;
        }

        return null;
    }

    private static Device ParseDevice(string line)
    {
        var fields = line.Split(',', StringSplitOptions.TrimEntries);
        if (fields.Length < 11)
            throw new InvalidOperationException($"Invalid device CSV row: {line}");

        return new Device(
            fields[0],   // TerminalID
            fields[1],   // Location
            fields[2],   // Address
            fields[3],   // Type
            fields[4],   // Manufacturer
            fields[5],   // Model
            fields[6],   // CustomerID
            fields[7],   // Region
            fields[8],   // Status
            fields[9],   // SoftwareVersion
            fields[10]); // LastService
    }
}

// ── Models ──────────────────────────────────────────────────────────────────────

internal sealed record Device(
    string TerminalID,
    string Location,
    string Address,
    string Type,
    string Manufacturer,
    string Model,
    string CustomerID,
    string Region,
    string Status,
    string SoftwareVersion,
    string LastService);

internal sealed record DeviceFilter(
    string Column,
    string Operator,
    string Value,
    Func<Device, bool> Predicate);
