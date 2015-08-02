﻿using System;
using System.Collections.Generic;
using System.Linq;
using HAL_Base;
using HAL_Simulator;
using NUnit.Framework;

namespace WPILib.Tests
{
    [TestFixture]
    public class TestAnalogTrigger
    {
        [TestFixtureSetUp]
        public static void Initialize()
        {
            TestBase.StartCode();
        }

        [TestFixtureTearDown]
        public static void Kill()
        {
            
            DriverStation.Instance.Release();
        }

        private Dictionary<dynamic, dynamic> GetData(int index)
        {
            return SimData.halData["analog_trigger"][index];
        }

        [Test]
        public void TestAnalogTriggerInitFree([Range(0, 8)]int pin)
        {
            int index = 0;
            using (AnalogTrigger trigger = new AnalogTrigger(pin))
            {
                index = trigger.Index;
                Assert.IsTrue(GetData(index)["initialized"]);
                Assert.NotNull(GetData(index)["port"]);
                Assert.AreEqual(pin, GetData(index)["port"].pin);
            }
            Assert.IsFalse(GetData(index)["initialized"]);
        }

        [Test]
        public void TestSetFiltered()
        {
            using (AnalogTrigger at = new AnalogTrigger(2))
            {
                at.Filtered = true;
                Assert.AreEqual("filtered", GetData(at.Index)["trig_type"]);
            }
        }
    }
}