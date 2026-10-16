using FCIWeb.Core.Utility;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class MapObjTests
    {
        private sealed class Target
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string NonImpostabile { get; }
        }

        private sealed class OtherTarget
        {
            public int Id { get; set; }
            public string Nome { get; set; }
        }

        [Fact]
        public void ObjectToStatic_CopiesMatchingProperties()
        {
            var target = new Target();
            var source = new OtherTarget { Id = 7, Nome = "Rossi" };

            var result = new MapObj().ObjectToStatic(source, target);

            Assert.Same(target, result);
            Assert.Equal(7, result.Id);
            Assert.Equal("Rossi", result.Nome);
        }

        [Fact]
        public void ObjectToStatic_AnonymousSource_CopiesMatchingProperties()
        {
            var target = new Target();
            var result = new MapObj().ObjectToStatic(new { Id = 42, Nome = "Bianchi", Ignored = true }, target);

            Assert.Equal(42, result.Id);
            Assert.Equal("Bianchi", result.Nome);
        }

        [Fact]
        public void ObjectToStatic_NullEntity_CreatesInstance()
        {
            var result = new MapObj().ObjectToStatic<OtherTarget>(new { Id = 1, Nome = "X" }, null);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("X", result.Nome);
        }

        [Fact]
        public void ObjectToStatic_NullSource_ReturnsEntity()
        {
            var target = new Target { Id = 5 };
            var result = new MapObj().ObjectToStatic(null, target);
            Assert.Same(target, result);
            Assert.Equal(5, result.Id);
        }

        [Fact]
        public void ObjectToStatic_DoesNotThrowOnReadOnlyProperty()
        {
            var result = new MapObj().ObjectToStatic(new { Id = 3, NonImpostabile = "x" }, new Target());
            Assert.Equal(3, result.Id);
        }
    }
}
