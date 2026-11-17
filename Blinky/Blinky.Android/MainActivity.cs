using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Blinky.Android;

[Activity(
    Label = "Blinky",
    MainLauncher = true,
    Exported = true,
    ScreenOrientation = ScreenOrientation.Portrait)]
public sealed class MainActivity : Activity
{
    private const int DefaultReadFrequency = 64;
    private const int DefaultSamplesPerBit = 8;

    private LinearLayout? _home;
    private View? _mainTab;
    private View? _debugTab;
    private View? _settingsTab;
    private View? _flashView;
    private EditText? _valueInput;
    private EditText? _latitudeInput;
    private EditText? _longitudeInput;
    private Spinner? _timeZoneSpinner;
    private Spinner? _readFrequencySpinner;
    private Spinner? _samplesPerBitSpinner;
    private TextView? _frequencySummary;
    private TextView? _dataLabel;
    private Button[] _transmitButtons = Array.Empty<Button>();
    private Button[] _tabButtons = Array.Empty<Button>();
    private CancellationTokenSource? _transmissionCancellation;
    private bool _transmitting;
    private string _selectedTab = "Main";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Window!.AddFlags(WindowManagerFlags.Fullscreen);
        Window.Attributes!.ScreenBrightness = 1f;
        ApplyImmersiveMode();
        ShowHome();
    }

    protected override void OnResume()
    {
        base.OnResume();
        ApplyImmersiveMode();
    }

    public override void OnBackPressed()
    {
        if (_transmitting)
        {
            _transmissionCancellation?.Cancel();
            return;
        }

        base.OnBackPressed();
    }

    protected override void OnDestroy()
    {
        _transmissionCancellation?.Cancel();
        base.OnDestroy();
    }

    private void ApplyImmersiveMode()
    {
        var flags = SystemUiFlags.Fullscreen
            | SystemUiFlags.HideNavigation
            | SystemUiFlags.ImmersiveSticky
            | SystemUiFlags.LayoutStable
            | SystemUiFlags.LayoutHideNavigation
            | SystemUiFlags.LayoutFullscreen;
        Window!.DecorView!.SystemUiVisibility = (StatusBarVisibility)(int)flags;
    }

    private void ShowHome()
    {
        _home = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        _home.SetPadding(Dp(18), Dp(20), Dp(18), Dp(12));
        _home.SetBackgroundColor(Color.ParseColor("#0B1018"));

        var header = new LinearLayout(this) { Orientation = Orientation.Vertical };
        var title = new TextView(this) { Text = "BLINKY", TextSize = 26 };
        title.SetTextColor(Color.ParseColor("#E8F0FF"));
        title.SetTypeface(null, TypefaceStyle.Bold);
        header.AddView(title, MatchWrap());

        var subtitle = new TextView(this)
        {
            Text = "Light-signal transmitter",
            TextSize = 14
        };
        subtitle.SetTextColor(Color.ParseColor("#91A0B8"));
        header.AddView(subtitle, MatchWrap(Dp(2)));
        _home.AddView(header, MatchWrap(Dp(4)));

        var tabBar = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };
        tabBar.SetBackgroundDrawable(CreateRoundedBackground("#151D2A", 18));
        tabBar.SetPadding(Dp(4), Dp(4), Dp(4), Dp(4));
        _tabButtons = new[]
        {
            MakeTabButton("Main"),
            MakeTabButton("Debug"),
            MakeTabButton("Settings")
        };
        foreach (var button in _tabButtons)
        {
            var tabParams = new LinearLayout.LayoutParams(0, Dp(44), 1f);
            tabBar.AddView(button, tabParams);
        }
        _home.AddView(tabBar, MatchWrap(Dp(18)));

        _dataLabel = new TextView(this)
        {
            Text = "Ready to transmit",
            TextSize = 12,
            Ellipsize = TextUtils.TruncateAt.End
        };
        _dataLabel.SetMaxLines(2);
        _dataLabel.SetTextColor(Color.ParseColor("#91A0B8"));
        _home.AddView(_dataLabel, MatchWrap(Dp(10)));

        var content = new FrameLayout(this);
        _home.AddView(content, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, 0, 1f));

        var mainContent = CreateTabScrollView();
        BuildMainTab(mainContent);
        _mainTab = mainContent;
        content.AddView(mainContent, MatchFrame());

        var debugContent = CreateTabScrollView();
        BuildDebugTab(debugContent);
        _debugTab = debugContent;
        content.AddView(debugContent, MatchFrame());

        var settingsContent = CreateTabScrollView();
        BuildSettingsTab(settingsContent);
        _settingsTab = settingsContent;
        content.AddView(settingsContent, MatchFrame());

        SelectTab(_selectedTab);
        SetContentView(_home);
    }

    private void BuildMainTab(ScrollView scrollView)
    {
        var content = CreateTabContent(scrollView);
        var setTimeCard = AddCard(content, "Date & time", "Synchronize the watch date and clock.");
        var setTimeButton = MakeActionButton("Set Time", async () =>
        {
            var frequency = GetTransmissionFrequency();
            var data = TransmissionProtocol.CreateDateTimeData(
                out var startAt, DateTime.Now, frequency);
            await StartTransmissionAsync(data, startAt, frequency);
        });
        setTimeCard.AddView(setTimeButton, MatchWrap(Dp(12)));

        var locationCard = AddCard(content, "Location", "Coordinates are transmitted in decimal degrees.");
        var coordinates = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        _latitudeInput = MakeCoordinateInput("Latitude", coordinates);
        _longitudeInput = MakeCoordinateInput("Longitude", coordinates);
        locationCard.AddView(coordinates, MatchWrap(Dp(8)));
        var locationButton = MakeActionButton("Send location", async () =>
        {
            var frequency = GetTransmissionFrequency();
            if (!TryGetLocation(out var latitude, out var longitude))
                return;

            var data = new[]
            {
                TransmissionProtocol.CreateLocationData(latitude, 4, multipacket: true),
                TransmissionProtocol.CreateLocationData(longitude, 5, multipacket: false)
            };
            await StartTransmissionAsync(data, DateTime.Now, frequency);
        });
        locationCard.AddView(locationButton, MatchWrap(Dp(12)));

        var timeZoneCard = AddCard(content, "Time zone", "Select the watch's Movement time zone.");
        _timeZoneSpinner = MakeSpinner(TransmissionProtocol.TimeZoneNames, 15);
        timeZoneCard.AddView(_timeZoneSpinner, MatchWrap(Dp(8)));
        var timeZoneButton = MakeActionButton("Set time zone", async () =>
        {
            var frequency = GetTransmissionFrequency();
            var data = TransmissionProtocol.CreateTimeZoneData(
                (byte)_timeZoneSpinner!.SelectedItemPosition);
            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });
        timeZoneCard.AddView(timeZoneButton, MatchWrap(Dp(12)));

        _transmitButtons = new[] { setTimeButton, locationButton, timeZoneButton };
    }

    private void BuildDebugTab(ScrollView scrollView)
    {
        var content = CreateTabContent(scrollView);
        var card = AddCard(content, "Diagnostics", "Send individual receiver commands.");

        var timestampButton = MakeActionButton("Timestamp", async () =>
        {
            var frequency = GetTransmissionFrequency();
            var data = TransmissionProtocol.CreateTimeData(
                out var startAt, 1, DateTime.Now, frequency);
            await StartTransmissionAsync(new[] { data }, startAt, frequency);
        });
        var dateButton = MakeActionButton("Date", async () =>
        {
            var frequency = GetTransmissionFrequency();
            var data = TransmissionProtocol.CreateDateData(DateTime.Now, 2);
            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });
        var heartbeatButton = MakeActionButton("Heartbeat", async () =>
        {
            var frequency = GetTransmissionFrequency();
            var data = TransmissionProtocol.CreateHeartbeatData();
            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });
        var testButton = MakeActionButton("Test", async () =>
        {
            var frequency = GetTransmissionFrequency();
            var data = TransmissionProtocol.CreateTestData();
            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });

        foreach (var button in new[] { timestampButton, dateButton, heartbeatButton, testButton })
            card.AddView(button, MatchWrap(Dp(8)));

        _valueInput = new EditText(this)
        {
            Text = "0x12345678",
            Hint = "32-bit hexadecimal value",
            InputType = InputTypes.ClassText
                | InputTypes.TextFlagCapCharacters
                | InputTypes.TextFlagNoSuggestions
        };
        _valueInput.SetSingleLine(true);
        _valueInput.SetTextColor(Color.White);
        _valueInput.SetHintTextColor(Color.ParseColor("#91A0B8"));
        _valueInput.SetSelectAllOnFocus(true);
        card.AddView(_valueInput, MatchWrap(Dp(12)));
        var valueButton = MakeActionButton("Transmit value", async () =>
        {
            var frequency = GetTransmissionFrequency();
            if (!TransmissionProtocol.TryParseHexData(_valueInput!.Text, out var data))
            {
                ShowAlert("Invalid value", "Enter a hexadecimal value from 0 to FFFFFFFF.");
                return;
            }

            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });
        card.AddView(valueButton, MatchWrap(Dp(8)));

        _transmitButtons = new[]
        {
            timestampButton, dateButton, heartbeatButton, testButton, valueButton
        };
    }

    private void BuildSettingsTab(ScrollView scrollView)
    {
        var content = CreateTabContent(scrollView);
        var card = AddCard(
            content,
            "Receiver sampling",
            "Match these values to the receiver settings on the watch.");

        AddFieldLabel(card, "Read frequency");
        _readFrequencySpinner = MakeSpinner(
            TransmissionProtocol.ReadFrequencies,
            Array.IndexOf(TransmissionProtocol.ReadFrequencyValues, DefaultReadFrequency));
        card.AddView(_readFrequencySpinner, MatchWrap(Dp(4)));

        AddFieldLabel(card, "Samples per bit", Dp(16));
        _samplesPerBitSpinner = MakeSpinner(
            TransmissionProtocol.SamplesPerBit,
            Array.IndexOf(TransmissionProtocol.SamplesPerBitValues, DefaultSamplesPerBit));
        card.AddView(_samplesPerBitSpinner, MatchWrap(Dp(4)));

        _frequencySummary = new TextView(this)
        {
            TextSize = 15,
            Text = string.Empty
        };
        _frequencySummary.SetTextColor(Color.ParseColor("#AFC8FF"));
        card.AddView(_frequencySummary, MatchWrap(Dp(14)));

        var selectionListener = new SpinnerSelectionListener(UpdateFrequencySummary);
        _readFrequencySpinner.OnItemSelectedListener = selectionListener;
        _samplesPerBitSpinner.OnItemSelectedListener = selectionListener;
        UpdateFrequencySummary();

        var hint = new TextView(this)
        {
            Text = "Transmit rate = read frequency ÷ samples per bit.",
            TextSize = 13
        };
        hint.SetTextColor(Color.ParseColor("#91A0B8"));
        card.AddView(hint, MatchWrap(Dp(8)));

        _transmitButtons = Array.Empty<Button>();
    }

    private void UpdateFrequencySummary()
    {
        if (_frequencySummary is null || _readFrequencySpinner is null || _samplesPerBitSpinner is null)
            return;

        var readFrequency = TransmissionProtocol.ReadFrequencyValues[_readFrequencySpinner.SelectedItemPosition];
        var samplesPerBit = TransmissionProtocol.SamplesPerBitValues[_samplesPerBitSpinner.SelectedItemPosition];
        var transmitFrequency = readFrequency / (double)samplesPerBit;
        _frequencySummary.Text = $"Light pulse rate: {transmitFrequency:0.###} Hz";
    }

    private double GetTransmissionFrequency()
    {
        var readFrequency = TransmissionProtocol.ReadFrequencyValues[
            _readFrequencySpinner!.SelectedItemPosition];
        var samplesPerBit = TransmissionProtocol.SamplesPerBitValues[
            _samplesPerBitSpinner!.SelectedItemPosition];
        return readFrequency / (double)samplesPerBit;
    }

    private ScrollView CreateTabScrollView() => new(this)
    {
        FillViewport = true,
        Visibility = ViewStates.Gone
    };

    private LinearLayout CreateTabContent(ScrollView scrollView)
    {
        var content = new LinearLayout(this) { Orientation = Orientation.Vertical };
        content.SetPadding(0, Dp(8), 0, Dp(20));
        scrollView.AddView(content, new ScrollView.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        return content;
    }

    private LinearLayout AddCard(LinearLayout parent, string title, string subtitle)
    {
        var card = new LinearLayout(this) { Orientation = Orientation.Vertical };
        card.SetPadding(Dp(16), Dp(16), Dp(16), Dp(16));
        card.SetBackgroundDrawable(CreateRoundedBackground("#151D2A", 18));

        var heading = new TextView(this)
        {
            Text = title,
            TextSize = 18
        };
        heading.SetTextColor(Color.ParseColor("#E8F0FF"));
        heading.SetTypeface(null, TypefaceStyle.Bold);
        card.AddView(heading, MatchWrap());

        var description = new TextView(this)
        {
            Text = subtitle,
            TextSize = 13
        };
        description.SetTextColor(Color.ParseColor("#91A0B8"));
        card.AddView(description, MatchWrap(Dp(4)));
        parent.AddView(card, MatchWrap(Dp(14)));
        return card;
    }

    private Button MakeTabButton(string title)
    {
        var button = new Button(this)
        {
            Text = title,
            TextSize = 13
        };
        button.SetTextColor(Color.ParseColor("#AAB6C8"));
        button.SetBackgroundDrawable(CreateRoundedBackground("#151D2A", 14));
        button.SetAllCaps(false);
        button.Click += (_, _) => SelectTab(title);
        return button;
    }

    private void SelectTab(string title)
    {
        _selectedTab = title;
        if (_mainTab is null || _debugTab is null || _settingsTab is null)
            return;

        _mainTab.Visibility = title == "Main" ? ViewStates.Visible : ViewStates.Gone;
        _debugTab.Visibility = title == "Debug" ? ViewStates.Visible : ViewStates.Gone;
        _settingsTab.Visibility = title == "Settings" ? ViewStates.Visible : ViewStates.Gone;
        foreach (var button in _tabButtons)
        {
            var selected = button.Text == title;
            button.SetTextColor(Color.ParseColor(selected ? "#FFFFFF" : "#AAB6C8"));
            button.SetBackgroundDrawable(CreateRoundedBackground(selected ? "#315DCE" : "#151D2A", 14));
        }
    }

    private GradientDrawable CreateRoundedBackground(string color, float radiusDp)
    {
        var background = new GradientDrawable();
        background.SetColor(Color.ParseColor(color));
        background.SetCornerRadius(Dp((int)radiusDp));
        return background;
    }

    private void AddFieldLabel(LinearLayout parent, string text, int topMargin = 0)
    {
        var label = new TextView(this) { Text = text, TextSize = 14 };
        label.SetTextColor(Color.ParseColor("#C4D0E0"));
        parent.AddView(label, MatchWrap(topMargin));
    }

    private Spinner MakeSpinner(System.Collections.Generic.IReadOnlyList<string> values, int selection)
    {
        var spinner = new Spinner(this);
        var adapter = new ArrayAdapter<string>(
            this,
            global::Android.Resource.Layout.SimpleSpinnerItem,
            new System.Collections.Generic.List<string>(values));
        adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        spinner.Adapter = adapter;
        spinner.SetSelection(selection);
        return spinner;
    }

    private void ShowAlert(string title, string message)
    {
        new AlertDialog.Builder(this)
            .SetTitle(title)
            .SetMessage(message)
            .SetPositiveButton("OK", (_, _) => { })
            .Show();
    }

    private FrameLayout.LayoutParams MatchFrame() =>
        new(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

    private FrameLayout CreateTransmissionView()
    {
        var frame = new FrameLayout(this);
        frame.SetBackgroundColor(Color.ParseColor("#080B10"));

        var metrics = Resources.DisplayMetrics;
        var flashWidth = (int)(metrics.WidthPixels * 0.78f);
        var flashHeight = (int)(metrics.HeightPixels * 0.42f);
        _flashView = new View(this);
        _flashView.SetBackgroundColor(Color.Black);
        _flashView.Click += (_, _) => _transmissionCancellation?.Cancel();
        frame.AddView(_flashView, new FrameLayout.LayoutParams(
            flashWidth, flashHeight, GravityFlags.Center));

        var instructions = new TextView(this)
        {
            Text = "TRANSMITTING  ·  TAP TO CANCEL",
            Gravity = GravityFlags.Center,
            TextSize = 12
        };
        instructions.SetTextColor(Color.ParseColor("#AAB6C8"));
        var labelParams = new FrameLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, Dp(48), GravityFlags.Top | GravityFlags.CenterHorizontal);
        frame.AddView(instructions, labelParams);
        frame.Click += (_, _) => _transmissionCancellation?.Cancel();
        return frame;
    }

    private EditText MakeCoordinateInput(string label, LinearLayout parent)
    {
        var textLabel = new TextView(this) { Text = label };
        textLabel.SetTextColor(Color.ParseColor("#C4D0E0"));
        parent.AddView(textLabel, MatchWrap(Dp(8)));

        var input = new EditText(this)
        {
            Hint = "Decimal degrees",
            InputType = InputTypes.ClassNumber
                | InputTypes.NumberFlagSigned
                | InputTypes.NumberFlagDecimal
        };
        input.SetSingleLine(true);
        input.SetTextColor(Color.White);
        input.SetHintTextColor(Color.ParseColor("#91A0B8"));
        var inputParams = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f);
        inputParams.LeftMargin = Dp(4);
        inputParams.RightMargin = Dp(4);
        parent.AddView(input, inputParams);
        return input;
    }

    private bool TryGetLocation(out short latitude, out short longitude)
    {
        if (TryParseCoordinate(_latitudeInput?.Text, -90m, 90m, out var latitudeDegrees)
            && TryParseCoordinate(_longitudeInput?.Text, -180m, 180m, out var longitudeDegrees))
        {
            latitude = (short)decimal.Round(latitudeDegrees * 100m, 0, MidpointRounding.AwayFromZero);
            longitude = (short)decimal.Round(longitudeDegrees * 100m, 0, MidpointRounding.AwayFromZero);
            return true;
        }

        var alert = new AlertDialog.Builder(this);
        alert.SetTitle("Invalid location");
        alert.SetMessage("Enter latitude from -90 to 90 and longitude from -180 to 180.");
        alert.SetPositiveButton("OK", (_, _) => { });
        alert.Show();
        latitude = 0;
        longitude = 0;
        return false;
    }

    private static bool TryParseCoordinate(
        string? text,
        decimal minimum,
        decimal maximum,
        out decimal coordinate)
    {
        return decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out coordinate)
            && coordinate >= minimum
            && coordinate <= maximum;
    }

    private Button MakeActionButton(string text, Func<Task> onClick)
    {
        var button = new Button(this)
        {
            Text = text,
            TextSize = 15
        };
        button.SetTextColor(Color.White);
        button.SetBackgroundDrawable(CreateRoundedBackground("#315DCE", 14));
        button.SetAllCaps(false);
        button.Click += async (_, _) => await onClick();
        return button;
    }

    private async Task StartTransmissionAsync(uint[] data, DateTime startAt, double frequency)
    {
        if (_transmitting)
            return;

        _transmitting = true;
        foreach (var button in _transmitButtons)
            button.Enabled = false;

        var dataText = string.Join(" / ", Array.ConvertAll(data, value => $"0x{value:X}"));
        _dataLabel!.Text = dataText;
        Window!.AddFlags(WindowManagerFlags.KeepScreenOn);
        _transmissionCancellation = new CancellationTokenSource();
        var cancellationToken = _transmissionCancellation.Token;
        var transmissionView = CreateTransmissionView();
        SetContentView(transmissionView);

        try
        {
            foreach (var value in data)
            {
                await TransmitAsync(value, startAt, frequency, cancellationToken);
                startAt = startAt.AddMilliseconds(34_000.0 / frequency);
            }
        }
        catch (System.OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _transmitting = false;
            Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
            if (!IsFinishing && !IsDestroyed)
            {
                SetContentView(_home);
                foreach (var button in _transmitButtons)
                    button.Enabled = true;
                _dataLabel!.Text = dataText;
            }

            _transmissionCancellation?.Dispose();
            _transmissionCancellation = null;
            _flashView = null;
        }
    }

    private async Task TransmitAsync(
        uint data,
        DateTime startAt,
        double frequency,
        CancellationToken cancellationToken)
    {
        var startDelay = startAt - DateTime.Now;
        if (startDelay > TimeSpan.Zero)
            await Task.Delay(startDelay, cancellationToken);

        var timer = Stopwatch.StartNew();
        var intervalTicks = (long)(Stopwatch.Frequency / (double)frequency);

        for (var step = 0; step <= 34; step++)
        {
            await WaitUntilAsync(timer, step * intervalTicks, cancellationToken);

            if (step == 0)
            {
                SetFlashColor(Color.White);
            }
            else if (step == 33)
            {
                SetFlashColor(Color.Black);
            }
            else if (step < 33)
            {
                var bit = 32 - step;
                SetFlashColor(((data >> bit) & 1) == 0 ? Color.Black : Color.White);
            }
        }
    }

    private async Task WaitUntilAsync(
        Stopwatch timer,
        long targetTicks,
        CancellationToken cancellationToken)
    {
        while (timer.ElapsedTicks < targetTicks)
        {
            var remainingTicks = targetTicks - timer.ElapsedTicks;
            var milliseconds = remainingTicks * 1000.0 / Stopwatch.Frequency;
            await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(1, milliseconds)), cancellationToken);
        }
    }

    private void SetFlashColor(Color color)
    {
        if (_flashView is not null)
        {
            _flashView.SetBackgroundColor(color);
        }
    }

    private int Dp(int value) =>
        (int)(value * Resources.DisplayMetrics.Density + 0.5f);

    private LinearLayout.LayoutParams MatchWrap(int topMargin = 0)
    {
        var layoutParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent);
        if (topMargin > 0)
            layoutParams.TopMargin = topMargin;
        return layoutParams;
    }

    private sealed class SpinnerSelectionListener : Java.Lang.Object, AdapterView.IOnItemSelectedListener
    {
        private readonly Action _selectionChanged;

        public SpinnerSelectionListener(Action selectionChanged)
        {
            _selectionChanged = selectionChanged;
        }

        public void OnItemSelected(AdapterView? parent, View? view, int position, long id) =>
            _selectionChanged();

        public void OnNothingSelected(AdapterView? parent) =>
            _selectionChanged();
    }
}

