namespace Auth.API.DTOs
{
    public class RoleDTO
    {
        public string Name { get; set; }
        public List<int> Permissions { get; set; }
    }
}
