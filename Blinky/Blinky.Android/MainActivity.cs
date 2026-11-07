using Android.App;
using Android.Content.PM;
using Android.Graphics;
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
    private const int DefaultFrequency = 16;

    private LinearLayout? _home;
    private View? _flashView;
    private EditText? _valueInput;
    private EditText? _frequencyInput;
    private TextView? _dataLabel;
    private Button[] _transmitButtons = Array.Empty<Button>();
    private CancellationTokenSource? _transmissionCancellation;
    private bool _transmitting;

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
        _home.SetPadding(Dp(20), Dp(24), Dp(20), Dp(20));
        _home.SetBackgroundColor(Color.Black);

        var title = new TextView(this)
        {
            Text = "Blinky",
            TextSize = 28,
            Gravity = GravityFlags.Center
        };
        title.SetTextColor(Color.White);
        _home.AddView(title, MatchWrap());

        var frequencyLabel = new TextView(this) { Text = "Frequency (Hz)" };
        frequencyLabel.SetTextColor(Color.White);
        _home.AddView(frequencyLabel, MatchWrap(Dp(8)));

        _frequencyInput = new EditText(this)
        {
            Text = DefaultFrequency.ToString(CultureInfo.InvariantCulture),
            Hint = "Enter a positive whole number",
            InputType = InputTypes.ClassNumber
        };
        _frequencyInput.SetSingleLine(true);
        _frequencyInput.SetTextColor(Color.White);
        _frequencyInput.SetHintTextColor(Color.LightGray);
        _home.AddView(_frequencyInput, MatchWrap());

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
        _valueInput.SetHintTextColor(Color.LightGray);
        _valueInput.SetSelectAllOnFocus(true);
        _home.AddView(_valueInput, MatchWrap(Dp(12)));

        _dataLabel = new TextView(this)
        {
            Text = "Ready to transmit",
            TextSize = 16
        };
        _dataLabel.SetTextColor(Color.White);
        _home.AddView(_dataLabel, MatchWrap(Dp(8)));

        var timestampButton = MakeButton("Transmit timestamp", async () =>
        {
            if (!TryGetFrequency(out var frequency))
                return;

            var data = TransmissionProtocol.CreateTimeData(out var startAt, 1, DateTime.Now, frequency);
            await StartTransmissionAsync(new[] { data }, startAt, frequency);
        });
        var dateButton = MakeButton("Transmit date", async () =>
        {
            if (!TryGetFrequency(out var frequency))
                return;

            var data = TransmissionProtocol.CreateDateData(DateTime.Now, 2);
            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });
        var heartbeatButton = MakeButton("Transmit heartbeat", async () =>
        {
            if (!TryGetFrequency(out var frequency))
                return;

            var data = TransmissionProtocol.CreateHeartbeatData();
            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });
        var dateTimeButton = MakeButton("Date/time MP", async () =>
        {
            if (!TryGetFrequency(out var frequency))
                return;

            var data = TransmissionProtocol.CreateDateTimeData(out var startAt, DateTime.Now, frequency);
            await StartTransmissionAsync(data, startAt, frequency);
        });
        var valueButton = MakeButton("Transmit value", async () =>
        {
            if (!TryGetFrequency(out var frequency))
                return;

            if (!TransmissionProtocol.TryParseHexData(_valueInput!.Text, out var data))
            {
                new AlertDialog.Builder(this)
                    .SetTitle("Invalid value")
                    .SetMessage("Enter a hexadecimal value from 0 to FFFFFFFF.")
                    .SetPositiveButton("OK", (_, _) => { })
                    .Show();
                return;
            }

            await StartTransmissionAsync(new[] { data }, DateTime.Now, frequency);
        });

        _transmitButtons = new[] { timestampButton, dateButton, heartbeatButton, dateTimeButton, valueButton };
        _home.AddView(timestampButton, MatchWrap(Dp(8)));
        _home.AddView(dateButton, MatchWrap(Dp(8)));
        _home.AddView(heartbeatButton, MatchWrap(Dp(8)));
        _home.AddView(dateTimeButton, MatchWrap(Dp(8)));
        _home.AddView(valueButton, MatchWrap(Dp(8)));
        SetContentView(_home);
    }

    private bool TryGetFrequency(out int frequency)
    {
        if (int.TryParse(
                _frequencyInput?.Text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out frequency)
            && frequency > 0)
        {
            return true;
        }

        new AlertDialog.Builder(this)
            .SetTitle("Invalid frequency")
            .SetMessage("Enter a positive whole number of hertz.")
            .SetPositiveButton("OK", (_, _) => { })
            .Show();
        return false;
    }

    private Button MakeButton(string text, Func<Task> onClick)
    {
        var button = new Button(this) { Text = text };
        button.Click += async (_, _) => await onClick();
        return button;
    }

    private async Task StartTransmissionAsync(uint[] data, DateTime startAt, int frequency)
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
        _flashView = new View(this);
        _flashView.Click += (_, _) => _transmissionCancellation?.Cancel();
        SetContentView(_flashView);

        try
        {
            foreach (var value in data)
            {
                await TransmitAsync(value, startAt, frequency, cancellationToken);
                startAt = DateTime.Now;
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
        int frequency,
        CancellationToken cancellationToken)
    {
        var startDelay = startAt - DateTime.Now;
        if (startDelay > TimeSpan.Zero)
            await Task.Delay(startDelay, cancellationToken);

        var timer = Stopwatch.StartNew();
        var intervalTicks = (long)(Stopwatch.Frequency / (double)frequency);

        for (var step = 0; step <= 33; step++)
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
            else
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
}

internal static class TransmissionProtocol
{
    public static uint CreateTimeData(out DateTime startAt, int header, DateTime now, int frequency)
    {
        return CreateTimeData(out startAt, header, now, frequency, 33);
    }

    public static uint[] CreateDateTimeData(out DateTime startAt, DateTime now, int frequency)
    {
        var timeData = CreateTimeData(out startAt, 1, now, frequency, 66);
        var dateData = CreateDateData(now, 2, multipacket: true);
        return new[] { dateData, timeData };
    }

    private static uint CreateTimeData(
        out DateTime startAt,
        int header,
        DateTime now,
        int frequency,
        int bitOffset)
    {
        startAt = now.AddMilliseconds(1000 - now.Millisecond);
        var timestamp = startAt.AddMilliseconds(bitOffset * (1000 / frequency));
        var data = ((uint)header << 28)
            | ((uint)timestamp.Second << 22)
            | ((uint)timestamp.Minute << 16)
            | ((uint)timestamp.Hour << 11);

        return AddChecksum(data);
    }

    public static uint CreateDateData(DateTime now, int header, bool multipacket = false)
    {
        var data = ((uint)header << 28)
            | ((uint)now.Day << 23)
            | ((uint)now.Month << 19)
            | ((uint)(now.Year - 2020) << 13);
        if (multipacket)
            data |= 0x80000000;
        return AddChecksum(data);
    }

    public static uint CreateHeartbeatData() =>
        AddChecksum(3u << 28);

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
