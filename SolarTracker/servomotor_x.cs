using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace SolarTracker
{
    public sealed class servomotor_x
    {
        private GpioPin servoPin;
        private const int servo_pin = 13;
        double currentPulseWidth;
        Stopwatch stopwatch;
        public void Run()
        {
            stopwatch = Stopwatch.StartNew();
            GpioController controller = GpioController.GetDefault();
            servoPin = controller.OpenPin(servo_pin);
            servoPin.SetDriveMode(GpioPinDriveMode.Output);
            //You do not need to await this, as your goal is to have this run for the lifetime of the application
            Windows.System.Threading.ThreadPool.RunAsync(this.MotorThread, Windows.System.Threading.WorkItemPriority.High);
        }

        public void SetPulse(double pulse) { currentPulseWidth = pulse; }
        private void MotorThread(IAsyncAction action)
        {
            //This motor thread runs on a high priority task and loops forever to pulse the motor as determined by the drive buttons
            while (true)
            {
                //If a button is pressed the pulsewidth is changed to cause the motor to spin in the appropriate direction
                //Write the pin high for the appropriate length of time
                servoPin.Write(GpioPinValue.High);
                //Use the wait helper method to wait for the length of the pulse
                Wait(currentPulseWidth);
                //The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                servoPin.Write(GpioPinValue.Low);
                Wait(2000);
            }
        }
        //A synchronous wait is used to avoid yielding the thread 
        //This method calculates the number of CPU ticks will elapse in the specified time and spins
        //in a loop until that threshold is hit. This allows for very precise timing.
        private void Wait(double milliseconds)
        {
            long initialTick = stopwatch.ElapsedTicks;
            long initialElapsed = stopwatch.ElapsedMilliseconds;
            double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {

            }
        }
    }
}
