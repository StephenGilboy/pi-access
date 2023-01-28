using System.Device.Gpio;

namespace PiAccess.Web
{
    public class Led
    {
        public int Pin { get; init; }
        private readonly PinMode _pinMode;
        private PinValue _pinValue { get; set; }
        private readonly GpioController _controller;

        public Led(int pin, GpioController controller)
        {
            Pin = pin;
            _pinMode = PinMode.Output;
            _pinValue = PinValue.Low;
            _controller = controller;
            controller.OpenPin(Pin, _pinMode);
            controller.Write(Pin, _pinValue);
        }

        public bool IsOn => _pinValue == PinValue.High;

        public void On()
        {
            _pinValue = PinValue.High;
            Write();
        }

        public void Off()
        {
            _pinValue = PinValue.Low;
            Write();
        }

        public void Toggle()
        {
            if (IsOn)
            {
                Off();
                return;
            }
            On();
        }

        private void Write()
        {
            _controller.Write(Pin, _pinValue);
        }
    }
}
