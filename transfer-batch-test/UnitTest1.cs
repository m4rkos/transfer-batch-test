using System;
using System.IO;
using Xunit;

namespace dotnet_transfer_batch_p;

public class UnitTest1
{
    [Fact]
    public void ProcessTransfers_CalculaComissoesCorretamente()
    {
        string[] lines = new string[]
        {
            "A10,T1000,100.00",
            "A11,T1001,100.00",
            "A10,T1002,200.00",
            "A10,T1003,300.00"
        };
        
        // Expected:
        // For A10: largest values: 100, 200 and 300. Excluding the largest (300) => 100 + 200 = 300, commission = 30.
        // For A11: commission = 10.
        string expectedOutput = "A10,30" + Environment.NewLine +
                                "A11,10" + Environment.NewLine;
        
        // Act: calls the method that processes the data
        var output = Program.ProcessTransfers(lines);
        
        Assert.Equal(expectedOutput, output);
    }
}
