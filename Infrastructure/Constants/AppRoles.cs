using System.Collections.ObjectModel;

namespace Infrastructure.Constants
{
    internal class AppRoles
    {
        public const string Admin = nameof(Admin);
        public const string Basic = nameof(Basic);

        public static IReadOnlyList<string> DefaultRoles { get; }
            = new ReadOnlyCollection<string>(
            [
                Admin,
                Basic
            ]);
    }
}