internal static class TransmissionProtocol
{
    private const int HeaderTimeSet = 1;
    private const int HeaderDateSet = 2;
    private const int HeaderHeartbeat = 3;
    private const int HeaderLatitude = 4;
    private const int HeaderLongitude = 5;
    private const int HeaderSetZone = 6;
    private const int HeaderTest = 7;
    private const int HeaderPatch = 8;

    public static readonly int[] ReadFrequencyValues = { 1, 2, 4, 8, 16, 32, 64, 128 };
    public static readonly int[] SamplesPerBitValues = { 1, 2, 3, 4, 5, 6, 7, 8, 16, 32 };
    public static readonly string[] ReadFrequencies =
        Array.ConvertAll(ReadFrequencyValues, value => $"{value} Hz");
    public static readonly string[] SamplesPerBit =
        Array.ConvertAll(SamplesPerBitValues, value => value.ToString(CultureInfo.InvariantCulture));

    public static readonly string[] TimeZoneNames =
    {
        "Pago Pago", "Honolulu", "Anchorage", "Los Angeles", "Denver", "Phoenix",
        "Chicago", "Regina", "New York", "Halifax", "Manaus", "Santiago",
        "Sao Paulo", "St Johns", "Nuuk", "UTC", "London", "Lagos", "Berlin",
        "Cairo", "Maputo", "Jerusalem", "Helsinki", "Nairobi", "Riyadh",
        "Moscow", "Tehran", "Dubai", "Kolkata", "Kathmandu", "Yangon",
        "Bangkok", "Shanghai", "Hong Kong", "Singapore", "Perth", "Tokyo",
        "Seoul", "Darwin", "Adelaide", "Brisbane", "Hobart", "Sydney",
        "Guam", "Tarawa", "Auckland"
    };

