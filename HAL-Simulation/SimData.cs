﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HAL_Simulator
{
    /// <summary>
    /// This class allows us to listen for changes in a specific part of the dictionary. 
    /// </summary>
    /// <remarks>Not everything is wrapped, because we don't care about changes in certain things.
    /// <para/>Note that since the indexer is not marked virtual, if you set a value in the
    /// dictionary manually, the notify callback will not be called unless you first
    /// box the dictionary as a NotifyDict. The recommended way to update the data however
    /// is to update the HalInData dictionary, and then call <see cref="SimData.UpdateHalData"/> 
    /// with that dictionary.</remarks>
    /// <typeparam name="T">Please use dynamic</typeparam>
    /// <typeparam name="T2">Please use dynamic</typeparam>
    public class NotifyDict<T, T2> : Dictionary<T, T2>
    {
        private Dictionary<T, Action<dynamic, dynamic>> m_callbacks = new Dictionary<T, Action<dynamic, dynamic>>(); 


        /// <summary>
        /// Register a notify function to get called when the field updates.
        /// </summary>
        /// <param name="key">The key to watch</param>
        /// <param name="action">The callback function</param>
        /// <param name="notify">Whether to notify immediately</param>
        public void Register(T key, Action<dynamic, dynamic> action, bool notify = false)
        {
            if (!this.ContainsKey(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key), "Cannot register for non existent key");
            }
            if (!m_callbacks.ContainsKey(key))
            {
                m_callbacks.Add(key, action);
            }
            else
            {
                m_callbacks[key] += action;
            }
            if (notify)
            {
                m_callbacks[key]?.Invoke(key, this[key]);
            }
        }

        /// <summary>
        /// Cancel a notify function
        /// </summary>
        /// <param name="key">The key the function is waiting for</param>
        /// <param name="action">The callback function to cancel.</param>
        public void Cancel(T key, Action<dynamic, dynamic> action)
        {
            if (action != null && m_callbacks.ContainsKey(key)) m_callbacks[key] -= action;
        }

        public new T2 this[T key]
        {
            get { return base[key]; }
            set
            {
                base[key] = value;
                if (m_callbacks.ContainsKey(key))
                {
                    m_callbacks[key]?.Invoke(key, value);
                }
            }
        }

    }

    /// <summary>
    /// Marks a variable in the dict as something the simulator can set.
    /// </summary>
    internal class IN
    {
        public dynamic value { get; set; }
        public IN(dynamic d)
        {
            value = d;
        }
    }

    /// <summary>
    /// Marks a variable in the dict as something the robot will set.
    /// </summary>
    internal class OUT
    {
        public dynamic value { get; set; }
        public OUT(dynamic d)
        {
            value = d;
        }
    }

    /// <summary>
    /// Marks a variable in the dict as something controlled by the driver station.
    /// </summary>
    internal class DS
    {
        public dynamic value { get; set; }
        public DS(dynamic d)
        {
            value = d;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SimData
    {
        internal static Dictionary<dynamic, dynamic> halData = new Dictionary<dynamic, dynamic>();

        public static Dictionary<dynamic, dynamic> HalData => halData;
         
        internal static Dictionary<dynamic, dynamic> halInData = new Dictionary<dynamic, dynamic>();

        public static Dictionary<dynamic, dynamic> HalInData => halInData;

        internal static Dictionary<dynamic, dynamic> halDSData = new Dictionary<dynamic, dynamic>();

        public static Dictionary<dynamic, dynamic> HalDSData => halDSData;

        /// <summary>
        /// This method gets a copy of both data dictionaries.
        /// It is here for use with reflection. Please use the Properties instead
        /// </summary>
        /// <param name="halDataOut"></param>
        /// <param name="halInDataOut"></param>
        public static void GetData(out Dictionary<dynamic, dynamic> halDataOut, 
            out Dictionary<dynamic, dynamic> halInDataOut, out Dictionary<dynamic, dynamic> halDSDataOut)
        {
            halDataOut = halData;
            halInDataOut = halInData;
            halDSDataOut = halDSData;
        }

        internal static IntPtr halNewDataSem = IntPtr.Zero;

        public static void ResetHALData()
        {
            halData.Clear();
            halInData.Clear();
            halDSData.Clear();
            halNewDataSem = IntPtr.Zero;


            halData["alliance_station"] = new DS(0);
            halData["time"] = new Dictionary<dynamic, dynamic>
            {
                {"has_source", new IN(false) },
                {"program_start", new OUT(SimHooks.GetTime())},
                {"match_start", new OUT(0.0)}
            };

            halData["control"] = new Dictionary<dynamic, dynamic>
            {
                {"has_source", new IN(false)},
                {"enabled", new DS(false)},
                {"autonomous", new DS(false)},
                {"test", new DS(false)},
                {"eStop", new DS(false)},
                {"fms_attached", new DS(false)},
                {"ds_attached", new DS(false)},
            };
            halData["reports"] = new NotifyDict<dynamic, dynamic>();

            halData["joysticks"] = new List<dynamic>();
            for (int i = 0; i < 6; i++)
            {
                halData["joysticks"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"has_source", new IN(false) },
                    {"buttons", new DS(new bool[13]) },
                    {"axes", new DS(new int[6]) },
                    {"povs", new DS(new int[12]) },
                    {"isXbox", new DS(0)},
                    {"type",  new DS(0)},
                    {"name", new DS("") },
                    {"axisCount", new DS(6) },
                    {"buttonCount", new DS(12) }
                });
            }

            halData["fpga_button"] = new IN(false);
            halData["error_data"] = new OUT(null);

            halData["accelerometer"] = new Dictionary<dynamic, dynamic>()
            {
                {"has_source", new IN(false) },
                {"active", new OUT(false) },
                {"range", new OUT(0) },
                {"x", new IN(0) },
                {"y", new IN(0) },
                {"z", new IN(0) },
            };

            halData["analog_sample_rate"] = new OUT(1024.0);

            halData["analog_out"] = new List<dynamic>();
            for (int i = 0; i < 8; i++)
            {
                halData["analog_out"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"initialized", new OUT(false) },
                    {"voltage", new OUT(0.0) },

                });
            }

            halData["analog_in"] = new List<dynamic>();
            for (int i = 0; i < 8; i++)
            {
                halData["analog_in"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"has_source", new IN(false) },
                    {"initialized", new OUT(false) },
                    {"avg_bits", new OUT(0) },
                    {"oversample_bits", new OUT(0) },
                    { "value", new IN(0) },
                    { "avg_value", new IN(0) },
                    { "voltage", new IN(0) },
                    { "avg_voltage", new IN(0) },
                    { "lsb_weight", new IN(1) },
                    { "offset", new IN(65535) },

                    {"accumulator_initialized", new OUT(0) },
                    {"accumulator_center", new OUT(0) },
                    { "accumulator_value", new IN(1) },
                    { "accumulator_count", new IN(1) },
                    {"accumulator_deadband", new OUT(0) },

                });
            }
            halData["analog_trigger"] = new List<dynamic>();
            for (int i = 0; i < 8; i++)
            {

                halData["analog_trigger"].Add(new Dictionary<dynamic, dynamic>()
                {
                    {"has_source", new IN(false)},
                    {"initialized", new OUT(false)},
                    {"port", new OUT(null)},
                    {"trig_lower", new OUT(null)},
                    {"trig_upper", new OUT(null)},
                    {"trig_type", new OUT(null)},
                    {"trig_state", new OUT(false)},
                });
            }

            halData["compressor"] = new NotifyDict<dynamic, dynamic>()
            {
                {"has_source", new IN(false) },
                {"initialized", new OUT(false) },
                {"on", new IN(false) },
                {"closed_loop_enabled", new OUT(false) },
                {"pressure_switch", new IN(false) },
                {"current", new IN(0.0) },
            };

            halData["pwm"] = new List<dynamic>();
            for (int i = 0; i < 20; i++)
            {
                halData["pwm"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"initialized", new OUT(false) },
                    {"type", new OUT(null) },
                    {"raw_value", new OUT(0.0) },
                    {"value", new OUT(0.0) },
                    {"period_scale", new OUT(null) },
                    {"zero_latch", new OUT(false) },

                });
            }

            halData["pwm_loop_timing"] = new IN(40);

            halData["d0_pwm"] = new OUT(new Dictionary<dynamic, dynamic>[6]); //# dict with keys: duty_cycle, pin
            halData["d0_pwm_rate"] = new OUT(null);


            halData["relay"] = new List<dynamic>();
            for (int i = 0; i < 8; i++)
            {
                halData["relay"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"initialized", new OUT(false) },
                    {"fwd", new OUT(false) },
                    {"rev", new OUT(false) },
                });
            }

            halData["mxp"] = new List<dynamic>();
            for (int i = 0; i < 16; i++)
            {
                halData["mxp"].Add(new Dictionary<dynamic, dynamic>
                    {
                        {"initialized", new OUT(false)},
                    }
                );
            }
            halData["dio"] = new List<dynamic>();
            for (int i = 0; i < 26; i++)
            {
                halData["dio"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"has_source", new IN(false) },
                    {"initialized", new OUT(false) },
                    {"value", new IN(false) },
                    {"pulse_length", new OUT(null) },
                    {"is_input", new OUT(false) },

                });
            }

            halData["encoder"] = new List<dynamic>();
            for (int i = 0; i < 4; i++)
            {
                halData["encoder"].Add(new Dictionary<dynamic, dynamic>()
                {
                    {"has_source", new IN(false) },
                    {"initialized", new OUT(false) },
                    {"config", new OUT(null)},
                    {"count", new IN(0) },
                    {"period", new IN(float.MaxValue) },
                    {"max_period", new OUT(0) },
                    {"direction", new IN(false) },
                    {"reverse_direction", new OUT(false) },
                    {"samples_to_average", new OUT(0) }
                });
            }
            halData["counter"] = new List<dynamic>();
            for (int i = 0; i < 8; i++)
            {
                halData["counter"].Add(new Dictionary<dynamic, dynamic>()
                {
                    {"has_source", new IN(false) },
                    {"initialized", new OUT(false) },
                    {"config", new OUT(null)},
                    {"count", new IN(0) },
                    {"period", new IN(float.MaxValue) },
                    {"max_period", new OUT(0) },
                    {"direction", new IN(false) },
                    {"reverse_direction", new OUT(false) },
                    {"samples_to_average", new OUT(0) },
                    {"mode", new OUT(0) },
                    {"average_size", new OUT(0) },

                    {"up_source_channel", new OUT(0u)},
                    {"up_source_trigger", new OUT(false)},
                    {"down_source_channel", new OUT(0u)},
                    {"down_source_trigger", new OUT(false)},

                    {"update_when_empty", new OUT(false)},

                    {"up_rising_edge", new OUT(false)},
                    {"up_falling_edge", new OUT(false)},
                    {"down_rising_edge",new OUT(false)},
                    {"down_falling_edge",new OUT(false)},

                    {"pulse_length_threshold" ,new OUT(0)},
                });
            }

            halData["user_program_state"] = new OUT(null);

            halData["power"] = new Dictionary<dynamic, dynamic>()
            {
                {"has_source", new IN(false) },
                { "vin_voltage", new IN(0) },
				{"vin_current", new IN(0) },
				{"user_voltage_6v", new IN(6.0)},
				{"user_current_6v", new IN(0)},
				{"user_active_6v", new  IN(false)},
			    {"user_faults_6v", new  IN(0)},
			    {"user_voltage_5v",new  IN(5.0)},
			 	{"user_current_5v",new  IN(0)},
                {"user_active_5v",  new IN(false)},
                {"user_faults_5v", new  IN(0)},
                {"user_voltage_3v3",new IN(3.3)},
				{"user_current_3v3",new IN(0)},
                {"user_active_3v3", new IN(false)},
                {"user_faults_3v3", new IN(0)},
            };

            halData["solenoid"] = new List<dynamic>();
            for (int i = 0; i < 8; i++)
            {
                halData["solenoid"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"initialized", new OUT(false)},
                    {"value", new OUT(false)}
                });
            };

            halData["pdp"] = new Dictionary<dynamic, dynamic>()
            {
                {"has_source", new IN(false) },
                {"temperature", new IN(0) },
                {"voltage", new IN(0) },
                {"current", new IN(new double[16]) },
                {"total_current", new IN(0) },
                {"total_power", new IN(0) },
                {"total_energy", new IN(0) },

            };



            halData["CAN"] = new NotifyDict<dynamic, dynamic>();

            //manually filling out DS data. Later this will be automated.
            halDSData["alliance_station"] = 0;
            halDSData["control"] = new Dictionary<dynamic, dynamic>
            {
                {"enabled", false},
                {"autonomous", false},
                {"test", false},
                {"eStop", false},
                {"fms_attached", false},
                {"ds_attached", false},
            };

            halDSData["joysticks"] = new List<dynamic>();
            for (int i = 0; i < 6; i++)
            {
                halDSData["joysticks"].Add(new NotifyDict<dynamic, dynamic>
                {
                    {"buttons", new bool[13] },
                    {"axes", new int[6] },
                    {"povs", new int[12] },
                    {"isXbox", 0},
                    {"type",  0},
                    {"name", "" },
                    {"axisCount", 6 },
                    {"buttonCount", 12 }
                });
            }



            FilterHalData(halData, halInData);
        }

        private static void FilterHalData(Dictionary<dynamic, dynamic> both, Dictionary<dynamic, dynamic> inData)
        {
            List<dynamic> myKeys = both.Keys.ToList();

            foreach (var s in myKeys)
            {
                dynamic v = both[s];
                dynamic k = s;
                if (v is IN)
                {
                    both[k] = v.value;
                    if (v is Array)
                    {
                        var vOut = both[k];
                        int count = 0;
                        foreach (var vv in v.value)
                        {
                            vOut.Add(vv);
                            count++;
                        }
                    }
                    else
                    {
                        inData[k] = v.value;
                    }
                }
                else if (v is OUT)
                {
                    both[k] = v.value;
                }
                else if (v is DS)
                {
                    both[k] = v.value;
                }
                else if (v is Dictionary<dynamic, dynamic>)
                {
                    Dictionary<dynamic, dynamic> vIn = new Dictionary<dynamic, dynamic>();
                    FilterHalData(v, vIn);
                    if (vIn.Count > 0)
                    {
                        inData[k] = vIn;
                    }
                }
                else if (v is List<dynamic>)
                {
                    List<dynamic> vIn = FilterHalList(v);
                    if (vIn.Count > 0)
                    {
                        inData[k] = vIn;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(both), " Must be dictionary, list, In, or Out.");
                }
            }
        }

        private static List<dynamic> FilterHalList(List<dynamic> both)
        {
            List<dynamic> inList = new List<dynamic>();

            foreach (var v in both)
            {
                if (!(v is Dictionary<dynamic, dynamic>))
                {
                    throw new ArgumentOutOfRangeException(nameof(both), "Lists can only contain dictionaries, otherwise must only be contained in IN or OUT.");
                }
                Dictionary<dynamic, dynamic> vIn = new Dictionary<dynamic, dynamic>();
                FilterHalData(v, vIn);
                if (vIn.Count != 0)
                {
                    inList.Add(vIn);
                }
            }
            return inList;
        }

        /// <summary>
        /// This function takes a dictionary, and updates it into the main HAL dictionary.
        /// </summary>
        /// <param name="inDict"></param>
        /// <param name="outData"></param>
        public static void UpdateHalData(Dictionary<dynamic, dynamic> inDict, Dictionary<dynamic, dynamic> outData = null)
        {
            if (outData == null)
                outData = halData;

            foreach (var o in inDict)
            {
                if (o.Value is Dictionary<dynamic, dynamic>)
                {
                    UpdateHalData(o.Value, outData[o.Key]);
                }
                else if (o.Value is List<dynamic> || o.Value is Array)
                {
                    var vOut = outData[o.Key];
                    int count = 0;
                    foreach (var vv in o.Value)
                    {
                        if (vv is Dictionary<dynamic, dynamic>)
                        {
                            UpdateHalData(vv, vOut[count]);
                        }
                        else
                        {
                            vOut[count] = vv;
                        }
                        count++;
                    }
                }
                else
                {
                    //Since the Dictionary Indexer is not marked virtual, we have to explcitly
                    //see if the dictionary is a NotifyDict, and if so, box it and then set the value.
                    NotifyDict<dynamic, dynamic> tmp = outData as NotifyDict<dynamic, dynamic>;
                    if (tmp != null)
                    {
                        tmp[o.Key] = o.Value;
                    }
                    else
                    {
                        outData[o.Key] = o.Value;
                    }
                }
            }
        }
    }
}
