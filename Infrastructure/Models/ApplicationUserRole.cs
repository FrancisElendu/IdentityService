using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Models
{
    public class ApplicationUserRole<TKey> : IdentityUserRole<TKey>
    where TKey : IEquatable<TKey>
    {
    }
}
