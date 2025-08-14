namespace MicrowaveOvenController;

public class MicrowaveOvenCtrl : IDisposable
{
    private bool _heaterOn;
    private bool _lightOn;
    private int _remainingMinutes;

    private CancellationTokenSource? _cts;

    private IMicrowaveOvenHW _microwaveOvenHW;
    private object _remainingTimeLock;
    private bool _isDisposed;

    public MicrowaveOvenCtrl(IMicrowaveOvenHW microwaveOvenHW)
    {
        _microwaveOvenHW = microwaveOvenHW;
        _remainingTimeLock = new object();
        _cts = null;

        _microwaveOvenHW.DoorOpenChanged += HandleDoor;
        _microwaveOvenHW.StartButtonPressed += HandleSwitchPressed;
    }

    private void HandleSwitchPressed(object? _, EventArgs __)
    {
        if (_microwaveOvenHW.DoorOpen) { return; }
        lock (_remainingTimeLock)
        {
            _remainingMinutes++;

            if (_cts == null || _cts.IsCancellationRequested)
            {
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                Task.Run(() => RunTimerAsync(_cts.Token));
            }
        }
    }

    private void HandleDoor(bool open)
    {
        if (open)
        {
            _lightOn = true;
            TurnOffHeater();
        }
        else
        {
            _lightOn = false;
        }
    }

    private async Task RunTimerAsync(CancellationToken token)
    {
        while (_remainingMinutes > 0 && token.CanBeCanceled)
        {
            _heaterOn = true;
            _microwaveOvenHW.TurnOnHeater();
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), token);
            }
            catch (TaskCanceledException e) { _ = e; }

            lock (_remainingTimeLock)
            {
                _remainingMinutes--;

                if (_remainingMinutes <= 0)
                {
                    break;
                }
            }
        }
        TurnOffHeater();
    }

    public void TurnOffHeater()
    {
        lock (_remainingTimeLock)
        {
            _microwaveOvenHW.TurnOffHeater();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _heaterOn = false;
            _remainingMinutes = 0;
        }
    }

    public bool LightOn { get => _lightOn; }
    public bool HeaterOn { get => _heaterOn; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;

            if (disposing)
            {
                TurnOffHeater();
            }
        }
    }
}