﻿using System;
using System.Text;

namespace NumberParserTestShared
{
    public class TestDouble : PerformTestBase
    {
        public TestDouble()
        {
            TestName = "TestDouble";

            tests = new Test[]
            {
                new Test("0", 0),
                new Test("1", 1),
                new Test("-1", -1),

                new Test("255", Byte.MaxValue),
                new Test("-128", -128),
                new Test("127", 127),

                new Test("65535", UInt16.MaxValue),
                new Test("-32768", -32768),
                new Test("32767"),

                new Test("4294967295", UInt32.MaxValue),
                new Test("-2147483648", -2147483648),
                new Test("2147483647", Int32.MaxValue),

                new Test("18446744073709551615", UInt64.MaxValue),
                new Test("-9223372036854775808", -9223372036854775808),
                new Test("9223372036854775807", Int64.MaxValue),

                new Test("18446744073709551616"),

                new Test("NaN", double.NaN),
                new Test("Infinity", double.PositiveInfinity),
                new Test("-Infinity", double.NegativeInfinity),
                new Test("1.401298E-45", double.Epsilon),

                new Test("null", true),
                new Test("123.1"),
                new Test("123,1"),
                new Test("1string", true),
                new Test("string1", true),
                new Test("", true),
                new Test(" ", true),
                new Test("+123", 123),
                new Test(" 26", 26),
                new Test("27 ", 27),
                new Test(" 28 " , 28),
                new Test("true", true),
                new Test("false", true),
                new Test("1,0e+1"),
                new Test("1.0e+1"),
                new Test("0123", 123),
                new Test("0x123", true)
            };

        }

        class Test : TestBase
        {
            public Test(string inputString, double result, bool throwsException = false)
                : base(inputString, throwsException)
            {
                InputString = inputString;
                ThrowsException = throwsException;
                Result = result;
            }

            public Test(string inputString, bool throwsException = false)
                : base(inputString, throwsException)
            {
                InputString = inputString;
                ThrowsException = throwsException;
                Result = 0.0;
            }
        }

        public override bool PerformParse(string testString, out object value)
        {
            value = 0.0;

            try
            {
                value = double.Parse(testString);

                return true;
            }
            catch
            {
                // just want to catch the exception
            }

            return false;
        }

        public override bool PerformCompare(object value, object expectedValue)
        {
            return ((double)value).CompareTo((double)expectedValue) == 0;
        }
    }
}