    public static uint CreateTimeData(out DateTime startAt, int header, DateTime now, double frequency)
    {
        return CreateTimeData(
            out startAt,
            header,
            now,
            frequency,
            CalculateBitOffset(1),
            multipacket: false,
            immediate: false);
    }

    public static uint[] CreateDateTimeData(out DateTime startAt, DateTime now, double frequency)
    {
        var heartbeatData = CreateHeartbeatData(
            out startAt,
            out var endAt,
            CalculateBitOffset(3),
            now,
            frequency,
            multipacket: true);
        var timeData = CreateTimeData(endAt, HeaderTimeSet, multipacket: true, immediate: false);
        var dateData = CreateDateData(endAt, HeaderDateSet);
        return new[] { heartbeatData, timeData, dateData };
    }

    public static uint[] CreateDebugData(out DateTime startAt, DateTime now, double frequency)
    {
        const int packetCount = 8;
        const int bitsPerPacketWithFraming = 34;
        const int finalPacketBitOffset = packetCount * bitsPerPacketWithFraming - 1;

        var timeData = CreateTimeData(
            out startAt,
            HeaderTimeSet,
            now,
            frequency,
            finalPacketBitOffset,
            multipacket: true,
            immediate: false);
        var transmissionTime = startAt.AddMilliseconds(finalPacketBitOffset * 1000.0 / frequency);

        return new[]
        {
            CreateDateData(transmissionTime, HeaderDateSet, multipacket: true),
            CreateLocationData(1234, HeaderLatitude, multipacket: true),
            CreateLocationData(-5678, HeaderLongitude, multipacket: true),
            CreateTimeZoneData(15, multipacket: true),
            CreatePatchData(0, isPlus: true, isSmaller: false, multipacket: true),
            timeData,
            CreateHeartbeatData(out _, out _, finalPacketBitOffset, now, frequency, multipacket: true),
            CreateTestData()
        };
    }

