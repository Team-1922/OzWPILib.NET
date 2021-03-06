﻿using System;
using System.Collections.Generic;
using HAL.Simulator;
using HAL.Simulator.Data;
using NUnit.Framework;
using WPILib.Exceptions;
using NetworkTables.Tables;
// ReSharper disable UnusedVariable

namespace WPILib.Tests
{
    [TestFixture(0)]
    [TestFixture(5)]
    [TestFixture(58)]
    public class TestSolenoid : TestBase
    {
        private readonly int m_module;

        public TestSolenoid(int module)
        {
            m_module = module;
        }
        public Solenoid NewSolenoid()
        {
            return new Solenoid(m_module, 0);
        }

        private IReadOnlyList<SolenoidData> GetSolenoids()
        {
            return SimData.GetPCM(m_module).Solenoids;
        }

        [Test]
        public void TestSolenoidCreateDefaultModule()
        {
            using (Solenoid s = new Solenoid(0))
            {
                Assert.IsTrue(SimData.GetPCM(0).Solenoids[0].Initialized);
            }
            Assert.That(SimData.GetPCM(0).Solenoids[0].Initialized, Is.False);
        }

        [Test]
        public void TestSolenoidModuleUnderLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var s = new Solenoid(-1, 0);
            });
        }

        [Test]
        public void TestSolenoidModuleOverLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var s = new Solenoid(SolenoidModules, 0);
            });
        }

        [Test]
        public void TestSolenoidCreate()
        {
            using (Solenoid s = NewSolenoid())
            {
                Assert.IsTrue(GetSolenoids()[0].Initialized);
            }
        }

        [Test]
        public void TestMultipleAllocation()
        {
            using (Solenoid ds = NewSolenoid())
            {
                Assert.Throws(typeof (AllocationException), () =>
                {
                    var p = NewSolenoid();
                });
            }
        }

        [Test]
        public void TestSolenoidCreateAll()
        {
            List<Solenoid> solenoids = new List<Solenoid>();
            for (int i = 0; i < SolenoidChannels; i++)
            {
                solenoids.Add(new Solenoid(m_module, i));
            }

            foreach (var ds in solenoids)
            {
                ds.Dispose();
            }
        }

        [Test]
        public void TestCreateLowerLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var p = new Solenoid(m_module, -1);
            });
        }

        [Test]
        public void TestCreateUpperLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var p = new Solenoid(m_module, SolenoidChannels);
            });
        }

        [Test]
        public void TestSolenoidSet()
        {
            using (Solenoid s = NewSolenoid())
            {
                s.Set(true);
                Assert.IsTrue(GetSolenoids()[0].Value);

                s.Set(false);
                Assert.IsFalse(GetSolenoids()[0].Value);
            }
        }

        [Test]
        public void TestSolenoidGet()
        {
            using (Solenoid s = NewSolenoid())
            {
                GetSolenoids()[0].Value = true;
                Assert.IsTrue(s.Get());

                GetSolenoids()[0].Value = false;
                Assert.IsFalse(s.Get());
            }
        }

        [Test]
        public void TestSolenoidGetAll()
        {
            using (Solenoid s = NewSolenoid())
            {
                GetSolenoids()[0].Value = true;
                GetSolenoids()[1].Value = true;
                GetSolenoids()[2].Value = false;
                GetSolenoids()[3].Value = false;
                GetSolenoids()[4].Value = false;
                GetSolenoids()[5].Value = true;
                GetSolenoids()[6].Value = false;
                GetSolenoids()[7].Value = false;

                byte allSolenoids = s.GetAll();

                bool solenoid0 = ((0x1 << 0) & allSolenoids) != 0;
                bool solenoid1 = ((0x1 << 1) & allSolenoids) != 0;
                bool solenoid2 = ((0x1 << 2) & allSolenoids) != 0;
                bool solenoid3 = ((0x1 << 3) & allSolenoids) != 0;
                bool solenoid4 = ((0x1 << 4) & allSolenoids) != 0;
                bool solenoid5 = ((0x1 << 5) & allSolenoids) != 0;
                bool solenoid6 = ((0x1 << 6) & allSolenoids) != 0;
                bool solenoid7 = ((0x1 << 7) & allSolenoids) != 0;

                Assert.That(solenoid0);
                Assert.That(solenoid1);
                Assert.That(!solenoid2);
                Assert.That(!solenoid3);
                Assert.That(!solenoid4);
                Assert.That(solenoid5);
                Assert.That(!solenoid6);
                Assert.That(!solenoid7);


            }
        }

        [Test]
        public void TestBlackList()
        {
            using (Solenoid s = NewSolenoid())
            {
                Assert.IsFalse(s.IsBlackListed());
            }
        }

        [Test]
        public void TestVoltageStickyFault()
        {
            using (Solenoid s = NewSolenoid())
            {
                Assert.That(!s.GetPCMSolenoidVoltageStickyFault());
            }
        }

        [Test]
        public void TestVoltageFault()
        {
            using (Solenoid s = NewSolenoid())
            {
                Assert.That(!s.GetPCMSolenoidVoltageFault());
            }
        }

        [Test]
        public void TestClearFaults()
        {
            using (Solenoid s = NewSolenoid())
            {
                s.ClearAllPCMStickyFaults();
            }
        }

        [Test]
        public void TestSmartDashboardType()
        {
            using (Solenoid s = NewSolenoid())
            {
                Assert.That(s.SmartDashboardType, Is.EqualTo("Solenoid"));
            }
        }

        [Test]
        public void TestUpdateTableNull()
        {
            using (Solenoid s = NewSolenoid())
            {
                Assert.DoesNotThrow(() =>
                {
                    s.UpdateTable();
                });
            }
        }

        [Test]
        public void TestStartLiveWindowModeTableNull()
        {
            using (Solenoid s = NewSolenoid())
            {
                s.Set(true);
                Assert.That(s.Get, Is.True);
                s.StartLiveWindowMode();
                Assert.That(s.Get, Is.False);
            }
        }

        [Test]
        public void TestStopLiveWindowModeTableNull()
        {
            using (Solenoid s = NewSolenoid())
            {
                s.Set(true);
                Assert.That(s.Get, Is.True);
                s.StopLiveWindowMode();
                Assert.That(s.Get, Is.False);
            }
        }

        [Test]
        public void TestStartLiveWindowModeTable()
        {
            using (Solenoid s = NewSolenoid())
            {
                ITable table = new MockNetworkTable();
                s.InitTable(table);
                s.Set(true);
                Assert.That(s.Get, Is.True);
                s.StartLiveWindowMode();
                Assert.That(s.Get, Is.False);
            }
        }

        [Test]
        public void TestStopLiveWindowModeTable()
        {
            using (Solenoid s = NewSolenoid())
            {
                ITable table = new MockNetworkTable();
                s.InitTable(table);
                s.Set(true);
                Assert.That(s.Get, Is.True);
                s.StopLiveWindowMode();
                Assert.That(s.Get, Is.False);
            }
        }

        [Test]
        public void TestValueChanged()
        {
            using (Solenoid s = NewSolenoid())
            {
                s.Set(false);
                Assert.That(s.Get, Is.False);
                s.ValueChanged(null, null, true, NetworkTables.NotifyFlags.NotifyLocal);
                Assert.That(s.Get, Is.True);
                s.ValueChanged(null, null, false, NetworkTables.NotifyFlags.NotifyLocal);
                Assert.That(s.Get, Is.False);
            }
        }


    }
}
