﻿using System;
using System.Runtime.InteropServices;
using HAL.Base;
using static HAL.Simulator.SimData;

// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

#pragma warning disable 1591

namespace HAL.SimulatorHAL
{
    ///<inheritdoc cref="HAL"/>
    internal class HALSolenoid
    {

        internal static void Initialize(IntPtr library, ILibraryLoader loader)
        {
            Base.HALSolenoid.InitializeSolenoidPort = initializeSolenoidPort;
            Base.HALSolenoid.FreeSolenoidPort = freeSolenoidPort;
            Base.HALSolenoid.CheckSolenoidModule = checkSolenoidModule;
            Base.HALSolenoid.GetSolenoid = getSolenoid;
            Base.HALSolenoid.GetAllSolenoids = getAllSolenoids;
            Base.HALSolenoid.SetSolenoid = setSolenoid;
            Base.HALSolenoid.GetPCMSolenoidBlackList = getPCMSolenoidBlackList;
            Base.HALSolenoid.GetPCMSolenoidVoltageStickyFault = getPCMSolenoidVoltageStickyFault;
            Base.HALSolenoid.GetPCMSolenoidVoltageFault = getPCMSolenoidVoltageFault;
            Base.HALSolenoid.ClearAllPCMStickyFaults_sol = clearAllPCMStickyFaults_sol;
        }

        [CalledSimFunction]
        public static IntPtr initializeSolenoidPort(IntPtr port_pointer, ref int status)
        {
            SolenoidPort p = new SolenoidPort
            {
                port = PortConverters.GetHalPort(port_pointer)
            };
            status = 0;
            InitializePCM(p.port.module);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(p));
            Marshal.StructureToPtr(p, ptr, true);
            HAL.freePort(port_pointer);
            return ptr;
        }

        [CalledSimFunction]
        public static void freeSolenoidPort(IntPtr solenoid_port_pointer)
        {
            if (solenoid_port_pointer == IntPtr.Zero) return;
            var p = PortConverters.GetSolenoidPort(solenoid_port_pointer);
            GetPCM(p.port.module).Solenoids[p.port.pin].Initialized = false;
            Marshal.FreeHGlobal(solenoid_port_pointer);
        }

        [CalledSimFunction]
        public static bool checkSolenoidModule(byte module)
        {
            return module < 63;
        }

        [CalledSimFunction]
        public static bool getSolenoid(IntPtr solenoid_port_pointer, ref int status)
        {
            status = 0;
            var p = PortConverters.GetSolenoidPort(solenoid_port_pointer);
            return GetPCM(p.port.module).Solenoids[p.port.pin].Value;
        }

        [CalledSimFunction]
        public static byte getAllSolenoids(IntPtr solenoid_port_pointer, ref int status)
        {
            status = 0;
            var port = PortConverters.GetSolenoidPort(solenoid_port_pointer);
            var solenoids = GetPCM(port.port.module).Solenoids;
            byte total = 0;
            for (int i = 0; i < solenoids.Count; i++)
            {
                total = (byte) (total + ((solenoids[i].Value ? 1 : 0) << i));
            }
            return total;
        }


        [CalledSimFunction]
        public static void setSolenoid(IntPtr solenoid_port_pointer, bool value,
            ref int status)
        {
            status = 0;
            var p = PortConverters.GetSolenoidPort(solenoid_port_pointer);
            GetPCM(p.port.module).Solenoids[p.port.pin].Value = value;
        }

        [CalledSimFunction]
        public static int getPCMSolenoidBlackList(IntPtr solenoid_port_pointer, ref int status)
        {
            status = 0;
            return 0;
        }

        [CalledSimFunction]
        public static bool getPCMSolenoidVoltageStickyFault(IntPtr solenoid_port_pointer, ref int status)
        {
            status = 0;
            return false;
        }


        [CalledSimFunction]
        public static bool getPCMSolenoidVoltageFault(IntPtr solenoid_port_pointer, ref int status)
        {
            status = 0;
            return false;
        }


        [CalledSimFunction]
        public static void clearAllPCMStickyFaults_sol(IntPtr solenoid_port_pointer, ref int status)
        {
            status = 0;
        }
    }
}
