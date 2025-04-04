namespace DotnetTransferBatch;

public class UnitTest1
{
    [Fact]
    public void ProcessTransfers_CalculaComissoesCorretamente()
    {
        // Arrange: Cria um arquivo temporário com os dados de teste
        string testData = string.Join(
            Environment.NewLine, 
            new List<string>
            {
                "A10,T1000,100.00",
                "A11,T1001,100.00",
                "A10,T1002,200.00",
                "A10,T1003,300.00"
            }) + Environment.NewLine;

        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, testData);

        // Esperado: para A10 a comissão é 30 (pois 100 + 200 = 300 e 10% de 300 = 30) e para A11 é 10
        string expectedOutput = "A10,30" + Environment.NewLine +
                                "A11,10" + Environment.NewLine;

        // Act: Redireciona a saída do console e chama o Main com o caminho do arquivo temporário
        using (var sw = new StringWriter())
        {
            var originalOut = Console.Out;
            Console.SetOut(sw);
                
            Program.Main([tempFilePath]);

            Console.SetOut(originalOut);

            // Assert: Compara a saída capturada com o esperado
            string output = sw.ToString();
            Assert.Equal(expectedOutput, output);
        }

        // Limpeza: exclui o arquivo temporário
        File.Delete(tempFilePath);
    }
}