    private static uint CreateTimeData(
        out DateTime startAt,
        int header,
        DateTime now,
        double frequency,
        int bitOffset,
        bool multipacket,
        bool immediate)
    {
        startAt = now.AddMilliseconds(1000 - now.Millisecond);
        var timestamp = startAt.AddMilliseconds(bitOffset * 1000.0 / frequency);
        return CreateTimeData(timestamp, header, multipacket, immediate);
    }

    private static uint CreateTimeData(
        DateTime timestamp,
        int header,
        bool multipacket,
        bool immediate)
    {
        var data = ((uint)header << 26)
            | ((uint)timestamp.Second << 20)
            | ((uint)timestamp.Minute << 14)
            | ((uint)timestamp.Hour << 9);
        if (multipacket)
            data |= 0x80000000;
        if (immediate)
            data |= 0x40000000;

        return AddChecksum(data);
    }

    private static uint CreateHeartbeatData(
        out DateTime startAt,
        out DateTime endAt,
        int bitOffset,
        DateTime now,
        double frequency,
        bool multipacket)
    {
        var offsetMilliseconds = (now.Millisecond + bitOffset * 1000.0 / frequency) % 1000;
        startAt = now.AddMilliseconds(2000 - offsetMilliseconds);
        endAt = startAt.AddMilliseconds(bitOffset * 1000.0 / frequency);
        return CreateHeartbeatData(multipacket);
    }

