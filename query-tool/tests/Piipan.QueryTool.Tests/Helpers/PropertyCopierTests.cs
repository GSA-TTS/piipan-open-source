using System.Collections.Generic;
using Piipan.QueryTool.Client.Helpers;
using Xunit;

namespace Piipan.QueryTool.Tests.Helpers
{
    public class DummyClass
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public (decimal, bool) Prop3 { get; set; }
    }
    public record DummyRecord
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public List<string> Prop3 { get; set; }
    }
    public class PropertyCopierTests
    {
        [Fact]
        public void PropertyCopier_CopiesProperties_FromClasses()
        {
            // Act
            DummyClass dummyClass = new DummyClass()
            {
                Prop1 = "test",
                Prop2 = 3,
                Prop3 = (0.3m, true)
            };
            DummyClass copiedClass = new DummyClass()
            {
                Prop1 = "oldvalue",
                Prop2 = 2,
                Prop3 = (1.4m, false)
            };
            PropertyCopier.CopyPropertiesTo(dummyClass, copiedClass);

            // Assert
            Assert.Equal("test", copiedClass.Prop1);
            Assert.Equal(3, copiedClass.Prop2);
            Assert.Equal((0.3m, true), copiedClass.Prop3);
        }

        [Fact]
        public void PropertyCopier_CopiesProperties_FromRecord()
        {
            // Act
            DummyRecord dummyClass = new DummyRecord()
            {
                Prop1 = "test",
                Prop2 = 3,
                Prop3 = new List<string> { "test1", "test2" }
            };
            DummyRecord copiedClass = new DummyRecord()
            {
                Prop1 = "oldvalue",
                Prop2 = 2,
                Prop3 = new List<string> { "old1", "old2" }
            };
            PropertyCopier.CopyPropertiesTo(dummyClass, copiedClass);

            // Assert
            Assert.Equal("test", copiedClass.Prop1);
            Assert.Equal(3, copiedClass.Prop2);
            Assert.Equal(new List<string> { "test1", "test2" }, copiedClass.Prop3);
            Assert.Equal(dummyClass, copiedClass);
        }
    }
}
