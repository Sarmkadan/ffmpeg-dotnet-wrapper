// SPDX-License-Identifier: MIT
// © 2024 RedRocket

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Tests
{
    public class ExtensionMethodsTests
    {
        [Fact]
        public void StringBuilderExtensions_ShouldHandleArguments()
        {
            var sb = new StringBuilder();
            sb.AppendArgument("first");
            Assert.Equal("first", sb.ToString());

            sb.AppendArgument(null);
            Assert.Equal("first", sb.ToString());

            sb.AppendArgument("second");
            Assert.Equal("first second", sb.ToString());

            sb.AppendArguments("third", null, "fourth");
            Assert.Equal("first second third fourth", sb.ToString());
        }

        [Fact]
        public void StringValidation_ShouldReturnCorrectStatus()
        {
            Assert.True(((string?)null).IsNullOrWhiteSpace());
            Assert.True("   ".IsNullOrWhiteSpace());
            Assert.False("content".IsNullOrWhiteSpace());

            Assert.False(((string?)null).HasValue());
            Assert.False("   ".HasValue());
            Assert.True("content".HasValue());
        }

        [Fact]
        public void StringManipulation_ShouldRepeatAndJoin()
        {
            Assert.Equal("", "ab".Repeat(0));
            Assert.Equal("ab", "ab".Repeat(1));
            Assert.Equal("ababab", "ab".Repeat(3));

            var list = new List<int> { 1, 2, 3 };
            Assert.Equal("1, 2, 3", list.Join());
            Assert.Equal("1-2-3", list.Join(x => x.ToString(), "-"));
        }

        [Fact]
        public void CollectionUtilities_ShouldCheckNullOrEmpty()
        {
            List<int>? nullList = null;
            Assert.True(nullList.IsNullOrEmpty());
            Assert.True(new List<int>().IsNullOrEmpty());
            Assert.False(new List<int> { 1 }.IsNullOrEmpty());
        }

        [Fact]
        public void CollectionUtilities_ShouldReturnSingleOrNull()
        {
            var single = new List<string?> { "only" };
            Assert.Equal("only", single.SingleOrNull());

            var empty = new List<string?>();
            Assert.Null(empty.SingleOrNull());

            var multiple = new List<string?> { "one", "two" };
            Assert.Null(multiple.SingleOrNull());
        }

        [Fact]
        public void CollectionUtilities_ShouldBatchCorrectly()
        {
            var source = Enumerable.Range(1, 10);
            var batches = source.Batch(3).ToList();

            Assert.Equal(4, batches.Count);
            Assert.Equal(3, batches[0].Count);
            Assert.Equal(3, batches[1].Count);
            Assert.Equal(3, batches[2].Count);
            Assert.Equal(1, batches[3].Count);
        }
    }
}
