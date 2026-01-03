namespace MonolithUpdateSite.Models.ViewModels;

public class Manage2FAViewModel
{
    public bool Is2FAEnabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public int RemainingRecoveryCodes { get; set; }
}
