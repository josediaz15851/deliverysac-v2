using API.Services;

namespace API.Tests;

public class PasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_NoDevuelveLaPasswordEnClaro()
    {
        var hash = _hasher.Hash("Admin123!");

        Assert.NotEqual("Admin123!", hash);
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void Hash_DosHashesDeLaMismaPasswordSonDistintos()
    {
        var hash1 = _hasher.Hash("Admin123!");
        var hash2 = _hasher.Hash("Admin123!");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ConPasswordCorrecta_ReturnsTrue()
    {
        var hash = _hasher.Hash("Repartidor123!");

        Assert.True(_hasher.Verify("Repartidor123!", hash));
    }

    [Fact]
    public void Verify_ConPasswordIncorrecta_ReturnsFalse()
    {
        var hash = _hasher.Hash("Repartidor123!");

        Assert.False(_hasher.Verify("incorrecta", hash));
    }

    [Fact]
    public void Hash_TieneCosteMinimo10()
    {
        var hash = _hasher.Hash("Supervisor123!");

        var costo = int.Parse(hash.Split('$')[2]);
        Assert.True(costo >= 10);
    }
}
