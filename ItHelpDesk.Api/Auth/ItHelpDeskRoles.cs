namespace ItHelpDesk.Api.Auth
{
    public static class ItHelpDeskRoles
    {
        public const string Employee = "Employee";
        public const string Technician = "Technician";
        public const string Admin = "Admin";

        public static readonly IReadOnlyCollection<string> All = new[] { Employee, Technician, Admin };
    }
}
