using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System;
using System.Threading;
using Toolbox.NETMF.Hardware;

namespace Scott_s_Lockbox
{
    public class Program
    {
        private const string PASSCODE_1 = "1670";
        private const string PASSCODE_2 = "8";
        
        // Buzzer on PIN 11
        private static BitBangBuzzer buzzer = new BitBangBuzzer(Pins.GPIO_PIN_D10);

        private static bool isLocked = true;
        private static MatrixKeyPad keyPad;

        // Locked LED on PIN 13
        private static OutputPort ledPin = new OutputPort(Pins.GPIO_PIN_D13, false);

        // This holds the numbers the person has pushed so far
        private static System.Collections.ArrayList passwordBuffer = new System.Collections.ArrayList();

        // Solenoid Control on PIN 11
        private static OutputPort solenoidPin = new OutputPort(Pins.GPIO_PIN_D11, false);

        /// <summary>
        /// This is the method that runs when we turn it on
        /// </summary>
        public static void Main()
        {
            // Set up the pins we are plugging in to
            Cpu.Pin[] RowPins = { Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D4 };
            Cpu.Pin[] ColPins = { Pins.GPIO_PIN_D3, Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D1 };

            // Turn on the key pad
            keyPad = new MatrixKeyPad(RowPins, ColPins);

            // Listen for when a button is pushed
            keyPad.OnKeyUp += keyPad_OnKeyUp;

            // Always lock when power is turned on
            LockTheBox();

            while (true)
            {
            }
        }

        private static void FlashLed(int howMany, int howFast)
        {
            var ledState = ledPin.Read();
            for (int i = 0; i < howMany; i++)
            {
                ledPin.Write(!ledState);
                Thread.Sleep(howFast); // Sleep for a sec
                ledPin.Write(ledState);
                Thread.Sleep(howFast); // Sleep for a sec
            }
        }

        private static string GetPasswordString()
        {
            string password = string.Empty;
            foreach (var item in passwordBuffer)
            {
                password = password + item.ToString();
            }
            return password;
        }

        private static void keyPad_OnKeyUp(uint data1, uint data2, DateTime time)
        {
            KeyWasPressed(data1);
        }

        /// <summary>
        /// This method runs when someone pushes a key
        /// </summary>
        /// <param name="keyCode"></param>
        private static void KeyWasPressed(uint keyCode)
        {
            FlashLed(1, 200); // Flash the LED to show the user we pressed a key

            switch (keyCode)
            {
                case 0: // 1 Key Was Pressed
                case 1: // 2 Key Was Pressed
                case 2: // 3 Key Was Pressed
                case 3: // 4 Key Was Pressed
                case 4: // 5 Key Was Pressed
                case 5: // 6 Key Was Pressed
                case 6: // 7 Key Was Pressed
                case 7: // 8 Key Was Pressed
                case 8: // 9 Key Was Pressed
                    passwordBuffer.Add(keyCode + 1);
                    break;

                case 10: // 0 Key Was Pressed
                    passwordBuffer.Add(0);
                    break;

                case 9: // * Key Was Pressed - Clear Password
                    passwordBuffer.Clear();
                    FlashLed(4, 200);
                    break;

                case 11: // # Key Was Pressed - Check Password
                    string password = GetPasswordString();
                    if (password == PASSCODE_1 || password == PASSCODE_2)
                    {
                        if (isLocked) UnlockTheBox();
                        else LockTheBox();
                    }
                    else // Wrong password - play the buzzer
                    {
                        buzzer.Write(true);
                        Thread.Sleep(1000);
                        buzzer.Write(false);
                        Thread.Sleep(1000);
                    }
                    passwordBuffer.Clear(); // Clear the password so they can try again
                    break;
            }
        }

        /// <summary>
        /// This method locks the box
        /// </summary>
        private static void LockTheBox()
        {
            solenoidPin.Write(false); // Turn off solenoid
            ledPin.Write(true); // Turn the light on
            passwordBuffer.Clear(); // Clear the password buffer
            isLocked = true;
        }

        /// <summary>
        /// This method unlocks the box
        /// </summary>
        private static void UnlockTheBox()
        {
            solenoidPin.Write(true); // Turn the solenoid on
            ledPin.Write(false); // Turn the light off
            passwordBuffer.Clear(); // Clear the password buffer
            isLocked = false;
        }
    }
}