    private static int CalculateBitOffset(int packetIndex) =>
        packetIndex * 33 + (packetIndex - 1);

    public static uint CreateDateData(DateTime now, int header, bool multipacket = false)
    {
        var data = ((uint)header << 26)
            | ((uint)now.Day << 21)
            | ((uint)now.Month << 17)
            | ((uint)(now.Year - 2020) << 11);
        if (multipacket)
            data |= 0x80000000;
        return AddChecksum(data);
    }

    public static uint CreateHeartbeatData(bool multipacket = false)
    {
        var data = (uint)HeaderHeartbeat << 26;
        if (multipacket)
            data |= 0x80000000;
        return AddChecksum(data);
    }

    public static uint CreateLocationData(short value, int header, bool multipacket)
    {
        var data = ((uint)header << 26) | ((uint)(ushort)value << 10);
        if (multipacket)
            data |= 0x80000000;
        return AddChecksum(data);
    }

    public static uint CreateTimeZoneData(byte index, bool multipacket = false)
    {
        var data = ((uint)HeaderSetZone << 26) | ((uint)index << 20);
        if (multipacket)
            data |= 0x80000000;
        return AddChecksum(data);
    }

    public static uint CreateTestData() =>
        AddChecksum((uint)HeaderTest << 26);

    private static uint CreatePatchData(byte offset, bool isPlus, bool isSmaller, bool multipacket)
    {
        var data = ((uint)HeaderPatch << 26)
            | ((uint)(isSmaller ? 1 : 0) << 25)
            | ((uint)(isPlus ? 1 : 0) << 24)
            | ((uint)offset << 16);
        if (multipacket)
            data |= 0x80000000;
        return AddChecksum(data);
    }

    public static bool TryParseHexData(string? text, out uint data)
    {
        var value = text?.Trim() ?? string.Empty;
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            value = value[2..];

        return uint.TryParse(
            value,
            NumberStyles.AllowHexSpecifier,
            CultureInfo.InvariantCulture,
            out data);
    }

    private static uint AddChecksum(uint data)
    {
        var checksum = (byte)(((data >> 24) + (data >> 16) + (data >> 8)) & 0xFF);
        return data + checksum;
    }
}
