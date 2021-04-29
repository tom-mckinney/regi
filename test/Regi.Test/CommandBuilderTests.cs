using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Regi.Test
{
    public class CommandBuilderTests
    {
        [Fact]
        public void Add_directive_success()
        {
            var builder = new CommandBuilder();

            builder.Add("--foo");
            builder.Add("--bar");

            Assert.Equal("--foo --bar", builder.Build());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Add_directive_throws_when_null_empty_or_whitespace(string directive)
        {
            var builder = new CommandBuilder();

            Assert.Throws<InvalidOperationException>(() => builder.Add(directive));
        }

        [Fact]
        public void Add_key_value_success()
        {
            var builder = new CommandBuilder();

            builder.Add("--foo", "bar");
            builder.Add("--up", "10");

            Assert.Equal("--foo bar --up 10", builder.Build());
        }

        [Theory]
        [InlineData(null, "bar")]
        [InlineData("foo", null)]
        [InlineData(null, null)]
        [InlineData("", "bar")]
        [InlineData("foo", "")]
        [InlineData("", "")]
        [InlineData(" ", "bar")]
        [InlineData("foo", " ")]
        [InlineData(" ", " ")]
        public void Add_key_value_throws_when_null_empty_or_whitespace(string key, string value)
        {
            var builder = new CommandBuilder();

            Assert.Throws<InvalidOperationException>(() => builder.Add(key, value));
        }

        [Fact]
        public void Add_collection_success()
        {
            var builder = new CommandBuilder();

            builder.Add("--foo", new[] { "bar", "baz" });

            Assert.Equal("--foo bar --foo baz", builder.Build());
        }

        [Fact]
        public void Add_collection_empty_success()
        {
            var builder = new CommandBuilder();

            builder.Add("--foo", Array.Empty<string>());

            Assert.Equal("", builder.Build());
        }

        [Fact]
        public void Add_collection_null_success()
        {
            var builder = new CommandBuilder();

            builder.Add("--foo", (string[])null);

            Assert.Equal("", builder.Build());
        }
    }
}
