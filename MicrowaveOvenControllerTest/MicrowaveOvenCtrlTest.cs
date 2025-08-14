using MicrowaveOvenController;
using NSubstitute;

namespace MicrowaveOvenControllerTest
{
    [TestClass]
    public sealed class MicrowaveOvenCtrlTest
    {
        private readonly MicrowaveOvenCtrl _microwaveOvenController;
        private readonly IMicrowaveOvenHW _mockHardware;

        public MicrowaveOvenCtrlTest()
        {
            _mockHardware = Substitute.For<IMicrowaveOvenHW>();
            _mockHardware.DoorOpen.Returns(false);
            _microwaveOvenController = new MicrowaveOvenCtrl(_mockHardware);
        }

        [TestMethod]
        public void StartButtonPressed_WithDoorClosed_TurnsOnHeater()
        {
            _mockHardware.DoorOpen.Returns(false);

            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);

            _mockHardware.Received(1).TurnOnHeater();
        }

        [TestMethod]
        public void DoorOpened_WhileHeaterRunning_StopsHeater()
        {
            _mockHardware.DoorOpen.Returns(false);

            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);
            _mockHardware.DidNotReceive().TurnOffHeater();
            _mockHardware.DoorOpenChanged += Raise.Event<Action<bool>>(true);

            _mockHardware.Received(1).TurnOffHeater();
        }

        [TestMethod]
        public void DoorOpened_TurnsOnLight()
        {
            _mockHardware.DoorOpenChanged += Raise.Event<Action<bool>>(true);

            Assert.IsTrue(_microwaveOvenController.LightOn);
        }

        [TestMethod]
        public void DoorClosed_TurnsOffLight()
        {
            _mockHardware.DoorOpenChanged += Raise.Event<Action<bool>>(true);

            _mockHardware.DoorOpenChanged += Raise.Event<Action<bool>>(false);

            Assert.IsFalse(_microwaveOvenController.LightOn);
        }


        [TestMethod]
        public void StartButtonPressed_WithDoorOpen_DoesNotTurnOnHeater()
        {
            _mockHardware.DoorOpen.Returns(true);
            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);

            _mockHardware.DidNotReceive().TurnOnHeater();
        }

        [TestMethod]
        public async Task StartButtonPressed_WithDoorClosed_HeaterRunsForOneMinute()
        {
            _mockHardware.DoorOpen.Returns(false);

            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);
            await Task.Delay(TimeSpan.FromSeconds(55));

            _mockHardware.DidNotReceive().TurnOffHeater();
            await Task.Delay(TimeSpan.FromSeconds(10));
            _mockHardware.Received(1).TurnOffHeater();
        }

        [TestMethod]
        public async Task StartButtonPressedTwice_WithDoorClosed_HeaterRunsForTwoMinutes()
        {
            _mockHardware.DoorOpen.Returns(false);

            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);
            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);
            await Task.Delay(TimeSpan.FromSeconds(115));

            _mockHardware.DidNotReceive().TurnOffHeater();
            await Task.Delay(TimeSpan.FromSeconds(10));
            _mockHardware.Received(1).TurnOffHeater();
        }

        [TestMethod]
        public async Task DoorOpened_AfterThreeStartButtonPresses_StopsHeaterImmediately()
        {
            _mockHardware.DoorOpen.Returns(false);

            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);
            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);
            _mockHardware.StartButtonPressed += Raise.Event<EventHandler>(this, EventArgs.Empty);
            await Task.Delay(TimeSpan.FromSeconds(100));

            _mockHardware.DidNotReceive().TurnOffHeater();
            _mockHardware.DoorOpenChanged += Raise.Event<Action<bool>>(true);
            _mockHardware.Received(1).TurnOffHeater();
        }
    }
}
