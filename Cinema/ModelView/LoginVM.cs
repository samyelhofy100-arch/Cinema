namespace Cinema.ModelView
{
    public class LoginVM
    {
        public int Id { get; set; }
        [Required]
        public string UserNameOrEmail { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
