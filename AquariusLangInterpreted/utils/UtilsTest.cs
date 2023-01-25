using Xunit;

namespace AquariusLang.utils; 

public class UtilsTest {
    struct StringToFloatTest {
        public string input;
        public object expected;
    }
    [Fact]
    public void TestStringToDecimal() {
        StringToFloatTest[] tests = {
            new () {input = "12.74f", expected = 12.74F},
            new () {input = "12.f", expected = 12F},
            
            new () {input = "234.8980d", expected = 234.898},
            new () {input = "0.d", expected = 0.0},
        };
        foreach (StringToFloatTest test in tests) {
            if (test.expected is float _test) {
                float? val = Utils.StringToFloat(test.input);
                Assert.Equal(test.expected, val);
            } else if (test.expected is double __test) {
                double? val = Utils.StringToDouble(test.input);
                Assert.Equal(test.expected, val);
            }
        }
    } 
}