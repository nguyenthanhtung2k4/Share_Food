namespace ShareFood.Web.Settings;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromName { get; set; } = "Share Food";

    public string FromEmail { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}
