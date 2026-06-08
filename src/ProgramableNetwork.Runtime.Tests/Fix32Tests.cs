using Mafi;
using Xunit;

namespace ProgramableNetwork.Runtime.Tests
{
    public class Fix32Tests
    {
        [Fact]
        public void Constants_have_q21_10_scale()
        {
            Assert.Equal(0, Fix32.Zero.RawValue);
            Assert.Equal(1024, Fix32.One.RawValue);
            Assert.Equal(1024, Fix32.FRACTION_RANGE);
        }

        [Fact]
        public void FromInt_scales_by_1024()
        {
            Assert.Equal(5120, Fix32.FromInt(5).RawValue);
            Assert.Equal(5, Fix32.FromInt(5).IntegerPart);
        }

        [Theory]
        [InlineData(3, 2, 1536)]   // 1.5
        [InlineData(1, 4, 256)]    // 0.25
        [InlineData(7, 2, 3584)]   // 3.5
        public void Division_is_half_rounded(int a, int b, int expectedRaw)
        {
            Fix32 r = Fix32.FromInt(a) / Fix32.FromInt(b);
            Assert.Equal(expectedRaw, r.RawValue);
        }

        [Fact]
        public void Multiplication_matches_fixed_point()
        {
            Fix32 oneAndHalf = Fix32.FromInt(3) / Fix32.FromInt(2);
            Fix32 product = oneAndHalf * Fix32.FromInt(2);
            Assert.Equal(Fix32.FromInt(3).RawValue, product.RawValue);
        }

        [Fact]
        public void IntegerPart_truncates_toward_zero_like_game()
        {
            Fix32 negOneAndHalf = Fix32.FromInt(-3) / Fix32.FromInt(2);
            Assert.Equal(-1, negOneAndHalf.IntegerPart);
        }

        [Fact]
        public void Pow_uses_double_math()
        {
            Assert.Equal(Fix32.FromInt(8).RawValue, Fix32.FromInt(2).Pow(Fix32.FromInt(3)).RawValue);
        }

        [Fact]
        public void Modulo_is_raw_remainder()
        {
            Fix32 r = Fix32.FromInt(7) % Fix32.FromInt(3);
            Assert.Equal(Fix32.FromInt(1).RawValue, r.RawValue);
        }

        [Fact]
        public void ToString_is_invariant_float()
        {
            Fix32 v = Fix32.FromInt(3) / Fix32.FromInt(2);
            Assert.Equal("1.5", v.ToString());
        }
    }
}
