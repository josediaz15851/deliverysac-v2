namespace API.Services;

public class JwtOptions
{
    public const string Section = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "DeliverySac";
    public string Audience { get; set; } = "DeliverySacApp";
    public int ExpiryMinutes { get; set; } = 480;
}
