using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.PayloadTransformerTests;

[TestClass]
public class PayloadTransformerTests
{
    [TestMethod]
    public void Payload_RoundTrip()
    {
        string test = "xyz 123 abc :/\\ !@#$%^ ABC XYZ";
        string expected = "abc 123 def :/\\ !@#$%^ DEF ABC";

        var transformer = new PayloadTransformer(3);

        Assert.AreEqual(expected, transformer.Scramble(test));
        Assert.AreEqual(test, transformer.Unscramble(expected));
    }
}
