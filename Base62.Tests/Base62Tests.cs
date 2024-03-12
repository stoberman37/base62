using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Base62.Tests
{
    public class Base62Tests
    {
        [Fact]
        public void Encoded_CanBe_Decoded()
        {
            var input = "120";
            var converter = new Base62Converter();
            var encoded = converter.Encode(input);

            var decoded = converter.Decode(encoded);

            Assert.Equal(input, decoded);
        }

        [Fact]
        public void Encoded_Inverted_CanBe_Decoded()
        {
            var input = "Whatup";
            var converter = new Base62Converter(Base62Converter.CharacterSet.INVERTED);
            var encoded = converter.Encode(input);

            var decoded = converter.Decode(encoded);

            Assert.Equal(input, decoded);
        }

        [Theory]
#if NET481
        [InlineData(null, typeof(ArgumentNullException), "value cannot be null or empty\r\nParameter name: charset")]
        [InlineData("", typeof(ArgumentNullException), "value cannot be null or empty\r\nParameter name: charset")]
        [InlineData(" ", typeof(ArgumentNullException), "value cannot be null or empty\r\nParameter name: charset")]
        [InlineData("\t", typeof(ArgumentNullException), "value cannot be null or empty\r\nParameter name: charset")]
        [InlineData("ABC", typeof(ArgumentException), "charset must contain 62 characters\r\nParameter name: charset")]
        [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.", typeof(ArgumentException), "charset must contain 62 characters\r\nParameter name: charset")]
        [InlineData("00123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstvwxyz", typeof(ArgumentException), "charset must contain unique characters\r\nParameter name: charset")]
        [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVVWXYZabcdefghijklmnopqrstvwxyz", typeof(ArgumentException), "charset must contain unique characters\r\nParameter name: charset")]
        [InlineData("013456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzz", typeof(ArgumentException), "charset must contain unique characters\r\nParameter name: charset")]
        [InlineData("123456789.ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", typeof(ArgumentException), "charset must contain only characters A-Z, a-z, and 0-9\r\nParameter name: charset")]
#else
        [InlineData(null, typeof(ArgumentNullException), "value cannot be null or empty (Parameter 'charset')")]
        [InlineData("", typeof(ArgumentNullException), "value cannot be null or empty (Parameter 'charset')")]
        [InlineData(" ", typeof(ArgumentNullException), "value cannot be null or empty (Parameter 'charset')")]
        [InlineData("\t", typeof(ArgumentNullException), "value cannot be null or empty (Parameter 'charset')")]
        [InlineData("ABC", typeof(ArgumentException), "charset must contain 62 characters (Parameter 'charset')")]
        [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.", typeof(ArgumentException), "charset must contain 62 characters (Parameter 'charset')")]
        [InlineData("00123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstvwxyz", typeof(ArgumentException), "charset must contain unique characters (Parameter 'charset')")]
        [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVVWXYZabcdefghijklmnopqrstvwxyz", typeof(ArgumentException), "charset must contain unique characters (Parameter 'charset')")]
        [InlineData("013456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzz", typeof(ArgumentException), "charset must contain unique characters (Parameter 'charset')")]
        [InlineData("123456789.ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", typeof(ArgumentException), "charset must contain only characters A-Z, a-z, and 0-9 (Parameter 'charset')")]
#endif
        public void CustomerCharacterSet_Constructor_Errors(string charset, Type exceptionType, string message)
        {
            var ex = Record.Exception(() => new Base62Converter(charset));
            Assert.True(ex.GetType() == exceptionType);
            switch (ex)
            {
                case ArgumentNullException a:
                    Assert.Equal("charset", a.ParamName);
                    break;
                case ArgumentException a:
                    Assert.Equal("charset", a.ParamName);
                    break;
            }
            Assert.Equal(message, ex.Message);
        }
        
        [Theory]
        [InlineData("120", "tewV2EFDk51cLaMphrnJSCyj4YNWzdxgOuTqIolQ6bfmK97XiA30UP8sGRBvHZ")]
        [InlineData("love爱", "tewV2EFDk51cLaMphrnJSCyj4YNWzdxgOuTqIolQ6bfmK97XiA30UP8sGRBvHZ")]
        [InlineData("abc123XYZ", "tewV2EFDk51cLaMphrnJSCyj4YNWzdxgOuTqIolQ6bfmK97XiA30UP8sGRBvHZ")]
        [InlineData("https://abc123XYZ.com/?@1234=2345", "tewV2EFDk51cLaMphrnJSCyj4YNWzdxgOuTqIolQ6bfmK97XiA30UP8sGRBvHZ")]
        public void Custom_CanBe_Decoded(string input, string charset)
        {
            var converter = new Base62Converter(charset);
            var encoded = converter.Encode(input);

            var decoded = converter.Decode(encoded);

            Assert.Equal(input, decoded);
        }

        [Fact]
        public void NonAscii_CanBe_Decoded()
        {
            var input = "love爱";
            var converter = new Base62Converter(Base62Converter.CharacterSet.DEFAULT);
            var encoded = converter.Encode(input);

            var decoded = converter.Decode(encoded);

            Assert.Equal(input, decoded);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ASCII_AND_UTF8_Can_RoundTrip(string input, string expected)
        {
            var converter = new Base62Converter(Base62Converter.CharacterSet.DEFAULT);
            var encoded = converter.Encode(input);
            var decoded = converter.Decode(encoded);

            Assert.Equal(expected, encoded);
            Assert.Equal(input, decoded);
        }

        [Fact]
        public void FirstZeroBytesAreConvertedCorrectly()
        {
            var sourceBytes = new byte[] { 0, 0, 1, 2, 0, 0 };
            var converter = new Base62Converter(Base62Converter.CharacterSet.DEFAULT);
            var encoded = converter.Encode(sourceBytes);
            var decoded = converter.Decode(encoded);
            
            Assert.Equal(sourceBytes, decoded);
        }

        public static IEnumerable<object[]> GetData()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "validation_data.txt");
            using (var fileReader = new StreamReader(filePath))
            {
                string row = null;
                while ((row = fileReader.ReadLine()) != null)
                {
                    yield return row.Split('\t');
                }
            }
        }
    }
}
