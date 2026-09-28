namespace Supabase.Gotrue;

/// <summary>
/// Options for signing in with a password.
/// </summary>
public sealed class SignInWithPasswordOptions
{
    /// <summary>
    /// Verification token received when the user completes the captcha on the site.
    /// </summary>
    public string? CaptchaToken { get; init; }
}
