/* AlarmByZones.cs
 * 
 * A simple alarm monitoring system using a typical alarm panel.  This implementation
 * could be used in conjunction with the P C 5 0 1 0 - Digital Security Controls (DSC) 
 * ProwerSeries Security System Control Panel and sensors .
 * 
 * This code was written by Gilberto García. It is released under the terms of 
 * the Creative Commons "Attribution 3.0 Unported" license.
 * http://creativecommons.org/licenses/by/3.0/
 * 
 * WARNING: This code contains information related to a typical home alarm systems.  Please, be aware that
 * this procedure may void any warranty.
 * Any alarm system of any type may be compromised deliberately or may fail to
 * operate as expected for a variety of reasons.
 * The author, G. García, is not liable for any System Failures such as: inadequate installation, 
 * criminal knowledge, access by intruders, power failure, failure of replaceable batteries,
 * compromise of Radio Frequency (Wireless) devices, system users, smoke detectors, motion
 * detectors, warning devices (sirens, bells, horns), telephone lines, insufficient time, component
 * failure, inadequate testing, security and insurance (property or life insurance).
 * 
 * *** DISCONNECT AC POWER AND TELEPHONE LINES PRIOR TO DOING ANYTHING.
 * 
 * NOTE:
 * - Tested on Netduino Plus.
 * - Tested on the following zone circuits:
 *      a) 1 Normally Open contact and 1 Normally closed contact with End Of Line (EOL) resistor.
 *      b) Double EOL circuit, 1 Normally closed contact with 5.6kohm EOL resistor.
 * - Alarm EOL Resistors: 5600 ohms.
 * 
 * REQUIREMENT:
 * - Netduino Plus
 * - SD Card
 * - Config.ini should be copied in the SD root directory.  If no SD card is detected default values will be considered.
 * - Sparkfun ProtoScrewShield (sku: DEV-09729)
 * - WiFi connection via WiFly or any WiFi Internet Adapter.  Tested on Netgear WNCE3001. 
 * 
 * OPTIONAL (If mounting Netduino inside the Alarm Panel):
 * - USB Ruggedized / Waterproof Panel Connector - for external access to USB port.
 * - Panel Mount LED (Subminiature Green LED Assemply) - external LED mounted on panel.
 * - USB 2.0 Cable, Type A Male to A Male (10 Feet) - To interface between alarm panel and computer.
 * 
 * 
 *     Date                  Version             By                 Comments
 *   02-22-2012              1.0.0.0             G. García          Initial release.  
 *                                                                  Base implementation with default alarm values recommended by DSC.
 *                                                                  
 *   03-17-2012              2.0.0.0             G. García          New methods and classes
 *                                                                  SMTP Client (based on BanskySPOTMail), InitArrays, Blink, Stopwatch (based on Chris Walker
 *                                                                  implementation) and Http Web Server (based on MFToolkit).
 *
 *   03-22-2012              3.0.0.0             G. García          Thread to startHTTP.
 *   
 *   03-27-2012              4.0.0.0             G. García          SD Card access to store alarm events and exceptions.
 *                                                                  Modified base implementation with new variables, method (DEBUG_ACTIVITY) 
 *                                                                  and comments.
 *                                                                  
 *   03-30-2012              5.0.0.0             G. García          New Config.ini file with zone and sensor description; and Http webserver port number.
 *   
 *   04-02-2012              6.0.0.0             G. García          Added email frequency settings to the Config.ini file.
 *                                                                  Added new URL for opening SD Card file with latest Exception or event. 
 *                                                                  Code cleanup.
 *                                                                  
 *   04-08-2012              7.0.0.0             G. García          Added Pachube capability.
 *   
 *   04-10-2012              8.0.0.0             G. García          Replaced ThreadStart instead of Thread.
 *   
 *   04-12-2012              9.0.0.0             G. García          Replaced most of StringBuilder with ArrayList. A lot of OutOfMemoryExceptions after adding more than
 *                                                                    approximately 90 characters.
 *                                                                  Added [STORE_LOG] and [STORE_EXCEPTION] options on Config.ini.
 *                                                                  
 *   04-14-2012              10.0.0.0            G. García          Modified Pachube's stopwatch method to update status when events occur.
 *   
 *   04-16-2012              11.0.0.0            G. García          Modified HTML Table border.
 *                                                                  Back to original thread implementation.
 *                                                                  Added Garbage collection on loop.
 *                                                                  
 *   04-20-2012              12.0.0.0            G. García          Added Pachube settings and Email settings to the Config.ini. 
 *                                                                  Code cleanup.
 *                                                                  
 *   04-25-2012              13.0.0.0            G. García          SMTPClient class initialization.
 *                                                                  Added code comment to Pachube class.
 *                                                                  Added alarm/sensor detail info to SDCard.saveFile method.
 * 
 *   04-25-2012              14.0.0.0            G. García          Excluded Config.ini when listing files in HTML page.
 *   
 *   04-28-2012              15.0.0.0            G. García          Added comments/description to Pachube class.
 *   
 *   05-07-2012              16.0.0.0            G. García          Pachube class now independent from AlarmByZones to make it modular.
 *                                                                  Added about.htm to project resources.
 *                                                                  
 *   05-08-2012              17.0.0.0            G. García          Initial implementation of ResourceGenerator project with Htm.About class.
 *                                                                   This class will generate an updated about.htm file with the latest AssemblyInfo
 *                                                                   file version to be displayed.
 *                                                                  Modified comment header on AlarmByZones.cs.
 *                                                                  
 *   05-08-2012              18.0.0.0            G. García          Added Alarm class library and moved alarm related methods.
 *   
 *   05-09-2012              19.0.0.0            G. García          Modified about.htm content generated on ResourceGenerator.Htm.About.
 *                                                                  Moved alarm definitions and constants to their own class.
 *                                                                  Modified comments on Config.ini file.
 *                                                                  
 *   05-11-2012              21.0.0.0            G. García          Renamed solution to HomeAlarmPlus.
 *                                                                  Code cleanup. Ready to share with Netduino community.
 *                                                                  
 *   06-10-2012              21.0.0.0            G. García          Added email tracking when accessing webserver.
 *                                                                  Added a fix (source: http://tf.nist.gov/tf-cgi/servers.cgi) NIST Internet Time Service
 *                                                                   instead of selecting a random one.
 *                                                                   
 *   06-14-2012              22.0.0.0                               HomeAlarmPlus base class library.
 *                                                                  Code cleanup. Ready to share library on github.
 *                                                                  Deleted unused references.
 *                                                                  Added Cascading Style Sheets (CSS) to HTML table (source http://www.textfixer.com/resources/css-tables.php).
 *                                                                  Added CSS to HTML button (source: http://www.pagetutor.com/button_designer/index.html).   
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;


namespace AlarmByZones
{
    public class AlarmByZones
    {
        #region Constructor
        public AlarmByZones()
        {

        }
        #endregion

        #region Declarations
        /// <summary>
        /// Alarm zones (Analog Input)
        /// </summary>
        static SecretLabs.NETMF.Hardware.AnalogInput[] Zones = new SecretLabs.NETMF.Hardware.AnalogInput[Alarm.User_Definitions.Constants.ACTIVE_ZONES];

        /// <summary>
        /// Alarm zones LEDs (Digital Output)
        /// </summary>
        static Microsoft.SPOT.Hardware.OutputPort[] AlarmLeds = new Microsoft.SPOT.Hardware.OutputPort[Alarm.User_Definitions.Constants.ACTIVE_ZONES];

        /// <summary>
        /// Motion detector sensors (Analog Input)
        /// </summary>
        static SecretLabs.NETMF.Hardware.AnalogInput[] Sensors = new SecretLabs.NETMF.Hardware.AnalogInput[Alarm.User_Definitions.Constants.MOTION_SENSORS];

        /// <summary>
        /// Motion detector LEDs (Digital Output)
        /// </summary>
        static Microsoft.SPOT.Hardware.OutputPort[] MotionLeds = new Microsoft.SPOT.Hardware.OutputPort[Alarm.User_Definitions.Constants.MOTION_SENSORS];

        /// <summary>
        /// Gets the total elapsed time measured by the current instance of each alarm zone.
        /// </summary>
        static System.Diagnostics.Stopwatch[] stopwatchZones = new Stopwatch[Alarm.User_Definitions.Constants.ACTIVE_ZONES];

        /// <summary>
        /// Gets the total elapsed time measured by the current instance of each motion detector sensor.
        /// </summary>
        // download the latest stopwatch class located at: http://forums.netduino.com/index.php?/topic/97-systemdiagnosticsstopwatch-class/
        static System.Diagnostics.Stopwatch[] stopwatchSensors = new Stopwatch[Alarm.User_Definitions.Constants.MOTION_SENSORS];

        /// <summary>
        /// Flag for detected zones when trigger.
        /// </summary>
        static bool[] detectedZones = new bool[Alarm.User_Definitions.Constants.ACTIVE_ZONES];

        /// <summary>
        /// Flag for detected sensors when is trigger.
        /// </summary>
        static bool[] detectedSensors = new bool[Alarm.User_Definitions.Constants.MOTION_SENSORS];

        #endregion

        #region Delegates

        /// <summary>
        /// Monitor zones delegate
        /// </summary>
        public delegate void MonitorZonesDelegate();

        /// <summary>
        /// Monitor motion sensor delegate
        /// </summary>
        public delegate void MonitorMotionSensorDelegate();
        #endregion

        public static void Main()
        {
            Zones[0] = new SecretLabs.NETMF.Hardware.AnalogInput(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A0);
            Zones[1] = new SecretLabs.NETMF.Hardware.AnalogInput(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A1);
            Zones[2] = new SecretLabs.NETMF.Hardware.AnalogInput(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A2);
            Zones[3] = new SecretLabs.NETMF.Hardware.AnalogInput(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A3);

            AlarmLeds[0] = new Microsoft.SPOT.Hardware.OutputPort(Pins.GPIO_PIN_D2, false);
            AlarmLeds[1] = new Microsoft.SPOT.Hardware.OutputPort(Pins.GPIO_PIN_D3, false);
            AlarmLeds[2] = new Microsoft.SPOT.Hardware.OutputPort(Pins.GPIO_PIN_D4, false);
            AlarmLeds[3] = new Microsoft.SPOT.Hardware.OutputPort(Pins.GPIO_PIN_D5, false);

            //Add description to every zone
            ArrayList zoneDescription = new ArrayList();
            zoneDescription.Add("Zone1=FIRST FLOOR - Living room windows, Dining room windows, Porch (sliding doors), Garage door access.");
            zoneDescription.Add("Zone2=SECOND FLOOR - Master Bedroom and Bathroom Windows.");
            zoneDescription.Add("Zone3=FIRST FLOOR - Master Bedroom windows.");
            zoneDescription.Add("Zone4=SECOND FLOOR - Bedroom 2 and bathroom windows.");
            for (int i = 0; i < Alarm.User_Definitions.Constants.ACTIVE_ZONES; i++)
            {
                string zone = (string)zoneDescription[i].ToString();
                string[] parse = zone.Split('=');
                Alarm.Common.Alarm_Info.zoneDescription.Add(parse[0], parse[1]);
                Debug.Print("Zones " + parse[0] + " ," + parse[1]);
            }

            //Add description to every zone
            ArrayList sensorDescription = new ArrayList();
            sensorDescription.Add("Sensor1=Main door access");
            for (int i = 0; i < Alarm.User_Definitions.Constants.MOTION_SENSORS; i++)
            {
                string sensor = (string)sensorDescription[i].ToString();
                string[] parse = sensor.Split('=');
                Alarm.Common.Alarm_Info.sensorDescription.Add(parse[0], parse[1]);
                Debug.Print("Sensors " + parse[0] + " ," + parse[1]);
            }

            Sensors[0] = new SecretLabs.NETMF.Hardware.AnalogInput(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A4);
            MotionLeds[0] = new Microsoft.SPOT.Hardware.OutputPort(Pins.GPIO_PIN_D7, false);

            MonitorZonesDelegate monitorZones = new MonitorZonesDelegate(MonitorZones);

            MonitorMotionSensorDelegate monitorMotion = new MonitorMotionSensorDelegate(MonitorSensors);

            //based on a post by Valkyrie-MT
            Debug.Print("Setting NTP-time");
            SetTime();

            InitArrays();
            Debug.Print(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);

            new Thread(Alarm.ProtoScrewShield_LED.Blink).Start();
            //Web Server based on MFToolkit library by: Michael Schwarz 
            //download latest library at: (http://mftoolkit.codeplex.com/")
            new Thread(Alarm.WebServer.startHttp).Start();

            while (true)
            {
                Debug.Print("Memory available: " + Debug.GC(true));
                monitorZones();
                monitorMotion();
                Thread.Sleep(Alarm.Common.Alarm_Constants.ALARM_DELAY_TIME);
            }
        }

        #region Methods

        /// <summary>
        /// Loops thru each alarm zone
        /// </summary>
        static void MonitorZones()
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                int vInput = Zones[i].Read();
                float volts = ((float)vInput / 1024.0f) * 3.3f;
                string strZoneDescription = "N/A"; //If zone description is not found on SD Card N/A is default description.

                Debug.Print("Zone " + (i + 1).ToString() + ": Volts: " + volts);

                AlarmLeds[i].Write(volts >= 3);

                //elapsed seconds
                double eSeconds = stopwatchZones[i].ElapsedSeconds;
                //elapsed minutes
                double eMinutes = stopwatchZones[i].ElapsedMinutes;

                Debug.Print("stopwatch[" + i.ToString() + "] = " + eSeconds.ToString() + " seconds");
                Debug.Print("stopwatch[" + i.ToString() + "] = " + eMinutes.ToString() + " minutes\n");

                if (volts >= 3)
                {
                    if (!detectedZones[i] || (detectedZones[i] && eMinutes >= Alarm.ConfigDefault.Data.EMAIL_FREQUENCY))
                    {
                        if (Alarm.Common.Alarm_Info.zoneDescription.Count > 0)
                        {
                            if (Alarm.Common.Alarm_Info.zoneDescription.Contains("Zone" + (i + 1).ToString()))
                            {
                                strZoneDescription = (string)Alarm.Common.Alarm_Info.zoneDescription["Zone" + (i + 1).ToString()];
                            }
                        }
                        string info = "Zone " + (i + 1).ToString() + " " + strZoneDescription;
                        stopwatchZones[i] = Stopwatch.StartNew();
                        detectedZones[i] = true;
                        Alarm.Common.Alarm_Info.sbActivity.AppendLine(DateTime.Now.ToString());
                        Alarm.Common.Alarm_Info.sbActivity.AppendLine("Zone " + (i + 1).ToString());
                        Alarm.Common.Alarm_Info.sbActivity.AppendLine(strZoneDescription);
                    }
                }
                else
                {
                    detectedZones[i] = false;
                }
            }
        }

        /// <summary>
        /// Loops thru each sensor
        /// </summary>
        static void MonitorSensors()
        {
            for (int i = 0; i < Sensors.Length; i++)
            {
                int vInput = Sensors[i].Read();
                float volts = ((float)vInput / 1024.0f) * 3.3f;
                string strSensorDescription = "N/A"; //If sensor description is not found on SD Card N/A is default description.


                Debug.Print("Sensor " + (i + 1).ToString() + ": Volts: " + volts);

                MotionLeds[i].Write(volts >= 3);

                double eSeconds = stopwatchSensors[i].ElapsedSeconds;
                double eMinutes = stopwatchSensors[i].ElapsedMinutes;

                Debug.Print("stopwatch[" + i.ToString() + "] = " + eSeconds.ToString() + " seconds");
                Debug.Print("stopwatch[" + i.ToString() + "] = " + eMinutes.ToString() + " minutes\n");

                if (volts >= 3)
                {
                    /*
                      Case #1:
                        !detectedZones[i] = not triggered before. This is the first time in this cycle and first email to send.
                      Case #2:
                        detectedZones[i] && eMinutes >= EMAIL_FREQUENCY = triggered before and time is up for sending another email.                     
                     */
                    if (!detectedSensors[i] || (detectedSensors[i] && eMinutes >= Alarm.ConfigDefault.Data.EMAIL_FREQUENCY))
                    {
                        if (Alarm.Common.Alarm_Info.sensorDescription.Count > 0)
                        {
                            if (Alarm.Common.Alarm_Info.sensorDescription.Contains("Sensor" + (i + 1).ToString()))
                            {
                                strSensorDescription = (string)Alarm.Common.Alarm_Info.sensorDescription["Sensor" + (i + 1).ToString()];
                            }
                        }
                        string info = "Sensor " + (i + 1).ToString() + " " + strSensorDescription;
                        stopwatchSensors[i] = Stopwatch.StartNew();
                        detectedSensors[i] = true;
                        Alarm.Common.Alarm_Info.sbActivity.AppendLine(DateTime.Now.ToString());
                        Alarm.Common.Alarm_Info.sbActivity.AppendLine("Sensor " + (i + 1).ToString());
                        Alarm.Common.Alarm_Info.sbActivity.AppendLine(strSensorDescription);
                    }
                }
                else
                {
                    detectedSensors[i] = false;
                }
            }
        }

        /// <summary>
        /// Synchronize Netduino time with NTP Server
        /// </summary>
        public static void SetTime()
        {
            //download Valkyrie-MT latest Extension class located at:
            //http://forums.netduino.com/index.php?/topic/475-still-learning-internet-way-to-grab-date-and-time-on-startup/
            //uncomment following line
            //Extension.SetFromNetwork(DateTime.Now, new TimeSpan(-5, 0, 0));
        }

        /// <summary>
        /// Initializes stopwatch and alarms/sensors arrays
        /// </summary>
        private static void InitArrays()
        {
            for (int i = 0; i < Alarm.User_Definitions.Constants.ACTIVE_ZONES; i++)
            {
                stopwatchZones[i] = Stopwatch.StartNew();
                detectedZones[i] = false;
            }

            for (int i = 0; i < Alarm.User_Definitions.Constants.MOTION_SENSORS; i++)
            {
                stopwatchSensors[i] = Stopwatch.StartNew();
                detectedSensors[i] = false;
            }
        }
        #endregion

    }
}
