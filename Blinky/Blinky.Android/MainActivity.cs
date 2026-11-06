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
    private const int Frequency = 8;

    private LinearLayout? _home;
    private View? _flashView;
    private EditText? _valueInput;
    private CheckBox? _exactCheckBox;
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

        _exactCheckBox = new CheckBox(this) { Text = "Exact timestamp" };
        _exactCheckBox.SetTextColor(Color.White);
        _home.AddView(_exactCheckBox, MatchWrap());

        _dataLabel = new TextView(this)
        {
            Text = "Ready to transmit",
            TextSize = 16
        };
        _dataLabel.SetTextColor(Color.White);
        _home.AddView(_dataLabel, MatchWrap(Dp(8)));

        var timestampButton = MakeButton("Transmit timestamp", async () =>
        {
            var data = TransmissionProtocol.CreateTimeData(
                out var startAt,
                _exactCheckBox!.Checked,
                DateTime.Now);
            await StartTransmissionAsync(data, startAt);
        });
        var dateButton = MakeButton("Transmit date", async () =>
        {
            var data = TransmissionProtocol.CreateDateData(DateTime.Now);
            await StartTransmissionAsync(data, DateTime.Now);
        });
        var valueButton = MakeButton("Transmit value", async () =>
        {
            if (!TransmissionProtocol.TryParseHexData(_valueInput!.Text, out var data))
            {
                new AlertDialog.Builder(this)
                    .SetTitle("Invalid value")
                    .SetMessage("Enter a hexadecimal value from 0 to FFFFFFFF.")
                    .SetPositiveButton("OK", (_, _) => { })
                    .Show();
                return;
            }

            await StartTransmissionAsync(data, DateTime.Now);
        });

        _transmitButtons = new[] { timestampButton, dateButton, valueButton };
        _home.AddView(timestampButton, MatchWrap(Dp(8)));
        _home.AddView(dateButton, MatchWrap(Dp(8)));
        _home.AddView(valueButton, MatchWrap(Dp(8)));
        SetContentView(_home);
    }

    private Button MakeButton(string text, Func<Task> onClick)
    {
        var button = new Button(this) { Text = text };
        button.Click += async (_, _) => await onClick();
        return button;
    }

    private async Task StartTransmissionAsync(uint data, DateTime startAt)
    {
        if (_transmitting)
            return;

        _transmitting = true;
        foreach (var button in _transmitButtons)
            button.Enabled = false;

        _dataLabel!.Text = $"Transmitting 0x{data:X8}";
        Window!.AddFlags(WindowManagerFlags.KeepScreenOn);
        _transmissionCancellation = new CancellationTokenSource();
        var cancellationToken = _transmissionCancellation.Token;
        _flashView = new View(this);
        _flashView.Click += (_, _) => _transmissionCancellation?.Cancel();
        SetContentView(_flashView);

        try
        {
            await TransmitAsync(data, startAt, cancellationToken);
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
                _dataLabel!.Text = $"Last value: 0x{data:X8}";
            }

            _transmissionCancellation?.Dispose();
            _transmissionCancellation = null;
            _flashView = null;
        }
    }

    private async Task TransmitAsync(uint data, DateTime startAt, CancellationToken cancellationToken)
    {
        var startDelay = startAt - DateTime.Now;
        if (startDelay > TimeSpan.Zero)
            await Task.Delay(startDelay, cancellationToken);

        var timer = Stopwatch.StartNew();
        var intervalTicks = Stopwatch.Frequency / Frequency;

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
    public static uint CreateTimeData(out DateTime startAt, bool exact, DateTime now)
    {
        startAt = now.AddSeconds(2)
            .AddMilliseconds(1000 - now.Millisecond)
            .AddMilliseconds(-125);

        var timestamp = startAt.AddMilliseconds(33 * (1000 / 8));
        var header = exact ? 3 : 1;
        var data = ((uint)header << 28)
            | ((uint)timestamp.Second << 22)
            | ((uint)timestamp.Minute << 16)
            | ((uint)timestamp.Hour << 11);

        return AddChecksum(data);
    }

    public static uint CreateDateData(DateTime now)
    {
        var data = (2u << 28)
            | ((uint)now.Day << 23)
            | ((uint)now.Month << 19)
            | ((uint)(now.Year - 2020) << 13);
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
