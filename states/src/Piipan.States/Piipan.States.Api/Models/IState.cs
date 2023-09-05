namespace Piipan.States.Api.Models
{
    public interface IState
    {
        string Id { get; set; }
        string State { get; set; }
        string StateAbbreviation { get; set; }
        string Email { get; set; }
        string Phone { get; set; }
        string Region { get; set; }
        string EmailCc { get; set; }

        T CopyTo<T>() where T : IState, new()
        {
            return new T()
            {
                Id = Id,
                State = State,
                StateAbbreviation = StateAbbreviation,
                Email = Email,
                Phone = Phone,
                Region = Region,
                EmailCc = EmailCc
            };
        }
    }
}
