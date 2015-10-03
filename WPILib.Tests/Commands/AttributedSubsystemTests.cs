﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Context;
using System.Reflection;
using WPILib.Extras.AttributedCommandModel;
using WPILib.Commands;

namespace WPILib.Tests.Commands
{
    [TestFixture]
    public class AttributedSubsystemTests : AbstractCommandTest
    {
        internal class TestReflectionContext : CustomReflectionContext
        {
            public override TypeInfo MapType(TypeInfo type)
            {
                string name = type.Name;
                return base.MapType(type);
            }
            protected override IEnumerable<object> GetCustomAttributes(MemberInfo member, IEnumerable<object> declaredAttributes)
            {
                if(member == typeof(ASubsystem))
                {
                    yield return new ExportSubsystemAttribute{ DefaultCommandType = typeof(AttributedMockCommand) };
                }
                foreach (var attr in declaredAttributes) yield return attr;
            }
        }

        [ExportSubsystem(DefaultCommandType = typeof(AttributedMockCommand))]
        [ExportSubsystem(DefaultCommandType = typeof(AttributedMockCommand))]
        public class BSubsystem : Subsystem
        {
            protected override void InitDefaultCommand()
            {
            }
        }

        public class AttributedMockCommand : MockCommand
        {
            private readonly Subsystem subsystem;

            public AttributedMockCommand(Subsystem subsystem)
            {
                this.subsystem = subsystem;
            }
        }

        [Test]
        public void SubsystemSetsDefaultCommandByAttribute()
        {
            using (AttributedRobot robot = new AttributedRobot())
            {
                robot.RobotInit();
                Assert.IsTrue(robot.Subsystems.OfType<BSubsystem>().Any());
                Scheduler.Instance.Run();
                Assert.IsInstanceOf<MockCommand>(robot.Subsystems.First().GetCurrentCommand());
            }
        }

        [Test]
        public void SubsystemSetsDefaultCommandByAttributeThroughReflectionContext()
        {
            using (AttributedRobot robot = new AttributedRobot(new TestReflectionContext()))
            {
                robot.RobotInit();
                Assert.IsTrue(robot.Subsystems.OfType<ASubsystem>().Any());
                Scheduler.Instance.Run();
                Assert.IsInstanceOf<MockCommand>(robot.Subsystems.OfType<ASubsystem>().First().GetCurrentCommand());
            }
        }

        [Test]
        public void MultipleSubsystemsGeneratedByMultipleAttributes()
        {
            using (AttributedRobot robot = new AttributedRobot())
            {
                robot.RobotInit();
                Assert.AreEqual(2, robot.Subsystems.OfType<BSubsystem>().Count());
            }
        }
    }
}
